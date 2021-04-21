using IncTrak.GoalSetter.Models;
using System;

namespace IncTrak.GoalSetter.data
{
    public class GOALSET_UI : BASE_DTO
    {
        public Guid GOALSET_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid? MEMBER_FK { get; set; }
        public Guid? TEAM_FK { get; set; }
        public Guid? SCHEDULE_FK { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }

        public string MEMBER_NAME { get; set; }
        public string TEAM_NAME { get; set; }
        public string SCHEDULE_NAME { get; set; }

        public Goalset SetFromGoalSet
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    GOALSET_PK = value.GoalsetPk;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    MEMBER_FK = DtoHelper.NullGuid(value.MemberFk);
                    SCHEDULE_FK = DtoHelper.NullGuid(value.ScheduleFk);
                    TEAM_FK = DtoHelper.NullGuid(value.TeamFk);
                }
            }
        }

        public Goalset GetGoalSet(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Models.Goalset()
                {
                    GoalsetPk = this.GOALSET_PK,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    MemberFk = this.MEMBER_FK ?? Guid.Empty,
                    ScheduleFk = this.SCHEDULE_FK ?? Guid.Empty,
                    TeamFk = this.TEAM_FK ?? Guid.Empty,
                };
            }

            return new Models.Goalset();
        }

        public void SetToGoalSet(Models.Goalset goalSet, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                goalSet.GroupFk = groupKey;
                goalSet.MemberFk = MEMBER_FK ?? Guid.Empty;
                goalSet.ScheduleFk = SCHEDULE_FK ?? Guid.Empty;
                goalSet.TeamFk = TEAM_FK ?? Guid.Empty;
            }
        }

        public GOALSET_UI()
            : base(Guid.Empty)
        {
        }

        public GOALSET_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public GOALSET_UI(Guid groupKeyCheck, Models.Goalset db)
            : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
                SetFromGoalSet = db;
        }
    }
}
