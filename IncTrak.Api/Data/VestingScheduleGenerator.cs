using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IncTrak.Data
{
    public sealed class VestingScheduleGenerator : IVestingScheduleGenerator
    {
        private readonly IVestingDefinitionValidator _validator;

        public VestingScheduleGenerator(IVestingDefinitionValidator validator)
        {
            _validator = validator;
        }

        public IReadOnlyList<VestingEvent> Generate(VestingDefinition definition)
        {
            IReadOnlyList<string> errors = _validator.ValidateForScheduleGeneration(definition);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join(" ", errors));
            }

            if (definition.Kind == VestingScheduleKind.Immediate)
            {
                return new[]
                {
                    new VestingEvent
                    {
                        Date = definition.GrantDate.Value,
                        Units = definition.TotalUnits.Value,
                        Percent = 100m,
                        Reason = "Immediate vesting"
                    }
                };
            }

            if (definition.ExplicitTranches.Count > 0)
            {
                return GenerateFromTranches(definition);
            }

            List<VestingSegmentDefinition> segments = definition.Segments.Count > 0
                ? definition.Segments
                : BuildSegmentsFromDefinition(definition);

            return GenerateFromSegments(definition, segments);
        }

        private static IReadOnlyList<VestingEvent> GenerateFromTranches(VestingDefinition definition)
        {
            List<VestingTrancheDefinition> tranches = definition.ExplicitTranches
                .Where(tranche => tranche.VestDate.HasValue)
                .OrderBy(tranche => tranche.VestDate)
                .ToList();

            if (tranches.Count == 0)
            {
                throw new InvalidOperationException("Explicit tranches need concrete dates.");
            }

            List<RawEvent> rawEvents = new List<RawEvent>();
            foreach (VestingTrancheDefinition tranche in tranches)
            {
                decimal rawUnits = tranche.Units ?? ((tranche.Percent ?? 0m) / 100m * definition.TotalUnits.Value);
                rawEvents.Add(new RawEvent
                {
                    Date = tranche.VestDate.Value,
                    RawUnits = rawUnits,
                    Percent = tranche.Percent ?? Math.Round(rawUnits / definition.TotalUnits.Value * 100m, 6),
                    Reason = tranche.Description ?? "Explicit tranche"
                });
            }

            return FinalizeUnits(definition.TotalUnits.Value, rawEvents);
        }

        private static IReadOnlyList<VestingEvent> GenerateFromSegments(VestingDefinition definition, List<VestingSegmentDefinition> segments)
        {
            int elapsedMonths = 0;
            List<RawEvent> rawEvents = new List<RawEvent>();

            foreach (VestingSegmentDefinition segment in segments)
            {
                int intervalMonths = segment.PeriodAmount * GetFrequencyMonths(segment.Frequency);
                for (int index = 0; index < segment.Increments; index++)
                {
                    int monthsFromGrant = elapsedMonths + (intervalMonths * (index + 1));
                    DateOnly eventDate = definition.GrantDate.Value.AddMonths(monthsFromGrant);
                    decimal rawUnits = segment.AmountUnits.HasValue
                        ? segment.AmountUnits.Value
                        : ((segment.AmountPercent ?? 0m) / 100m * definition.TotalUnits.Value);
                    decimal percent = segment.AmountPercent
                        ?? Math.Round(rawUnits / definition.TotalUnits.Value * 100m, 6);

                    rawEvents.Add(new RawEvent
                    {
                        Date = eventDate,
                        RawUnits = rawUnits,
                        Percent = percent,
                        Reason = string.IsNullOrWhiteSpace(segment.Description)
                            ? $"{segment.Frequency} vesting"
                            : segment.Description
                    });
                }

                elapsedMonths += intervalMonths * segment.Increments;
            }

            return FinalizeUnits(definition.TotalUnits.Value, rawEvents);
        }

        private static IReadOnlyList<VestingEvent> FinalizeUnits(int totalUnits, List<RawEvent> rawEvents)
        {
            List<int> roundedUnits = rawEvents
                .Select(raw => (int)Math.Floor(raw.RawUnits))
                .ToList();
            int allocatedUnits = roundedUnits.Sum();
            int remainder = totalUnits - allocatedUnits;
            if (roundedUnits.Count > 0)
            {
                roundedUnits[roundedUnits.Count - 1] += remainder;
            }

            return rawEvents
                .Select((raw, index) => new VestingEvent
                {
                    Date = raw.Date,
                    Units = roundedUnits[index],
                    Percent = raw.Percent,
                    Reason = raw.Reason
                })
                .ToArray();
        }

        private static List<VestingSegmentDefinition> BuildSegmentsFromDefinition(VestingDefinition definition)
        {
            var segments = new List<VestingSegmentDefinition>();
            int durationMonths = definition.DurationMonths ?? 0;
            int cliffMonths = definition.CliffMonths ?? 0;

            if (definition.Kind == VestingScheduleKind.PeriodicNoCliff)
            {
                int intervalMonths = GetFrequencyMonths(definition.PostCliffFrequency);
                int increments = durationMonths / intervalMonths;
                segments.Add(new VestingSegmentDefinition
                {
                    Frequency = definition.PostCliffFrequency,
                    PeriodAmount = 1,
                    Increments = increments,
                    AmountPercent = Math.Round(100m / increments, 6),
                    Description = "Periodic vesting"
                });
                return segments;
            }

            if (definition.Kind == VestingScheduleKind.StandardCliffThenPeriodic)
            {
                if (cliffMonths > 0)
                {
                    segments.Add(new VestingSegmentDefinition
                    {
                        Frequency = ToFrequency(cliffMonths),
                        PeriodAmount = 1,
                        Increments = 1,
                        AmountPercent = definition.CliffPercent ?? Math.Round(100m * cliffMonths / durationMonths, 6),
                        Description = "Cliff vesting"
                    });
                }

                int intervalMonths = GetFrequencyMonths(definition.PostCliffFrequency);
                int remainingMonths = durationMonths - cliffMonths;
                int increments = remainingMonths / intervalMonths;
                decimal remainingPercent = 100m - (definition.CliffPercent ?? 0m);
                segments.Add(new VestingSegmentDefinition
                {
                    Frequency = definition.PostCliffFrequency,
                    PeriodAmount = 1,
                    Increments = increments,
                    AmountPercent = Math.Round(remainingPercent / increments, 6),
                    Description = "Post-cliff vesting"
                });
            }

            return segments;
        }

        internal static int GetFrequencyMonths(VestingFrequency frequency)
        {
            return frequency switch
            {
                VestingFrequency.Monthly => 1,
                VestingFrequency.Quarterly => 3,
                VestingFrequency.SemiAnnual => 6,
                VestingFrequency.Annual => 12,
                VestingFrequency.OneTime => 1,
                _ => 1
            };
        }

        private static VestingFrequency ToFrequency(int months)
        {
            return months switch
            {
                12 => VestingFrequency.Annual,
                6 => VestingFrequency.SemiAnnual,
                3 => VestingFrequency.Quarterly,
                _ => VestingFrequency.Monthly
            };
        }

        private sealed class RawEvent
        {
            public DateOnly Date { get; set; }
            public decimal RawUnits { get; set; }
            public decimal Percent { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
