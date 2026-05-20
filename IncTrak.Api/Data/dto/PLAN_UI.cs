using IncTrak.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.data
{
    public class PLAN_UI : BASE_DTO
    {
        public Guid PLAN_PK { get; set; }
        public Guid STOCK_CLASS_FK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public Guid GROUP_FK { get; set; }
        public decimal TOTAL_SHARES { get; set; }
        public decimal GRANTED_SHARES { get; set; }
        public string STOCK_CLASS_NAME { get; set; }

        public Plans SetFromPlan
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    PLAN_PK = value.PlanPk;
                    STOCK_CLASS_FK = value.StockClassFk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    TOTAL_SHARES = value.TotalShares;
                }
            }
        }

        public Plans GetPlan(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Plans()
                {
                    PlanPk = this.PLAN_PK,
                    StockClassFk = this.STOCK_CLASS_FK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    TotalShares = this.TOTAL_SHARES
                };
            }

            return new Plans();
        }

        public void SetToPlan(Plans plan, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                plan.Name = NAME;
                plan.StockClassFk = STOCK_CLASS_FK;
                plan.TotalShares = TOTAL_SHARES;
                plan.GroupFk = groupKey;
            }
        }

        public PLAN_UI()
            : base(Guid.Empty)
        {
        }

        public PLAN_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public PLAN_UI(Guid groupKeyCheck, Plans db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromPlan = db;
            }
        }

        public PLAN_UI(Guid groupKeyCheck, Plans db, decimal grantedShares) : this(groupKeyCheck, db)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                GRANTED_SHARES = grantedShares;
            }
        }
    }
}
