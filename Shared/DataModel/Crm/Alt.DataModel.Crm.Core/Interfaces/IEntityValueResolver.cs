using Microsoft.Xrm.Sdk;

namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface IEntityValueResolver
    {
        string GetAttributeValue(string attributeName, Entity entity);
    }
}
