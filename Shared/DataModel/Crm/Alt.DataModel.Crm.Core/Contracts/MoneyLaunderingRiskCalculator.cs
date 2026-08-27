using Alt.DataModel.Crm.Core.Enums;


namespace Alt.DataModel.Crm.Core.Contracts
{
    public class MoneyLaunderingRiskCalculator
    {
        public Moneylaunderingcalculator MoneyLaunderingCalculator { get; set; }
    }

    public class Moneylaunderingcalculator
    {
        public string logicalNameEntity { get; set; }
        public AttributesMoneyLaunderingCalculator[] attributesMoneyLaunderingCalculator { get; set; }
    }

    public class AttributesMoneyLaunderingCalculator
    {
        public string fieldNameDestination { get; set; }
        //convert to enum CrmPropertyType
        public CrmPropertyType fieldTypeDestination { get; set; }
        public DataSource dataSource { get; set; }
    }

    public class DataSource
    {
        public string fieldNameSource { get; set; }
        //convert to enum CrmPropertyType
        public CrmPropertyType fieldTypeSource { get; set; }
        public Sourcevalue[] sourceValues { get; set; }
    }

    public class Sourcevalue
    {
        public int? currentValue { get; set; }
        public int? scoreDestination { get; set; }
    }
}