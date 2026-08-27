
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum EmailStatusCode
    {
        //state = open (0)
        Draft = 1,
        Failed = 8,

        //state = Completed (1)
        Completed = 2,
        Sent = 3,
        Received = 4,
        PendingSend = 6,
        Sending = 7,

        //state = Canceled (2)
        Canceled = 5
    }
}
