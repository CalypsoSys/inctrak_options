namespace IncTrak.Data
{
    public class QuickVestingInterpretRequest
    {
        public string Prompt { get; set; }
        public bool StrictAi { get; set; }
        public bool AllowAiFallback { get; set; }
        public string PreferredProvider { get; set; }
    }
}
