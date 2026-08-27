using Alt.DataModel.Crm.Entities;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace Alt.DataAccessLayer.Crm
{
    public class ArchiveDocumentSearchDAL : CrmBaseDAL<alt_ArchiveDocumentSearch>
    {
        public ArchiveDocumentSearchDAL(GlobalContext globalContext) : base(globalContext, alt_ArchiveDocumentSearch.EntityLogicalName) { }
    }
}
