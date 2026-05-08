namespace IncTrak.Data
{
    public class SupabaseIdentity
    {
        public string ExternalIdentity { get; set; }
        public string EmailAddress { get; set; }

        public bool IsAuthenticated()
        {
            return string.IsNullOrWhiteSpace(ExternalIdentity) == false;
        }
    }
}
