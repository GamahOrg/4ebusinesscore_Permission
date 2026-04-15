using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace _4EPlatform.Shared.CustomValidationInjection
{
    public class PreventInjectionAttribute : ValidationAttribute
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

            string sanitizedInput = Regex.Replace(input, "<.*?>", string.Empty);

            // If the sanitized input differs from the original, you can optionally return a validation failure or adjust the model value here.
            if (!input.Equals(sanitizedInput))
            {
                return new ValidationResult("Input contains HTML or script tags.");
            }


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
