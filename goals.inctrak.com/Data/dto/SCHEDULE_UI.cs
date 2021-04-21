using IncTrak.GoalSetter.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.GoalSetter.data
{
    public class SCHEDULE_UI : BASE_DTO
    {
        public Guid SCHEDULE_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public DateTime START_DATE { get; set; }
        public DateTime END_DATE { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public string NAME { get; set; }
        public int TOTAL_GOALS { get; set; }

        public Schedules SetFromSchedule
        {
            get { return null; }
            set 
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    SCHEDULE_PK = value.SchedulePk;
                    GROUP_FK = value.GroupFk;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    NAME = value.Name;
                    START_DATE = value.StartDate;
                    END_DATE = value.EndDate;
                }
            }
        }

        public Schedules GetSchedule(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Schedules()
                {
                    SchedulePk = this.SCHEDULE_PK,
                    GroupFk = groupFk,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    Name = this.NAME,
                    StartDate = this.START_DATE,
                    EndDate = this.END_DATE
            };
            }

            return new Schedules();
        }

        public void SetToSchedule(Schedules schedule, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                schedule.Name = NAME;
                schedule.StartDate = START_DATE;
                schedule.EndDate = END_DATE;
                schedule.GroupFk = groupKey;
            }
        }

        public SCHEDULE_UI()
            : base(Guid.Empty)
        {
        }

        public SCHEDULE_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public SCHEDULE_UI(Guid groupKeyCheck, Schedules db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromSchedule = db;
            }
        }
    }
}
