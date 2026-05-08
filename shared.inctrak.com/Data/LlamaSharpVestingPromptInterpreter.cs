using System;
using System.Collections.Generic;
using System.Text;
using IncTrak.data;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class LlamaSharpVestingPromptInterpreter : IVestingPromptInterpreterProvider
    {
        private readonly AppSettings _settings;
        private readonly VestingRuleExtractor _ruleExtractor;
        private readonly LocalAiVestingExtractor _extractor;
        private readonly IVestingDefinitionValidator _validator;
        private readonly object _sync = new object();
        private LLamaWeights _weights;

        public LlamaSharpVestingPromptInterpreter(
            IOptions<AppSettings> options,
            VestingRuleExtractor ruleExtractor,
            LocalAiVestingExtractor extractor,
            IVestingDefinitionValidator validator)
        {
            _settings = options.Value ?? new AppSettings();
            _ruleExtractor = ruleExtractor;
            _extractor = extractor;
            _validator = validator;
        }

        public string Name => "llamasharp";

        public int Priority => 300;

        public bool IsAiProvider => true;

        public bool IsConfigured()
        {
            return string.IsNullOrWhiteSpace(_settings.GetLocalAiModelPath()) == false;
        }

        public QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request)
        {
            if (IsConfigured() == false)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = "Embedded local AI is not configured.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            string prompt = request?.Prompt?.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = "Describe the vesting schedule you want to build.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            try
            {
                var parameters = new ModelParams(_settings.GetLocalAiModelPath())
                {
                    ContextSize = Math.Max(1024u, (uint)_settings.GetLocalAiContextSize()),
                    GpuLayerCount = _settings.GetLocalAiGpuLayerCount()
                };

                LLamaWeights weights = GetWeights(parameters);
                IReadOnlyList<string> hints = _ruleExtractor.ExtractHints(prompt);
                string content = RunPrompt(weights, parameters, _extractor.BuildSystemPrompt(), _extractor.BuildUserPrompt(prompt, hints), 0.1f);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new QuickVestingInterpretResult
                    {
                        Success = false,
                        Provider = Name,
                        Message = "Embedded local AI returned an empty response.",
                        Periods = Array.Empty<PERIOD_UI>()
                    };
                }

                VestingParseResult parseResult = _extractor.ParseResponse(
                    prompt,
                    hints,
                    content,
                    brokenJson => RunPrompt(weights, parameters, "Repair invalid vesting-definition JSON. Return JSON only.", brokenJson, 0.05f));

                return VestingDefinitionConversion.ToQuickResult(Name, 0.65m, false, parseResult, _validator);
            }
            catch (Exception excp)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = $"Embedded local AI failed: {excp.Message}",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }
        }

        private string RunPrompt(LLamaWeights weights, ModelParams parameters, string systemPrompt, string userPrompt, float temperature)
        {
            using var context = weights.CreateContext(parameters);
            var executor = new InteractiveExecutor(context);
            var chatHistory = new ChatHistory();
            chatHistory.AddMessage(AuthorRole.System, systemPrompt);
            ChatSession session = new ChatSession(executor, chatHistory);

            var inferenceParams = new InferenceParams
            {
                MaxTokens = Math.Max(256, _settings.GetLocalAiMaxTokens()),
                AntiPrompts = new List<string> { "USER:", "User:", "\nUSER:", "\nUser:" },
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = temperature
                }
            };

            StringBuilder output = new StringBuilder();
            var userMessage = new ChatHistory.Message(AuthorRole.User, userPrompt);
            IAsyncEnumerable<string> stream = session.ChatAsync(userMessage, inferenceParams);
            var enumerator = stream.GetAsyncEnumerator();
            try
            {
                while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
                {
                    output.Append(enumerator.Current);
                }
            }
            finally
            {
                enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            return output.ToString().Trim();
        }

        private LLamaWeights GetWeights(ModelParams parameters)
        {
            lock (_sync)
            {
                if (_weights == null)
                {
                    _weights = LLamaWeights.LoadFromFile(parameters);
                }

                return _weights;
            }
        }
    }
}
