using IncTrak.GoalSetter.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.GoalSetter.data
{
    public class TEAM_UI : BASE_DTO
    {
        public Guid TEAM_PK { get; set; }
        public Guid DEPARTMENT_FK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public Guid GROUP_FK { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public int CURRENT_GOALS { get; set; }
        public int TOTAL_GOALS { get; set; }

        public Teams SetFromTeam
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    TEAM_PK = value.TeamPk;
                    DEPARTMENT_FK = value.DepartmentFk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                }
            }
        }

        public Teams GetTeam(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Teams()
                {
                    TeamPk = this.TEAM_PK,
                    DepartmentFk = this.DEPARTMENT_FK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk
                };
            }

            return new Teams();
        }

        public void SetToTeam(Teams team, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                team.Name = NAME;
                team.DepartmentFk = DEPARTMENT_FK;
                team.GroupFk = groupKey;
            }
        }

        public TEAM_UI()
            : base(Guid.Empty)
        {
        }

        public TEAM_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public TEAM_UI(Guid groupKeyCheck, Teams db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromTeam = db;
            }
        }
    }
}
