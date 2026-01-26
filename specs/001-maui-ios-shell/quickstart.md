# Quick Start: .NET MAUI iOS Shell Application

**Feature**: [001-maui-ios-shell](spec.md)  
**Date**: January 26, 2026  
**Target Audience**: Developers setting up the MAUI iOS shell project for the first time

## Prerequisites

Before starting, ensure you have:

- **macOS** (required for iOS development)
- **.NET SDK 8.0 or later** ([download](https://dotnet.microsoft.com/download))
- **Xcode 15.x or later** (latest stable version from Mac App Store)
- **Visual Studio for Mac** or **Visual Studio Code** with C# Dev Kit extension
- **MAUI workload** installed (see installation steps below)

## Step 1: Install .NET MAUI Workload

Open Terminal and run:

```bash
dotnet workload install maui
```

Verify installation:

```bash
dotnet workload list
```

You should see `maui` in the installed workloads list.

## Step 2: Create the MAUI Project

Navigate to the repository's `maui/` directory:

```bash
cd /path/to/pepsico-orchestrator/maui
```

Create a new MAUI app:

```bash
dotnet new maui -n MauiShellApp -f net10.0
cd MauiShellApp
```

**Note**: Using .NET 10.0 (latest stable). If newer version available, update framework version accordingly.

## Step 3: Configure iOS Deployment Target

Edit `MauiShellApp.csproj` and update the `SupportedOSPlatformVersion` for iOS to **16.0**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-ios</TargetFrameworks>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">16.0</SupportedOSPlatformVersion>
    <!-- Other properties -->
  </PropertyGroup>
</Project>
```

## Step 4: Register URL Scheme in Info.plist

Edit `Platforms/iOS/Info.plist` and add the URL scheme configuration:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <!-- Existing keys like UIDeviceFamily, etc. -->
    
    <!-- ADD THIS SECTION -->
    <key>CFBundleURLTypes</key>
    <array>
        <dict>
            <key>CFBundleURLName</key>
            <string>com.pepsico.mauiapp</string>
            <key>CFBundleURLSchemes</key>
            <array>
                <string>mauiapp</string>
            </array>
        </dict>
    </array>
</dict>
</plist>
```

**Explanation**:
- `CFBundleURLName`: Unique identifier for the URL scheme (reverse-domain notation)
- `CFBundleURLSchemes`: Array of URL schemes this app handles (we use `mauiapp`)

## Step 5: Implement Deep Link Handling in App.xaml.cs

Edit `App.xaml.cs` and override `OnAppLinkRequestReceived` plus add launch mode tracking:

```csharp
using System;
using System.Web;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MauiShellApp
{
    public partial class App : Application
    {
        // Track launch mode: DEEP_LINK or STANDALONE
        public static LaunchMode CurrentLaunchMode { get; private set; } = LaunchMode.STANDALONE;
        public static string? OrchestratorReturnUrl { get; private set; }
        public static string? OriginalRequestId { get; private set; }

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();
            // Standalone launch - set mode
            CurrentLaunchMode = LaunchMode.STANDALONE;
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            // Validate the deep link
            if (uri.Scheme != "mauiapp" || uri.Host != "check-in")
            {
                // Invalid deep link - show error or ignore
                System.Diagnostics.Debug.WriteLine($"Invalid deep link: {uri}");
                return;
            }

            // Set launch mode to DEEP_LINK
            CurrentLaunchMode = LaunchMode.DEEP_LINK;

            // Parse query parameters
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            
            // Extract orchestrator return URL and request ID
            OrchestratorReturnUrl = queryParams["returnUrl"];
            OriginalRequestId = queryParams["requestId"];
            
            // Navigate to CheckInPage with parameters
            MainPage = new NavigationPage(new CheckInPage(queryParams));
        }
    }

    public enum LaunchMode
    {
        STANDALONE,
        DEEP_LINK
    }
}
```

**Note**: You'll need to add a reference to `System.Web` for `HttpUtility`. Add this to `MauiShellApp.csproj`:

```xml
<ItemGroup>
  <Reference Include="System.Web" />
</ItemGroup>
```

## Step 6: Create CheckInPage

Create a new XAML page: `Views/CheckInPage.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MauiShellApp.Views.CheckInPage"
             Title="Check-In">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="15">
            
            <Label Text="Check-In Details" 
                   FontSize="24" 
                   FontAttributes="Bold"
                   Margin="0,0,0,20"/>
            
            <!-- User ID -->
            <Label Text="User ID:" FontAttributes="Bold" FontSize="16"/>
            <Label x:Name="UserIdLabel" 
                   Text="(not provided)" 
                   FontSize="16"
                   Margin="0,0,0,10"/>
            
            <!-- Location -->
            <Label Text="Location:" FontAttributes="Bold" FontSize="16"/>
            <Label x:Name="LocationLabel" 
                   Text="(not provided)" 
                   FontSize="16"
                   Margin="0,0,0,10"/>
            
            <!-- Event -->
            <Label Text="Event:" FontAttributes="Bold" FontSize="16"/>
            <Label x:Name="EventLabel" 
                   Text="(not provided)" 
                   FontSize="16"
                   Margin="0,0,0,10"/>
            
            <!-- Custom Fields (if any) -->
            <Label Text="Additional Parameters:" 
                   FontAttributes="Bold" 
                   FontSize="16"
                   Margin="0,20,0,0"/>
            <Label x:Name="CustomFieldsLabel" 
                   Text="None" 
                   FontSize="14"
                   TextColor="Gray"/>
            
            <!-- Done Button -->
            <Button x:Name="DoneButton"
                    Text="Done"
                    FontSize="18"
                    HeightRequest="50"
                    Margin="0,30,0,0"
                    Clicked="OnDoneClicked"/>
            
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

And the code-behind: `Views/CheckInPage.xaml.cs`

```csharp
using System.Collections.Specialized;
using System.Text;
using Microsoft.Maui.Controls;

namespace MauiShellApp.Views
{
    public partial class CheckInPage : ContentPage
    {
        public CheckInPage(NameValueCollection parameters)
        {
            InitializeComponent();
            DisplayParameters(parameters);
        }

        private void DisplayParameters(NameValueCollection parameters)
        {
            // Display known fields
            UserIdLabel.Text = parameters["userId"] ?? "(not provided)";
            LocationLabel.Text = parameters["location"] ?? "(not provided)";
            EventLabel.Text = parameters["event"] ?? "(not provided)";

            // Display custom fields (any parameter not in the known set)
            var knownKeys = new[] { "userId", "location", "event", "timestamp", "requestId", "returnUrl" };
            var customFields = new StringBuilder();

            foreach (string key in parameters.AllKeys)
            {
                if (!knownKeys.Contains(key))
                {
                    customFields.AppendLine($"{key}: {parameters[key]}");
                }
            }

            CustomFieldsLabel.Text = customFields.Length > 0 
                ? customFields.ToString().Trim() 
                : "None";
        }

        private async void OnDoneClicked(object sender, EventArgs e)
        {
            // Check launch mode
            if (App.CurrentLaunchMode == LaunchMode.DEEP_LINK)
            {
                // Send handoff response back to orchestrator
                await SendHandoffResponse("SUCCESS", "Check-in completed successfully");
            }
            else
            {
                // Standalone mode - just show confirmation
                await DisplayAlert("Success", "Check-in completed!", "OK");
                // Optionally close the app or navigate elsewhere
            }
        }

        private async Task SendHandoffResponse(string status, string message)
        {
            try
            {
                // Get orchestrator return URL (or use default)
                var returnUrl = App.OrchestratorReturnUrl ?? "reactnativeapp://handoff-complete";
                
                // Build handoff response URL
                var responseUrl = $"{returnUrl}?status={status}&completedTimestamp={DateTime.UtcNow:O}&message={Uri.EscapeDataString(message)}";
                
                if (!string.IsNullOrEmpty(App.OriginalRequestId))
                {
                    responseUrl += $"&originalRequestId={App.OriginalRequestId}";
                }

                // Invoke orchestrator via deep link
                var canOpen = await Launcher.CanOpenAsync(responseUrl);
                if (canOpen)
                {
                    await Launcher.OpenAsync(responseUrl);
                    // Close this app or show confirmation
                    await DisplayAlert("Success", "Returned to orchestrator", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Unable to return to orchestrator. URL scheme not registered.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to send handoff response: {ex.Message}", "OK");
            }
        }
    }
}
```

**Create the Views folder**:

```bash
mkdir Views
```

Then add the files above to the `Views/` directory.

## Step 7: Update Project File (if needed)

Ensure `CheckInPage.xaml` is included in the `.csproj`:

```xml
<ItemGroup>
  <MauiXaml Include="Views\CheckInPage.xaml" />
</ItemGroup>
```

MAUI should auto-detect XAML files, but verify if build errors occur.

## Step 8: Build and Run on iOS Simulator

Build the project:

```bash
dotnet build -f net10.0-ios
```

Run on iOS Simulator:

```bash
dotnet build -t:Run -f net10.0-ios
```

**Alternative (Visual Studio for Mac)**:
1. Open `MauiShellApp.sln` in Visual Studio for Mac
2. Select an iOS Simulator from the device dropdown
3. Click the **Run** button

## Step 9: Test Deep Link

Once the app is running on the simulator, test the deep link.

### Option 1: Safari (iOS Simulator)

1. Open Safari in the simulator
2. Type in the address bar: `mauiapp://check-in?userId=TestUser&location=Store5&event=Delivery`
3. Tap Go
4. iOS will prompt to open the app → Tap "Open"
5. The MAUI app should launch and display the parameters

### Option 2: Command Line (from macOS Terminal)

```bash
xcrun simctl openurl booted "mauiapp://check-in?userId=driver_001&location=Warehouse%20B&event=Delivery"
```

**Note**: `booted` refers to the currently running simulator. Ensure the simulator is open first.

### Option 3: Notes App (iOS Simulator)

1. Open Notes app in simulator
2. Create a new note
3. Type the deep link: `mauiapp://check-in?userId=ABC123&location=NYC`
4. Tap the link (it should become blue/clickable)
5. The app will launch with the parameters displayed

## Step 10: Verify Output

After triggering the deep link, the CheckInPage should display:

```
Check-In Details

User ID:
TestUser

Location:
Store5

Event:
Delivery

Additional Parameters:
None
```

If you pass custom fields (e.g., `?userId=ABC&customData=Value1`), they should appear under "Additional Parameters".

## Troubleshooting

### Issue: App doesn't launch when tapping deep link

**Solution**:
- Verify `Info.plist` has the `CFBundleURLSchemes` configuration
- Rebuild the project: `dotnet build -f net10.0-ios`
- Reinstall the app on the simulator (delete old version first)

### Issue: "HttpUtility not found" error

**Solution**:
- Add `<Reference Include="System.Web" />` to `MauiShellApp.csproj`
- Alternatively, use manual parsing (split on `&` and `=`) but URL decoding becomes complex

### Issue: Simulator says "No app to open this link"

**Solution**:
- Ensure the app is installed on the simulator
- Verify the URL scheme is spelled correctly (`mauiapp`, not `mauiApp` or `MAUIApp`)

### Issue: Build fails with "Provisioning profile required"

**Solution**:
- For simulator builds, provisioning is NOT required
- Ensure you're targeting a simulator, not a physical device
- If targeting device, create a development provisioning profile in Xcode

## Next Steps

Once the basic deep link handling is working:

1. **Add error handling**: Wrap `OnAppLinkRequestReceived` in try-catch to handle malformed URIs
2. **Improve UI**: Add styling, images, and better layout per Driver-Centric UX principles (44pt touch targets, larger fonts)
3. **Implement return handoff** (post-POC): Add a "Done" button that invokes the orchestrator's deep link with a `HandoffResponse`
4. **Add unit tests**: Test `CheckInRequest` parsing logic, parameter validation, edge cases

## Reference

- **Spec**: [spec.md](spec.md)
- **Data Model**: [data-model.md](data-model.md)
- **Contracts**: [contracts/](contracts/)
- **MAUI Documentation**: https://learn.microsoft.com/en-us/dotnet/maui/
- **iOS URL Schemes**: https://developer.apple.com/documentation/xcode/defining-a-custom-url-scheme-for-your-app
