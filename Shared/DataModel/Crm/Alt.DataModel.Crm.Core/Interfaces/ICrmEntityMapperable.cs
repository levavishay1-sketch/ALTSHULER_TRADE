
namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface ICrmEntityMapperable
    {
        object GetValueByKey(string key);
        bool Contains(string propertyName);
    }
}
