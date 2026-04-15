using System.ComponentModel.DataAnnotations;

namespace _4EPlatform.Shared.CustomValidationInjection
{
    public class PreventInjectionScriptAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string input = value.ToString();

            // Simple regex to detect SQL or script tags, adjust as needed
            string[] blackList = { "<script>", "</script>", ".js" };

            foreach (var item in blackList)
            {
                if (input.IndexOf(item, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new ValidationResult("Input contains script tags.");
                }
            }

            return ValidationResult.Success;
        }
    }
}