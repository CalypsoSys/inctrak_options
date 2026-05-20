using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class TermFroms
    {
        public TermFroms()
        {
            Terminations = new HashSet<Terminations>();
        }

        public int TermFromPk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Terminations> Terminations { get; set; }
    }
}
