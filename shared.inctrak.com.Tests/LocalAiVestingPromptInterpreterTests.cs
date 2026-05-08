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
        private static VestingRuleExtractor CreateRuleExtractor() => new VestingRuleExtractor();
        private static VestingDefinitionValidator CreateValidator() => new VestingDefinitionValidator();
        private static LocalAiVestingExtractor CreateExtractor() => new LocalAiVestingExtractor();

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
        ""content"": ""{\""kind\"":\""PeriodicNoCliff\"",\""grantDate\"":null,\""totalUnits\"":null,\""durationMonths\"":60,\""cliffMonths\"":0,\""cliffPercent\"":0,\""postCliffFrequency\"":\""Monthly\"",\""segments\"":[{\""periodAmount\"":1,\""frequency\"":\""Annual\"",\""increments\"":2,\""amountPercent\"":20,\""amountUnits\"":null,\""description\"":\""Equal yearly vesting\""}, {\""periodAmount\"":1,\""frequency\"":\""Monthly\"",\""increments\"":36,\""amountPercent\"":1.666667,\""amountUnits\"":null,\""description\"":\""Monthly vesting after annual segment\""}],\""explicitTranches\"":[],\""assumptions\"":[],\""missingFields\"":[],\""warnings\"":[]}"" 
      }
    }
  ]
}");

            var interpreter = new LocalAiVestingPromptInterpreter(httpClientFactory, settings, CreateRuleExtractor(), CreateExtractor(), CreateValidator());

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
        ""content"": ""{\""kind\"":\""PeriodicNoCliff\"",\""grantDate\"":null,\""totalUnits\"":null,\""durationMonths\"":\""60\"",\""cliffMonths\"":\""0\"",\""cliffPercent\"":\""0\"",\""postCliffFrequency\"":\""Monthly\"",\""segments\"":[{\""periodAmount\"":\""1\"",\""frequency\"":\""Annual\"",\""increments\"":\""2\"",\""amountPercent\"":\""20\"",\""amountUnits\"":null,\""description\"":\""Equal yearly vesting\""}, {\""periodAmount\"":\""1\"",\""frequency\"":\""Monthly\"",\""increments\"":\""36\"",\""amountPercent\"":\""1.666667\"",\""amountUnits\"":null,\""description\"":\""Monthly vesting after annual segment\""}],\""explicitTranches\"":[],\""assumptions\"":[],\""missingFields\"":[],\""warnings\"":[]}"" 
      }
    }
  ]
}");

            var interpreter = new LocalAiVestingPromptInterpreter(httpClientFactory, settings, CreateRuleExtractor(), CreateExtractor(), CreateValidator());

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
            var ruleExtractor = CreateRuleExtractor();
            var validator = CreateValidator();
            var localExtractor = CreateExtractor();
            var parser = new HybridNlpVestingDefinitionParser(ruleExtractor, validator);
            var pattern = new PatternVestingPromptInterpreter(ruleExtractor, validator);
            var llama = new LlamaSharpVestingPromptInterpreter(Options.Create(new AppSettings()), ruleExtractor, localExtractor, validator);
            var local = new LocalAiVestingPromptInterpreter(new StubHttpClientFactory("{}"), Options.Create(new AppSettings()), ruleExtractor, localExtractor, validator);
            var rules = new RulesVestingPromptInterpreter(parser, ruleExtractor, validator);
            IVestingPromptInterpreterProvider[] providers = { pattern, llama, local, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a three-year quarterly vesting schedule."
            });

            Assert.True(result.Success);
            Assert.Single(result.Periods);
            Assert.Equal(12, result.Periods[0].INCREMENTS);
            Assert.Equal("parser", result.Provider);
        }

        [Fact]
        public void HybridInterpreter_StrictAi_DoesNotFallBackToRules()
        {
            var ruleExtractor = CreateRuleExtractor();
            var validator = CreateValidator();
            var localExtractor = CreateExtractor();
            var parser = new HybridNlpVestingDefinitionParser(ruleExtractor, validator);
            var pattern = new PatternVestingPromptInterpreter(ruleExtractor, validator);
            var llama = new LlamaSharpVestingPromptInterpreter(Options.Create(new AppSettings()), ruleExtractor, localExtractor, validator);
            var local = new LocalAiVestingPromptInterpreter(new StubHttpClientFactory("{}"), Options.Create(new AppSettings()), ruleExtractor, localExtractor, validator);
            var rules = new RulesVestingPromptInterpreter(parser, ruleExtractor, validator);
            IVestingPromptInterpreterProvider[] providers = { pattern, llama, local, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a three-year quarterly vesting schedule.",
                StrictAi = true
            });

            Assert.False(result.Success);
            Assert.Equal("strict-ai", result.Provider);
            Assert.Contains("AI interpreters could not build", result.Message);
        }

        [Fact]
        public void HybridInterpreter_PrefersBuiltInParserBeforeConfiguredAi()
        {
            var ruleExtractor = CreateRuleExtractor();
            var validator = CreateValidator();
            var localExtractor = CreateExtractor();
            var parser = new HybridNlpVestingDefinitionParser(ruleExtractor, validator);
            var pattern = new PatternVestingPromptInterpreter(ruleExtractor, validator);
            var llama = new LlamaSharpVestingPromptInterpreter(Options.Create(new AppSettings()), ruleExtractor, localExtractor, validator);
            var local = new LocalAiVestingPromptInterpreter(
                new StubHttpClientFactory(@"
{
  ""choices"": [
    {
      ""message"": {
        ""content"": ""{\""kind\"":\""PeriodicNoCliff\"",\""grantDate\"":null,\""totalUnits\"":null,\""durationMonths\"":48,\""cliffMonths\"":0,\""cliffPercent\"":0,\""postCliffFrequency\"":\""Monthly\"",\""segments\"":[{\""periodAmount\"":1,\""frequency\"":\""Monthly\"",\""increments\"":48,\""amountPercent\"":2.083333,\""amountUnits\"":null,\""description\"":\""Monthly vesting\""}],\""explicitTranches\"":[],\""assumptions\"":[],\""missingFields\"":[],\""warnings\"":[]}"" 
      }
    }
  ]
}"),
                Options.Create(new AppSettings
                {
                    LocalAiEndpoint = "http://localhost:11434/v1/chat/completions",
                    LocalAiModel = "llama3.2"
                }),
                ruleExtractor,
                localExtractor,
                validator);
            var rules = new RulesVestingPromptInterpreter(parser, ruleExtractor, validator);
            IVestingPromptInterpreterProvider[] providers = { pattern, llama, local, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a standard four-year monthly vesting schedule."
            });

            Assert.True(result.Success);
            Assert.Equal("parser", result.Provider);
            Assert.False(result.RequiresAi);
        }

        [Fact]
        public void HybridInterpreter_PatternResultKeepsGrantedSharesAndGrantDate()
        {
            var ruleExtractor = CreateRuleExtractor();
            var validator = CreateValidator();
            var localExtractor = CreateExtractor();
            var parser = new HybridNlpVestingDefinitionParser(ruleExtractor, validator);
            var pattern = new PatternVestingPromptInterpreter(ruleExtractor, validator);
            var rules = new RulesVestingPromptInterpreter(parser, ruleExtractor, validator);
            IVestingPromptInterpreterProvider[] providers = { pattern, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "create a vesting schedule 5 years, equal per year, first 2 years vest, then monthly after for 3 years. granted 50000 shares on 1/1/2023"
            });

            Assert.True(result.Success);
            Assert.Equal("pattern", result.Provider);
            Assert.Equal(50000m, result.SharesGranted);
            Assert.Equal("2023-01-01", result.VestingStart);
        }

        [Fact]
        public void HybridInterpreter_ExposesAlternateBuiltInProviderWhenResultsDiffer()
        {
            IVestingPromptInterpreterProvider[] providers =
            {
                new StubPromptInterpreterProvider("pattern", 100, false, new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = "pattern",
                    Confidence = 0.98m,
                    Periods = new[] { new IncTrak.data.PERIOD_UI(Guid.Empty) { PERIOD_AMOUNT = 1, PERIOD_TYPE_FK = 1, AMOUNT_TYPE_FK = 2, AMOUNT = 25m, INCREMENTS = 1 } }
                }),
                new StubPromptInterpreterProvider("parser", 200, false, new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = "parser",
                    Confidence = 0.90m,
                    Periods = new[] { new IncTrak.data.PERIOD_UI(Guid.Empty) { PERIOD_AMOUNT = 1, PERIOD_TYPE_FK = 2, AMOUNT_TYPE_FK = 2, AMOUNT = 2.083333m, INCREMENTS = 48 } }
                })
            };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "some prompt"
            });

            Assert.True(result.Success);
            Assert.Equal("pattern", result.Provider);
            Assert.Equal("parser", result.AlternateProvider);
        }

        [Fact]
        public void HybridInterpreter_CanApplyPreferredAlternateBuiltInProvider()
        {
            var ruleExtractor = CreateRuleExtractor();
            var validator = CreateValidator();
            var localExtractor = CreateExtractor();
            var parser = new HybridNlpVestingDefinitionParser(ruleExtractor, validator);
            var pattern = new PatternVestingPromptInterpreter(ruleExtractor, validator);
            var rules = new RulesVestingPromptInterpreter(parser, ruleExtractor, validator);
            IVestingPromptInterpreterProvider[] providers = { pattern, rules };
            var interpreter = new HybridVestingPromptInterpreter(providers);

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "create a vesting schedule 5 years, equal per year, first 2 years vest, then monthly after for 3 years. granted 50000 shares on 1/1/2023",
                PreferredProvider = "parser"
            });

            Assert.True(result.Success);
            Assert.Equal("parser", result.Provider);
            Assert.Equal(50000m, result.SharesGranted);
            Assert.Equal("2023-01-01", result.VestingStart);
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

        private sealed class StubPromptInterpreterProvider : IVestingPromptInterpreterProvider
        {
            private readonly QuickVestingInterpretResult _result;

            public StubPromptInterpreterProvider(string name, int priority, bool isAiProvider, QuickVestingInterpretResult result)
            {
                Name = name;
                Priority = priority;
                IsAiProvider = isAiProvider;
                _result = result;
            }

            public string Name { get; }

            public int Priority { get; }

            public bool IsAiProvider { get; }

            public bool IsConfigured()
            {
                return true;
            }

            public QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request)
            {
                return _result;
            }
        }
    }
}
