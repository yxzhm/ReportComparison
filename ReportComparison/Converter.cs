using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReportComparison
{
    internal class RedValues : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            if (string.IsNullOrEmpty(value.ToString())) return Brushes.Red;

            if (value.ToString() == "0" || value.ToString() == "0.00") return Brushes.Black;

            return Brushes.Red;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class CompareEnable : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2) return false;
            if (values.Any(x => x == null || x == DependencyProperty.UnsetValue)) return false;

            return !string.IsNullOrEmpty(values[0].ToString()) && !string.IsNullOrEmpty(values[1].ToString());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
