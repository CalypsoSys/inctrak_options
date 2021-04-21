using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class PARTICIPANT_TYPES_UI : BASE_DTO
    {
        public int PARTICIPANT_TYPE_PK { get; set; }
        public string NAME { get; set; }

        public ParticipantTypes PartType
        {
            get { return null; }
            set
            {
                PARTICIPANT_TYPE_PK = value.ParticipantTypePk;
                NAME = value.Name;
            }
        }

        public PARTICIPANT_TYPES_UI() : base(Guid.Empty)
        {
        }

        public PARTICIPANT_TYPES_UI(ParticipantTypes db) : this()
        {
            PartType = db;
        }
    }
}
