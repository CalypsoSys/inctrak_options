using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class PeriodTypes
    {
        public PeriodTypes()
        {
            Periods = new HashSet<Periods>();
        }

        public int PeriodTypePk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Periods> Periods { get; set; }
    }
}
