using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IncTrak.data;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class LocalAiVestingPromptInterpreter : IVestingPromptInterpreterProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _settings;

        public LocalAiVestingPromptInterpreter(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value ?? new AppSettings();
        }

        public string Name => "local-http";

        public int Priority => 200;

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
            using var message = new HttpRequestMessage(HttpMethod.Post, _settings.GetLocalAiEndpoint());
            string apiKey = _settings.GetLocalAiApiKey();
            if (string.IsNullOrWhiteSpace(apiKey) == false)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var payload = new
            {
                model = _settings.GetLocalAiModel(),
                temperature = 0.1m,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = BuildSystemPrompt()
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = client.SendAsync(message).GetAwaiter().GetResult();
            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode == false)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = $"Local AI request failed with status {(int)response.StatusCode}.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            string content = ExtractContent(responseBody);
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

            AiScheduleResponse parsed = JsonSerializer.Deserialize<AiScheduleResponse>(ExtractJsonObject(content), JsonOptions);
            if (parsed?.Periods == null || parsed.Periods.Length == 0)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = Name,
                    Message = "Local AI did not return any usable vesting periods.",
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
                    Message = "Local AI returned periods, but they did not pass validation.",
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
                "Do not include markdown. " +
                "Interpret natural requests sensibly. Example: " +
                "'create a vesting schedule 5 year, equal per year, first 2 years vest, then monthly after for 3 years' " +
                "should become two periods: yearly percentage vesting for the first two years, then monthly percentage vesting for the remaining three years.";
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
