using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.Domain
{
    public enum TermFrom
    {
        GrantDate = 1, 
        VestStart = 2,
        SpecificDate = 3,
        AbsoluteDate = -1
    }
}