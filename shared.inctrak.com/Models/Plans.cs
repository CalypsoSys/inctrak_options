using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Plans
    {
        public Plans()
        {
            Grants = new HashSet<Grants>();
        }

        public Guid PlanPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid StockClassFk { get; set; }
        public string Name { get; set; }
        public decimal TotalShares { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual StockClasses StockClassFkNavigation { get; set; }
        public virtual ICollection<Grants> Grants { get; set; }
    }
}
