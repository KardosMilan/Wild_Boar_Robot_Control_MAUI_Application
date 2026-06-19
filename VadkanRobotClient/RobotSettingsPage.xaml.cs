using VadkanRobotClient.ViewModels;
using System.Diagnostics;

namespace VadkanRobotClient;

public partial class RobotSettingsPage : ContentPage
{
    public RobotSettingsPage()
	{
		InitializeComponent();
        BindingContext = new RobotSettingsViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is RobotSettingsViewModel vm)
        {
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _ = vm.LoadSettings();
            });
        }
    }
}