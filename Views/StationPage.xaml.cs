using 簡易的行控中心.ViewModels;

namespace 簡易的行控中心;

public partial class StationPage : ContentPage
{
	public StationPage(TrafficDataService dataService)
	{
		InitializeComponent();
        BindingContext = new StationPageViewModel(dataService);
    }
}