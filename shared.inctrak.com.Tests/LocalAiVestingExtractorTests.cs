using System.Collections.Generic;
using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class LocalAiVestingExtractorTests
    {
        [Fact]
        public void ParseResponse_ExtractsMarkdownWrappedJson()
        {
            var extractor = new LocalAiVestingExtractor();

            VestingParseResult result = extractor.ParseResponse(
                "12,000 RSUs vest monthly over 3 years starting February 15, 2026, no cliff.",
                new List<string> { "totalUnits=12000", "grantDate=2026-02-15" },
                "```json\n{\"kind\":\"PeriodicNoCliff\",\"grantDate\":\"2026-02-15\",\"totalUnits\":12000,\"durationMonths\":36,\"cliffMonths\":0,\"cliffPercent\":0,\"postCliffFrequency\":\"Monthly\",\"segments\":[{\"periodAmount\":1,\"frequency\":\"Monthly\",\"increments\":36,\"amountPercent\":2.777778,\"amountUnits\":null,\"description\":\"Monthly vesting\"}],\"explicitTranches\":[],\"assumptions\":[],\"missingFields\":[],\"warnings\":[]}\n```");

            Assert.True(result.UsedAi);
            Assert.Equal(VestingScheduleKind.PeriodicNoCliff, result.Definition.Kind);
            Assert.Equal(12000, result.Definition.TotalUnits);
            Assert.Equal(36, result.Definition.DurationMonths);
        }

        [Fact]
        public void ParseResponse_UsesRepairPassWhenNeeded()
        {
            var extractor = new LocalAiVestingExtractor();

            VestingParseResult result = extractor.ParseResponse(
                "Options vest upon acquisition or IPO.",
                new List<string>(),
                "not-json",
                _ => "{\"kind\":\"MilestoneBased\",\"grantDate\":null,\"totalUnits\":null,\"durationMonths\":null,\"cliffMonths\":null,\"cliffPercent\":null,\"postCliffFrequency\":\"Unknown\",\"segments\":[],\"explicitTranches\":[],\"assumptions\":[],\"missingFields\":[\"grantDate\",\"totalUnits\",\"milestoneDates\"],\"warnings\":[\"Milestone-based vesting cannot be converted to dated schedule events without concrete milestone dates.\"]}");

            Assert.True(result.JsonRepairAttempted);
            Assert.Equal(VestingScheduleKind.MilestoneBased, result.Definition.Kind);
            Assert.Contains("milestoneDates", result.Definition.MissingFields);
        }

        [Fact]
        public void ParseResponse_PreservesGrantedSharesAndGrantDateFromHints()
        {
            var extractor = new LocalAiVestingExtractor();

            VestingParseResult result = extractor.ParseResponse(
                "create a vesting schedule 5 years, equal per year, first 2 years vest, then monthly after for 3 years. granted 50000 shares on 1/1/2023",
                new List<string> { "totalUnits=50000", "grantDate=2023-01-01" },
                "{\"kind\":\"PeriodicNoCliff\",\"grantDate\":null,\"totalUnits\":null,\"durationMonths\":60,\"cliffMonths\":null,\"cliffPercent\":null,\"postCliffFrequency\":\"Monthly\",\"segments\":[{\"periodAmount\":1,\"frequency\":\"Annual\",\"increments\":2,\"amountPercent\":20,\"amountUnits\":null,\"description\":\"Equal yearly vesting\"},{\"periodAmount\":1,\"frequency\":\"Monthly\",\"increments\":36,\"amountPercent\":1.666667,\"amountUnits\":null,\"description\":\"Monthly vesting after the annual segment\"}],\"explicitTranches\":[],\"assumptions\":[],\"missingFields\":[],\"warnings\":[]}");

            Assert.True(result.UsedAi);
            Assert.Equal(50000, result.Definition.TotalUnits);
            Assert.Equal(new System.DateOnly(2023, 1, 1), result.Definition.GrantDate);
        }
    }
}
