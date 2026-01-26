# Research: .NET MAUI iOS Deep Linking

**Feature**: [001-maui-ios-shell](spec.md)  
**Date**: January 26, 2026  
**Phase**: Phase 0 - Technical Research

## Research Tasks Completed

1. ✅ MAUI deep linking implementation patterns for iOS
2. ✅ URL scheme registration in Info.plist via MAUI
3. ✅ Query parameter parsing from deep link URIs
4. ✅ App lifecycle handling (cold start, warm start, foreground)
5. ✅ MAUI project structure and file organization
6. ✅ iOS-specific MAUI configuration requirements

## Decision 1: Deep Link Handling Approach

**Decision**: Use MAUI's `App.OnAppLinkRequestReceived` method combined with iOS platform-specific Info.plist configuration

**Rationale**:
- MAUI provides cross-platform app link handling via `Application.OnAppLinkRequestReceived` override
- This method receives a `Uri` object containing the full deep link URL with query parameters
- Works consistently across cold start, warm start, and foreground scenarios
- Built-in to MAUI framework, no additional packages required
- iOS-specific URL scheme registration happens in `Platforms/iOS/Info.plist`

**Alternatives Considered**:
- **Platform-specific AppDelegate override**: Rejected because MAUI's abstraction layer already handles this and provides cross-platform API
- **Third-party deep linking library (e.g., Branch, Firebase Dynamic Links)**: Rejected as overkill for simple URL scheme handling; adds unnecessary dependencies
- **Custom URI routing framework**: Rejected because the requirement is simple (one URL scheme, one route); MAUI's built-in support is sufficient

**Implementation Pattern**:
```csharp
// In App.xaml.cs
protected override void OnAppLinkRequestReceived(Uri uri)
{
    base.OnAppLinkRequestReceived(uri);
    
    // uri.Scheme = "mauiapp"
    // uri.Host = "check-in"
    // uri.Query = "?userId=123&location=Store5"
    
    // Parse query parameters and navigate to CheckInPage
}
```

## Decision 2: URL Scheme Registration

**Decision**: Configure `CFBundleURLSchemes` in `Platforms/iOS/Info.plist` with scheme `mauiapp`

**Rationale**:
- iOS requires URL scheme registration in Info.plist for the system to route deep links to the app
- MAUI projects have platform-specific folders; iOS configuration goes in `Platforms/iOS/Info.plist`
- The scheme `mauiapp` must be globally unique (no conflicts with other apps on device)
- The URL structure `mauiapp://check-in?params` breaks down as:
  - Scheme: `mauiapp` (registered in Info.plist)
  - Host: `check-in` (parsed in app code)
  - Query: `?params` (parsed in app code)

**Alternatives Considered**:
- **Universal Links (Associated Domains)**: Rejected because they require a web domain, HTTPS endpoint, and apple-app-site-association file; excessive for POC
- **Generic "app" or "myapp" scheme**: Rejected in favor of descriptive `mauiapp` to align with project naming
- **Multiple URL schemes**: Out of scope; only `mauiapp://check-in` required per spec

**Configuration Example**:
```xml
<!-- Platforms/iOS/Info.plist -->
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
```

## Decision 3: Query Parameter Parsing

**Decision**: Use `System.Web.HttpUtility.ParseQueryString` from `System.Web` assembly for robust URL parsing

**Rationale**:
- `HttpUtility.ParseQueryString` handles URL decoding automatically (e.g., `%20` → space)
- Returns a `NameValueCollection` with all query parameters as key-value pairs
- Handles edge cases: duplicate keys, empty values, special characters
- Standard .NET library, no third-party dependencies
- Works with the `Uri.Query` property from `OnAppLinkRequestReceived`

**Alternatives Considered**:
- **Manual string splitting** (e.g., `uri.Query.Split('&')`): Rejected because it's error-prone, doesn't handle URL encoding, and requires custom parsing logic
- **LINQ query string parsing**: Rejected as less readable and still requires manual decoding
- **Third-party NuGet package** (e.g., Flurl): Rejected as unnecessary dependency for simple query parsing

**Implementation Pattern**:
```csharp
using System.Web;

var queryParams = HttpUtility.ParseQueryString(uri.Query);
string userId = queryParams["userId"];      // "123"
string location = queryParams["location"];  // "Store5"
```

**Note**: Requires adding `System.Web` reference to the MAUI project (available in .NET standard libraries).

## Decision 4: App Lifecycle Handling

**Decision**: MAUI's `OnAppLinkRequestReceived` automatically handles all three scenarios (cold start, warm start, foreground) without additional code

**Rationale**:
- iOS invokes the app link handler regardless of app state
- MAUI framework ensures `OnAppLinkRequestReceived` is called after `OnStart` (cold) or `OnResume` (warm/foreground)
- Navigation to `CheckInPage` can be performed directly in the handler using `MainPage = new NavigationPage(new CheckInPage(params))`
- No need for state management or queuing; deep link is processed immediately

**Alternatives Considered**:
- **Separate handlers for each lifecycle state**: Rejected because MAUI abstracts this complexity
- **Queue deep link if app is busy**: Out of scope for POC; accept that new deep link always navigates (interruption is acceptable per edge case discussion)
- **AppDelegate OnOpenUrl override**: Rejected because MAUI's cross-platform API is preferred

**Edge Case Handling**:
- **Rapid successive deep links**: Last link wins; no queuing mechanism
- **Deep link during navigation**: Current navigation is interrupted; new CheckInPage is pushed
- **Malformed URLs**: `Uri` parsing will throw; wrap in try-catch and show error page (to be designed in Phase 1)

## Decision 5: MAUI Project Structure

**Decision**: Use standard MAUI single-project structure with platform-specific folders for iOS configuration

**Rationale**:
- .NET MAUI uses a single `.csproj` file with platform-specific subfolders (`Platforms/iOS`, `Platforms/Android`, etc.)
- Shared code (Pages, ViewModels, Services) lives in the root or organized folders (e.g., `Views/`, `Services/`)
- Platform-specific code (Info.plist, entitlements) lives in `Platforms/iOS/`
- This structure is the default for `dotnet new maui` and aligns with Microsoft documentation

**Alternatives Considered**:
- **Xamarin.Forms legacy structure**: Rejected; Xamarin is deprecated, MAUI is the current framework
- **Multi-project solution** (separate iOS/Android projects): Rejected; MAUI's single-project model simplifies build and dependencies
- **Custom folder organization**: Rejected for POC; stick to defaults for developer familiarity

**Folder Layout**:
```
maui/
├── MauiShellApp.csproj       # Single project file
├── App.xaml / App.xaml.cs    # Application entry point, deep link handler
├── AppShell.xaml             # Shell navigation (if using Shell, optional for POC)
├── MainPage.xaml             # Default landing page (may not be used if deep link launches first)
├── Views/
│   └── CheckInPage.xaml      # Check-in screen that displays parameters
├── Platforms/
│   └── iOS/
│       ├── Info.plist        # URL scheme registration
│       ├── Entitlements.plist
│       └── AppDelegate.cs    # iOS-specific startup (MAUI auto-generates)
└── Resources/                # Images, fonts, etc.
```

## Decision 6: iOS-Specific Configuration Requirements

**Decision**: Minimum iOS deployment target is iOS 16.0, configured in `.csproj` via `<SupportedOSPlatformVersion>`

**Rationale**:
- Constitution Principle IV requires iOS 16+ for modern API availability
- MAUI .NET 8/9 officially supports iOS 15+ but recommends iOS 16+ for best compatibility
- iOS 16 is widely deployed (released September 2022); acceptable minimum for POC
- Higher versions (iOS 17/18) reduce device compatibility without added value for this feature

**Alternatives Considered**:
- **iOS 15.0 minimum**: Rejected to align with constitution requirement of iOS 16+
- **iOS 17.0 minimum**: Rejected; unnecessarily restricts testing devices
- **No minimum specified**: Rejected; MAUI defaults to iOS 15, which violates constitution

**Configuration**:
```xml
<!-- MauiShellApp.csproj -->
<PropertyGroup>
    <TargetFrameworks>net8.0-ios</TargetFrameworks>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">16.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

**Additional iOS Requirements**:
- **Xcode version**: Latest stable (Xcode 15.x or newer as of Jan 2026)
- **Provisioning**: Development provisioning profile acceptable for POC (no App Store distribution)
- **Entitlements**: None required for basic URL scheme handling (Associated Domains only needed for Universal Links)

## Technology Stack Summary

Based on research, the final technology choices:

| Category | Technology | Version/Details |
|----------|-----------|-----------------|
| **Framework** | .NET MAUI | Latest with .NET 10.0 SDK |
| **Language** | C# | Version 14 (with .NET 10) |
| **UI Framework** | MAUI Controls (XAML) | ContentPage, Label, StackLayout for simple check-in UI |
| **Deep Linking** | `OnAppLinkRequestReceived` | Built-in MAUI app link handling |
| **URL Parsing** | `System.Web.HttpUtility` | Standard .NET library for query parameter parsing |
| **iOS Target** | iOS 16.0+ | Deployment target in .csproj |
| **Build Tools** | Xcode, dotnet CLI | Xcode for iOS build, `dotnet build -f net10.0-ios` |
| **Testing** | Manual | Deep link testing via Safari, Notes app, or `xcrun simctl openurl` |

## Open Questions & Next Steps

**Resolved** (no further research needed):
- ✅ How to register URL scheme? → Info.plist `CFBundleURLSchemes`
- ✅ How to handle deep links? → `OnAppLinkRequestReceived` override
- ✅ How to parse parameters? → `HttpUtility.ParseQueryString`
- ✅ What .NET version? → .NET 10.0 (latest stable)

**Deferred to Phase 1**:
- Return handoff protocol (success/failure/cancel states) → contracts/handoff-response.json
- Error handling for malformed URLs → data-model.md "ErrorState" entity
- UI styling and layout specifics → quickstart.md will show basic XAML structure

**Next Phase**:
Proceed to **Phase 1: Design & Contracts** to define:
1. Data model (CheckInRequest entity, parameters schema)
2. Handoff response contract (for returning to orchestrator in future)
3. Quickstart guide (minimal MAUI project setup steps)
