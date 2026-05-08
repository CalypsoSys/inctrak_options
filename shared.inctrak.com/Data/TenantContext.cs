using System;

namespace IncTrak.Data
{
    public class TenantContext
    {
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; }
        public string TenantDatabaseName { get; set; }

        public bool IsResolved()
        {
            return TenantId != Guid.Empty && string.IsNullOrWhiteSpace(TenantSlug) == false;
        }
    }
}
