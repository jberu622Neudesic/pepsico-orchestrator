using Microsoft.Extensions.DependencyInjection;
using MauiShellApp.Views;
using MauiShellApp.Models;
using System.Web;

namespace MauiShellApp;

public partial class App : Application
{
	public static LaunchMode CurrentLaunchMode { get; set; } = LaunchMode.STANDALONE;
	public static string? OrchestratorReturnUrl { get; set; }
	public static string? OriginalRequestId { get; set; }

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	protected override void OnStart()
	{
		base.OnStart();
		// Default to STANDALONE mode when app launches normally
		CurrentLaunchMode = LaunchMode.STANDALONE;
	}

	/// <summary>
	/// Handles deep link activation (T016-T021, T056-T059: Error handling)
	/// </summary>
	protected override void OnAppLinkRequestReceived(Uri uri)
	{
		base.OnAppLinkRequestReceived(uri);

		try
		{
			// T017: Validate URI scheme and host
			if (uri.Scheme != "mauiapp" || uri.Host != "check-in")
			{
				// T058: Show user-friendly error message
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await Application.Current.MainPage.DisplayAlert(
						"Invalid Deep Link",
						$"Expected 'mauiapp://check-in' but received '{uri.Scheme}://{uri.Host}'",
						"OK");
				});
				
				// T059: Log technical details
				Console.WriteLine($"[ERROR] Invalid deep link: {uri}");
				return;
			}

			// T018: Set launch mode to DEEP_LINK
			CurrentLaunchMode = LaunchMode.DEEP_LINK;

			// T019: Parse query parameters
			var query = HttpUtility.ParseQueryString(uri.Query);
			var parameters = new Dictionary<string, string>();
			foreach (string key in query)
			{
				if (key != null)
				{
					parameters[key] = query[key] ?? string.Empty;
				}
			}

			// T020: Extract returnUrl and requestId
			if (parameters.ContainsKey("returnUrl"))
			{
				OrchestratorReturnUrl = parameters["returnUrl"];
			}
			if (parameters.ContainsKey("requestId"))
			{
				OriginalRequestId = parameters["requestId"];
			}

			// T021: Navigate to CheckInPage with parameters
			MainThread.BeginInvokeOnMainThread(() =>
			{
				var checkInPage = new CheckInPage(query);
				Application.Current.MainPage = new NavigationPage(checkInPage);
			});
		}
		catch (Exception ex)
		{
			// T056-T057: Create ErrorState and wrap in try-catch
			var errorState = new ErrorState("DEEP_LINK_PARSE_ERROR", "Failed to process deep link")
			{
				TechnicalDetails = ex.ToString(),
				RecoveryAction = "Please try the deep link again or contact support"
			};

			// T058: Show user-friendly error
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error",
					errorState.UserMessage,
					"OK");
			});

			// T059: Log technical details
			Console.WriteLine($"[ERROR] Deep link processing failed: {errorState.TechnicalDetails}");
		}
	}
}