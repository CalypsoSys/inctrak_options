using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class StockHolders
    {
        internal Guid STOCK_HOLDER_PK;

        public Guid StockHolderPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid ParticipantFk { get; set; }
        public Guid StockClassFk { get; set; }
        public decimal Shares { get; set; }
        public decimal Price { get; set; }
        public DateTime DateOfSale { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual Participants ParticipantFkNavigation { get; set; }
        public virtual StockClasses StockClassFkNavigation { get; set; }
    }
}
