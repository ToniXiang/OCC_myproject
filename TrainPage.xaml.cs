namespace 簡易的行控中心;

public partial class TrainPage : ContentPage
{
	public TrainPage(TrafficDataService dataService)
	{
		InitializeComponent();
        BindingContext = new TrainPageViewModel(dataService);
    }
}