using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Alt.Framework.External.Validators
{
    public class DataAnnotationsValidator
    {
        public static IEnumerable<ValidationResult> Validate<T>(object objectToValidate)
              where T : class
        {
            var results = new List<ValidationResult>();
            var properties = objectToValidate.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(objectToValidate);
                if (value is T)
                {
                    HandleBuildPropertyValidationResult<T>(results, (value as T), propertyInfo.Name);
                }
                else if (value != null && value is IEnumerable<T>)
                {
                    IEnumerable<T> collection = value as IEnumerable<T>;
                    foreach (var item in collection)
                    {
                        HandleBuildPropertyValidationResult<T>(results, item, propertyInfo.Name);
                    }
                }
            }
            return results;
        }

        private static void HandleBuildPropertyValidationResult<T>(List<ValidationResult> globalValidationResults, T propertyValue, string propertyName) where T : class
        {
            var validationResult = new List<ValidationResult>();
            Validator.TryValidateObject(propertyValue, new ValidationContext(propertyValue), validationResult, true);
            globalValidationResults.AddRange(validationResult.Select(e =>
            {
                e.ErrorMessage = $"{propertyName}.{e.ErrorMessage}";
                return e;
            }).ToList());
        }
    }
}
