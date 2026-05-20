using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class AmountTypes
    {
        public AmountTypes()
        {
            Periods = new HashSet<Periods>();
        }

        public int AmountTypePk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Periods> Periods { get; set; }
    }
}
