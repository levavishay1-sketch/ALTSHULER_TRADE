namespace Alt.DataModel.Crm.External.Models
{
    public class FetchXMLReportConfig : ReportConfigurations
    {
        public string[] ReportsFetchParams { get; set; }
        public string[] ReportsFetchViews { get; set; }
    }
}