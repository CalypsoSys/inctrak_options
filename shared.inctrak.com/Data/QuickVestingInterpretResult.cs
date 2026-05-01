using IncTrak.data;

namespace IncTrak.Data
{
    public class QuickVestingInterpretResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Summary { get; set; }
        public string Provider { get; set; }
        public decimal? SharesGranted { get; set; }
        public string VestingStart { get; set; }
        public PERIOD_UI[] Periods { get; set; }
    }
}
