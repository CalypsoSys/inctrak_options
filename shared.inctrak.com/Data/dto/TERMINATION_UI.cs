using IncTrak.Models;
using System;

namespace IncTrak.data
{
    public class TERMINATION_UI : BASE_DTO
    {
        public Guid TERMINATION_PK { get; set; }
        public Guid GROUP_FK { get; set; }
        public string NAME { get; set; }
        public DateTime ABSOLUTE_DATE { get; set; }
        public int YEARS { get; set; }
        public int MONTHS { get; set; }
        public int DAYS { get; set; }
        public int? TERM_FROM_FK { get; set; }
        public DateTime SPECIFIC_DATE { get; set; }
        public bool IS_ABSOLUTE { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }

        public Terminations SetFromTermination
        {
            get { return null; }
            set 
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    TERMINATION_PK = value.TerminationPk;
                    GROUP_FK = value.GroupFk;
                    NAME = value.Name;
                    ABSOLUTE_DATE = value.AbsoluteDate;
                    YEARS = value.Years;
                    MONTHS = value.Months;
                    DAYS = value.Days;
                    TERM_FROM_FK = DtoHelper.NullInt(value.TermFromFk);
                    SPECIFIC_DATE = value.SpecificDate;
                    IS_ABSOLUTE = value.IsAbsolute;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                }
            }
        }

        public Terminations GetTermination(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Terminations()
                {
                    TerminationPk = this.TERMINATION_PK,
                    GroupFk = groupFk,
                    Name = this.NAME,
                    AbsoluteDate = this.ABSOLUTE_DATE,
                    Years = this.YEARS,
                    Months = this.MONTHS,
                    Days = this.DAYS,
                    TermFromFk = this.TERM_FROM_FK ?? 0,
                    SpecificDate = this.SPECIFIC_DATE,
                    IsAbsolute = this.IS_ABSOLUTE,
                    Created = this.CREATED,
                    Updated = this.UPDATED
                };
            }

            return new Terminations();
        }

        public void SetToTermination(Terminations termination, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                termination.Name = NAME;
                termination.GroupFk = groupKey;
                termination.AbsoluteDate = ABSOLUTE_DATE;
                termination.Years = YEARS;
                termination.Months = MONTHS;
                termination.Days = DAYS;
                termination.TermFromFk = TERM_FROM_FK ?? 0;
                termination.SpecificDate = SPECIFIC_DATE;
                termination.IsAbsolute = IS_ABSOLUTE;
            }
        }

        public TERMINATION_UI()
            : base(Guid.Empty)
        {
        }

        public TERMINATION_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public TERMINATION_UI(Guid groupKeyCheck, Terminations db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromTermination = db;
            }
        }
    }
}
