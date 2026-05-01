using System;
using System.Globalization;
using System.Text.RegularExpressions;
using IncTrak.data;

namespace IncTrak.Data
{
    public static class VestingPromptPostProcessor
    {
        private static readonly Regex SharesRegex = new Regex(@"\b(?<shares>\d[\d,]*(?:\.\d+)?)\s+shares?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex StartDateRegex = new Regex(@"\b(?:vesting\s+start|start(?:ing|s)?|begin(?:ning|s)?)\s+(?:on\s+)?(?<date>(?:[A-Za-z]{3,9}\s+\d{1,2},\s+\d{4})|(?:\d{4}-\d{2}-\d{2})|(?:\d{1,2}/\d{1,2}/\d{4}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex EqualPerYearPattern = new Regex(@"\b(?<total>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\b.*?\bequal per year\b.*?\bfirst\s+(?<initial>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\s+vest\b.*?\bmonthly after\b.*?\bfor\s+(?<remaining>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s+years?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static QuickVestingInterpretResult Apply(string prompt, QuickVestingInterpretResult result)
        {
            string normalizedPrompt = prompt?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedPrompt) || result == null)
            {
                return result;
            }

            result.SharesGranted = ParseSharesGranted(normalizedPrompt);
            result.VestingStart = ParseVestingStart(normalizedPrompt);

            if (TryBuildStandardFourYearCliffMonthly(normalizedPrompt, out PERIOD_UI[] standardPeriods, out string standardSummary))
            {
                result.Periods = standardPeriods;
                result.Summary = standardSummary;
                result.Message = "Built a suggested vesting schedule from your description.";
                return result;
            }

            if (TryBuildEqualPerYearThenMonthly(normalizedPrompt, out PERIOD_UI[] equalPerYearPeriods, out string equalPerYearSummary))
            {
                result.Periods = equalPerYearPeriods;
                result.Summary = equalPerYearSummary;
                result.Message = "Built a suggested vesting schedule from your description.";
                return result;
            }

            return result;
        }

        private static bool TryBuildStandardFourYearCliffMonthly(string prompt, out PERIOD_UI[] periods, out string summary)
        {
            string normalized = prompt.ToLowerInvariant();
            if (normalized.Contains("four-year") == false &&
                normalized.Contains("four year") == false &&
                normalized.Contains("4 year") == false &&
                normalized.Contains("4-year") == false)
            {
                periods = null;
                summary = null;
                return false;
            }

            if (normalized.Contains("one-year cliff") == false &&
                normalized.Contains("one year cliff") == false &&
                normalized.Contains("1-year cliff") == false &&
                normalized.Contains("1 year cliff") == false)
            {
                periods = null;
                summary = null;
                return false;
            }

            if (normalized.Contains("monthly after") == false)
            {
                periods = null;
                summary = null;
                return false;
            }

            periods = new[]
            {
                new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = 1,
                    PERIOD_TYPE_FK = 1,
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = 25m,
                    INCREMENTS = 1,
                    ORDER = 0,
                    EVEN_OVER_N = 0
                },
                new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = 1,
                    PERIOD_TYPE_FK = 2,
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = 2.083333m,
                    INCREMENTS = 36,
                    ORDER = 1,
                    EVEN_OVER_N = 0
                }
            };
            summary = "Cliff at 1 year for 25% of the grant, then vest monthly for the remaining 75% over 36 steps.";
            return true;
        }

        private static bool TryBuildEqualPerYearThenMonthly(string prompt, out PERIOD_UI[] periods, out string summary)
        {
            Match match = EqualPerYearPattern.Match(prompt);
            if (match.Success == false)
            {
                periods = null;
                summary = null;
                return false;
            }

            int totalYears = ParseWordNumber(match.Groups["total"].Value);
            int initialYears = ParseWordNumber(match.Groups["initial"].Value);
            int remainingYears = ParseWordNumber(match.Groups["remaining"].Value);
            if (totalYears <= 0 || initialYears <= 0 || remainingYears <= 0 || initialYears + remainingYears != totalYears)
            {
                periods = null;
                summary = null;
                return false;
            }

            decimal yearlyPercent = Math.Round(100m / totalYears, 6);
            decimal remainingPercent = 100m - (yearlyPercent * initialYears);
            int remainingMonths = remainingYears * 12;
            decimal monthlyPercent = Math.Round(remainingPercent / remainingMonths, 6);

            periods = new[]
            {
                new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = 1,
                    PERIOD_TYPE_FK = 1,
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = yearlyPercent,
                    INCREMENTS = initialYears,
                    ORDER = 0,
                    EVEN_OVER_N = 0
                },
                new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = 1,
                    PERIOD_TYPE_FK = 2,
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = monthlyPercent,
                    INCREMENTS = remainingMonths,
                    ORDER = 1,
                    EVEN_OVER_N = 0
                }
            };
            summary = $"Vest yearly for {initialYears} years at {yearlyPercent:0.######}% per year, then vest monthly for {remainingYears} years at {monthlyPercent:0.######}% per month.";
            return true;
        }

        private static decimal? ParseSharesGranted(string prompt)
        {
            Match match = SharesRegex.Match(prompt);
            if (match.Success == false)
            {
                return null;
            }

            string sharesText = match.Groups["shares"].Value.Replace(",", string.Empty);
            return decimal.TryParse(sharesText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal shares)
                ? shares
                : null;
        }

        private static string ParseVestingStart(string prompt)
        {
            Match match = StartDateRegex.Match(prompt);
            if (match.Success == false)
            {
                return null;
            }

            string dateText = match.Groups["date"].Value.Trim().TrimEnd('.', ';', ':');
            if (DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsed) ||
                DateTime.TryParse(dateText, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces, out parsed))
            {
                return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static int ParseWordNumber(string value)
        {
            if (int.TryParse(value, out int numeric))
            {
                return numeric;
            }

            return value.Trim().ToLowerInvariant() switch
            {
                "one" => 1,
                "two" => 2,
                "three" => 3,
                "four" => 4,
                "five" => 5,
                "six" => 6,
                "seven" => 7,
                "eight" => 8,
                "nine" => 9,
                "ten" => 10,
                "eleven" => 11,
                "twelve" => 12,
                _ => 0
            };
        }
    }
}
