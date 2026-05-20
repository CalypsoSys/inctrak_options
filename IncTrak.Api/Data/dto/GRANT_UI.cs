using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class GRANT_UI : BASE_DTO
    {
        public Guid GRANT_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid? PARTICIPANT_FK { get; set; }
        public Guid? VESTING_SCHEDULE_FK { get; set; }
        public decimal SHARES { get; set; }
        public decimal OPTION_PRICE { get; set; }
        public DateTime VESTING_START { get; set; }
        public Guid? TERMINATION_FK { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public DateTime DATE_OF_GRANT { get; set; }
        public Guid? PLAN_FK { get; set; }

        public string PARTICIPANT_NAME { get; set; }
        public string PLAN_NAME { get; set; }


        public Grants SetFromGrant
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    GRANT_PK = value.GrantPk;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                    PARTICIPANT_FK = DtoHelper.NullGuid(value.ParticipantFk);
                    VESTING_SCHEDULE_FK = DtoHelper.NullGuid(value.VestingScheduleFk);
                    SHARES = value.Shares;
                    OPTION_PRICE = value.OptionPrice;
                    VESTING_START = value.VestingStart;
                    TERMINATION_FK = DtoHelper.NullGuid(value.TerminationFk);
                    DATE_OF_GRANT = value.DateOfGrant;
                    PLAN_FK = DtoHelper.NullGuid(value.PlanFk);
                }
            }
        }

        public Grants GetGrant(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Grants()
                {
                    GrantPk = this.GRANT_PK,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                    ParticipantFk = this.PARTICIPANT_FK ?? Guid.Empty,
                    VestingScheduleFk = this.VESTING_SCHEDULE_FK ?? Guid.Empty,
                    Shares = this.SHARES,
                    OptionPrice = this.OPTION_PRICE,
                    VestingStart = this.VESTING_START,
                    TerminationFk = this.TERMINATION_FK ?? Guid.Empty,
                    DateOfGrant = this.DATE_OF_GRANT,
                    PlanFk = this.PLAN_FK ?? Guid.Empty,

                };
            }

            return new Grants();
        }

        public void SetToGrant(Grants grant, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                grant.GroupFk = groupKey;
                grant.ParticipantFk = PARTICIPANT_FK ?? Guid.Empty;
                grant.VestingScheduleFk = VESTING_SCHEDULE_FK ?? Guid.Empty;
                grant.Shares = SHARES;
                grant.OptionPrice = OPTION_PRICE;
                grant.VestingStart = VESTING_START;
                grant.TerminationFk = TERMINATION_FK ?? Guid.Empty;
                grant.DateOfGrant = DATE_OF_GRANT;
                grant.PlanFk = PLAN_FK ?? Guid.Empty;
            }
        }

        public GRANT_UI()
            : base(Guid.Empty)
        {
        }

        public GRANT_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public GRANT_UI(Guid groupKeyCheck, Grants db)
            : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
                SetFromGrant = db;
        }
    }
}
