using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class Goals
    {
        public Guid GoalsPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid GoalsetFk { get; set; }
        public int ImportanceTypeFk { get; set; }
        public string Description { get; set; }
        public int? RatingTypeFk { get; set; }
        public string ManagerComments { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string MemberComments { get; set; }

        public virtual Goalset GoalsetFkNavigation { get; set; }
        public virtual Groups GroupFkNavigation { get; set; }
        public virtual ImportanceType ImportanceTypeFkNavigation { get; set; }
        public virtual RatingType RatingTypeFkNavigation { get; set; }
    }
}
