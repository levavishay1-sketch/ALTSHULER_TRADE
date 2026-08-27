using Alt.BusinessLogicLayer.Crm;
using Alt.Framework.EntryPoints.Crm;

namespace Alt.Crm.Plugins.Appointment
{
    public class PreCreateAppointment: PluginBase
    {
        public PreCreateAppointment(string unsecure, string secure) : base(typeof(PreCreateAppointment)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Appointment targetAppointment = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Appointment>();

            AppointmentBL appointmentBl = new AppointmentBL(localContext.ToGlobal());
            appointmentBl.SetSubject(targetAppointment);
        }
    }
}
