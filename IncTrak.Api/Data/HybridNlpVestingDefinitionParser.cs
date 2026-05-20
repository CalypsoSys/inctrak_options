using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IncTrak.Data
{
    public sealed class HybridNlpVestingDefinitionParser : IVestingDefinitionParser
    {
        private readonly VestingRuleExtractor _ruleExtractor;
        private readonly IVestingDefinitionValidator _validator;

        public HybridNlpVestingDefinitionParser(VestingRuleExtractor ruleExtractor, IVestingDefinitionValidator validator)
        {
            _ruleExtractor = ruleExtractor;
            _validator = validator;
        }

        public Task<VestingParseResult> ParseAsync(string input, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            VestingRuleFacts facts = _ruleExtractor.ExtractFacts(input);
            var result = new VestingParseResult
            {
                OriginalText = input ?? string.Empty,
                RuleHints = new List<string>(facts.Hints),
                UsedAi = false
            };

            if (VestingDefinitionPatterns.TryParse(input, out VestingDefinition patternDefinition, out _))
            {
                ApplyFacts(patternDefinition, facts);
                result.Definition = patternDefinition;
            }
            else
            {
                result.Definition = BuildDefinitionFromFacts(facts);
            }

            foreach (string warning in _validator.ValidateForParsing(result.Definition))
            {
                if (result.Definition.Warnings.Contains(warning) == false)
                {
                    result.Definition.Warnings.Add(warning);
                }
            }

            stopwatch.Stop();
            result.Elapsed = stopwatch.Elapsed;
            return Task.FromResult(result);
        }

        private static VestingDefinition BuildDefinitionFromFacts(VestingRuleFacts facts)
        {
            var definition = new VestingDefinition
            {
                GrantDate = facts.GrantDate,
                TotalUnits = facts.TotalUnits,
                DurationMonths = facts.DurationMonths,
                CliffMonths = facts.CliffMonths,
                CliffPercent = facts.CliffPercent,
                PostCliffFrequency = facts.Frequency
            };

            if (facts.Kind != VestingScheduleKind.Unknown)
            {
                definition.Kind = facts.Kind;
            }
            else if (facts.DurationMonths.HasValue && facts.Frequency != VestingFrequency.Unknown)
            {
                definition.Kind = facts.CliffMonths.GetValueOrDefault() > 0
                    ? VestingScheduleKind.StandardCliffThenPeriodic
                    : VestingScheduleKind.PeriodicNoCliff;
            }

            if (definition.Kind == VestingScheduleKind.Unknown && facts.DurationMonths.HasValue)
            {
                definition.Warnings.Add("The phrase is still somewhat ambiguous, so this is only a best-effort draft.");
            }

            bool ambiguousStandard = facts.NormalizedInput.Contains("standard") &&
                                     facts.NormalizedInput.Contains("vesting") &&
                                     facts.CliffMonths.HasValue == false &&
                                     facts.Frequency == VestingFrequency.Unknown;
            if (ambiguousStandard)
            {
                definition.Warnings.Add("The phrase standard vesting is ambiguous and was not expanded into a cliff/frequency assumption.");
                definition.Kind = VestingScheduleKind.Unknown;
                if (definition.MissingFields.Contains("postCliffFrequency") == false)
                {
                    definition.MissingFields.Add("postCliffFrequency");
                }
            }

            if (facts.TotalUnits.HasValue == false)
            {
                definition.MissingFields.Add("totalUnits");
            }

            if (facts.GrantDate.HasValue == false)
            {
                definition.MissingFields.Add("grantDate");
            }

            if (definition.Kind == VestingScheduleKind.StandardCliffThenPeriodic && definition.CliffMonths.HasValue == false)
            {
                definition.MissingFields.Add("cliffMonths");
            }

            if ((definition.Kind == VestingScheduleKind.PeriodicNoCliff || definition.Kind == VestingScheduleKind.StandardCliffThenPeriodic) &&
                definition.PostCliffFrequency == VestingFrequency.Unknown)
            {
                definition.MissingFields.Add("postCliffFrequency");
            }

            BuildSegments(definition);
            return definition;
        }

        private static void BuildSegments(VestingDefinition definition)
        {
            if (definition.Segments.Count > 0 || definition.DurationMonths.HasValue == false)
            {
                return;
            }

            if (definition.Kind == VestingScheduleKind.PeriodicNoCliff && definition.PostCliffFrequency != VestingFrequency.Unknown)
            {
                int cadenceMonths = VestingScheduleGenerator.GetFrequencyMonths(definition.PostCliffFrequency);
                if (definition.DurationMonths.Value % cadenceMonths == 0)
                {
                    int increments = definition.DurationMonths.Value / cadenceMonths;
                    definition.Segments.Add(new VestingSegmentDefinition
                    {
                        Frequency = definition.PostCliffFrequency,
                        PeriodAmount = 1,
                        Increments = increments,
                        AmountPercent = Math.Round(100m / increments, 6),
                        Description = "Periodic vesting"
                    });
                }
            }

            if (definition.Kind == VestingScheduleKind.StandardCliffThenPeriodic &&
                definition.CliffMonths.HasValue &&
                definition.PostCliffFrequency != VestingFrequency.Unknown)
            {
                int cliffMonths = definition.CliffMonths.Value;
                int durationMonths = definition.DurationMonths.Value;
                int cadenceMonths = VestingScheduleGenerator.GetFrequencyMonths(definition.PostCliffFrequency);
                decimal cliffPercent = definition.CliffPercent ?? Math.Round(100m * cliffMonths / durationMonths, 6);
                int remainingMonths = durationMonths - cliffMonths;
                if (remainingMonths > 0 && remainingMonths % cadenceMonths == 0)
                {
                    int increments = remainingMonths / cadenceMonths;
                    definition.Segments.Add(new VestingSegmentDefinition
                    {
                        Frequency = cliffMonths >= 12 ? VestingFrequency.Annual : VestingFrequency.Custom,
                        PeriodAmount = cliffMonths >= 12 ? cliffMonths / 12 : cliffMonths,
                        Increments = 1,
                        AmountPercent = cliffPercent,
                        Description = "Cliff vesting"
                    });
                    definition.Segments.Add(new VestingSegmentDefinition
                    {
                        Frequency = definition.PostCliffFrequency,
                        PeriodAmount = 1,
                        Increments = increments,
                        AmountPercent = Math.Round((100m - cliffPercent) / increments, 6),
                        Description = "Post-cliff vesting"
                    });
                }
            }
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

            if (definition.DurationMonths.HasValue == false && facts.DurationMonths.HasValue)
            {
                definition.DurationMonths = facts.DurationMonths;
            }

            if (definition.CliffMonths.HasValue == false && facts.CliffMonths.HasValue)
            {
                definition.CliffMonths = facts.CliffMonths;
            }

            if (definition.CliffPercent.HasValue == false && facts.CliffPercent.HasValue)
            {
                definition.CliffPercent = facts.CliffPercent;
            }
        }
    }
}
