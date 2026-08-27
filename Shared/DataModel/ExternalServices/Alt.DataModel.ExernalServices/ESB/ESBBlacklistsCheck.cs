using System;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBBlacklistsCheck: ExternalEntityBase
    {
        public string ExternalNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
