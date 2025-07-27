using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrafficNode;

public class TrainPageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<string> TrainNames { get; }
    public ObservableCollection<Train> Trains { get; }
    private Train _selectedTrain;
    public Train SelectedTrain
    {
        get => _selectedTrain;
        set
        {
            if (_selectedTrain != value)
            {
                _selectedTrain = value;
                OnPropertyChanged(nameof(SelectedTrain));
            }
        }
    }
    private int _speed;
    public int speed
    {
        get => _speed;
        set
        {
            if (_speed != value)
            {
                _speed = value;
                OnPropertyChanged();
            }
        }
    }
    private int _destinationNAME;
    public int destinationNAME
    {
        get => _destinationNAME;
        set
        {
            if (_destinationNAME != value)
            {
                _destinationNAME = value;
                OnPropertyChanged();
            }
        }
    }
    public ICommand AccelerateCommand { get; }
    public ICommand DecelerateCommand { get; }
    public ICommand StartCommand { get; }

    public TrainPageViewModel(TrafficDataService dataService)
    {
        Trains = dataService.Trains;
        TrainNames = new ObservableCollection<string>(Trains.Select(t => t.name));
        WeakReferenceMessenger.Default.Register<SimulationUpdatedMessage>(this, (r, m) => UpdateTrainList());
        SelectedTrain = Trains[0];
        AccelerateCommand = new Command(Accelerate);
        DecelerateCommand = new Command(Decelerate);
        StartCommand = new Command(Start);
    }

    private void Accelerate()
    {
        if (SelectedTrain != null)
        {
            SelectedTrain.speed += 10;
        }
    }

    private void Decelerate()
    {
        if (SelectedTrain != null && SelectedTrain.speed > 0)
        {
            SelectedTrain.speed -= 10;
            if (SelectedTrain.speed < 0) SelectedTrain.speed = 0;
        }
    }

    private void Start()
    {
        // 尚未完成
    }
    private void UpdateTrainList()
    {
        OnPropertyChanged(nameof(TrainNames));
        OnPropertyChanged(nameof(Trains));
        OnPropertyChanged(nameof(SelectedTrain));
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}