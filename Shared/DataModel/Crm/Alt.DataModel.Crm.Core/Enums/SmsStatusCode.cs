
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum SmsStatusCode
    {
        Draft = 1,
        Send = 100000000,
        SendingNow = 100000001,
        SentSuccessfully = 2,
        Failed = 100000003,
        Canceled = 3,
        Scheduled = 4
    }
}
