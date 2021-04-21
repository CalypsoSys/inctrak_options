using IncTrak.GoalSetter.Models;
using System;

namespace IncTrak.GoalSetter.data
{
    public class GROUP_UI : BASE_DTO
    {
        public Guid GROUP_PK { get; set; }
        public string DESCRIPTION { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }

        public GROUP_UI()
            : base(Guid.Empty)
        {
        }

        public GROUP_UI(Groups db) : base(db.GroupPk)
        {
            GROUP_PK = db.GroupPk;
            DESCRIPTION = db.Description;
            CREATED = db.Created;
            UPDATED = db.Updated;
        }
    }
}
