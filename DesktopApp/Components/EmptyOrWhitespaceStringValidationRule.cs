using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;

namespace DesktopApp.Components
{
    public class EmptyOrWhitespaceStringValidationRule : ValidationRule
    {
        public EmptyOrWhitespaceStringValidationRule()
        {
            return;
        }

        public override ValidationResult Validate(object value, CultureInfo ci)
        {
            string str = (string)value;
            if (string.IsNullOrEmpty(str) | string.IsNullOrWhiteSpace(str))
                return new ValidationResult(false, "Строка должна быть непустой и состоять не только из пробелов.");

            return ValidationResult.ValidResult;
        }
    }
}
