using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IncTrak.data;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class LlamaSharpVestingPromptInterpreter : IVestingPromptInterpreterProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private readonly AppSettings _settings;
        private readonly object _sync = new object();
        private LLamaWeights _weights;

        public LlamaSharpVestingPromptInterpreter(IOptions<AppSettings> options)
        {
            _settings = options.Value ?? new AppSettings();
        }

        public string Name => "llamasharp";

        public int Priority => 100;

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
                using var context = weights.CreateContext(parameters);
                var executor = new InteractiveExecutor(context);

                var chatHistory = new ChatHistory();
                chatHistory.AddMessage(AuthorRole.System, BuildSystemPrompt());
                ChatSession session = new ChatSession(executor, chatHistory);

                var inferenceParams = new InferenceParams
                {
                    MaxTokens = Math.Max(256, _settings.GetLocalAiMaxTokens()),
                    AntiPrompts = new List<string> { "USER:", "User:", "\nUSER:", "\nUser:" },
                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        Temperature = 0.1f
                    }
                };

                StringBuilder output = new StringBuilder();
                var userMessage = new ChatHistory.Message(AuthorRole.User, prompt);
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

                string content = output.ToString().Trim();
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

                AiScheduleResponse parsed = JsonSerializer.Deserialize<AiScheduleResponse>(ExtractJsonObject(content), JsonOptions);
                if (parsed?.Periods == null || parsed.Periods.Length == 0)
                {
                    return new QuickVestingInterpretResult
                    {
                        Success = false,
                        Provider = Name,
                        Message = "Embedded local AI did not return any usable vesting periods.",
                        Periods = Array.Empty<PERIOD_UI>()
                    };
                }

                PERIOD_UI[] periods = parsed.Periods
                    .Select((period, index) => ToPeriodUi(period, index))
                    .Where(period => period != null)
                    .ToArray();

                if (periods.Length == 0)
                {
                    return new QuickVestingInterpretResult
                    {
                        Success = false,
                        Provider = Name,
                        Message = "Embedded local AI returned periods, but they did not pass validation.",
                        Periods = Array.Empty<PERIOD_UI>()
                    };
                }

                return VestingPromptPostProcessor.Apply(prompt, new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = Name,
                    Message = "Built a suggested vesting schedule from your description.",
                    Summary = parsed.Summary,
                    SharesGranted = parsed.SharesGranted,
                    VestingStart = parsed.VestingStart,
                    Periods = periods
                });
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

        private static string BuildSystemPrompt()
        {
            return
                "You convert employee equity vesting descriptions into strict JSON only. " +
                "Return one JSON object with properties 'summary', 'sharesGranted', 'vestingStart', and 'periods'. " +
                "Set sharesGranted only if the prompt explicitly mentions a share count. " +
                "Set vestingStart only if the prompt explicitly mentions a start date, formatted as yyyy-MM-dd. " +
                "Each item in periods must contain: periodAmount, periodType, amountType, amount, increments, evenOverN. " +
                "Allowed periodType values: Years, Months, Weeks, Days. " +
                "Allowed amountType values: Shares, Percentage. " +
                "Allowed evenOverN values: 0, 1, 2. " +
                "Do not include markdown, prose outside the JSON object, or code fences. " +
                "Use AI only to interpret the English. Keep the periods simple and explicit. " +
                "Example: 'create a vesting schedule 5 year, equal per year, first 2 years vest, then monthly after for 3 years' " +
                "should become two percentage-based periods: yearly for two years, then monthly for thirty-six months.";
        }

        private static string ExtractJsonObject(string content)
        {
            int firstBrace = content.IndexOf('{');
            int lastBrace = content.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return content.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            return content;
        }

        private static PERIOD_UI ToPeriodUi(AiPeriod period, int index)
        {
            if (period == null || period.PeriodAmount <= 0 || period.Increments <= 0 || period.Amount <= 0)
            {
                return null;
            }

            int? periodType = period.PeriodType?.Trim().ToLowerInvariant() switch
            {
                "years" or "year" => 1,
                "months" or "month" => 2,
                "weeks" or "week" => 3,
                "days" or "day" => 4,
                _ => null
            };

            int? amountType = period.AmountType?.Trim().ToLowerInvariant() switch
            {
                "shares" or "share" => 1,
                "percentage" or "percent" => 2,
                _ => null
            };

            if (periodType == null || amountType == null)
            {
                return null;
            }

            return new PERIOD_UI(Guid.Empty)
            {
                PERIOD_AMOUNT = period.PeriodAmount,
                PERIOD_TYPE_FK = periodType,
                AMOUNT_TYPE_FK = amountType,
                AMOUNT = period.Amount,
                INCREMENTS = period.Increments,
                ORDER = index,
                EVEN_OVER_N = period.EvenOverN
            };
        }

        private sealed class AiScheduleResponse
        {
            public string Summary { get; set; }

            public decimal? SharesGranted { get; set; }

            public string VestingStart { get; set; }

            public AiPeriod[] Periods { get; set; }
        }

        private sealed class AiPeriod
        {
            public int PeriodAmount { get; set; }

            public string PeriodType { get; set; }

            public string AmountType { get; set; }

            public decimal Amount { get; set; }

            public int Increments { get; set; }

            public int EvenOverN { get; set; }
        }
    }
}
