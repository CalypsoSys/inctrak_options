using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class Members
    {
        public Members()
        {
            Goalset = new HashSet<Goalset>();
        }

        public Guid MemberPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid? UserFk { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual Users UserFkNavigation { get; set; }
        public virtual ICollection<Goalset> Goalset { get; set; }
    }
}
