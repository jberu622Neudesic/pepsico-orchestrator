using Foundation;
using UIKit;

namespace MauiShellApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
	{
		if (url?.AbsoluteString != null)
		{
			var uri = new Uri(url.AbsoluteString);
			(IPlatformApplication.Current?.Application as App)?.SendOnAppLinkRequestReceived(uri);
		}
		return true;
	}
}
