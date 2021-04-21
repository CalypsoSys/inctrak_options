using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class STOCK_HOLDER_UI : BASE_DTO
    {
        public Guid STOCK_HOLDER_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid? PARTICIPANT_FK { get; set; }
        public decimal SHARES { get; set; }
        public decimal PRICE { get; set; }
        public DateTime DATE_OF_SALE { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public Guid? STOCK_CLASS_FK { get; set; }

        public string PARTICIPANT_NAME { get; set; }
        public string STOCK_CLASS_NAME { get; set; }


        public StockHolders SetFromStockHolder
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    STOCK_HOLDER_PK = value.StockHolderPk;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    PARTICIPANT_FK = DtoHelper.NullGuid(value.ParticipantFk);
                    SHARES = value.Shares;
                    PRICE = value.Price;
                    DATE_OF_SALE = value.DateOfSale;
                    STOCK_CLASS_FK = DtoHelper.NullGuid(value.StockClassFk);
                }
            }
        }

        public StockHolders GetStockHolder(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new StockHolders()
                {
                    StockHolderPk = this.STOCK_HOLDER_PK,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    ParticipantFk = this.PARTICIPANT_FK ?? Guid.Empty,
                    Shares = this.SHARES,
                    Price = this.PRICE,
                    DateOfSale = this.DATE_OF_SALE,
                    StockClassFk = this.STOCK_CLASS_FK ?? Guid.Empty,

                };
            }

            return new StockHolders();
        }

        public void SetToStockHolder(StockHolders stockHolder, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                stockHolder.GroupFk = groupKey;
                stockHolder.ParticipantFk = PARTICIPANT_FK ?? Guid.Empty;
                stockHolder.Shares = SHARES;
                stockHolder.Price = PRICE;
                stockHolder.DateOfSale = DATE_OF_SALE;
                stockHolder.StockClassFk = STOCK_CLASS_FK ?? Guid.Empty;
            }
        }

        public STOCK_HOLDER_UI()
            : base(Guid.Empty)
        {
        }

        public STOCK_HOLDER_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public STOCK_HOLDER_UI(Guid groupKeyCheck, StockHolders db)
            : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
                SetFromStockHolder = db;
        }
    }
}
