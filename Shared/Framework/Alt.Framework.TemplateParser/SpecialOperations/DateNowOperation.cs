using Alt.Framework.TemplateParser.Models;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.TemplateParser.SpecialOperations
{
    public class DateNowOperation : SpecialOperationBase
    {
        public DateNowOperation(string prefix, string suffix) : base(prefix, suffix)
        {
        }

        public override string ExecuteSpecialOperationLogic(Entity entity, string key, SpecialOperationPlaceHolder specialOperationPlaceHolder)
        {
            return this.GetIsraelLocalDateTimeFromUtc(DateTime.UtcNow).ToString("dd/MM/yyyy");
        }

        private DateTime GetIsraelLocalDateTimeFromUtc(DateTime dateTimeUtc)
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, cstZone);
        }
    }
}
