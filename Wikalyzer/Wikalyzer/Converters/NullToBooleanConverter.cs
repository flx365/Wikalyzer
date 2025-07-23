using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Wikalyzer.Converters
{
    /// <summary>
    /// Konvertiert null/empty-Strings zu false, ansonsten true.
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // für string oder null generell
            if (value is string s)
                return !string.IsNullOrEmpty(s);
            return value != null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}