using System;
using System.Collections.Generic;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class AutomaticMailingSettings
    {
        public List<AutomaticMailingProcessSettings> MailingProcessesSettings { get; set; }
    }

    public class AutomaticMailingProcessSettings
    {
        public string ProcessName { get; set; }
        public string ProcessNameHeb { get; set; }
        public int? EmailTemplateCode { get; set; }
        public int? SmsTemplateCode { get; set; }

        public override string ToString()
        {
            return $"Automatic mailing process name: {this.ProcessName}, Email template code: {this.EmailTemplateCode}, Sms teplate code: {this.SmsTemplateCode}";
        }
    }
}
