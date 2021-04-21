using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class RatingType
    {
        public RatingType()
        {
            Goals = new HashSet<Goals>();
        }

        public int RatingTypePk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Goals> Goals { get; set; }
    }
}
