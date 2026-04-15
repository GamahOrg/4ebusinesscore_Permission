using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace _4EPlatform.Shared.CustomValidationInjection
{
    public class PreventInjectionHTMLAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string input = value.ToString();


            string sanitizedInput = Regex.Replace(input, "<.*?>", string.Empty);

            // If the sanitized input differs from the original, you can optionally return a validation failure or adjust the model value here.
            if (!input.Equals(sanitizedInput))
            {
                return new ValidationResult("Input contains HTML or script tags.");
            }

            return ValidationResult.Success;
        }
    }
}