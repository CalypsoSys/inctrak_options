using IncTrak.data;

namespace IncTrak.Data
{
    public class QuickVestingInterpretResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Summary { get; set; }
        public string Provider { get; set; }
        public string AlternateProvider { get; set; }
        public decimal Confidence { get; set; }
        public bool RequiresAi { get; set; }
        public decimal? SharesGranted { get; set; }
        public string VestingStart { get; set; }
        public PERIOD_UI[] Periods { get; set; }
        public string Kind { get; set; }
        public bool UsedAi { get; set; }
        public bool JsonRepairAttempted { get; set; }
        public string[] Warnings { get; set; } = System.Array.Empty<string>();
        public string[] MissingFields { get; set; } = System.Array.Empty<string>();
        public string[] Assumptions { get; set; } = System.Array.Empty<string>();
    }
}
