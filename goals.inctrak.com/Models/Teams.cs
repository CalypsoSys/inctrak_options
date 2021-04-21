using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class Teams
    {
        public Teams()
        {
            Goalset = new HashSet<Goalset>();
        }

        public Guid TeamPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid DepartmentFk { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Departments DepartmentFkNavigation { get; set; }
        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Goalset> Goalset { get; set; }
    }
}
