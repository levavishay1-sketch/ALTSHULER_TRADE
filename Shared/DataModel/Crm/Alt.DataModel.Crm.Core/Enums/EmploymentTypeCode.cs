using System.ComponentModel;

namespace Alt.DataModel.Crm.Core.Enums
{
    public enum EmploymentTypeCode
    {
        [Description("Independent")]
        Independent = 1,
        [Description("Employee")]
        Employee = 2,
        [Description("CompanyOwner")]
        CompanyOwner = 3,
        [Description("Student")]
        Student = 4,
        [Description("Soldier")]
        Soldier = 5,
        [Description("Pensioner")]
        Pensioner = 6,
        [Description("UnEmployee")]
        Unemployed = 7,
        [Description("Other")]
        Else = 8,
    }
}