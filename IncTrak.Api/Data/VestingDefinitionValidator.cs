using System.Collections.Generic;

namespace IncTrak.Data
{
    public sealed class VestingDefinitionValidator : IVestingDefinitionValidator
    {
        public IReadOnlyList<string> ValidateForParsing(VestingDefinition definition)
        {
            var warnings = new List<string>();
            if (definition == null)
            {
                warnings.Add("No vesting definition was produced.");
                return warnings;
            }

            if (definition.Kind == VestingScheduleKind.Unknown)
            {
                warnings.Add("The vesting schedule type is still ambiguous.");
            }

            if (definition.CliffMonths.HasValue && definition.DurationMonths.HasValue && definition.CliffMonths > definition.DurationMonths)
            {
                warnings.Add("The cliff cannot be longer than the full vesting duration.");
            }

            if (definition.CliffPercent.HasValue && (definition.CliffPercent < 0 || definition.CliffPercent > 100))
            {
                warnings.Add("The cliff percent must be between 0 and 100.");
            }

            return warnings;
        }

        public IReadOnlyList<string> ValidateForScheduleGeneration(VestingDefinition definition)
        {
            var errors = new List<string>();
            if (definition == null)
            {
                errors.Add("No vesting definition was produced.");
                return errors;
            }

            if (definition.TotalUnits.HasValue == false || definition.TotalUnits <= 0)
            {
                errors.Add("TotalUnits is required and must be positive.");
            }

            if (definition.GrantDate.HasValue == false)
            {
                errors.Add("GrantDate is required for dated schedule generation.");
            }

            if (definition.Kind == VestingScheduleKind.Immediate)
            {
                return errors;
            }

            if (definition.Kind == VestingScheduleKind.MilestoneBased || definition.Kind == VestingScheduleKind.PerformanceBased)
            {
                errors.Add("Milestone or performance schedules need concrete dates before dated events can be generated.");
                return errors;
            }

            if (definition.Segments.Count > 0)
            {
                foreach (VestingSegmentDefinition segment in definition.Segments)
                {
                    if (segment.PeriodAmount <= 0 || segment.Increments <= 0)
                    {
                        errors.Add("Each vesting segment needs a positive period amount and increments.");
                    }

                    if (segment.AmountPercent.HasValue == false && segment.AmountUnits.HasValue == false)
                    {
                        errors.Add("Each vesting segment needs either units or a percent.");
                    }
                }

                return errors;
            }

            if (definition.Kind == VestingScheduleKind.ExplicitTranches)
            {
                if (definition.ExplicitTranches.Count == 0)
                {
                    errors.Add("Explicit tranches need dated tranche entries.");
                }

                return errors;
            }

            if (definition.DurationMonths.HasValue == false || definition.DurationMonths <= 0)
            {
                errors.Add("DurationMonths must be positive for generated schedules.");
            }

            if (definition.CliffMonths.HasValue && definition.DurationMonths.HasValue && definition.CliffMonths > definition.DurationMonths)
            {
                errors.Add("CliffMonths cannot exceed DurationMonths.");
            }

            if (definition.Kind == VestingScheduleKind.PeriodicNoCliff || definition.Kind == VestingScheduleKind.StandardCliffThenPeriodic)
            {
                if (definition.PostCliffFrequency == VestingFrequency.Unknown)
                {
                    errors.Add("A known frequency is required for periodic schedules.");
                }
            }

            return errors;
        }
    }
}
