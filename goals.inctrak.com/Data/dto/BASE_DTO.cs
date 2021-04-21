using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IncTrak.GoalSetter.data
{
    public abstract class BASE_DTO
    {
        public Guid GroupKeyCheck { get; set; }

        protected BASE_DTO()
        {
            GroupKeyCheck = Guid.Empty;
        }

        protected BASE_DTO(Guid groupKeyCheck)
        {
            GroupKeyCheck = groupKeyCheck;
        }
    }
}