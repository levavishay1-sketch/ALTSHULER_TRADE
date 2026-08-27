using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.BlacklistsCheck
{
    public class PreCreateBlacklistsCheck : PluginBase
    {
        public PreCreateBlacklistsCheck(string unsecure, string secure) : base(typeof(PreCreateBlacklistsCheck)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_BlacklistsCheck targetBlacklistsCheck = localContext.TargetEntity?.ToEntity<alt_BlacklistsCheck>();

            BlacklistsCheckBL blacklistsCheckBL = new BlacklistsCheckBL(localContext.ToGlobal());
            blacklistsCheckBL.SetDefaultValues(targetBlacklistsCheck);
            blacklistsCheckBL.SetName(targetBlacklistsCheck);
            blacklistsCheckBL.HandleStatusCode(targetBlacklistsCheck);
        }
    }
}