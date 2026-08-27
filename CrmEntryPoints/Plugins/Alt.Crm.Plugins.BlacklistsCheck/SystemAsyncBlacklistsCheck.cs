using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.BlacklistsCheck
{
    public class SystemAsyncBlacklistsCheck : PluginBase
    {
        public SystemAsyncBlacklistsCheck(string unsecure, string secure) : base(typeof(SystemAsyncBlacklistsCheck), false) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_BlacklistsCheck targetBlacklistsCheck = localContext.TargetEntity?.ToEntity<alt_BlacklistsCheck>();
            alt_BlacklistsCheck preBlacklistsCheck = localContext.PreEntity?.ToEntity<alt_BlacklistsCheck>();

            BlacklistsCheckBL blacklistsCheckBL = new BlacklistsCheckBL(localContext.ToGlobal());
            blacklistsCheckBL.SendAppNotificationOnReceivedResponse(targetBlacklistsCheck, preBlacklistsCheck);
        }
    }
}
