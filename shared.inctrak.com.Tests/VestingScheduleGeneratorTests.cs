using System;
using System.Linq;
using IncTrak.Data;
using Xunit;

namespace inctrak.com.Tests
{
    public class VestingScheduleGeneratorTests
    {
        private static VestingScheduleGenerator CreateGenerator()
        {
            return new VestingScheduleGenerator(new VestingDefinitionValidator());
        }

        [Fact]
        public void Generate_BuildsClassicFourYearCliffSchedule()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.StandardCliffThenPeriodic,
                GrantDate = new DateOnly(2026, 1, 1),
                TotalUnits = 10000,
                DurationMonths = 48,
                CliffMonths = 12,
                CliffPercent = 25,
                PostCliffFrequency = VestingFrequency.Monthly
            };

            var events = CreateGenerator().Generate(definition);

            Assert.Equal(37, events.Count);
            Assert.Equal(new DateOnly(2027, 1, 1), events[0].Date);
            Assert.Equal(2500, events[0].Units);
            Assert.Equal(10000, events.Sum(e => e.Units));
        }

        [Fact]
        public void Generate_BuildsMonthlyNoCliffSchedule()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.PeriodicNoCliff,
                GrantDate = new DateOnly(2026, 2, 15),
                TotalUnits = 12000,
                DurationMonths = 36,
                CliffMonths = 0,
                PostCliffFrequency = VestingFrequency.Monthly
            };

            var events = CreateGenerator().Generate(definition);

            Assert.Equal(36, events.Count);
            Assert.Equal(12000, events.Sum(e => e.Units));
        }

        [Fact]
        public void Generate_BuildsImmediateSchedule()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.Immediate,
                GrantDate = new DateOnly(2026, 3, 1),
                TotalUnits = 5000
            };

            var events = CreateGenerator().Generate(definition);

            Assert.Single(events);
            Assert.Equal(new DateOnly(2026, 3, 1), events[0].Date);
            Assert.Equal(5000, events[0].Units);
        }

        [Fact]
        public void Generate_BuildsQuarterlySchedule()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.PeriodicNoCliff,
                GrantDate = new DateOnly(2026, 1, 1),
                TotalUnits = 16000,
                DurationMonths = 48,
                CliffMonths = 0,
                PostCliffFrequency = VestingFrequency.Quarterly
            };

            var events = CreateGenerator().Generate(definition);

            Assert.Equal(16, events.Count);
            Assert.Equal(16000, events.Sum(e => e.Units));
        }

        [Fact]
        public void Generate_ThrowsForInvalidCliff()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.StandardCliffThenPeriodic,
                GrantDate = new DateOnly(2026, 1, 1),
                TotalUnits = 10000,
                DurationMonths = 12,
                CliffMonths = 24,
                PostCliffFrequency = VestingFrequency.Monthly
            };

            Assert.Throws<InvalidOperationException>(() => CreateGenerator().Generate(definition));
        }

        [Fact]
        public void Generate_RoundsFinalEventToMatchTotalUnits()
        {
            VestingDefinition definition = new VestingDefinition
            {
                Kind = VestingScheduleKind.PeriodicNoCliff,
                GrantDate = new DateOnly(2026, 1, 1),
                TotalUnits = 10000,
                DurationMonths = 36,
                CliffMonths = 0,
                PostCliffFrequency = VestingFrequency.Monthly
            };

            var events = CreateGenerator().Generate(definition);

            Assert.Equal(10000, events.Sum(e => e.Units));
            Assert.NotEqual(events[0].Units, events[^1].Units);
        }
    }
}
