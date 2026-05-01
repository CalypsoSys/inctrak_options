using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IncTrak.Data;
using Microsoft.Extensions.Options;
using Xunit;

namespace inctrak.com.Tests
{
    public class LocalAiVestingPromptInterpreterTests
    {
        [Fact]
        public void TryInterpret_ParsesOpenAiCompatibleLocalResponse()
        {
            var settings = Options.Create(new AppSettings
            {
                LocalAiEndpoint = "http://localhost:11434/v1/chat/completions",
                LocalAiModel = "llama3.2"
            });
            var httpClientFactory = new StubHttpClientFactory(@"
{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""{\""summary\"":\""Yearly for two years, then monthly for three years.\"",\""sharesGranted\"":4800,\""vestingStart\"":\""2026-01-01\"",\""periods\"":[{\""periodAmount\"":1,\""periodType\"":\""Years\"",\""amountType\"":\""Percentage\"",\""amount\"":20,\""increments\"":2,\""evenOverN\"":0},{\""periodAmount\"":1,\""periodType\"":\""Months\"",\""amountType\"":\""Percentage\"",\""amount\"":1.666667,\""increments\"":36,\""evenOverN\"":0}]}""
      }
    }
  ]
}");

            var interpreter = new LocalAiVestingPromptInterpreter(httpClientFactory, settings);

            QuickVestingInterpretResult result = interpreter.TryInterpret(new QuickVestingInterpretRequest
            {
                Prompt = "create a vesting schedule 5 year, equal per year, first 2 years vest, then monthly after for 3 years"
            });

            Assert.True(result.Success);
            Assert.Null(result.SharesGranted);
            Assert.Null(result.VestingStart);
            Assert.Equal(2, result.Periods.Length);
            Assert.Equal(1, result.Periods[0].PERIOD_TYPE_FK);
            Assert.Equal(20m, result.Periods[0].AMOUNT);
            Assert.Equal(2, result.Periods[0].INCREMENTS);
            Assert.Equal(2, result.Periods[1].PERIOD_TYPE_FK);
            Assert.Equal(36, result.Periods[1].INCREMENTS);
        }

        [Fact]
        public void TryInterpret_AllowsStringifiedNumericFields()
        {
            var settings = Options.Create(new AppSettings
            {
                LocalAiEndpoint = "http://localhost:11434/v1/chat/completions",
                LocalAiModel = "llama3.2"
            });
            var httpClientFactory = new StubHttpClientFactory(@"
{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""{\""summary\"":\""Yearly for two years, then monthly for three years.\"",\""periods\"":[{\""periodAmount\"":\""1\"",\""periodType\"":\""Years\"",\""amountType\"":\""Percentage\"",\""amount\"":\""20\"",\""increments\"":\""2\"",\""evenOverN\"":\""0\""} ,{\""periodAmount\"":\""1\"",\""periodType\"":\""Months\"",\""amountType\"":\""Percentage\"",\""amount\"":\""1.666667\"",\""increments\"":\""36\"",\""evenOverN\"":\""0\""}]}""
      }
    }
  ]
}");

            var interpreter = new LocalAiVestingPromptInterpreter(httpClientFactory, settings);

            QuickVestingInterpretResult result = interpreter.TryInterpret(new QuickVestingInterpretRequest
            {
                Prompt = "create a vesting schedule 5 year, equal per year, first 2 years vest, then monthly after for 3 years"
            });

            Assert.True(result.Success);
            Assert.Equal(2, result.Periods.Length);
            Assert.Equal(2, result.Periods[0].INCREMENTS);
            Assert.Equal(36, result.Periods[1].INCREMENTS);
            Assert.Equal(1.666667m, result.Periods[1].AMOUNT);
        }

        [Fact]
        public void HybridInterpreter_FallsBackToRules_WhenLocalAiNotConfigured()
        {
            var llama = new LlamaSharpVestingPromptInterpreter(Options.Create(new AppSettings()));
            var local = new LocalAiVestingPromptInterpreter(new StubHttpClientFactory("{}"), Options.Create(new AppSettings()));
            var rules = new RulesVestingPromptInterpreter();
            IVestingPromptInterpreterProvider[] providers = { llama, local, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a three-year quarterly vesting schedule."
            });

            Assert.True(result.Success);
            Assert.Single(result.Periods);
            Assert.Equal(12, result.Periods[0].INCREMENTS);
            Assert.Equal("rules", result.Provider);
        }

        [Fact]
        public void HybridInterpreter_StrictAi_DoesNotFallBackToRules()
        {
            var llama = new LlamaSharpVestingPromptInterpreter(Options.Create(new AppSettings()));
            var local = new LocalAiVestingPromptInterpreter(new StubHttpClientFactory("{}"), Options.Create(new AppSettings()));
            var rules = new RulesVestingPromptInterpreter();
            IVestingPromptInterpreterProvider[] providers = { llama, local, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a three-year quarterly vesting schedule.",
                StrictAi = true
            });

            Assert.False(result.Success);
            Assert.Equal("strict-ai", result.Provider);
            Assert.Contains("no AI interpreter is configured", result.Message);
        }

        private sealed class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly string _responseBody;

            public StubHttpClientFactory(string responseBody)
            {
                _responseBody = responseBody;
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient(new StubHandler(_responseBody))
                {
                    BaseAddress = new Uri("http://localhost")
                };
            }
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly string _responseBody;

            public StubHandler(string responseBody)
            {
                _responseBody = responseBody;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
