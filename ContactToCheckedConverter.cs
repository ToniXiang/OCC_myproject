using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace 簡易的行控中心;

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
        var isChecked = (bool)value;
        var contact = parameter as string;
        var selectedContacts = App.Current?.MainPage?.BindingContext as EventPageViewModel;
        if (selectedContacts == null || contact == null)
            return null;

        // You should get the actual SelectedContacts collection from the binding context
        // Instead, use a Command or event to update the collection in the ViewModel
        // This is a limitation of using converters for collection updates

        return null;
    }
}