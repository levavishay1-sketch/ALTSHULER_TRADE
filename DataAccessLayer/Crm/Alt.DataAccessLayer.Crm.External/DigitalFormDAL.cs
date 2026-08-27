using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.Framework;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataAccessLayer.Crm.External
{
    public class DigitalFormDAL : CrmExternalBaseDAL<ApiDigitalForm>
    {
        public DigitalFormDAL(GlobalContext globalContext) : base(globalContext, ApiDigitalForm.EntityLogicalName) { }


    }
}
