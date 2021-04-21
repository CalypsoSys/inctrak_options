using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class Departments
    {
        public Departments()
        {
            Teams = new HashSet<Teams>();
        }

        public Guid DepartmentPk { get; set; }
        public Guid GroupFk { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Teams> Teams { get; set; }
    }
}
