using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace 簡易的行控中心.Converters
{
    public class PriorityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                0 => "低(Low)",
                1 => "中(Normal)",
                2 => "高(High)",
                _ => $"未知({value})"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "低(Low)" => 0,
                "中(Normal)" => 1,
                "高(High)" => 2,
                _ => 0
            };
        }
    }
}