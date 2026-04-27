using System;

namespace IncTrak.Data
{
    public class ControlPlaneTenantRecord
    {
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; }
        public string TenantDatabaseName { get; set; }
    }
}
