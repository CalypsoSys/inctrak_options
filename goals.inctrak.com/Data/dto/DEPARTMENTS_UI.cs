using IncTrak.GoalSetter.Models;
using System;
using System.Xml.Serialization;

namespace IncTrak.GoalSetter.data
{
    public class DEPARTMENTS_UI : BASE_DTO
    {
        public Guid DEPARTMENT_PK { get; set; }
        public string NAME { get; set; }
        public DateTime CREATED { get; set; }
        public DateTime UPDATED { get; set; }
        public Guid GROUP_FK { get; set; }
        public int CURRENT_GOALS { get; set; }
        public int TOTAL_GOALS { get; set; }

        public Departments SetFromDepartment
        {
            get { return null; }
            set
            {
                if (value != null && GroupKeyCheck == value.GroupFk)
                {
                    DEPARTMENT_PK = value.DepartmentPk;
                    NAME = value.Name;
                    CREATED = value.Created;
                    UPDATED = value.Updated;
                    GROUP_FK = value.GroupFk;
                }
            }
        }

        public Departments GetDepartment(Guid groupFk)
        {
            if (GroupKeyCheck == groupFk)
            {
                return new Departments()
                {
                    DepartmentPk = this.DEPARTMENT_PK,
                    Name = this.NAME,
                    Created = this.CREATED,
                    Updated = this.UPDATED,
                    GroupFk = groupFk,
                };
            }

            return new Departments();
        }

        public void SetToDepartment(Departments department, Guid groupKey)
        {
            if (GroupKeyCheck == groupKey)
            {
                department.Name = NAME;
                department.GroupFk = groupKey;
            }
        }

        public DEPARTMENTS_UI()
            : base(Guid.Empty)
        {
        }

        public DEPARTMENTS_UI(Guid groupKeyCheck) : base(groupKeyCheck)
        {
        }

        public DEPARTMENTS_UI(Guid groupKeyCheck, Departments db) : this(groupKeyCheck)
        {
            if (groupKeyCheck == db.GroupFk)
            {
                SetFromDepartment = db;
            }
        }
    }
}
