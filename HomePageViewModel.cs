using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using TrafficNode;
using CommunityToolkit.Mvvm.Messaging;
namespace 簡易的行控中心
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private string _currentDateTime;
        private bool _isRunning = false;
        private readonly System.Timers.Timer _timer;
        private readonly System.Timers.Timer _simulationTimer;
        public ObservableCollection<TrafficNode.Train> Trains { get; }
        public ObservableCollection<Station> Stations { get; }

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _simulationButtonText = "開啟模擬";
        public string SimulationButtonText
        {
            get => _simulationButtonText;
            set
            {
                if (_simulationButtonText != value)
                {
                    _simulationButtonText = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _simulationText = "已關閉模擬";
        public string SimulationText
        {
            get => _simulationText;
            set
            {
                if (_simulationText != value)
                {
                    _simulationText = value;
                    OnPropertyChanged();
                }
            }
        }
        public ICommand ToggleSimulationCommand { get; }
        public HomePageViewModel()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                CurrentDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            };
            _timer.Start();
            CurrentDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            ToggleSimulationCommand = new Command(ToggleSimulation);
            _simulationTimer = new System.Timers.Timer(1000);
            _simulationTimer.Elapsed += SimulationTimer_Tick;
        }
        private void ToggleSimulation()
        {
            _isRunning = !_isRunning;
            if (_isRunning)
            {
                SimulationButtonText = "關閉模擬";
                SimulationText = "正在模擬...";
                _simulationTimer.Start();
            }
            else
            {
                SimulationButtonText = "開啟模擬";
                SimulationText = $"模擬已暫停，最後執行於 {DateTime.Now.ToString("HH:mm:ss")}";
                _simulationTimer.Stop();
            }
        }
        private void SimulationTimer_Tick(object sender, ElapsedEventArgs e)
        {
            // 若要更新 UI，需切回主執行緒
            Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                foreach (var train in Traffic.trains)
                {
                    if ((train.cur + 2) % 2 == 0)
                    {
                        if (train.cur == train.destination)
                        {
                            train.stats = "已到達終點站";
                        }
                        else
                        {
                            train.stats = "等待出站";
                            bool check = false;
                            int rd = 0;
                            foreach (var other_train in Traffic.trains)
                            {
                                if (train == other_train) continue;
                                if (train.next == other_train.next)
                                {
                                    if (train.next + train.cur == other_train.cur)
                                    {
                                        check = true;
                                        break;
                                    }
                                    else if (train.next * 2 + train.cur == other_train.cur)
                                    {
                                        rd++;
                                    }
                                }
                            }
                            if (train.wait == -2 || check || rd >= ((Station)Traffic.traffics[train.cur + train.next * 2]).platform)
                            {
                                continue;
                            }
                            if (train.cur + train.next >= 0 && train.cur + train.next < Traffic.traffics.Count && ++train.wait >= 10)
                            {
                                train.cur += train.next;
                                train.wait = 0;
                                train.stats = "正在行駛中";
                            }
                        }
                    }
                    else
                    {
                        Track track = (Track)Traffic.traffics[train.cur];
                        train.move();
                        double end_u = train.speed / 3.6;
                        double end_a = 2.0;
                        double s = end_u * end_u / (2 * end_a);
                        if (s >= (track.length - train.length) * 1E3)
                        {
                            if (((Station)Traffic.traffics[train.cur + train.next]).priority < train.priority)
                            {
                                if (train.length >= track.length)
                                {
                                    train.length = 0;
                                    train.wait = 0;
                                    train.cur += train.next * 2;
                                }
                            }
                            else if (train.length > track.length && train.speed <= 4)
                            {
                                train.length = 0;
                                train.speed = 0;
                                train.cur += train.next;
                            }
                            else
                            {
                                end_u = end_u + (-end_a) * 1.0;
                                train.speed = end_u * 3.6 < 4.0 ? 4.0 : end_u * 3.6;
                                train.stats = "正在進站中";
                            }
                        }
                        else if (train.speed < 110 && train.wait != -2)
                        {
                            double start_u = train.speed / 3.6;
                            double start_a = 2.5;
                            start_u = start_u + start_a;
                            train.speed = start_u * 3.6 > 110.0 ? 110.0 : start_u * 3.6;
                            train.wait += 1;
                        }
                        else if (train.speed > track.limitspeed)
                        {
                            train.stats = $"速度過快，請減速!";
                        }
                    }
                }
                WeakReferenceMessenger.Default.Send(new SimulationUpdatedMessage());
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}