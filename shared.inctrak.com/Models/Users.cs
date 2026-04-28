using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Users
    {
        public Users()
        {
            Participants = new HashSet<Participants>();
        }

        public Guid UserPk { get; set; }
        public Guid GroupFk { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public bool Administrator { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Participants> Participants { get; set; }
    }
}
