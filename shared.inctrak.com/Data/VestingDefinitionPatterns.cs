using System;
using System.Text.RegularExpressions;

namespace IncTrak.Data
{
    public static class VestingDefinitionPatterns
    {
        private static readonly Regex EqualPerYearPattern = new Regex(@"\b(?<total>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\b.*?\bequal per year\b.*?\bfirst\s+(?<initial>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\s+vest\b.*?\bmonthly after\b.*?\bfor\s+(?<remaining>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParse(string prompt, out VestingDefinition definition, out string summary)
        {
            if (TryBuildStandardFourYearCliffMonthly(prompt, out definition, out summary))
            {
                return true;
            }

            if (TryBuildEqualPerYearThenMonthly(prompt, out definition, out summary))
            {
                return true;
            }

            definition = null;
            summary = null;
            return false;
        }

        private static bool TryBuildStandardFourYearCliffMonthly(string prompt, out VestingDefinition definition, out string summary)
        {
            string normalized = prompt?.ToLowerInvariant() ?? string.Empty;
            bool hasFourYears = normalized.Contains("four-year") || normalized.Contains("four year") || normalized.Contains("4 year") || normalized.Contains("4-year");
            bool hasCliff = normalized.Contains("one-year cliff") || normalized.Contains("one year cliff") || normalized.Contains("1-year cliff") || normalized.Contains("1 year cliff");
            bool hasMonthlyAfter = normalized.Contains("monthly after") || normalized.Contains("monthly thereafter");
            if (hasFourYears == false || hasCliff == false || hasMonthlyAfter == false)
            {
                definition = null;
                summary = null;
                return false;
            }

            definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.StandardCliffThenPeriodic,
                DurationMonths = 48,
                CliffMonths = 12,
                CliffPercent = 25m,
                PostCliffFrequency = VestingFrequency.Monthly
            };
            definition.Segments.Add(new VestingSegmentDefinition
            {
                Frequency = VestingFrequency.Annual,
                PeriodAmount = 1,
                Increments = 1,
                AmountPercent = 25m,
                Description = "1-year cliff"
            });
            definition.Segments.Add(new VestingSegmentDefinition
            {
                Frequency = VestingFrequency.Monthly,
                PeriodAmount = 1,
                Increments = 36,
                AmountPercent = 2.083333m,
                Description = "Monthly vesting after the cliff"
            });
            summary = "Cliff at 1 year for 25% of the grant, then vest monthly for the remaining 75% over 36 steps.";
            return true;
        }

        private static bool TryBuildEqualPerYearThenMonthly(string prompt, out VestingDefinition definition, out string summary)
        {
            Match match = EqualPerYearPattern.Match(prompt ?? string.Empty);
            if (match.Success == false)
            {
                definition = null;
                summary = null;
                return false;
            }

            int totalYears = VestingRuleExtractor.ParseWordNumber(match.Groups["total"].Value);
            int initialYears = VestingRuleExtractor.ParseWordNumber(match.Groups["initial"].Value);
            int remainingYears = VestingRuleExtractor.ParseWordNumber(match.Groups["remaining"].Value);
            if (totalYears <= 0 || initialYears <= 0 || remainingYears <= 0 || initialYears + remainingYears != totalYears)
            {
                definition = null;
                summary = null;
                return false;
            }

            decimal yearlyPercent = Math.Round(100m / totalYears, 6);
            decimal remainingPercent = 100m - (yearlyPercent * initialYears);
            int remainingMonths = remainingYears * 12;
            decimal monthlyPercent = Math.Round(remainingPercent / remainingMonths, 6);

            definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.PeriodicNoCliff,
                DurationMonths = totalYears * 12,
                PostCliffFrequency = VestingFrequency.Monthly
            };
            definition.Segments.Add(new VestingSegmentDefinition
            {
                Frequency = VestingFrequency.Annual,
                PeriodAmount = 1,
                Increments = initialYears,
                AmountPercent = yearlyPercent,
                Description = "Equal yearly vesting"
            });
            definition.Segments.Add(new VestingSegmentDefinition
            {
                Frequency = VestingFrequency.Monthly,
                PeriodAmount = 1,
                Increments = remainingMonths,
                AmountPercent = monthlyPercent,
                Description = "Monthly vesting after the annual segment"
            });
            definition.Assumptions.Add("The first segment uses equal annual installments before switching to monthly vesting.");
            summary = $"Vest yearly for {initialYears} years at {yearlyPercent:0.######}% per year, then vest monthly for {remainingYears} years at {monthlyPercent:0.######}% per month.";
            return true;
        }
    }
}
