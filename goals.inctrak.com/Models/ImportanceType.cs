using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.Models
{
    public partial class ImportanceType
    {
        public ImportanceType()
        {
            Goals = new HashSet<Goals>();
        }

        public int ImportanceTypePk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Goals> Goals { get; set; }
    }
}
