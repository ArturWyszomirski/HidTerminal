namespace HidTerminal.Views;

public partial class DeviceWatcherView : ContentPage
{
	public DeviceWatcherView(DeviceWatcherViewModel deviceWatcherViewModel)
	{
		InitializeComponent();

		BindingContext = deviceWatcherViewModel;
	}
}