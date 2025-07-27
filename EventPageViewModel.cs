using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace 簡易的行控中心;

public class EventPageViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Dictionary<string, List<string>> _errors = new();
    public string? EventTitleError => GetErrors(nameof(EventTitle)).Cast<string>().FirstOrDefault();
    public string? SelectedTrainError => GetErrors(nameof(SelectedTrain)).Cast<string>().FirstOrDefault();
    public string? EventDetailsError => GetErrors(nameof(EventDetails)).Cast<string>().FirstOrDefault();

    private string? _eventTitle;
    public string? EventTitle
    {
        get => _eventTitle;
        set
        {
            if (_eventTitle != value)
            {
                _eventTitle = value;
                OnPropertyChanged();
                ValidateProperty(value, nameof(EventTitle));
            }
        }
    }

    private string? _selectedTrain;
    public string? SelectedTrain
    {
        get => _selectedTrain;
        set
        {
            if (_selectedTrain != value)
            {
                _selectedTrain = value;
                OnPropertyChanged();
                ValidateProperty(value, nameof(SelectedTrain));
            }
        }
    }

    public ObservableCollection<string> SelectedContacts { get; set; } = new();

    private string? _eventDetails;
    public string? EventDetails
    {
        get => _eventDetails;
        set
        {
            if (_eventDetails != value)
            {
                _eventDetails = value;
                OnPropertyChanged();
                ValidateProperty(value, nameof(EventDetails));
            }
        }
    }

    public ObservableCollection<string> TrainNames { get; set; } = new() { "台北", "新北", "桃園", "新竹", "苗栗", "台中", "彰化" };
    public ObservableCollection<string> Contacts { get; } = new() { "消防局", "警察局", "救援隊", "醫院", "維修人員", "站務人員" };

    public ICommand? SubmitCommand { get; private set; }
    public void SetSubmitCommand(Func<Task> submitAction)
    {
        SubmitCommand = new Command(async () =>
        {
            ValidateAll();
            if (!HasErrors)
            {
                await submitAction();
                ClearForm();
            }
        });
        OnPropertyChanged(nameof(SubmitCommand));
    }
    public bool HasErrors => _errors.Any();
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public System.Collections.IEnumerable GetErrors(string? propertyName)
        => propertyName != null && _errors.ContainsKey(propertyName) ? _errors[propertyName] : Enumerable.Empty<string>();
    private void ClearForm()
    {
        EventTitle = string.Empty;
        SelectedTrain = null;
        EventDetails = string.Empty;
        SelectedContacts.Clear();
        _errors.Clear();
        OnPropertyChanged(nameof(EventTitleError));
        OnPropertyChanged(nameof(SelectedTrainError));
        OnPropertyChanged(nameof(EventDetailsError));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(EventTitle)));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SelectedTrain)));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(EventDetails)));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SelectedContacts)));
    }
    private void ValidateProperty(object? value, string propertyName)
    {
        switch (propertyName)
        {
            case nameof(EventTitle):
                SetErrors(propertyName, string.IsNullOrWhiteSpace((string?)value) ? new[] { "事件標題為必填" } : null);
                break;
            case nameof(SelectedTrain):
                SetErrors(propertyName, string.IsNullOrWhiteSpace((string?)value) ? new[] { "請選擇車站" } : null);
                break;
            case nameof(EventDetails):
                SetErrors(propertyName, string.IsNullOrWhiteSpace((string?)value) ? new[] { "詳細內容為必填" } : null);
                break;
            case nameof(SelectedContacts):
                SetErrors(propertyName, SelectedContacts == null || SelectedContacts.Count == 0 ? new[] { "請選擇至少一個聯絡單位" } : null);
                break;
        }
    }

    private void ValidateAll()
    {
        ValidateProperty(EventTitle, nameof(EventTitle));
        ValidateProperty(SelectedTrain, nameof(SelectedTrain));
        ValidateProperty(EventDetails, nameof(EventDetails));
    }

    private void SetErrors(string propertyName, IEnumerable<string>? errors)
    {
        if (errors != null && errors.Any())
            _errors[propertyName] = errors.ToList();
        else
            _errors.Remove(propertyName);
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        switch (propertyName)
        {
            case nameof(EventTitle):
                OnPropertyChanged(nameof(EventTitleError));
                break;
            case nameof(SelectedTrain):
                OnPropertyChanged(nameof(SelectedTrainError));
                break; 
            case nameof(EventDetails):
                OnPropertyChanged(nameof(EventDetailsError));
                break;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}