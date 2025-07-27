using System.Text;

namespace 簡易的行控中心;

public partial class EventPage : ContentPage
{
    public EventPage()
    {
        InitializeComponent();
        BindingContext = new EventPageViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is EventPageViewModel vm)
        {
            vm.SetSubmitCommand(async () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine($"事件: {vm.EventTitle}");
                sb.AppendLine($"車站: {vm.SelectedTrain}");
                sb.AppendLine($"聯絡: {string.Join(", ", vm.SelectedContacts)}");
                sb.AppendLine($"詳細內容: {vm.EventDetails}");

                await DisplayAlert("送出結果", sb.ToString(), "OK");
            });
        }
    }
    private void OnContactCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is EventPageViewModel vm && sender is CheckBox cb && cb.BindingContext is string contact)
        {
            if (e.Value)
            {
                if (!vm.SelectedContacts.Contains(contact))
                    vm.SelectedContacts.Add(contact);
            }
            else
            {
                if (vm.SelectedContacts.Contains(contact))
                    vm.SelectedContacts.Remove(contact);
            }
        }
    }
}
