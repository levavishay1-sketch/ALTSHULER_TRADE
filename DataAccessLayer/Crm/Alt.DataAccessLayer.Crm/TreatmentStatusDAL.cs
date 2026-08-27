using Alt.DataModel.Crm.Entities;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm
{
    public class TreatmentStatusDAL : CrmBaseDAL<alt_TreatmentStatus>
    {
        private string[] treatmentStatusFields = new string[]
        {
            alt_TreatmentStatus.Fields.Id,
            alt_TreatmentStatus.Fields.alt_Name
        };

        public TreatmentStatusDAL(GlobalContext globalContext) : base(globalContext, alt_TreatmentStatus.EntityLogicalName) { }

        public alt_TreatmentStatus GetByCode(int code)
        {
            this.GlobalContext.LogEntry();
            return this.GetFirstOrDefaultByAttribute(alt_TreatmentStatus.Fields.alt_CodeInt, code, treatmentStatusFields);
        }
    }
}
