namespace IncTrak.Data
{
    public interface ITenantSignupProvisioner
    {
        TenantSignupResult ProvisionInitialTenant(SupabaseIdentity identity, TenantSignupRequest request);
    }
}
