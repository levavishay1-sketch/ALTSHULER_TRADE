namespace Alt.DataModel.Crm.Core.Enums
{
    public enum TeamCodes
    {
        JoiningControlTeam = 1000,
        OperationalControlTeam = 1001,
        MoneyLaunderingControlTeam = 1002,
        ManagementControlTeam = 1003
    }
     public static class TeamNames
     {
         public const string OperationalControl = "OperationalControl";
         public const string MoneyLaunderingControl = "MoneyLaunderingControl";
         public const string ManagerControl = "ManagerControl";
         public const string JoiningControl = "JoiningControl";
     }
}
