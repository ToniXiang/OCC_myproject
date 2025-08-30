using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Collections.Specialized;

namespace 簡易的行控中心.ViewModels;

public class EventPageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> TrainNames { get; } = new();
    public ObservableCollection<string> SelectedContacts { get; } = new();

    private string _eventTitle;
    public string EventTitle
    {
        get => _eventTitle;
        set { _eventTitle = value; OnPropertyChanged(); }
    }

    private string _selectedTrain;
    public string SelectedTrain
    {
        get => _selectedTrain;
        set { _selectedTrain = value; OnPropertyChanged(); }
    }

    private string _eventDetails;
    public string EventDetails
    {
        get => _eventDetails;
        set { _eventDetails = value; OnPropertyChanged(); }
    }

    public ICommand SubmitCommand { get; private set; }
    public ICommand ToggleContactCommand { get; }

    // Accept TrafficDataService via DI (optional) and populate TrainNames from Stations
    public EventPageViewModel(TrafficDataService dataService = null)
    {
        SubmitCommand = new Command(OnSubmit);
        ToggleContactCommand = new Command<string>(ToggleContact);

        // Fill with sample data from TrafficDataService if available at runtime.
        if (dataService != null)
        {
            foreach (var station in dataService.Stations)
            {
                // Station likely has a name property; fall back to ToString
                var nameProp = station.GetType().GetProperty("name");
                if (nameProp != null)
                {
                    var val = nameProp.GetValue(station) as string;
                    TrainNames.Add(string.IsNullOrEmpty(val) ? station.ToString() : val);
                }
                else
                {
                    TrainNames.Add(station.ToString());
                }
            }

            // Keep TrainNames in sync if Stations changes at runtime
            dataService.Stations.CollectionChanged += (s, e) =>
            {
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    TrainNames.Clear();
                    foreach (var station in dataService.Stations)
                    {
                        var nameProp = station.GetType().GetProperty("name");
                        if (nameProp != null)
                        {
                            var val = nameProp.GetValue(station) as string;
                            TrainNames.Add(string.IsNullOrEmpty(val) ? station.ToString() : val);
                        }
                        else
                        {
                            TrainNames.Add(station.ToString());
                        }
                    }
                    OnPropertyChanged(nameof(TrainNames));
                });
            };
        }
    }

    private void ToggleContact(string contact)
    {
        if (string.IsNullOrEmpty(contact)) return;
        if (SelectedContacts.Contains(contact))
            SelectedContacts.Remove(contact);
        else
            SelectedContacts.Add(contact);
        OnPropertyChanged(nameof(SelectedContacts));
    }

    public void SetSubmitCommand(Func<System.Threading.Tasks.Task> action)
    {
        SubmitCommand = new Command(async () => await action());
        OnPropertyChanged(nameof(SubmitCommand));
    }

    private void OnSubmit()
    {
        // Default behavior if not overridden by page.
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}