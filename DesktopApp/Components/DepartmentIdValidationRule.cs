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
    public class DepartmentIdValidationRule : ValidationRule
    {
        public DepartmentIdValidationRule()
        {
            return;
        }

        public override ValidationResult Validate(object value, CultureInfo ci)
        {
            if (((DepartmentModel)value).Equals(DepartmentModel.Null()))
                return new ValidationResult(false, "Сотрудник должен быть назначен в подразделение.");

            return ValidationResult.ValidResult;
        }
    }
}
