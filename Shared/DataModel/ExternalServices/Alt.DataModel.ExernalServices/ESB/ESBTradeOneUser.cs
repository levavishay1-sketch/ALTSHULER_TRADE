using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.ExernalServices.ESB
{
    public class ESBTradeOneUser : ExternalEntityBase
    {
        [Required]
        public string ContactId { get; set; }
        [Required]
        public string FirstNameEng { get; set; }
        [Required]
        public string LastNameEng { get; set; }
        [Required]
        public string FirstNameHeb { get; set; }
        [Required]
        public string LastNameHeb { get; set; }
        [Required]
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string AccountNumber { get; set; }
        public string IsPro { get; set; }
        public string group1 { get; set; }
    }
}
