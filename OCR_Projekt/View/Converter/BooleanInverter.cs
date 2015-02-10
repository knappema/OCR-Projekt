using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace OCR_Projekt.View.Converter
{
    /// <summary>
    /// Inverts a Boolean Value
    /// </summary>
    public class BooleanInverter : IValueConverter
    {
        private object Invert(object value)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;
            bool objValue = (bool)value;
            return !objValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Invert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
