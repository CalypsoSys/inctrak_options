using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class Schedules
    {
        public Schedules()
        {
            Goalset = new HashSet<Goalset>();
        }

        public Guid SchedulePk { get; set; }
        public Guid GroupFk { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Goalset> Goalset { get; set; }
    }
}
