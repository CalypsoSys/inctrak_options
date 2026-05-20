using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Periods
    {
        public Guid PeriodPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid ScheduleFk { get; set; }
        public int PeriodTypeFk { get; set; }
        public int AmountTypeFk { get; set; }
        public decimal Amount { get; set; }
        public int Order { get; set; }
        public int EvenOverN { get; set; }
        public int Increments { get; set; }
        public int PeriodAmount { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual AmountTypes AmountTypeFkNavigation { get; set; }
        public virtual Groups GroupFkNavigation { get; set; }
        public virtual PeriodTypes PeriodTypeFkNavigation { get; set; }
        public virtual Schedules ScheduleFkNavigation { get; set; }
    }
}
