using System;
using IncTrak.GoalSetter.Models;

namespace IncTrak.GoalSetter.data
{
    public class IMPORTANCE_TYPE_UI : BASE_DTO
    {
        public int IMPORTANCE_TYPE_PK { get; set; }
        public string NAME { get; set; }

        public ImportanceType Importance
        {
            get { return null; }
            set
            {
                IMPORTANCE_TYPE_PK = value.ImportanceTypePk;
                NAME = value.Name;
            }
        }

        public ImportanceType GetImportanceType()
        {
            return new ImportanceType()
            {
                ImportanceTypePk = this.IMPORTANCE_TYPE_PK,
                Name = this.NAME
            };
        }

        public IMPORTANCE_TYPE_UI() : base(Guid.Empty)
        {
        }

        public IMPORTANCE_TYPE_UI(ImportanceType db) : this()
        {
            Importance = db;
        }
    }
}
