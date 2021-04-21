using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class Users
    {
        public Users()
        {
            ActivateUuids = new HashSet<ActivateUuids>();
            Members = new HashSet<Members>();
        }

        public Guid UserPk { get; set; }
        public Guid GroupFk { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public bool Activated { get; set; }
        public string Password { get; set; }
        public bool Administrator { get; set; }
        public bool AcceptTerms { get; set; }
        public bool GoogleLogon { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<ActivateUuids> ActivateUuids { get; set; }
        public virtual ICollection<Members> Members { get; set; }
    }
}
