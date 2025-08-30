using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace 簡易的行控中心.Converters;

public class ContactToCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedContacts = value as ObservableCollection<string>;
        var contact = parameter as string;
        return selectedContacts != null && contact != null && selectedContacts.Contains(contact);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ContactToCheckedConverter does not support ConvertBack. Use a command or CheckedChanged handler to modify the collection in the ViewModel.");
    }
}