using Alt.DataModel.Crm.Core.Interfaces;
using Alt.Framework.External.Json.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace Alt.Framework.External.Json
{
    public class EntityModifiedPropertiesSerializer
    {
        public string Serialize(IModifiedProperties externalEntityBase)
        {
            string serializeESBModel = null;
            if (externalEntityBase != null)
            {

                var dynamicObjectWithOnlyModifiedProperties = ToDynamic(externalEntityBase);
                serializeESBModel = JsonConvert.SerializeObject(dynamicObjectWithOnlyModifiedProperties, Formatting.None,
                    new IsraelDateTimeConverter());
            }
            return serializeESBModel;
        }

        private dynamic ToDynamic<T>(T obj) where T : IModifiedProperties
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (obj.Contains(propertyInfo.Name))
                {
                    var value = propertyInfo.GetValue(obj);
                    if (value is T)
                    {
                        expando.Add(propertyInfo.Name, ToDynamic<T>((T)value));
                    }
                    else if (value != null && value is IEnumerable<T>)
                    {
                        IEnumerable<T> collection = value as IEnumerable<T>;
                        var parsedCollection = new List<dynamic>();
                        expando.Add(propertyInfo.Name, parsedCollection);
                        foreach (var item in collection)
                        {
                            parsedCollection.Add(ToDynamic<T>((T)item));
                        }
                    }
                    else
                    {
                        var currentValue = propertyInfo.GetValue(obj);
                        expando.Add(propertyInfo.Name, currentValue);
                    }
                }
            }
            return expando as ExpandoObject;
        }
    }
}
