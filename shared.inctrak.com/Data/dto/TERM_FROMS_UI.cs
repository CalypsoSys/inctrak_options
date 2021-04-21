using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class TERM_FROMS_UI : BASE_DTO
    {
        public enum IDs { ABSOLUTE = -1 };
        public int TERM_FROM_PK { get; set; }
        public string NAME { get; set; }

        public TermFroms TermFromType
        {
            get { return null; }
            set
            {
                TERM_FROM_PK = value.TermFromPk;
                NAME = value.Name;
            }
        }

        public TERM_FROMS_UI() : base(Guid.Empty)
        {
        }

        public TERM_FROMS_UI(TermFroms db) : this()
        {
            TermFromType = db;
        }
    }
}
