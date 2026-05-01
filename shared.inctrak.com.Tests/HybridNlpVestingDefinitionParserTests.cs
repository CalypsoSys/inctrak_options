using System.Linq;
using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class HybridNlpVestingDefinitionParserTests
    {
        private static HybridNlpVestingDefinitionParser CreateParser()
        {
            var validator = new VestingDefinitionValidator();
            var extractor = new VestingRuleExtractor();
            return new HybridNlpVestingDefinitionParser(extractor, validator);
        }

        [Fact]
        public async System.Threading.Tasks.Task ParseAsync_ParsesClassicFourYearCliffPrompt()
        {
            VestingParseResult result = await CreateParser()
                .ParseAsync("10,000 options vest over 4 years with a 1 year cliff and monthly vesting thereafter, grant date Jan 1 2026.");

            Assert.Equal(VestingScheduleKind.StandardCliffThenPeriodic, result.Definition.Kind);
            Assert.Equal(10000, result.Definition.TotalUnits);
            Assert.Equal(new System.DateOnly(2026, 1, 1), result.Definition.GrantDate);
            Assert.Equal(48, result.Definition.DurationMonths);
            Assert.Equal(12, result.Definition.CliffMonths);
            Assert.Equal(VestingFrequency.Monthly, result.Definition.PostCliffFrequency);
            Assert.Equal(2, result.Definition.Segments.Count);
        }

        [Fact]
        public async System.Threading.Tasks.Task ParseAsync_KeepsStandardVestingAmbiguous()
        {
            VestingParseResult result = await CreateParser()
                .ParseAsync("Standard 4 year vesting.");

            Assert.Equal(VestingScheduleKind.Unknown, result.Definition.Kind);
            Assert.Contains(result.Definition.Warnings, warning => warning.Contains("ambiguous"));
            Assert.Contains("postCliffFrequency", result.Definition.MissingFields);
        }
    }
}
