using IncTrak.FeedbackModels;
using System;

namespace IncTrak.data
{
    public class MESSAGE_TYPE_UI : BASE_DTO
    {
        public int MESSAGE_TYPE_PK { get; set; }
        public string MESSAGE_TYPE1 { get; set; }

        public MESSAGE_TYPE_UI()
            : base(Guid.Empty)
        {
        }

        public MESSAGE_TYPE_UI(MessageType db) : base(Guid.Empty)
        {
            MESSAGE_TYPE_PK = db.MessageTypePk;
            MESSAGE_TYPE1 = db.MessageType1;
        }
    }
}
