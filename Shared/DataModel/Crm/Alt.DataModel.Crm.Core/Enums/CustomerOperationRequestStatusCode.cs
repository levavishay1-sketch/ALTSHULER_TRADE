
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum CustomerOperationRequestStatusCode
    {
        //active
        Draft = 1,
        Send = 399020001,
        Sending = 399020002,
        Fail = 399020005,

        //inactive
        Canceled = 2,
        SentSuccessful = 399020004,
    }
}
