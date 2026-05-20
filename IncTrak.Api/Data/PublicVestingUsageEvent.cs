namespace IncTrak.Data
{
    public class PublicVestingUsageEvent
    {
        public string EventType { get; set; }
        public string Path { get; set; }
        public string Prompt { get; set; }
        public string Provider { get; set; }
        public string AlternateProvider { get; set; }
        public decimal? Confidence { get; set; }
        public bool RequiresAi { get; set; }
        public bool UsedAi { get; set; }
        public bool StrictAi { get; set; }
        public string PreferredProvider { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Kind { get; set; }
        public decimal? SharesGranted { get; set; }
        public string VestingStart { get; set; }
        public int PeriodCount { get; set; }
        public string SourceIp { get; set; }
        public string UserAgent { get; set; }
    }
}
