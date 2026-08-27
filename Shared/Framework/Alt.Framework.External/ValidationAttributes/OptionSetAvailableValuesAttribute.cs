using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alt.Framework.External.ValidationAttributes
{
    public class OptionSetAvailableValuesAttribute : ValidationAttribute
    {
        private readonly HashSet<int> values;

        /// <summary>
        ///     Initializes a new instance of the Alt.DataModel.Crm.External.Attributes.OptionSetAvailableValuesAttribute
        ///     class based on the available values.
        /// </summary>
        /// <param name="values">all available numeric values in a string format, also available to provide a range. for example: "1","7-12","4"</param>
        public OptionSetAvailableValuesAttribute(string[] values)
        {
            this.values = new HashSet<int>();

            foreach (string value in values)
            {
                string[] val = value.Split('-');
                if (val.Length == 2)
                {
                    int min = int.Parse(val[0]);
                    int max = int.Parse(val[1]);
                    for (int i = min; i <= max; i++)
                    {
                        this.values.Add(i);
                    }
                }
                else
                {
                    this.values.Add(int.Parse(val[0]));
                }
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            return this.values.Contains((int)value);
        }
    }
}
