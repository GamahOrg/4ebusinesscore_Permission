using System.ComponentModel.DataAnnotations;

namespace _4EPlatform.Shared.CustomValidationInjection
{
    public class PreventInjectionCSVAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string input = value.ToString();
            //string pattern = "[@\\-\\+=]";
            //if (Regex.IsMatch(input, pattern))
            //{
            //    return new ValidationResult("Input contains CSV tags.");
            //}

            return ValidationResult.Success;
        }
    }
}