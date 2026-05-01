using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class VestingRuleExtractorTests
    {
        [Fact]
        public void ExtractFacts_FindsCommonVestingHints()
        {
            var extractor = new VestingRuleExtractor();

            VestingRuleFacts facts = extractor.ExtractFacts("10,000 options vest over 4 years with a one year cliff and monthly thereafter starting January 1, 2026.");

            Assert.Equal(10000, facts.TotalUnits);
            Assert.Equal(48, facts.DurationMonths);
            Assert.Equal(12, facts.CliffMonths);
            Assert.Equal(VestingFrequency.Monthly, facts.Frequency);
            Assert.Equal(new System.DateOnly(2026, 1, 1), facts.GrantDate);
        }

        [Fact]
        public void ExtractFacts_RecognizesImmediateAndMilestoneSignals()
        {
            var extractor = new VestingRuleExtractor();

            VestingRuleFacts immediate = extractor.ExtractFacts("Fully vested immediately on grant date March 1 2026 for 5,000 shares.");
            VestingRuleFacts milestone = extractor.ExtractFacts("Options vest upon acquisition or IPO.");

            Assert.Equal(VestingScheduleKind.Immediate, immediate.Kind);
            Assert.Equal(VestingScheduleKind.MilestoneBased, milestone.Kind);
        }

        [Fact]
        public void ExtractFacts_RecognizesNoCliffAndQuarterly()
        {
            var extractor = new VestingRuleExtractor();

            VestingRuleFacts facts = extractor.ExtractFacts("12,000 RSUs vest quarterly over 3 years, no cliff.");

            Assert.Equal(0, facts.CliffMonths);
            Assert.Equal(VestingFrequency.Quarterly, facts.Frequency);
            Assert.Equal(36, facts.DurationMonths);
        }

        [Fact]
        public void ExtractFacts_RecognizesGrantedSharesOnSpecificDate()
        {
            var extractor = new VestingRuleExtractor();

            VestingRuleFacts facts = extractor.ExtractFacts("create a vesting schedule 5 years, equal per year, first 2 years vest, then monthly after for 3 years. granted 50000 shares on 1/1/2023");

            Assert.Equal(50000, facts.TotalUnits);
            Assert.Equal(new System.DateOnly(2023, 1, 1), facts.GrantDate);
        }

        [Fact]
        public void ExtractFacts_RecognizesUnitsOnGrantedDateWording()
        {
            var extractor = new VestingRuleExtractor();

            VestingRuleFacts facts = extractor.ExtractFacts("Create a three-year quarterly vesting schedule. 100000 shares on granted 1/1/2022");

            Assert.Equal(100000, facts.TotalUnits);
            Assert.Equal(new System.DateOnly(2022, 1, 1), facts.GrantDate);
        }
    }
}
