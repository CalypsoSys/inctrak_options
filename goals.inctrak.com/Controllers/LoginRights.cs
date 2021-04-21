using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.GoalSetter.Controllers
{
    public class LoginRights
    {
        public bool IsAdmin { get; private set; }
        public Guid GroupKey { get; private set; }
        public Guid UserKey { get; private set; }
        public string UUID { get; private set; }
        public Guid UserKeyForError { get; private set; }
        public Guid GroupKeyCheck { get; private set; }
        public LoginRights(string uuid, bool isAdmin, Guid groupKey, Guid userKey, Guid userKeyForError, Guid groupKeyCheck)
        {
            UUID = uuid;
            IsAdmin = isAdmin;
            GroupKey = groupKey;
            UserKey = userKey;
            UserKeyForError = userKeyForError;
            GroupKeyCheck = groupKeyCheck;
        }
    }
}
