using IncTrak.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.data
{
    public class PARTICIPANT_UI : BASE_DTO
    {
        public Guid PARTICIPANT_PK { get; set; }
        public int PARTICIPANT_TYPE_FK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid? USER_FK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public decimal TOTAL_GRANTS { get; set; }
        public decimal GRANTED_SHARES { get; set; }
        public bool HAS_USER { get; set; }
        public string USER_NAME { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string USER_ACTION { get; set; }
        public bool SEND_EMAIL { get; set; }

        public Participants SetFromParticipant
        {
            get { return null; }
            set 
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    PARTICIPANT_PK = value.ParticipantPk;
                    PARTICIPANT_TYPE_FK = value.ParticipantTypeFk;
                    GROUP_FK = value.GroupFk;
                    USER_FK = value.UserFk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                }
            }
        }

        public Participants GetParticipant(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Participants()
                {
                    ParticipantPk = this.PARTICIPANT_PK,
                    ParticipantTypeFk = this.PARTICIPANT_TYPE_FK,
                    GroupFk = groupFk,
                    UserFk = this.USER_FK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED
                };
            }

            return new Participants();
        }
        public void SetToParticipant(Participants participant, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                participant.Name = NAME;
                participant.ParticipantTypeFk = PARTICIPANT_TYPE_FK;
                participant.UserFk = USER_FK;
                participant.GroupFk = groupKey;
            }
        }

        public PARTICIPANT_UI()
            : base(Guid.Empty)
        {
            SEND_EMAIL = true;
        }

        public PARTICIPANT_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
            SEND_EMAIL = true;
        }

        public PARTICIPANT_UI(Guid groupKeyCheck, Participants db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromParticipant = db;
                if (USER_FK.HasValue)
                {
                    USER_ACTION = "update_user";
                }
                else
                {
                    USER_ACTION = "create_user";
                }
            }
        }

        public PARTICIPANT_UI(Guid groupKeyCheck, Participants db, decimal totalGrants, decimal grantedShares, bool hasUser) : this(groupKeyCheck, db)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromParticipant = db;
                TOTAL_GRANTS = totalGrants;
                GRANTED_SHARES = grantedShares;
                HAS_USER = hasUser;
            }
        }
    }
}
