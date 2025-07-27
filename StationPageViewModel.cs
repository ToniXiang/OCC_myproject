using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using TrafficNode;
using CommunityToolkit.Mvvm.Messaging;
namespace 簡易的行控中心
{
    public class StationPageViewModel : INotifyPropertyChanged
    {
        private Station _selectedStation;
        public Station SelectedStation
        {
            get => _selectedStation;
            set
            {
                if (_selectedStation != value)
                {
                    _selectedStation = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedStation));
                    UpdateTimetable();
                }
            }
        }
        public ObservableCollection<TrafficNode.Train> Trains { get; }
        public ObservableCollection<Station> Stations { get; }
        public StationPageViewModel(TrafficDataService dataService)
        {
            Trains = dataService.Trains;
            Stations = dataService.Stations;
            WeakReferenceMessenger.Default.Register<SimulationUpdatedMessage>(this, (r, m) => UpdateTimetable());
            if (Stations.Count > 0)
                SelectedStation = Stations[0];
        }
        #region 更新時間表
        private void UpdateTimetable()
        {
            Timetable.Clear();
            foreach (TrafficNode.Train train in Traffic.trains)
            {
                int cur_index = train.cur;
                double total_length = -train.length;
                int wait = 0;
                var currentStationName = SelectedStation?.name;
                if ((cur_index + 2) % 2 == 0 && cur_index == Traffic.GetIndex(currentStationName))
                {
                    Timetable.Add(new TimetableEntry
                    {
                        TrainNumber = train.name,
                        Status = "等待發車",
                        Destination = ((Station)Traffic.traffics[train.destination]).name,
                        TrainState = train.stats
                    });
                    continue;
                }
                while (cur_index >= 0 && cur_index < Traffic.traffics.Count)
                {
                    if ((cur_index + 2) % 2 == 0)
                    {
                        var station = (Station)Traffic.traffics[cur_index];
                        if (station.name == currentStationName && station.priority >= train.priority)
                        {
                            Timetable.Add(new TimetableEntry
                            {
                                TrainNumber = train.name,
                                Status = Traffic.GetTime(total_length, wait),
                                Destination = ((Station)Traffic.traffics[train.destination]).name,
                                TrainState = train.stats
                            });
                            break;
                        }
                        if (cur_index == train.destination)
                        {
                            break;
                        }
                        else if (train.priority <= station.priority) wait += 10;
                    }
                    else
                    {
                        total_length += ((Track)Traffic.traffics[cur_index]).length;
                    }
                    cur_index += train.next;
                }
            }
            OnPropertyChanged(nameof(Timetable));
        }
        #endregion
        public ObservableCollection<TimetableEntry> Timetable { get; } = new();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class TimetableEntry 
    {
        public string TrainNumber { get; set; }
        public string Status { get; set; }
        public string Destination { get; set; }
        public string TrainState { get; set; }
    }
}