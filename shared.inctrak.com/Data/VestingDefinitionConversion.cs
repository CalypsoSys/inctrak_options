using System;
using System.Globalization;
using System.Linq;
using IncTrak.data;

namespace IncTrak.Data
{
    public static class VestingDefinitionConversion
    {
        public static QuickVestingInterpretResult ToQuickResult(
            string provider,
            decimal confidence,
            bool requiresAi,
            VestingParseResult parseResult,
            IVestingDefinitionValidator validator)
        {
            VestingDefinition definition = parseResult?.Definition ?? new VestingDefinition();
            PERIOD_UI[] periods = definition.Segments.Count > 0
                ? definition.Segments.Select((segment, index) => ToPeriodUi(segment, index)).Where(period => period != null).ToArray()
                : Array.Empty<PERIOD_UI>();

            if (periods.Length == 0 && definition.Kind != VestingScheduleKind.Unknown)
            {
                definition.Warnings.Add("This vesting shape is not yet editable in the quick period builder.");
            }

            foreach (string warning in validator.ValidateForParsing(definition))
            {
                if (definition.Warnings.Contains(warning) == false)
                {
                    definition.Warnings.Add(warning);
                }
            }

            bool success = periods.Length > 0;
            string message = success
                ? "Built a suggested vesting schedule from your description."
                : BuildMessage(definition);

            return new QuickVestingInterpretResult
            {
                Success = success,
                Message = message,
                Summary = BuildSummary(definition),
                Provider = provider,
                Confidence = confidence,
                RequiresAi = requiresAi,
                SharesGranted = definition.TotalUnits,
                VestingStart = definition.GrantDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Periods = periods,
                Kind = definition.Kind.ToString(),
                UsedAi = parseResult?.UsedAi == true,
                JsonRepairAttempted = parseResult?.JsonRepairAttempted == true,
                Warnings = definition.Warnings.ToArray(),
                MissingFields = definition.MissingFields.ToArray(),
                Assumptions = definition.Assumptions.ToArray()
            };
        }

        private static PERIOD_UI ToPeriodUi(VestingSegmentDefinition segment, int order)
        {
            int? periodType = segment.Frequency switch
            {
                VestingFrequency.Annual => 1,
                VestingFrequency.Monthly => 2,
                VestingFrequency.Quarterly => 2,
                VestingFrequency.SemiAnnual => 2,
                _ => null
            };

            int periodAmount = segment.Frequency switch
            {
                VestingFrequency.Quarterly => 3,
                VestingFrequency.SemiAnnual => 6,
                _ => segment.PeriodAmount <= 0 ? 1 : segment.PeriodAmount
            };

            if (periodType == null || segment.Increments <= 0)
            {
                return null;
            }

            return new PERIOD_UI(Guid.Empty)
            {
                PERIOD_AMOUNT = periodAmount,
                PERIOD_TYPE_FK = periodType,
                AMOUNT_TYPE_FK = segment.AmountUnits.HasValue ? 1 : 2,
                AMOUNT = segment.AmountUnits ?? segment.AmountPercent ?? 0m,
                INCREMENTS = segment.Increments,
                ORDER = order,
                EVEN_OVER_N = 0
            };
        }

        private static string BuildSummary(VestingDefinition definition)
        {
            if (definition.Segments.Count == 0)
            {
                if (definition.Kind == VestingScheduleKind.MilestoneBased || definition.Kind == VestingScheduleKind.PerformanceBased)
                {
                    return "This schedule depends on a milestone or performance trigger rather than a dated vesting table.";
                }

                return string.Join(" ", definition.Warnings.Concat(definition.MissingFields.Select(field => $"Missing {field}."))).Trim();
            }

            string segments = string.Join(" ", definition.Segments.Select(segment =>
            {
                string amount = segment.AmountUnits.HasValue
                    ? $"{segment.AmountUnits.Value} units"
                    : $"{segment.AmountPercent.GetValueOrDefault():0.######}%";
                return $"{segment.Increments} x every {segment.PeriodAmount} {segment.Frequency}: {amount}.";
            }));
            return segments.Trim();
        }

        private static string BuildMessage(VestingDefinition definition)
        {
            if (definition.MissingFields.Count > 0)
            {
                return "I still need: " + string.Join(", ", definition.MissingFields) + ".";
            }

            if (definition.Warnings.Count > 0)
            {
                return definition.Warnings[0];
            }

            return "I could not build a vesting schedule from that description yet.";
        }
    }
}
