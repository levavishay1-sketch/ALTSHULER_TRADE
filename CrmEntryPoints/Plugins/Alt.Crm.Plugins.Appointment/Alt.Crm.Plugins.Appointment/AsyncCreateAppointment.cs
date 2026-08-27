using Alt.Framework.EntryPoints.Crm;
using Alt.BusinessLogicLayer.Crm;

namespace Alt.Crm.Plugins.Appointment
{
    public class AsyncCreateAppointment : PluginBase
    {
        public AsyncCreateAppointment(string unsecure, string secure) : base(typeof(AsyncCreateAppointment)) { }

        protected override void ExecuteCrmPlugin(LocalContext localContext)
        {
            DataModel.Crm.Entities.Appointment targetAppointment = localContext.TargetEntity?.ToEntity<DataModel.Crm.Entities.Appointment>();

            AppointmentBL appointmentBl = new AppointmentBL(localContext.ToGlobal());
            appointmentBl.SendSmsAndEmailByActivitySubject(targetAppointment);
        }
    }
}
