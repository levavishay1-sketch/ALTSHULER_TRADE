using Alt.DataModel.Crm.Core.Enums;
using Alt.Framework.Mapper;
using System.ComponentModel.DataAnnotations;

namespace Alt.DataModel.Crm.External.Contracts
{
    public class ApiSubject2 : ApiEntity
    {
        public const string EntityLogicalName = "alt_subject2";

        public ApiSubject2() : base(EntityLogicalName)
        {
        }

        private int? code;
        [CrmEntityMapper("alt_codeint", CrmPropertyType.Int)]
        public int? Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.SetProperty(value);
                this.code = value;
            }
        }

        private ApiSubject1 subject1;
        [CrmEntityMapper("alt_subject1id", CrmPropertyType.EntityReference)]
        public ApiSubject1 Subject1
        {
            get
            {
                return this.subject1;
            }
            set
            {
                this.SetProperty(value);
                this.subject1 = value;
            }
        }

        private string name;
        [CrmEntityMapper("alt_name", CrmPropertyType.String)]
        [StringLength(100)]
        public string Name
        {
            get { return name; }
            set
            {
                this.SetProperty(value);
                name = value;
            }
        }
    }
}
