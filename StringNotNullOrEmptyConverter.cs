﻿using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace 簡易的行控中心.Converters
{
    public class StringNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value as string);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}