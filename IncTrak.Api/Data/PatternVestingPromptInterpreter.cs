using System;
using IncTrak.data;

namespace IncTrak.Data
{
    public class PatternVestingPromptInterpreter : IVestingPromptInterpreterProvider
    {
        private readonly VestingRuleExtractor _ruleExtractor;
        private readonly IVestingDefinitionValidator _validator;

        public PatternVestingPromptInterpreter(VestingRuleExtractor ruleExtractor, IVestingDefinitionValidator validator)
        {
            _ruleExtractor = ruleExtractor;
            _validator = validator;
        }

        public string Name => "pattern";

        public int Priority => 100;

        public bool IsAiProvider => false;

        public bool IsConfigured()
        {
            return true;
        }

        public QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request)
        {
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

            if (VestingDefinitionPatterns.TryParse(prompt, out VestingDefinition definition, out _))
            {
                ApplyFacts(definition, _ruleExtractor.ExtractFacts(prompt));
                var parseResult = new VestingParseResult
                {
                    OriginalText = prompt,
                    Definition = definition
                };
                return VestingDefinitionConversion.ToQuickResult(Name, 0.98m, false, parseResult, _validator);
            }

            return new QuickVestingInterpretResult
            {
                Success = false,
                Provider = Name,
                Message = "No standard vesting pattern matched that description.",
                Periods = Array.Empty<PERIOD_UI>()
            };
        }

        private static void ApplyFacts(VestingDefinition definition, VestingRuleFacts facts)
        {
            if (facts.TotalUnits.HasValue)
            {
                definition.TotalUnits = facts.TotalUnits;
            }

            if (facts.GrantDate.HasValue)
            {
                definition.GrantDate = facts.GrantDate;
            }
        }
    }
}
