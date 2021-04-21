using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class ACTIVATE_UUIDS_UI : BASE_DTO
    {
        public int UUID_PK { get; set; }
        public string UUID { get; set; }
        public int TYPE { get; set; }
        public DateTime VALID_UNTIL { get; set; }
        public Guid USER_FK { get; set; }

        public ACTIVATE_UUIDS_UI() :base(Guid.Empty)
        {
        }

        public ACTIVATE_UUIDS_UI(ActivateUuids db) : base(Guid.Empty)
        {
            UUID_PK = db.UuidPk;
            UUID = db.Uuid;
            TYPE = db.Type;
            VALID_UNTIL = db.ValidUntil;
            USER_FK = db.UserFk;
        }
    }
}
