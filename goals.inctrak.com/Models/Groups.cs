using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class Groups
    {
        public Groups()
        {
            Departments = new HashSet<Departments>();
            Goals = new HashSet<Goals>();
            Goalset = new HashSet<Goalset>();
            Members = new HashSet<Members>();
            Schedules = new HashSet<Schedules>();
            Teams = new HashSet<Teams>();
            Users = new HashSet<Users>();
        }

        public Guid GroupPk { get; set; }
        public string Description { get; set; }
        public string GroupKey { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual ICollection<Departments> Departments { get; set; }
        public virtual ICollection<Goals> Goals { get; set; }
        public virtual ICollection<Goalset> Goalset { get; set; }
        public virtual ICollection<Members> Members { get; set; }
        public virtual ICollection<Schedules> Schedules { get; set; }
        public virtual ICollection<Teams> Teams { get; set; }
        public virtual ICollection<Users> Users { get; set; }
    }
}
