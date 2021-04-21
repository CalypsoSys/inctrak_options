using System;
using System.Collections.Generic;

namespace ModelDriver.Models
{
    public partial class ActivateUuids
    {
        public int UuidPk { get; set; }
        public Guid UserFk { get; set; }
        public string Uuid { get; set; }
        public int Type { get; set; }
        public DateTime ValidUntil { get; set; }

        public virtual Users UserFkNavigation { get; set; }
    }
}
