using TrafficNode;
namespace 簡易的行控中心;

public partial class HomePage : ContentPage
{
    public HomePage(HomePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        if (!Traffic.IsInitialized)
        {
            Traffic.info = new string[] { "台北", "新北", "桃園", "新竹", "苗栗", "台中", "彰化" };
            int[] limitSpeeds = { 135, 135, 130, 125, 140, 150 };
            int[] lengths = { 2, 4, 4, 2, 2, 1 };
            int[] platform = { 4, 2, 2, 2, 2, 2, 4 };
            int[] prio = { 2, 2, 1, 0, 1, 1, 2 };
            for (int i = 0; i < 6; i++)
            {
                Traffic.traffics.Add(new Station(Traffic.info[i], platform[i], prio[i]));
                Traffic.traffics.Add(new Track(limitSpeeds[i], lengths[i]));
            }
            var lastStation = new Station(Traffic.info.Last(), platform.Last(), prio.Last());
            Traffic.traffics.Add(lastStation);
            Traffic.trains.Add(new TrafficNode.Train("3104", 0, "台北", "彰化"));
            Traffic.trains.Add(new TrafficNode.Train("3211", 0, "新北", "桃園"));
            Traffic.trains.Add(new TrafficNode.Train("3288", 0, "台中", "彰化"));
            Traffic.trains.Add(new TrafficNode.Train("3518", 0, "彰化", "新竹"));
            Traffic.trains.Add(new TrafficNode.Train("3704", 0, "桃園", "台北"));

            Traffic.IsInitialized = true;
        }
    }
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingPage());
    }
}