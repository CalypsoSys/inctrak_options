using System;
using System.Collections.Generic;

namespace IncTrak.FeedbackModels
{
    public partial class MessageType
    {
        public MessageType()
        {
            Feedback = new HashSet<Feedback>();
        }

        public int MessageTypePk { get; set; }
        public string MessageType1 { get; set; }

        public virtual ICollection<Feedback> Feedback { get; set; }
    }
}
