using Alt.BusinessLogicLayer.Crm;
using Alt.DataModel.Crm.Entities;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.RepresentativeReward
{
    public class PreUpdateRepresentativeReward : PluginBase
    {
        public PreUpdateRepresentativeReward(string unsecure, string secure)
         : base(typeof(PreUpdateRepresentativeReward)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            alt_RepresentativeReward targetRepresentativeReward = localContext.TargetEntity?.ToEntity<alt_RepresentativeReward>();
            alt_RepresentativeReward preRepresentativeReward = localContext.PreEntity?.ToEntity<alt_RepresentativeReward>();

            RepresentativeRewardBL representativeRewardBl = new RepresentativeRewardBL(localContext.ToGlobal());
            representativeRewardBl.SetName(targetRepresentativeReward, preRepresentativeReward);
        }
    }
}
