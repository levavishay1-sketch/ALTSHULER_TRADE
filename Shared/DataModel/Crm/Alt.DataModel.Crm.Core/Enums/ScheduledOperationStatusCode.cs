
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum ScheduledOperationStatusCode
    {
        //active
        Draft = 1,
        Run = 491170001,
        Running = 491170002,

        //inactive
        Canceled = 2,
        Failed = 491170003,
        FinishedSuccessfully = 491170004
    }
}
