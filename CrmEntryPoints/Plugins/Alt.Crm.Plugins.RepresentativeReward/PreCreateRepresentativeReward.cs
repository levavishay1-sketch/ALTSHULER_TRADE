using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.RepresentativeReward
{
    public class PreCreateRepresentativeReward : PluginBase
    {
        public PreCreateRepresentativeReward(string unsecure, string secure)
            : base(typeof(PreCreateRepresentativeReward)) { }
        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_RepresentativeReward targetRepresentativeReward = localContext.TargetEntity?.ToEntity<alt_RepresentativeReward>();

            RepresentativeRewardBL representativeRewardBl = new RepresentativeRewardBL(localContext.ToGlobal());
            representativeRewardBl.SetName(targetRepresentativeReward);
            representativeRewardBl.SetRepresentativeRewardTypeCode(targetRepresentativeReward);
        }
    }
}
