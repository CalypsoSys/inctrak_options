using System;
using IncTrak.Data;
using IncTrak.data;
using Xunit;

namespace inctrak.com.Tests
{
    public class VestingPromptPostProcessorTests
    {
        [Fact]
        public void Apply_StripsInventedGrantFields_WhenPromptDidNotMentionThem()
        {
            QuickVestingInterpretResult result = VestingPromptPostProcessor.Apply(
                "I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after.",
                new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = "llamasharp",
                    SharesGranted = 10000m,
                    VestingStart = "2023-01-01",
                    Periods = new[]
                    {
                        new PERIOD_UI(Guid.Empty)
                        {
                            PERIOD_AMOUNT = 1,
                            PERIOD_TYPE_FK = 1,
                            AMOUNT_TYPE_FK = 2,
                            AMOUNT = 100m,
                            INCREMENTS = 1,
                            ORDER = 0,
                            EVEN_OVER_N = 0
                        }
                    }
                });

            Assert.True(result.Success);
            Assert.Null(result.SharesGranted);
            Assert.Null(result.VestingStart);
        }

        [Fact]
        public void Apply_NormalizesStandardFourYearCliffMonthlyPrompt()
        {
            QuickVestingInterpretResult result = VestingPromptPostProcessor.Apply(
                "I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after.",
                new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = "llamasharp",
                    Periods = Array.Empty<PERIOD_UI>()
                });

            Assert.True(result.Success);
            Assert.Equal(2, result.Periods.Length);
            Assert.Equal(1, result.Periods[0].PERIOD_TYPE_FK);
            Assert.Equal(25m, result.Periods[0].AMOUNT);
            Assert.Equal(2, result.Periods[1].PERIOD_TYPE_FK);
            Assert.Equal(36, result.Periods[1].INCREMENTS);
            Assert.Equal(2.083333m, result.Periods[1].AMOUNT);
        }

        [Fact]
        public void Apply_NormalizesEqualPerYearThenMonthlyPrompt()
        {
            QuickVestingInterpretResult result = VestingPromptPostProcessor.Apply(
                "create a vesting schedule 5 years, equal per year, first 2 years vest, then monthly after for 3 years",
                new QuickVestingInterpretResult
                {
                    Success = true,
                    Provider = "llamasharp",
                    Periods = Array.Empty<PERIOD_UI>()
                });

            Assert.True(result.Success);
            Assert.Equal(2, result.Periods.Length);
            Assert.Equal(1, result.Periods[0].PERIOD_TYPE_FK);
            Assert.Equal(20m, result.Periods[0].AMOUNT);
            Assert.Equal(2, result.Periods[0].INCREMENTS);
            Assert.Equal(2, result.Periods[1].PERIOD_TYPE_FK);
            Assert.Equal(36, result.Periods[1].INCREMENTS);
            Assert.Equal(1.666667m, result.Periods[1].AMOUNT);
        }
    }
}
