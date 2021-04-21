using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class LoggedActions
    {
        public long LoggedActionsPk { get; set; }
        public Guid GroupFk { get; set; }
        public Guid? UserFk { get; set; }
        public string UserName { get; set; }
        public string TableName { get; set; }
        public DateTime Created { get; set; }
        public string Action { get; set; }
        public Dictionary<string, string> OriginalData { get; set; }
        public Dictionary<string, string> NewData { get; set; }
    }
}
