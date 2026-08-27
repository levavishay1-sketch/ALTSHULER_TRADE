using Alt.DataModel.Crm.Core.Enums;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.DataModel.Crm.Core.Contracts
{
    public class AppNotificationSettings
    {
        public AppNotificationToastTypeCode? ToastType { get; set; }
        public AppNotificationIconTypeCode? IconType { get; set; }
        public string Title { get; set; }
        public List<EntityReference> Recipients { get; set; }
        public string Body { get; set; }
        public Entity Actions { get; set; }
        public bool SendToOwner { get; set; }
    }
}
