using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.Domain
{
    public class VestingPeriod
    {
        public decimal Order { get; set; }
        public DateTime VestDate { get; set; }
        public bool IsVested { get; set; }
        public decimal Percent { get; set; }
        public decimal Shares { get; set; }
        public decimal TotalPercent { get; set; }
        public decimal TotalShares { get; set; }
        public PeriodTypes Period { get; set; }
        public int PeriodAmount { get; set; }
    }
}