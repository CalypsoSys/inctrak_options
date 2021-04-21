using IncTrak.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.data
{
    public class PERIOD_UI : BASE_DTO
    {
        public Guid PERIOD_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid SCHEDULE_FK { get; set; }
        public int? PERIOD_TYPE_FK { get; set; }
        public int? AMOUNT_TYPE_FK { get; set; }
        public decimal AMOUNT { get; set; }
        public int ORDER { get; set; }
        public int EVEN_OVER_N { get; set; }
        public int INCREMENTS { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public int PERIOD_AMOUNT { get; set; }

        public Periods SetFromPeriod
        {
            get { return null;}
            set 
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    PERIOD_PK = value.PeriodPk;
                    GROUP_FK = value.GroupFk;
                    SCHEDULE_FK = value.ScheduleFk;
                    PERIOD_TYPE_FK = DtoHelper.NullInt(value.PeriodTypeFk);
                    AMOUNT_TYPE_FK = DtoHelper.NullInt(value.AmountTypeFk);
                    AMOUNT = value.Amount;
                    ORDER = value.Order;
                    EVEN_OVER_N = value.EvenOverN;
                    INCREMENTS = value.Increments;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    PERIOD_AMOUNT = value.PeriodAmount;
                }
            }
        }

        public Periods GetPeriod(Guid groupKey, Guid scheduleKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                return new Periods()
                {
                    PeriodPk = this.PERIOD_PK,
                    GroupFk = groupKey,
                    ScheduleFk = scheduleKey,
                    PeriodTypeFk = this.PERIOD_TYPE_FK ?? 0,
                    AmountTypeFk = this.AMOUNT_TYPE_FK ?? 0,
                    Amount = this.AMOUNT,
                    Order = this.ORDER,
                    EvenOverN = this.EVEN_OVER_N,
                    Increments = this.INCREMENTS,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    PeriodAmount = this.PERIOD_AMOUNT
                };
            }

            return new Periods();
        }

        public void SetToPeriod(Periods period, Guid groupKey, Guid scheduleKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                period.GroupFk = groupKey;
                period.ScheduleFk = scheduleKey;
                period.PeriodTypeFk = PERIOD_TYPE_FK ?? 0;
                period.AmountTypeFk = AMOUNT_TYPE_FK ?? 0;
                period.Amount = AMOUNT;
                period.Order = ORDER;
                period.EvenOverN = EVEN_OVER_N;
                period.Increments = INCREMENTS;
                period.PeriodAmount = PERIOD_AMOUNT;
            }
        }

        public PERIOD_UI()
            : base(Guid.Empty)
        {
        }

        public PERIOD_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public PERIOD_UI(Guid groupKeyCheck, Periods db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromPeriod = db;
            }
        }
    }
}
