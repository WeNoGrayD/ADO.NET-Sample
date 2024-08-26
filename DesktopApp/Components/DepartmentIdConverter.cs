using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using DesktopApp.Models;

namespace DesktopApp.Components
{
    public class DepartmentIdConverter : IValueConverter
    {
        private DesktopApp.ViewModels.DepartmentsViewModel _departmentsViewModel;

        public DepartmentIdConverter(
            DesktopApp.ViewModels.DepartmentsViewModel departmentsViewModel)
        {
            _departmentsViewModel = departmentsViewModel;

            return;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int depId = (int)value;

            return _departmentsViewModel.FindDepartmentById(depId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DepartmentModel)value).Id;
        }
    }
}
