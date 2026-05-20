using System;
using System.Collections.Generic;

namespace IncTrak.Data
{
    public enum VestingScheduleKind
    {
        Unknown,
        Immediate,
        PeriodicNoCliff,
        StandardCliffThenPeriodic,
        ExplicitTranches,
        MilestoneBased,
        PerformanceBased
    }

    public enum VestingFrequency
    {
        Unknown,
        OneTime,
        Monthly,
        Quarterly,
        SemiAnnual,
        Annual,
        Custom
    }

    public sealed class VestingDefinition
    {
        public VestingScheduleKind Kind { get; set; }
        public DateOnly? GrantDate { get; set; }
        public int? TotalUnits { get; set; }
        public int? DurationMonths { get; set; }
        public int? CliffMonths { get; set; }
        public decimal? CliffPercent { get; set; }
        public VestingFrequency PostCliffFrequency { get; set; }
        public List<VestingSegmentDefinition> Segments { get; set; } = new List<VestingSegmentDefinition>();
        public List<VestingTrancheDefinition> ExplicitTranches { get; set; } = new List<VestingTrancheDefinition>();
        public List<string> Assumptions { get; set; } = new List<string>();
        public List<string> MissingFields { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public sealed class VestingSegmentDefinition
    {
        public int PeriodAmount { get; set; } = 1;
        public VestingFrequency Frequency { get; set; }
        public int Increments { get; set; }
        public decimal? AmountPercent { get; set; }
        public int? AmountUnits { get; set; }
        public string Description { get; set; }
    }

    public sealed class VestingTrancheDefinition
    {
        public DateOnly? VestDate { get; set; }
        public decimal? Percent { get; set; }
        public int? Units { get; set; }
        public string Description { get; set; }
    }

    public sealed class VestingEvent
    {
        public DateOnly Date { get; set; }
        public int Units { get; set; }
        public decimal Percent { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class VestingParseResult
    {
        public string OriginalText { get; set; } = string.Empty;
        public VestingDefinition Definition { get; set; } = new VestingDefinition();
        public List<string> RuleHints { get; set; } = new List<string>();
        public bool UsedAi { get; set; }
        public bool JsonRepairAttempted { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}
