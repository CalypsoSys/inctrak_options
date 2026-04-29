using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IncTrak.data;

namespace IncTrak.Data
{
    public class RulesVestingPromptInterpreter : IVestingPromptInterpreter
    {
        private static readonly Regex NumberAndUnitRegex = new Regex(@"\b(?<value>\d+|one|two|three|four|five|six|seven|eight|nine|ten|eleven|twelve)\s*[- ]?(?<unit>year|years|month|months|week|weeks|day|days)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public QuickVestingInterpretResult Interpret(QuickVestingInterpretRequest request)
        {
            string prompt = request?.Prompt?.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Failure("Describe the vesting schedule you want to build.");
            }

            string normalized = prompt.ToLowerInvariant();
            DurationSpec totalDuration = ParseTotalDuration(normalized);
            if (totalDuration == null)
            {
                return Failure("I could not find the total vesting duration. Try something like 'standard four-year vesting' or 'three-year quarterly vesting'.");
            }

            DurationSpec cadence = ParseCadence(normalized) ?? InferDefaultCadence(totalDuration, normalized);
            if (cadence == null)
            {
                return Failure("I could not tell how often vesting should happen. Try words like monthly, quarterly, weekly, or yearly.");
            }

            DurationSpec cliffDuration = ParseCliffDuration(normalized);
            int totalMonths = ToMonths(totalDuration);
            int cadenceMonths = ToMonths(cadence);
            int cliffMonths = cliffDuration == null ? 0 : ToMonths(cliffDuration);

            if (totalMonths <= 0 || cadenceMonths <= 0)
            {
                return Failure("That schedule is missing a usable duration.");
            }

            if (cliffMonths < 0 || cliffMonths >= totalMonths)
            {
                return Failure("The cliff must be shorter than the full vesting duration.");
            }

            if (totalMonths % cadenceMonths != 0)
            {
                return Failure("The total vesting duration must divide evenly into the selected cadence for this first version.");
            }

            if (cliffMonths > 0 && cliffMonths % cadenceMonths != 0 && normalized.Contains("after"))
            {
                return Failure("The cliff and the follow-on cadence do not line up cleanly. Try a cliff and cadence that divide evenly, such as one year then monthly.");
            }

            var periods = new List<PERIOD_UI>();
            if (cliffMonths > 0)
            {
                decimal cliffPercent = Math.Round(100m * cliffMonths / totalMonths, 6);
                periods.Add(new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = cliffDuration.Value,
                    PERIOD_TYPE_FK = GetPeriodTypeKey(cliffDuration.Unit),
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = cliffPercent,
                    INCREMENTS = 1,
                    ORDER = 0,
                    EVEN_OVER_N = 0
                });

                int remainingMonths = totalMonths - cliffMonths;
                int remainingSteps = remainingMonths / cadenceMonths;
                decimal remainingPercent = 100m - cliffPercent;
                periods.Add(new PERIOD_UI(Guid.Empty)
                {
                    PERIOD_AMOUNT = cadence.Value,
                    PERIOD_TYPE_FK = GetPeriodTypeKey(cadence.Unit),
                    AMOUNT_TYPE_FK = 2,
                    AMOUNT = Math.Round(remainingPercent / remainingSteps, 6),
                    INCREMENTS = remainingSteps,
                    ORDER = 1,
                    EVEN_OVER_N = 0
                });

                return new QuickVestingInterpretResult
                {
                    Success = true,
                    Message = "Built a suggested vesting schedule from your description.",
                    Summary = $"Cliff at {DescribeDuration(cliffDuration)} for {cliffPercent:0.######}% of the grant, then {DescribeCadence(cadence)} for the remaining {remainingPercent:0.######}% over {remainingSteps} steps.",
                    Periods = periods.ToArray()
                };
            }

            int steps = totalMonths / cadenceMonths;
            periods.Add(new PERIOD_UI(Guid.Empty)
            {
                PERIOD_AMOUNT = cadence.Value,
                PERIOD_TYPE_FK = GetPeriodTypeKey(cadence.Unit),
                AMOUNT_TYPE_FK = 2,
                AMOUNT = Math.Round(100m / steps, 6),
                INCREMENTS = steps,
                ORDER = 0,
                EVEN_OVER_N = 0
            });

            return new QuickVestingInterpretResult
            {
                Success = true,
                Message = "Built a suggested vesting schedule from your description.",
                Summary = $"{DescribeCadence(cadence, true)} over {DescribeDuration(totalDuration)} in {steps} equal steps.",
                Periods = periods.ToArray()
            };
        }

        private static QuickVestingInterpretResult Failure(string message)
        {
            return new QuickVestingInterpretResult
            {
                Success = false,
                Message = message,
                Periods = Array.Empty<PERIOD_UI>()
            };
        }

        private static DurationSpec ParseTotalDuration(string normalized)
        {
            Match[] matches = NumberAndUnitRegex.Matches(normalized).Cast<Match>().ToArray();
            if (matches.Length == 0)
            {
                return null;
            }

            foreach (Match match in matches)
            {
                string unit = match.Groups["unit"].Value;
                int value = ParseNumber(match.Groups["value"].Value);
                int start = Math.Max(0, match.Index - 16);
                string context = normalized.Substring(start, Math.Min(normalized.Length - start, 40));
                if (context.Contains("cliff"))
                {
                    continue;
                }

                return new DurationSpec(value, NormalizeUnit(unit));
            }

            Match first = matches[0];
            return new DurationSpec(ParseNumber(first.Groups["value"].Value), NormalizeUnit(first.Groups["unit"].Value));
        }

        private static DurationSpec ParseCliffDuration(string normalized)
        {
            foreach (Match match in NumberAndUnitRegex.Matches(normalized))
            {
                int start = Math.Max(0, match.Index - 16);
                int length = Math.Min(normalized.Length - start, match.Length + 24);
                string context = normalized.Substring(start, length);
                if (context.Contains("cliff"))
                {
                    return new DurationSpec(ParseNumber(match.Groups["value"].Value), NormalizeUnit(match.Groups["unit"].Value));
                }
            }

            return null;
        }

        private static DurationSpec ParseCadence(string normalized)
        {
            if (normalized.Contains("monthly"))
            {
                return new DurationSpec(1, TimeUnit.Months);
            }

            if (normalized.Contains("quarterly"))
            {
                return new DurationSpec(3, TimeUnit.Months);
            }

            if (normalized.Contains("weekly"))
            {
                return new DurationSpec(1, TimeUnit.Weeks);
            }

            if (normalized.Contains("yearly") || normalized.Contains("annually") || normalized.Contains("annual"))
            {
                return new DurationSpec(1, TimeUnit.Years);
            }

            return null;
        }

        private static DurationSpec InferDefaultCadence(DurationSpec totalDuration, string normalized)
        {
            if (normalized.Contains("standard") || normalized.Contains("time-based") || totalDuration.Unit == TimeUnit.Years)
            {
                return new DurationSpec(1, TimeUnit.Months);
            }

            return null;
        }

        private static string DescribeDuration(DurationSpec duration)
        {
            string unit = duration.Unit switch
            {
                TimeUnit.Years => duration.Value == 1 ? "year" : "years",
                TimeUnit.Months => duration.Value == 1 ? "month" : "months",
                TimeUnit.Weeks => duration.Value == 1 ? "week" : "weeks",
                _ => duration.Value == 1 ? "day" : "days"
            };

            return $"{duration.Value} {unit}";
        }

        private static string DescribeCadence(DurationSpec cadence, bool capitalize = false)
        {
            string prefix = capitalize ? "Vest" : "vest";
            if (cadence.Unit == TimeUnit.Months && cadence.Value == 3)
            {
                return $"{prefix} quarterly";
            }

            if (cadence.Unit == TimeUnit.Months && cadence.Value == 1)
            {
                return $"{prefix} monthly";
            }

            if (cadence.Unit == TimeUnit.Weeks && cadence.Value == 1)
            {
                return $"{prefix} weekly";
            }

            if (cadence.Unit == TimeUnit.Years && cadence.Value == 1)
            {
                return $"{prefix} yearly";
            }

            return $"{prefix} every {DescribeDuration(cadence)}";
        }

        private static int ParseNumber(string value)
        {
            if (int.TryParse(value, out int numeric))
            {
                return numeric;
            }

            return value switch
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

        private static TimeUnit NormalizeUnit(string unit)
        {
            return unit switch
            {
                "year" or "years" => TimeUnit.Years,
                "month" or "months" => TimeUnit.Months,
                "week" or "weeks" => TimeUnit.Weeks,
                _ => TimeUnit.Days
            };
        }

        private static int ToMonths(DurationSpec duration)
        {
            return duration.Unit switch
            {
                TimeUnit.Years => duration.Value * 12,
                TimeUnit.Months => duration.Value,
                TimeUnit.Weeks => duration.Value / 4 == 0 ? duration.Value : duration.Value,
                TimeUnit.Days => duration.Value / 30 == 0 ? duration.Value : duration.Value,
                _ => duration.Value
            };
        }

        private static int GetPeriodTypeKey(TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Years => 1,
                TimeUnit.Months => 2,
                TimeUnit.Weeks => 3,
                _ => 4
            };
        }

        private sealed class DurationSpec
        {
            public DurationSpec(int value, TimeUnit unit)
            {
                Value = value;
                Unit = unit;
            }

            public int Value { get; }

            public TimeUnit Unit { get; }
        }

        private enum TimeUnit
        {
            Years,
            Months,
            Weeks,
            Days
        }
    }
}
