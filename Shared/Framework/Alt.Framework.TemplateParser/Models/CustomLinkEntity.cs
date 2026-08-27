using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public class CustomLinkEntity : CustomEntity
    {
        public CustomLinkEntity() { }
        public CustomLinkEntity(string linkFromEntityName, string linkFromAttributeName, string linkToEntityName, string linkToAttributeName)
        {
            LinkFromAttributeName = linkFromAttributeName;
            LinkFromEntityName = linkFromEntityName;
            LinkToAttributeName = linkToAttributeName;
            LinkToEntityName = linkToEntityName;
        }
        public string LinkFromAttributeName { get; set; }
        public string LinkFromEntityName { get; set; }
        public string LinkToAttributeName { get; set; }
        public string LinkToEntityName { get; set; }
       

    }
}
