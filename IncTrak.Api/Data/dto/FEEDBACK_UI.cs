using IncTrak.FeedbackModels;
using System;

namespace IncTrak.data
{
    public class FEEDBACK_UI : BASE_DTO
    {
        public DateTime CREATED { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string NAME { get; set; }
        public string SUBJECT { get; set; }
        public string MESSAGE { get; set; }
        public int MESSAGE_TYPE_FK { get; set; }
        public int FEEDBACK_PK { get; set; }

        public FEEDBACK_UI()
            : base(Guid.Empty)
        {
        }

        public FEEDBACK_UI(Feedback db) : base(Guid.Empty)
        {
            CREATED =  db.Created;
            EMAIL_ADDRESS =  db.EmailAddress;
            NAME =  db.Name;
            SUBJECT =  db.Subject;
            MESSAGE =  db.Message;
            MESSAGE_TYPE_FK =  db.MessageTypeFk;
            FEEDBACK_PK =  db.FeedbackPk;
        }
    }
}
