using Alt.BusinessLogicLayer.Crm.External;
using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;
using Alt.External.Services.CrmApi.Framework;
using System.Configuration;
using System.Web.Http;

namespace Alt.External.Services.CrmApi.Controllers
{
    [GlobalContextManagerAttribute]
    public class DigitalFormsController: BaseController
    {
        public DigitalFormsController()
        {
            DefaultQueueName = ConfigurationManager.AppSettings["DigitalFormsQueueName"];
        }

        public IHttpActionResult Post([FromBody] ApiDigitalForm  apiDigitalForm)
        {
            DigitalFormBL digitalFormBL = new DigitalFormBL(base.ThirdPartyBase.GlobalContext);
            ActionResult actionResult = digitalFormBL.HandleDigitalFormPost(apiDigitalForm);

            return base.HandleGenerateResponse(actionResult);
        }

        public IHttpActionResult Put([FromBody] ApiDigitalForm apiDigitalForm)
        {
            DigitalFormBL digitalFormBL = new DigitalFormBL(base.ThirdPartyBase.GlobalContext);
            ActionResult actionResult = digitalFormBL.HandleDigitalFormPut(apiDigitalForm);

            return base.HandleGenerateResponse(actionResult);
        }
    }
}