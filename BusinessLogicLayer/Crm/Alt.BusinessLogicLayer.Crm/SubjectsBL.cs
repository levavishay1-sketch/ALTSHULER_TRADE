using Alt.DataAccessLayer.Crm;
using Alt.Framework;
using Microsoft.Xrm.Sdk;

namespace Alt.BusinessLogicLayer.Crm
{
    public class SubjectsBL : CrmBaseBL
    {
        public SubjectsBL(GlobalContext globalContext) : base(globalContext)
        {
        }

        public string GetSubjectName(EntityReference subjectEntityReference)
        {
            this.GlobalContext.LogEntry();
            CommonDAL commonDal = new CommonDAL(this.GlobalContext, subjectEntityReference.LogicalName);
            return commonDal.GetPrimeryAttributeValue(subjectEntityReference, "alt_name");
        }
    }
}
