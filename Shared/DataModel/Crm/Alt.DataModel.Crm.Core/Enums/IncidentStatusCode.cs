
namespace Alt.DataModel.Crm.Core.Enums
{
    public enum IncidentStatusCode
    {
        OnGoing = 1,
        Holding = 2,
        WaitingForDetails = 3,
        Checking = 4,
        Solved = 5,
        Canceled = 6,
        InformationProvided = 1000,
        Merged = 2000,

        /*
        THIS STATUS IS ONLY USED INTERNALLY TO OPEN CLOSED INCIDENTS 
        AND IS NOT APPLICABLE FOR ANY OTHER USE!
        */
        OpenedBySystem = 455710001
    }
}
