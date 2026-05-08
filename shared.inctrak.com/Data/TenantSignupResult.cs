using System;

namespace IncTrak.Data
{
    public class TenantSignupResult
    {
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; }
        public string TenantDatabaseName { get; set; }
        public bool Created { get; set; }
    }
}
