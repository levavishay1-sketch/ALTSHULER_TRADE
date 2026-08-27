using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.RepresentativeReward
{
    public class AsyncCreateRepresentativeReward : PluginBase
    {
        public AsyncCreateRepresentativeReward(string unsecure, string secure)
          : base(typeof(AsyncCreateRepresentativeReward)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_RepresentativeReward targetRepresentativeReward = localContext.TargetEntity?.ToEntity<alt_RepresentativeReward>();

            RepresentativeRewardBL representativeRewardBl = new RepresentativeRewardBL(localContext.ToGlobal());
            representativeRewardBl.ClosePreviousRepresentativeReward(targetRepresentativeReward);
        }
    }
}
