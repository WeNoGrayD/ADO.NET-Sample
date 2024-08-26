using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;
using DesktopApp.Models;

namespace DesktopApp.Components
{
    public class NonNegativeDecimalValidationRule : ValidationRule
    {
        public NonNegativeDecimalValidationRule()
        {
            return;
        }

        public override ValidationResult Validate(object value, CultureInfo ci)
        {
            decimal d;

            if (!decimal.TryParse((string)value, ci, out d))
                return new ValidationResult(false, "Значение невозможно считать.");

            if (d < 0)
                return new ValidationResult(false, "Значение должно быть больше 0.");

            return ValidationResult.ValidResult;
        }
    }
}
