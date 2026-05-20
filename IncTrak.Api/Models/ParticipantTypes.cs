using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class ParticipantTypes
    {
        public ParticipantTypes()
        {
            Participants = new HashSet<Participants>();
        }

        public int ParticipantTypePk { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Participants> Participants { get; set; }
    }
}
