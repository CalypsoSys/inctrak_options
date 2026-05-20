using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IncTrak.Data
{
    public sealed class LocalAiVestingExtractor
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
        };

        public string BuildSystemPrompt()
        {
            return
                "You extract stock option, RSU, equity grant, and vesting schedule definitions from English. " +
                "Return only valid JSON. Do not use markdown. Do not include comments. Do not explain your answer. " +
                "Do not calculate final vesting rows. Do not invent missing values. Use null for unknown scalar values. " +
                "Use empty arrays for unknown lists. Put inferred conventions in assumptions. Put required-but-missing values in missingFields. " +
                "Put ambiguous or unsupported language in warnings. " +
                "Valid kind values: Unknown, Immediate, PeriodicNoCliff, StandardCliffThenPeriodic, ExplicitTranches, MilestoneBased, PerformanceBased. " +
                "Valid frequency values: Unknown, OneTime, Monthly, Quarterly, SemiAnnual, Annual, Custom. " +
                "If the prompt says standard vesting, do not silently assume a cliff or cadence. " +
                "If the prompt matches a classic four-year one-year cliff monthly schedule, preserve that meaning. " +
                "If the prompt mixes annual and monthly segments, use segments instead of inventing final rows.";
        }

        public string BuildUserPrompt(string input, IReadOnlyList<string> hints)
        {
            string ruleHints = hints == null || hints.Count == 0
                ? "[]"
                : "[\"" + string.Join("\",\"", hints.Select(hint => hint.Replace("\"", "\\\""))) + "\"]";

            return
                $"Input:\n{input}\n\n" +
                $"RuleHints:\n{ruleHints}\n\n" +
                "Return JSON with this shape:\n" +
                "{\n" +
                "  \"kind\": \"StandardCliffThenPeriodic\",\n" +
                "  \"grantDate\": \"2026-01-01\",\n" +
                "  \"totalUnits\": 10000,\n" +
                "  \"durationMonths\": 48,\n" +
                "  \"cliffMonths\": 12,\n" +
                "  \"cliffPercent\": 25,\n" +
                "  \"postCliffFrequency\": \"Monthly\",\n" +
                "  \"segments\": [{\"periodAmount\":1,\"frequency\":\"Monthly\",\"increments\":36,\"amountPercent\":2.083333,\"amountUnits\":null,\"description\":\"Monthly vesting\"}],\n" +
                "  \"explicitTranches\": [],\n" +
                "  \"assumptions\": [],\n" +
                "  \"missingFields\": [],\n" +
                "  \"warnings\": []\n" +
                "}";
        }

        public VestingParseResult ParseResponse(string input, IReadOnlyList<string> hints, string rawResponse, Func<string, string> repairJson = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string json = TryExtractJson(rawResponse);
            bool repairAttempted = false;

            if (TryDeserialize(json, out VestingDefinition definition) == false && repairJson != null)
            {
                repairAttempted = true;
                string repaired = repairJson(json);
                json = TryExtractJson(repaired);
                TryDeserialize(json, out definition);
            }

            if (definition == null)
            {
                definition = new VestingDefinition
                {
                    Kind = VestingScheduleKind.Unknown
                };
                definition.Warnings.Add("The local model response could not be converted into a valid vesting definition.");
            }

            ApplyHintNormalization(input, hints, definition);
            if (VestingDefinitionPatterns.TryParse(input, out VestingDefinition patternDefinition, out _))
            {
                MergePatternOverride(patternDefinition, definition);
                definition = patternDefinition;
            }

            stopwatch.Stop();
            return new VestingParseResult
            {
                OriginalText = input ?? string.Empty,
                Definition = definition,
                RuleHints = hints?.ToList() ?? new List<string>(),
                UsedAi = true,
                JsonRepairAttempted = repairAttempted,
                Elapsed = stopwatch.Elapsed
            };
        }

        private static void MergePatternOverride(VestingDefinition target, VestingDefinition source)
        {
            if (source.TotalUnits.HasValue)
            {
                target.TotalUnits = source.TotalUnits;
            }

            if (source.GrantDate.HasValue)
            {
                target.GrantDate = source.GrantDate;
            }

            foreach (string assumption in source.Assumptions)
            {
                if (target.Assumptions.Contains(assumption) == false)
                {
                    target.Assumptions.Add(assumption);
                }
            }

            foreach (string warning in source.Warnings)
            {
                if (target.Warnings.Contains(warning) == false)
                {
                    target.Warnings.Add(warning);
                }
            }
        }

        private static void ApplyHintNormalization(string input, IReadOnlyList<string> hints, VestingDefinition definition)
        {
            string totalUnitsHint = hints?.FirstOrDefault(hint => hint.StartsWith("totalUnits=", StringComparison.Ordinal));
            string grantDateHint = hints?.FirstOrDefault(hint => hint.StartsWith("grantDate=", StringComparison.Ordinal));
            bool mentionsTotalUnits = string.IsNullOrWhiteSpace(totalUnitsHint) == false;
            bool mentionsGrantDate = string.IsNullOrWhiteSpace(grantDateHint) == false;

            if (mentionsTotalUnits)
            {
                if (definition.TotalUnits.HasValue == false &&
                    int.TryParse(totalUnitsHint.Substring("totalUnits=".Length), out int totalUnits))
                {
                    definition.TotalUnits = totalUnits;
                }
            }
            else
            {
                definition.TotalUnits = null;
            }

            if (mentionsGrantDate)
            {
                if (definition.GrantDate.HasValue == false &&
                    VestingRuleExtractor.TryParseDate(grantDateHint.Substring("grantDate=".Length), out DateOnly grantDate))
                {
                    definition.GrantDate = grantDate;
                }
            }
            else
            {
                definition.GrantDate = null;
            }
        }

        private static bool TryDeserialize(string json, out VestingDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                definition = JsonSerializer.Deserialize<VestingDefinition>(json, JsonOptions);
                return definition != null;
            }
            catch
            {
                return false;
            }
        }

        private static string TryExtractJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            string trimmed = content.Trim();
            int firstBrace = trimmed.IndexOf('{');
            int lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
            }

            return trimmed;
        }
    }
}
