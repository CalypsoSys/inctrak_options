using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Data
{
    public class ResetPasswordLink
    {
        public string ResetPasswordKey { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public bool AcceptTerms { get; set; }
    }
}
