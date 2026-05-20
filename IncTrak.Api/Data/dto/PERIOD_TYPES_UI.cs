using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class PERIOD_TYPES_UI : BASE_DTO
    {
        public int PERIOD_TYPE_PK { get; set; }
        public string NAME { get; set; }

        public PeriodTypes PeriodType
        {
            get { return null;}
            set 
            {
                PERIOD_TYPE_PK = value.PeriodTypePk;
                NAME = value.Name;
            }
        }

        public PeriodTypes GetPeriodType()
        {
            return new PeriodTypes()
            {
                PeriodTypePk = this.PERIOD_TYPE_PK,
                Name = this.NAME
            };
        }

        public PERIOD_TYPES_UI() : base(Guid.Empty)
        {
        }

        public PERIOD_TYPES_UI(PeriodTypes db) : this()
        {
            PeriodType = db;
        }
    }
}
