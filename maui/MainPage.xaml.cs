using MauiShellApp.Views;
using Microsoft.Maui.ApplicationModel;

namespace MauiShellApp;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

		//testing navigation to CheckInPage without parameters
		var checkInPage = new CheckInPage(null);
		Application.Current.MainPage = new NavigationPage(checkInPage);


	}

	private async void OnOpenAppSelectionClicked(object? sender, EventArgs e)
	{
		await Launcher.OpenAsync(new Uri("flnalauncher://app-selection"));
	}
}
