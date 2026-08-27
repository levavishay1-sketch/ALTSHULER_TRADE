using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class UrlOperation : SpecialOperationBase
    {
        private readonly string crmUrl = null;
        private readonly string rootRegardingObjectEntityName;
        private readonly string rootRegardingObjectId;
        public UrlOperation(string prefix, string suffix,string rootRegardingObjectEntityName, string rootRegardingObjectId,  string crmUrl) : base(prefix, suffix)
        {
            this.crmUrl = crmUrl;
            this.rootRegardingObjectEntityName = rootRegardingObjectEntityName;
            this.rootRegardingObjectId = rootRegardingObjectId;
        }

        public override string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            EntityReference attribute = null;
            if (!specialOperationPlaceHolder.IsValidToParse)
            {
                attribute = new EntityReference(rootRegardingObjectEntityName, new Guid(rootRegardingObjectId));
            }
            else
            {
                attribute = entity[key] as EntityReference;
                if (entity[key].GetType().Name == "AliasedValue")
                {
                    var aliasedValue = ((AliasedValue)entity[key])?.Value;
                    attribute = aliasedValue as EntityReference;
                }
            }
            return attribute != null ? $"{crmUrl}&pagetype=entityrecord&etn={attribute.LogicalName}&id={attribute.Id}" : null;
        }
    }
}
