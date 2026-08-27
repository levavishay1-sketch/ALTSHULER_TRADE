
using System.Collections.Concurrent;

namespace Alt.DataModel.Crm.Core.Interfaces
{
    public interface IModifiedProperties
    {
        ConcurrentDictionary<string, object> ModifiedProperties { get; }
        void SetProperty(object value, string propertyName);
        bool Contains(string propertyName);
        object GetValueByKey(string key);
    }
}
