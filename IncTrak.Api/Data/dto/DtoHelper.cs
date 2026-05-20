using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.data
{
    public static class DtoHelper
    {
        public static Guid? NullGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;
            else
                return guid;
        }
        public static int? NullInt(int key)
        {
            if (key == 0)
                return null;
            else
                return key;
        }
    }
}