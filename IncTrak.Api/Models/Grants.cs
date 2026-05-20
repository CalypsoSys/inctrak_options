using System;
using System.Collections.Generic;

namespace IncTrak.Models
{
    public partial class Grants
    {
        public Guid GrantPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid ParticipantFk { get; set; }
        public Guid PlanFk { get; set; }
        public Guid VestingScheduleFk { get; set; }
        public Guid TerminationFk { get; set; }
        public decimal Shares { get; set; }
        public decimal OptionPrice { get; set; }
        public DateTime VestingStart { get; set; }
        public DateTime DateOfGrant { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Groups GroupFkNavigation { get; set; }
        public virtual Participants ParticipantFkNavigation { get; set; }
        public virtual Plans PlanFkNavigation { get; set; }
        public virtual Terminations TerminationFkNavigation { get; set; }
        public virtual Schedules VestingScheduleFkNavigation { get; set; }
    }
}
