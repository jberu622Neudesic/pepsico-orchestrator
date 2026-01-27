# How to Find Deep Link Protocol (URL Scheme) for an Xcode Swift App

## Method 1: Check Info.plist File

The URL scheme is defined in the app's `Info.plist` file under the `CFBundleURLTypes` key.

### Steps:
1. Open the Xcode project
2. Navigate to your app target's `Info.plist` file (usually in the project navigator)
3. Look for `CFBundleURLTypes` or "URL Types" in the plist
4. Expand it to see `CFBundleURLSchemes` - this contains your URL scheme(s)

### Example in Info.plist:
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleTypeRole</key>
        <string>Editor</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>myapp</string>  <!-- This is your URL scheme -->
        </array>
    </dict>
</array>
```

In this example, the deep link would be: `myapp://`

## Method 2: Using Xcode UI

1. Open your Xcode project
2. Select your app target in the Project Navigator
3. Go to the **Info** tab
4. Scroll down to find **URL Types** section
5. Expand it to see the URL Schemes

## Method 3: Check AppDelegate.swift

Sometimes the URL scheme handling is visible in the AppDelegate:

```swift
func application(_ app: UIApplication, 
                 open url: URL, 
                 options: [UIApplication.OpenURLOptionsKey : Any] = [:]) -> Bool {
    // Handle deep link here
    // The URL scheme is what comes before "://"
    // e.g., if URL is "myapp://path", scheme is "myapp"
    return true
}
```

## Method 4: Check if App is Already Installed

If the app is already installed on a device:

1. Open Terminal
2. Use `xcrun simctl listapps` to list installed apps (for simulator)
3. Or check the app's bundle to see Info.plist

## Method 5: Check Project Settings

1. In Xcode, select your target
2. Go to **Build Settings**
3. Search for "URL" or "Scheme"
4. Check for any custom URL scheme configurations

## How to Add a URL Scheme (if it doesn't exist)

### Using Xcode UI:
1. Select your app target
2. Go to **Info** tab
3. Click the **+** button under **URL Types**
4. Add a new URL Type
5. Set the **Identifier** (e.g., `com.yourapp.deeplink`)
6. Add your **URL Schemes** (e.g., `myapp`)

### Using Info.plist directly:
Add this to your Info.plist:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleTypeRole</key>
        <string>Editor</string>
        <key>CFBundleURLName</key>
        <string>com.yourapp.deeplink</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>myapp</string>
        </array>
    </dict>
</array>
```

## Testing the URL Scheme

Once you know the URL scheme, test it:

### From Terminal:
```bash
# For simulator
xcrun simctl openurl booted "myapp://"

# For device (using xcrun devicectl or similar)
```

### From Safari (on device/simulator):
Type in the address bar: `myapp://` or `myapp://somepath`

### From another app:
Use `UIApplication.shared.open(URL(string: "myapp://")!)` in Swift

## Common URL Scheme Formats

- Simple: `myapp://`
- With path: `myapp://screen/home`
- With parameters: `myapp://screen/home?userId=123`

## Notes

- URL schemes are case-insensitive
- They should be unique to avoid conflicts
- Common practice: use reverse domain notation (e.g., `com.company.app`)
- The scheme is everything before the `://` in the URL
