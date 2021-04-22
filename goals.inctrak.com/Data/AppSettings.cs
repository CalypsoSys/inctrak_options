using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Data
{
    public class AppSettings
    {
        public string IncTrakDns { get; set; }
        public string GoogleSecretKey { get; set; }
        public string GoogleClientId { get; set; }

        public string ErrorsHost { get; set; }
        public string ErrorsUsername { get; set; }
        public string ErrorsPassword { get; set; }

        public string IncTrakGoalsConnection { get; set; }
        public string FeedbackConnection { get; set; }

        public bool UseSNMP { get; set; }
        public string SNMPServer { get; set; }
        public int SNMPPort { get; set; }
        public string SNMPAddress { get; set; }
        public string SNMPPassword { get; set; }

        public string EmailApiKey { get; set; }
        public string EmailFrom { get; set; }
    }
}
