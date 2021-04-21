using System;
using IncTrak.GoalSetter.Models;

namespace IncTrak.GoalSetter.data
{
    public class RATING_TYPE_UI : BASE_DTO
    {
        public int RATING_TYPE_PK { get; set; }
        public string NAME { get; set; }

        public RatingType Rating
        {
            get { return null; }
            set
            {
                RATING_TYPE_PK = value.RatingTypePk;
                NAME = value.Name;
            }
        }

        public RatingType GetPeriodType()
        {
            return new RatingType()
            {
                RatingTypePk = this.RATING_TYPE_PK,
                Name = this.NAME
            };
        }

        public RATING_TYPE_UI() : base(Guid.Empty)
        {
        }

        public RATING_TYPE_UI(RatingType db) : this()
        {
            Rating = db;
        }
    }
}
