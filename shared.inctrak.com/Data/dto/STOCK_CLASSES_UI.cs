using IncTrak.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.data
{
    public class STOCK_CLASSES_UI : BASE_DTO
    {
        public Guid STOCK_CLASS_PK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public Guid GROUP_FK { get; set; }
        public decimal TOTAL_SHARES { get; set; }

        public StockClasses SetFromStockClass
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    STOCK_CLASS_PK = value.StockClassPk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    TOTAL_SHARES = value.TotalShares;
                }
            }
        }

        public StockClasses GetStockClass(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new StockClasses()
                {
                    StockClassPk = this.STOCK_CLASS_PK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    TotalShares = this.TOTAL_SHARES
                };
            }

            return new StockClasses();
        }

        public void SetToStockClass(StockClasses stockClass, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                stockClass.Name = NAME;
                stockClass.TotalShares = TOTAL_SHARES;
                stockClass.GroupFk = groupKey;
            }
        }

        public STOCK_CLASSES_UI()
            : base(Guid.Empty)
        {
        }

        public STOCK_CLASSES_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public STOCK_CLASSES_UI(Guid groupKeyCheck, StockClasses db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromStockClass = db;
            }
        }
    }
}
