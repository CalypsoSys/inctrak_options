using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Groups
    {
        public Groups()
        {
            Grants = new HashSet<Grants>();
            Participants = new HashSet<Participants>();
            Periods = new HashSet<Periods>();
            Plans = new HashSet<Plans>();
            Schedules = new HashSet<Schedules>();
            StockClasses = new HashSet<StockClasses>();
            StockHolders = new HashSet<StockHolders>();
            Terminations = new HashSet<Terminations>();
            Users = new HashSet<Users>();
        }

        public Guid GroupPk { get; set; }
        public string Description { get; set; }
        public string GroupKey { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual ICollection<Grants> Grants { get; set; }
        public virtual ICollection<Participants> Participants { get; set; }
        public virtual ICollection<Periods> Periods { get; set; }
        public virtual ICollection<Plans> Plans { get; set; }
        public virtual ICollection<Schedules> Schedules { get; set; }
        public virtual ICollection<StockClasses> StockClasses { get; set; }
        public virtual ICollection<StockHolders> StockHolders { get; set; }
        public virtual ICollection<Terminations> Terminations { get; set; }
        public virtual ICollection<Users> Users { get; set; }
    }
}
