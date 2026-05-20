namespace IncTrak.Data
{
    public static class ControlPlaneHeaders
    {
        public const string TenantId = "X-IncTrak-Tenant-Id";
        public const string TenantSlug = "X-IncTrak-Tenant-Slug";
        public const string TenantDatabaseName = "X-IncTrak-Tenant-Db";
        public const string UserId = "X-IncTrak-User-Id";
        public const string UserRole = "X-IncTrak-User-Role";
        public const string UserExternalIdentity = "X-IncTrak-User-External-Id";
    }
}
