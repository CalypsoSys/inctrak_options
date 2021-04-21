using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class Goalset
    {
        public Goalset()
        {
            Goals = new HashSet<Goals>();
        }

        public Guid GoalsetPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid MemberFk { get; set; }
        public Guid TeamFk { get; set; }
        public Guid ScheduleFk { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual Members MemberFkNavigation { get; set; }
        public virtual Schedules ScheduleFkNavigation { get; set; }
        public virtual Teams TeamFkNavigation { get; set; }
        public virtual ICollection<Goals> Goals { get; set; }
    }
}
