using System;

namespace IncTrak.Data
{
    public class UserContext
    {
        public Guid UserId { get; set; }
        public string ExternalIdentity { get; set; }
        public MembershipRole Role { get; set; }

        public bool IsResolved()
        {
            return UserId != Guid.Empty;
        }
    }
}
