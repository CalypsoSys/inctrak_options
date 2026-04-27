using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class USER_UI : BASE_DTO
    {
        public Guid USER_PK { get; set; }
        public string USER_NAME { get; set; }
        public Guid GROUP_FK { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public bool ACTIVATED { get; set; }
        public string PASSWORD { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public bool ADMINISTRATOR { get; set; }
        public bool ACCEPT_TERMS { get; set; }
        public string PASSWORD2 { get; set; }
        public string GROUP_NAME { get; set; }
        public bool IS_REGISTERING { get; set; }

        public USER_UI() : base(Guid.Empty)
        {
        }

        public USER_UI(Users db) : this()
        {
            USER_PK = db.UserPk;
            USER_NAME = db.UserName;
            GROUP_FK = db.GroupFk;
            EMAIL_ADDRESS = db.EmailAddress;
            ACTIVATED = db.Activated;
            PASSWORD = db.Password;
            CREATED = db.Created;
            UPDATED = db.Updated;
            ADMINISTRATOR = db.Administrator;
            ACCEPT_TERMS = db.AcceptTerms;
        }
    }
}
