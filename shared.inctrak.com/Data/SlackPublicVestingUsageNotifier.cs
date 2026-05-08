using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace IncTrak.Data
{
    public class SlackPublicVestingUsageNotifier : IPublicVestingUsageNotifier
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<AppSettings> _options;

        public SlackPublicVestingUsageNotifier(IHttpClientFactory httpClientFactory, IOptions<AppSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public void Notify(PublicVestingUsageEvent usageEvent)
        {
            if (usageEvent == null)
                throw new ArgumentNullException(nameof(usageEvent));

            string webhookUrl = _options.Value.GetSlackFeedbackWebhookUrl();
            if (string.IsNullOrWhiteSpace(webhookUrl))
                return;

            string text = PublicVestingUsageFormatter.BuildSlackMessage(usageEvent);

            using (HttpClient client = _httpClientFactory.CreateClient(nameof(SlackPublicVestingUsageNotifier)))
            using (var content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json"))
            {
                var response = client.PostAsync(webhookUrl, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode == false)
                {
                    string responseBody = response.Content == null
                        ? string.Empty
                        : response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    throw new InvalidOperationException(string.Format(
                        "Slack vesting webhook rejected notification. StatusCode={0}, Body={1}",
                        (int)response.StatusCode,
                        string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody));
                }
            }
        }
    }
}
