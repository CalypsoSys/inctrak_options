using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace IncTrak.Data
{
    public sealed class VestingRuleExtractor : IVestingRuleExtractor
    {
        private static readonly Regex UnitsRegex = new Regex(@"\b(?<value>\d[\d,]*)\s+(?:options|shares|rsus?|units?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DurationRegex = new Regex(@"\b(?<value>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)[- ]+years?\b|\b(?<months>\d+)[- ]+months?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CliffRegex = new Regex(@"\b(?<value>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)[- ]+(?:year|month)s?[- ]+cliff\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CliffPercentRegex = new Regex(@"\b(?<value>\d+(?:\.\d+)?)\s*(?:%|percent)\s+(?:after|at)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex GrantDateRegex = new Regex(@"\b(?:grant date|starting|start|vesting start|beginning)\s+(?:on\s+)?(?<date>(?:[A-Za-z]{3,9}\s+\d{1,2}(?:st|nd|rd|th)?(?:,\s*|\s+)\d{4})|(?:\d{4}-\d{2}-\d{2})|(?:\d{1,2}/\d{1,2}/\d{4}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IReadOnlyList<string> ExtractHints(string input)
        {
            return ExtractFacts(input).Hints;
        }

        public VestingRuleFacts ExtractFacts(string input)
        {
            string normalized = Normalize(input);
            var facts = new VestingRuleFacts
            {
                NormalizedInput = normalized
            };

            Match unitsMatch = UnitsRegex.Match(normalized);
            if (unitsMatch.Success && int.TryParse(unitsMatch.Groups["value"].Value.Replace(",", string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out int units))
            {
                facts.TotalUnits = units;
                facts.Hints.Add($"totalUnits={units}");
            }

            foreach (Match match in DurationRegex.Matches(normalized))
            {
                if (match.Value.Contains("cliff", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int? months = ParseDurationMonths(match);
                if (months.HasValue && facts.DurationMonths.HasValue == false)
                {
                    facts.DurationMonths = months.Value;
                    facts.Hints.Add($"durationMonths={months.Value}");
                }
            }

            Match cliffMatch = CliffRegex.Match(normalized);
            if (cliffMatch.Success)
            {
                int? cliffMonths = ParseDurationMonths(cliffMatch);
                if (cliffMonths.HasValue)
                {
                    facts.CliffMonths = cliffMonths.Value;
                    facts.Hints.Add($"cliffMonths={cliffMonths.Value}");
                }
            }

            Match cliffPercentMatch = CliffPercentRegex.Match(normalized);
            if (cliffPercentMatch.Success && decimal.TryParse(cliffPercentMatch.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal cliffPercent))
            {
                facts.CliffPercent = cliffPercent;
                facts.Hints.Add($"cliffPercent={cliffPercent.ToString(CultureInfo.InvariantCulture)}");
            }

            if (normalized.Contains("monthly"))
            {
                facts.Frequency = VestingFrequency.Monthly;
                facts.Hints.Add("frequency=Monthly");
            }
            else if (normalized.Contains("quarterly"))
            {
                facts.Frequency = VestingFrequency.Quarterly;
                facts.Hints.Add("frequency=Quarterly");
            }
            else if (normalized.Contains("semiannual") || normalized.Contains("semi-annual"))
            {
                facts.Frequency = VestingFrequency.SemiAnnual;
                facts.Hints.Add("frequency=SemiAnnual");
            }
            else if (normalized.Contains("annual") || normalized.Contains("annually") || normalized.Contains("yearly") || normalized.Contains("per year") || normalized.Contains("each year"))
            {
                facts.Frequency = VestingFrequency.Annual;
                facts.Hints.Add("frequency=Annual");
            }

            if (normalized.Contains("fully vested immediately") || normalized.Contains("immediate vest"))
            {
                facts.Kind = VestingScheduleKind.Immediate;
                facts.Hints.Add("kind=Immediate");
            }
            else if (normalized.Contains("upon acquisition") || normalized.Contains("upon ipo") || normalized.Contains("board approval") || normalized.Contains("change of control"))
            {
                facts.Kind = VestingScheduleKind.MilestoneBased;
                facts.Hints.Add("kind=MilestoneBased");
            }

            if (normalized.Contains("no cliff"))
            {
                facts.CliffMonths = 0;
                facts.Hints.Add("cliffMonths=0");
            }

            Match dateMatch = GrantDateRegex.Match(normalized);
            if (dateMatch.Success && TryParseDate(dateMatch.Groups["date"].Value, out DateOnly parsedDate))
            {
                facts.GrantDate = parsedDate;
                facts.Hints.Add($"grantDate={parsedDate:yyyy-MM-dd}");
            }

            return facts;
        }

        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string normalized = input
                .Replace('\u2019', '\'')
                .Replace('\u2018', '\'')
                .Replace('\u201c', '"')
                .Replace('\u201d', '"')
                .Replace('\u2013', '-')
                .Replace('\u2014', '-');

            normalized = Regex.Replace(normalized, @"(?<=\d),(?=\d)", string.Empty);
            normalized = Regex.Replace(normalized, @"\b(\d{1,2})(st|nd|rd|th)\b", "$1", RegexOptions.IgnoreCase);
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized.ToLowerInvariant();
        }

        internal static bool TryParseDate(string dateText, out DateOnly parsedDate)
        {
            if (DateOnly.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsedDate) ||
                DateOnly.TryParse(dateText, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces, out parsedDate))
            {
                return true;
            }

            parsedDate = default;
            return false;
        }

        private static int? ParseDurationMonths(Match match)
        {
            if (match.Groups["months"].Success && int.TryParse(match.Groups["months"].Value, out int monthValue))
            {
                return monthValue;
            }

            string value = match.Groups["value"].Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int numeric = ParseWordNumber(value);
            if (numeric <= 0)
            {
                return null;
            }

            return match.Value.Contains("month", StringComparison.OrdinalIgnoreCase)
                ? numeric
                : numeric * 12;
        }

        internal static int ParseWordNumber(string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numeric))
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

    public sealed class VestingRuleFacts
    {
        public string NormalizedInput { get; set; } = string.Empty;
        public List<string> Hints { get; set; } = new List<string>();
        public int? TotalUnits { get; set; }
        public DateOnly? GrantDate { get; set; }
        public int? DurationMonths { get; set; }
        public int? CliffMonths { get; set; }
        public decimal? CliffPercent { get; set; }
        public VestingFrequency Frequency { get; set; } = VestingFrequency.Unknown;
        public VestingScheduleKind Kind { get; set; } = VestingScheduleKind.Unknown;
    }
}
