using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Terminations
    {
        public Terminations()
        {
            Grants = new HashSet<Grants>();
        }

        public Guid TerminationPk { get; set; }
        public Guid GroupFk { get; set; }
        public int TermFromFk { get; set; }
        public string Name { get; set; }
        public DateTime AbsoluteDate { get; set; }
        public int Years { get; set; }
        public int Months { get; set; }
        public int Days { get; set; }
        public DateTime SpecificDate { get; set; }
        public bool IsAbsolute { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual TermFroms TermFromFkNavigation { get; set; }
        public virtual ICollection<Grants> Grants { get; set; }
    }
}
