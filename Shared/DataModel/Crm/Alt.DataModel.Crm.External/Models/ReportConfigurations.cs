
namespace Alt.DataModel.Crm.External.Models
{
    public class ReportConfigurations
    {
        public int? ReportCode { get; set; }
        public int? XDaysBefore { get; set; }
        public string Name { get; set; }
        public EmailSettings emailSettings { get; set; }
        public string EmptyResultMessage { get; set; }
        public string Description { get; set; }
    }  
}
