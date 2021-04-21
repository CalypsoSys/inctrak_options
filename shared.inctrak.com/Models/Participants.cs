using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Participants
    {
        public Participants()
        {
            Grants = new HashSet<Grants>();
            StockHolders = new HashSet<StockHolders>();
        }

        public Guid ParticipantPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid? UserFk { get; set; }
        public int ParticipantTypeFk { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ParticipantTypes ParticipantTypeFkNavigation { get; set; }
        public virtual Users UserFkNavigation { get; set; }
        public virtual ICollection<Grants> Grants { get; set; }
        public virtual ICollection<StockHolders> StockHolders { get; set; }
    }
}
