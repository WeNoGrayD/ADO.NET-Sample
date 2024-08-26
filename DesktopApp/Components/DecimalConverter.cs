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

namespace DesktopApp.Components
{
    public class DecimalConverter : IValueConverter
    {
        private string Format = "N2";

        //private NumberFormatInfo nfi = new CultureInfo("ru-RU").NumberFormat;

        public DecimalConverter()
        {
            return;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((decimal)value).ToString(Format, culture.NumberFormat);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return decimal.Parse((string)value, culture.NumberFormat);
        }
    }
}
