using Alt.DataModel.Crm.Core.Contracts;
using Alt.DataModel.Crm.External.Contracts;

namespace Alt.DataModel.Crm.External.Interfaces
{
    public interface ICrmOutgoing<TApiEntity> where TApiEntity : ApiEntityBase
    {
        ActionResult ExecuteOutgoingLogicHandler(ApiContext<TApiEntity> apiContext);
    }
}
