using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Schedules
    {
        public Schedules()
        {
            Grants = new HashSet<Grants>();
            Periods = new HashSet<Periods>();
        }

        public Guid SchedulePk { get; set; }
        public Guid GroupFk { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Grants> Grants { get; set; }
        public virtual ICollection<Periods> Periods { get; set; }
    }
}
