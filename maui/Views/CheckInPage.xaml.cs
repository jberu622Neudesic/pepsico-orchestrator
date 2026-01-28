using System.Collections.Specialized;
using MauiShellApp.Models;

namespace MauiShellApp.Views;

public partial class CheckInPage : ContentPage
{
	private NameValueCollection? _parameters;

	/// <summary>
	/// Constructor accepting query parameters from deep link (T029)
	/// </summary>
	public CheckInPage(NameValueCollection? parameters = null)
	{
		InitializeComponent();
		_parameters = parameters;

		// Display parameters on load
		DisplayParameters();
	}

	/// <summary>
	/// Populates labels with parameter values (T030)
	/// </summary>
	private void DisplayParameters()
	{
		if (_parameters == null || _parameters.Count == 0)
		{
			// No parameters provided (T037: handle empty case)
			UserIdLabel.Text = "User ID: (not provided)";
			LocationLabel.Text = "Location: (not provided)";
			EventLabel.Text = "Event: (not provided)";
			return;
		}

		// T031: Set label text for UserId, Location, Event
		var userId = _parameters["userId"];
		var location = _parameters["location"];
		var eventName = _parameters["event"];

		UserIdLabel.Text = !string.IsNullOrEmpty(userId) ? $"User ID: {userId}" : "User ID: (not provided)";
		LocationLabel.Text = !string.IsNullOrEmpty(location) ? $"Location: {location}" : "Location: (not provided)";
		EventLabel.Text = !string.IsNullOrEmpty(eventName) ? $"Event: {eventName}" : "Event: (not provided)";

		// T032: Extract and display custom fields
		var knownKeys = new HashSet<string> { "userId", "location", "event", "timestamp", "requestId", "returnUrl" };
		var customFields = new Dictionary<string, string>();

		foreach (string key in _parameters)
		{
			if (key != null && !knownKeys.Contains(key))
			{
				customFields[key] = _parameters[key] ?? string.Empty;
			}
		}

		if (customFields.Count > 0)
		{
			CustomFieldsHeader.IsVisible = true;

			foreach (var kvp in customFields)
			{
				var label = new Label
				{
					Text = $"{kvp.Key}: {kvp.Value}",
					FontSize = 16,
					LineBreakMode = LineBreakMode.WordWrap
				};
				CustomFieldsContainer.Children.Add(label);
			}
		}
	}

	/// <summary>
	/// Handles Done button click (T043)
	/// </summary>
	private async void OnDoneClicked(object sender, EventArgs e)
	{

		// Deep link mode - send handoff response
			await SendHandoffResponse();

		// // T044: Check launch mode
		// if (App.CurrentLaunchMode == LaunchMode.STANDALONE)
		// {
		// 	// T050: Standalone confirmation
		// 	await DisplayAlert("Success", "Check-in completed!", "OK");
		// }
		// else
		// {
		// 	// Deep link mode - send handoff response
		// 	await SendHandoffResponse();
		// }
	}

	/// <summary>
	/// Sends handoff response back to orchestrator (T045-T049)
	/// </summary>
	private async Task SendHandoffResponse()
	{
		try
		{
			
			// T046: Get orchestrator return URL
			var returnUrl = App.OrchestratorReturnUrl ?? "flnalauncher://app-selection";

			// T047: Build query string with response data
			var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			var message = Uri.EscapeDataString("Check-in completed successfully");
			var requestId = App.OriginalRequestId ?? string.Empty;

			var handoffUrl = $"{returnUrl}?status=SUCCESS&completedTimestamp={timestamp}&message={message}&originalRequestId={requestId}";

			// T048: Invoke orchestrator using Launcher
			//Launcher.CanOpenAsync() is being too strict when checking the full URL with query parameters. It's a known limitation in MAUI on iOS - the API validates the entire URL string and may reject URLs with query parameters, even though OpenAsync() can actually open them successfully.
			// Note: CanOpenAsync can return false negatives on iOS, so we try opening directly
			try
			{
				await Launcher.OpenAsync(handoffUrl);
			}
			catch (Exception launcherEx)
			{
				// T049: Handle error if orchestrator URL scheme not registered
				await DisplayAlert(
					"Handoff Error",
					$"Cannot open orchestrator URL scheme. The orchestrator app may not be installed.\n\nAttempted URL: {returnUrl}",
					"OK");
				Console.WriteLine($"[ERROR] Failed to launch URL: {launcherEx.Message}");
			}
		}
		catch (Exception ex)
		{
			// T049: Error handling
			await DisplayAlert(
				"Error",
				$"Failed to send handoff response: {ex.Message}",
				"OK");
		}
	}
}
