using IncTrak.GoalSetter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.GoalSetter.data
{
    public class MEMBER_UI : BASE_DTO
    {
        public Guid MEMBER_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public Guid? USER_FK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public bool HAS_USER { get; set; }
        public string USER_NAME { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string USER_ACTION { get; set; }
        public bool GOOGLE_USER { get; set; }
        public bool SEND_EMAIL { get; set; }
        public int CURRENT_GOALS { get; set; }
        public int TOTAL_GOALS { get; set; }

        public Members SetFromMember
        {
            get { return null; }
            set 
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    MEMBER_PK = value.MemberPk;
                    GROUP_FK = value.GroupFk;
                    USER_FK = value.UserFk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                }
            }
        }

        public Members GetMember(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Members()
                {
                    MemberPk = this.MEMBER_PK,
                    GroupFk = groupFk,
                    UserFk = this.USER_FK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED
                };
            }

            return new Members();
        }
        public void SetToMember(Members member, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                member.Name = NAME;
                member.UserFk = USER_FK;
                member.GroupFk = groupKey;
            }
        }

        public MEMBER_UI()
            : base(Guid.Empty)
        {
            GOOGLE_USER = false;
            SEND_EMAIL = true;
        }

        public MEMBER_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
            GOOGLE_USER = false;
            SEND_EMAIL = true;
        }

        public MEMBER_UI(Guid groupKeyCheck, Members db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromMember = db;
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

        public MEMBER_UI(Guid groupKeyCheck, Members db, bool hasUser) : this(groupKeyCheck, db)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromMember = db;
                HAS_USER = hasUser;
            }
        }
    }
}