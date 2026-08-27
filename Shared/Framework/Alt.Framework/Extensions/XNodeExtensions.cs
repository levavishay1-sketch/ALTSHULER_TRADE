using System.Xml.Linq;

namespace Alt.Framework.Extensions
{
    public static class XNodeExtensions
    {
        public static string InnerXml(this XNode node)
        {
            using (var reader = node.CreateReader())
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }

        }
    }
}
