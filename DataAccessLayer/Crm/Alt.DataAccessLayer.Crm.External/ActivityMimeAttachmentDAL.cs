using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;

namespace Alt.DataAccessLayer.Crm.External
{
    public class ActivityMimeAttachmentDAL : CrmExternalBaseDAL<ApiActivityMimeAttachment>
    {
        public ActivityMimeAttachmentDAL(GlobalContext globalContext) : base(globalContext, ApiActivityMimeAttachment.EntityLogicalName)
        {
        }
    }
}
