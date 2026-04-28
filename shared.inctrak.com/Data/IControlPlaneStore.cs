namespace IncTrak.Data
{
    public interface IControlPlaneStore
    {
        ControlPlaneTenantRecord FindTenantByHostName(string hostName);
        ControlPlaneUserRecord FindUserByExternalIdentity(string externalIdentity);
        MembershipRole FindMembershipRole(string tenantId, string userId);
        bool IsTenantSlugAvailable(string tenantSlug);
    }
}
