using System;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class CustomEntityReference
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public object ExtensionData { get; set; }
    }
}
