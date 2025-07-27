using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TrafficNode;

public class TrafficDataService : INotifyPropertyChanged
{
    public ObservableCollection<Train> Trains { get; } = new();
    public ObservableCollection<Station> Stations { get; } = new();
    public TrafficDataService()
    {
        foreach (var train in Traffic.trains)
            Trains.Add(train);
        foreach (var node in Traffic.traffics)
            if (node is Station station)
                Stations.Add(station);
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}