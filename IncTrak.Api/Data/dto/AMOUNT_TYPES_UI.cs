using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class AMOUNT_TYPES_UI : BASE_DTO
    {
        public int AMOUNT_TYPE_PK { get; set; }
        public string NAME { get; set; }

        public AmountTypes AmountType
        {
            get { return null;}
            set 
            {
                AMOUNT_TYPE_PK = value.AmountTypePk;
                NAME = value.Name;
            }
        }

        public AmountTypes GetAmountType()
        {
            return new AmountTypes()
            {
                AmountTypePk = this.AMOUNT_TYPE_PK,
                Name = this.NAME
            };
        }

        public AMOUNT_TYPES_UI() : base(Guid.Empty)
        {
        }

        public AMOUNT_TYPES_UI(AmountTypes db) : this()
        {
            AmountType = db;
        }
    }
}
