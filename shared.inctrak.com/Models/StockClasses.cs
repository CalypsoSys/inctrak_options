using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class StockClasses
    {
        public StockClasses()
        {
            Plans = new HashSet<Plans>();
            StockHolders = new HashSet<StockHolders>();
        }

        public Guid StockClassPk { get; set; }
        public Guid GroupFk { get; set; }
        public string Name { get; set; }
        public decimal TotalShares { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ICollection<Plans> Plans { get; set; }
        public virtual ICollection<StockHolders> StockHolders { get; set; }
    }
}
