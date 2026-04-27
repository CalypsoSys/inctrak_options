using System;

namespace IncTrak.Data
{
    public enum MembershipRole
    {
        None = 0,
        TenantParticipant = 1,
        TenantAdmin = 2,
        PlatformOperator = 3
    }

    public static class MembershipRoleParser
    {
        public static MembershipRole Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return MembershipRole.None;
            }

            string normalized = value.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();
            return normalized switch
            {
                "tenant_participant" => MembershipRole.TenantParticipant,
                "tenant_admin" => MembershipRole.TenantAdmin,
                "platform_operator" => MembershipRole.PlatformOperator,
                _ => MembershipRole.None
            };
        }
    }
}
