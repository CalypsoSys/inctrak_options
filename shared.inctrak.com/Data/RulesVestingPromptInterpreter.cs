using System;
using System.Linq;
using IncTrak.data;

namespace IncTrak.Data
{
    public class RulesVestingPromptInterpreter : IVestingPromptInterpreter, IVestingPromptInterpreterProvider
    {
        private readonly HybridNlpVestingDefinitionParser _parser;
        private readonly VestingRuleExtractor _ruleExtractor;
        private readonly IVestingDefinitionValidator _validator;

        public RulesVestingPromptInterpreter(
            HybridNlpVestingDefinitionParser parser,
            VestingRuleExtractor ruleExtractor,
            IVestingDefinitionValidator validator)
        {
            _parser = parser;
            _ruleExtractor = ruleExtractor;
            _validator = validator;
        }

        public string Name => "parser";

        public int Priority => 200;

        public bool IsAiProvider => false;

        public bool IsConfigured()
        {
            return true;
        }

        public QuickVestingInterpretResult Interpret(QuickVestingInterpretRequest request)
        {
            string prompt = request?.Prompt?.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Failure("Describe the vesting schedule you want to build.");
            }

            VestingParseResult parseResult = _parser.ParseAsync(prompt).GetAwaiter().GetResult();
            VestingRuleFacts facts = _ruleExtractor.ExtractFacts(prompt);
            bool hasShapeGap = parseResult.Definition.MissingFields.Any(field =>
                string.Equals(field, "grantDate", StringComparison.OrdinalIgnoreCase) == false &&
                string.Equals(field, "totalUnits", StringComparison.OrdinalIgnoreCase) == false);
            bool ambiguous = parseResult.Definition.Kind == VestingScheduleKind.Unknown ||
                             hasShapeGap ||
                             parseResult.Definition.Warnings.Any(warning => warning.IndexOf("ambiguous", StringComparison.OrdinalIgnoreCase) >= 0);
            decimal confidence = ambiguous ? 0.72m : 0.9m;
            bool requiresAi = ambiguous;

            QuickVestingInterpretResult result = VestingDefinitionConversion.ToQuickResult(Name, confidence, requiresAi, parseResult, _validator);
            if (result.Success == false && parseResult.Definition.DurationMonths.HasValue == false)
            {
                return Failure("I could not find the total vesting duration. Try something like 'standard four-year vesting' or 'three-year quarterly vesting'.");
            }

            if (result.Success == false && parseResult.Definition.PostCliffFrequency == VestingFrequency.Unknown)
            {
                return Failure("I could not tell how often vesting should happen. Try words like monthly, quarterly, weekly, or yearly.");
            }

            return result;
        }

        public QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request)
        {
            return Interpret(request);
        }

        private static QuickVestingInterpretResult Failure(string message)
        {
            return new QuickVestingInterpretResult
            {
                Success = false,
                Provider = "parser",
                Message = message,
                Periods = Array.Empty<PERIOD_UI>()
            };
        }
    }
}
