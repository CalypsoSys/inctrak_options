using System;
using System.Collections.Generic;

namespace IncTrak.GoalSetter.FeedbackModels
{
    public partial class Feedback
    {
        public int FeedbackPk { get; set; }
        public int MessageTypeFk { get; set; }
        public DateTime Created { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string ClientData { get; set; }

        public virtual MessageType MessageTypeFkNavigation { get; set; }
    }
}
