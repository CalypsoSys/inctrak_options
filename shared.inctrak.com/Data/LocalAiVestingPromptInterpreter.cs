using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IncTrak.data;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class LocalAiVestingPromptInterpreter : IVestingPromptInterpreterProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;
        private readonly VestingRuleExtractor _ruleExtractor;
        private readonly LocalAiVestingExtractor _extractor;
        private readonly IVestingDefinitionValidator _validator;

        public LocalAiVestingPromptInterpreter(
            IHttpClientFactory httpClientFactory,
            IOptions<AppSettings> options,
            VestingRuleExtractor ruleExtractor,
            LocalAiVestingExtractor extractor,
            IVestingDefinitionValidator validator)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value ?? new AppSettings();
            _ruleExtractor = ruleExtractor;
            _extractor = extractor;
            _validator = validator;
        }

        public string Name => "local-http";

        public int Priority => 400;

        public bool IsAiProvider => true;

        public bool IsConfigured()
        {
            return string.IsNullOrWhiteSpace(_settings.GetLocalAiEndpoint()) == false &&
                   string.IsNullOrWhiteSpace(_settings.GetLocalAiModel()) == false;
        }

        public QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request)
        {
            if (IsConfigured() == false)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = "Local AI is not configured.",
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

            HttpClient client = _httpClientFactory.CreateClient(nameof(LocalAiVestingPromptInterpreter));
            IReadOnlyList<string> hints = _ruleExtractor.ExtractHints(prompt);
            string content = SendChatRequest(client, _extractor.BuildSystemPrompt(), _extractor.BuildUserPrompt(prompt, hints), 0.1m);
            if (string.IsNullOrWhiteSpace(content))
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = "Local AI returned an empty response.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            VestingParseResult parseResult = _extractor.ParseResponse(
                prompt,
                hints,
                content,
                brokenJson => SendChatRequest(client, "Repair invalid vesting-definition JSON. Return JSON only.", brokenJson, 0.05m));

            return VestingDefinitionConversion.ToQuickResult(Name, 0.65m, false, parseResult, _validator);
        }

        private string SendChatRequest(HttpClient client, string systemPrompt, string userPrompt, decimal temperature)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, _settings.GetLocalAiEndpoint());
            string apiKey = _settings.GetLocalAiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey) == false)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var payload = new
            {
                model = _settings.GetLocalAiModel(),
                temperature,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = client.SendAsync(message).GetAwaiter().GetResult();
            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode == false)
            {
                throw new InvalidOperationException($"Local AI request failed with status {(int)response.StatusCode}.");
            }

            return ExtractContent(responseBody);
        }

        private static string ExtractContent(string responseBody)
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;
            if (root.TryGetProperty("choices", out JsonElement choices) == false || choices.GetArrayLength() == 0)
            {
                return null;
            }

            JsonElement choice = choices[0];
            if (choice.TryGetProperty("message", out JsonElement message) &&
                message.TryGetProperty("content", out JsonElement content))
            {
                return content.GetString();
            }

            return null;
        }
    }
}
