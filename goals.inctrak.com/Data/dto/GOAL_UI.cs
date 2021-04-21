using IncTrak.GoalSetter.Models;
using System;

namespace IncTrak.GoalSetter.data
{
    public class GOAL_UI : BASE_DTO
    {
        public Guid GOAL_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public int IMPORTANCE_TYPE_FK { get; set; }
        public string DESCRIPTION { get; set; }
        public int? RATING_TYPE_FK { get; set; }
        public string MANAGER_COMMENTS { get; set; }
        public string MEMBER_COMMENTS { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }

        public Goals SetFromGoal
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    GOAL_PK = value.GoalsPk;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    IMPORTANCE_TYPE_FK = value.ImportanceTypeFk;
                    DESCRIPTION = value.Description;
                    RATING_TYPE_FK = value.RatingTypeFk;
                    MANAGER_COMMENTS = value.ManagerComments;
                    MEMBER_COMMENTS = value.MemberComments;
                }
            }
        }

        public Goals GetGoal(Guid groupFk, Guid goalSetKey)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Models.Goals()
                {
                    GoalsPk = this.GOAL_PK,
                    GoalsetFk = goalSetKey,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    ImportanceTypeFk = this.IMPORTANCE_TYPE_FK,
                    Description =  this.DESCRIPTION,
                    RatingTypeFk = this.RATING_TYPE_FK,
                    ManagerComments =  this.MANAGER_COMMENTS,
                    MemberComments = this.MEMBER_COMMENTS,
                };
            }

            return new Models.Goals();
        }

        public void SetToGoal(Models.Goals goal, Guid groupKey, Guid goalSetKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                goal.GoalsetFk = goalSetKey;
                goal.GroupFk = groupKey;
                goal.ImportanceTypeFk = IMPORTANCE_TYPE_FK;
                goal.Description = DESCRIPTION;
                goal.RatingTypeFk = RATING_TYPE_FK;
                goal.ManagerComments = MANAGER_COMMENTS;
                //goal.MemberComments = MEMBER_COMMENTS; WE DO NOT SET MEMBER COMMENTS IN ADMIN
            }
        }

        public GOAL_UI()
            : base(Guid.Empty)
        {
        }

        public GOAL_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public GOAL_UI(Guid groupKeyCheck, Models.Goals db)
            : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
                SetFromGoal = db;
        }
    }
}
