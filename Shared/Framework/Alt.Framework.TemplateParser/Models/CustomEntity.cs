using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.Models
{
    public class CustomEntity
    {
        public string Id { get; internal set; }
        public string EntityName { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();

        public List<CustomLinkEntity> LinkEntities { get; set; } = new List<CustomLinkEntity>();
        public string Alias { get; set; }

        public bool IsLinkEntityQuery { get; set; } = false;

        public string TableAttributeFilter { get; set; }
    }
}
