using IncTrak.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.data
{
    public class SCHEDULE_UI : BASE_DTO
    {
        public Guid SCHEDULE_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public string NAME { get; set; }
        public decimal PERIODS { get; set; }

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
                    Name = this.NAME
                };
            }

            return new Schedules();
        }

        public void SetToSchedule(Schedules schedule, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                schedule.Name = NAME;
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

        public SCHEDULE_UI(Guid groupKeyCheck, Schedules db, decimal periods) : this(groupKeyCheck, db)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                PERIODS = periods;
            }
        }
    }
}
