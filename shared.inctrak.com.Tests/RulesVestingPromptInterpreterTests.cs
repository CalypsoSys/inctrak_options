using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class RulesVestingPromptInterpreterTests
    {
        [Fact]
        public void Interpret_BuildsStandardFourYearCliffMonthlySchedule()
        {
            var interpreter = new RulesVestingPromptInterpreter();

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after."
            });

            Assert.True(result.Success);
            Assert.Equal(2, result.Periods.Length);
            Assert.Equal(1, result.Periods[0].PERIOD_AMOUNT);
            Assert.Equal(1, result.Periods[0].PERIOD_TYPE_FK);
            Assert.Equal(25m, result.Periods[0].AMOUNT);
            Assert.Equal(1, result.Periods[0].INCREMENTS);
            Assert.Equal(1, result.Periods[1].PERIOD_AMOUNT);
            Assert.Equal(2, result.Periods[1].PERIOD_TYPE_FK);
            Assert.Equal(36, result.Periods[1].INCREMENTS);
            Assert.Equal(2.083333m, result.Periods[1].AMOUNT);
        }

        [Fact]
        public void Interpret_BuildsThreeYearQuarterlySchedule()
        {
            var interpreter = new RulesVestingPromptInterpreter();

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a three-year quarterly vesting schedule."
            });

            Assert.True(result.Success);
            Assert.Single(result.Periods);
            Assert.Equal(3, result.Periods[0].PERIOD_AMOUNT);
            Assert.Equal(2, result.Periods[0].PERIOD_TYPE_FK);
            Assert.Equal(12, result.Periods[0].INCREMENTS);
            Assert.Equal(8.333333m, result.Periods[0].AMOUNT);
        }

        [Fact]
        public void Interpret_FillsSharesAndVestingStartWhenPromptIncludesThem()
        {
            var interpreter = new RulesVestingPromptInterpreter();

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Create a standard four-year monthly vesting schedule for 4800 shares starting January 1, 2026."
            });

            Assert.True(result.Success);
            Assert.Equal(4800m, result.SharesGranted);
            Assert.Equal("2026-01-01", result.VestingStart);
        }

        [Fact]
        public void Interpret_ReturnsHelpfulErrorWhenDurationMissing()
        {
            var interpreter = new RulesVestingPromptInterpreter();

            QuickVestingInterpretResult result = interpreter.Interpret(new QuickVestingInterpretRequest
            {
                Prompt = "Make it monthly after a cliff."
            });

            Assert.False(result.Success);
            Assert.Contains("total vesting duration", result.Message);
        }
    }
}
