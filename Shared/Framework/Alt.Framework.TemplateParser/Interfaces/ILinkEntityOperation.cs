using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Alt.Framework.TemplateParser.Interfaces
{
    public interface ILinkEntityOperation
    {
        string ExtractOperationTemplateResultPattern(string tablePlaceHolder);

        string ParseOperationTemplateResultPattern(IEnumerable<Entity> records, string tableReturnTemplate);

        CustomLinkEntity HandleCreateCustomLinkEntitiesByLinkEntityPlaceHolders(SpecialOperationPlaceHolder specialOperationPlaceHolder);
    }
}
