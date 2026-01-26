# Feature Specification: .NET MAUI iOS Shell Application

**Feature Branch**: `001-maui-ios-shell`  
**Created**: January 26, 2026  
**Status**: Draft  
**Input**: User description: "US001 - Create .Net Core MAUI Project - As a developer, I want to create a .Net Core MAUI project so that I can develop a shell mobile app for iOS. Acceptance Criteria: The project shall leverage the latest version of MAUI; The app shall implement a custom URL scheme for xcode deep link; The app shall have a screen that displays attributes provided by a calling app; the url scheme should be mauiapp://check-in"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Deep Link Navigation from External App (Priority: P1)

A user taps a link or performs an action in an external iOS application that invokes the MAUI shell app using the custom URL scheme `mauiapp://check-in`. The MAUI app launches (or comes to foreground if already running) and displays the check-in screen with the parameters passed from the calling application.

**Why this priority**: This is the core value proposition of the shell app - enabling seamless integration between external apps and the check-in functionality. Without this working, the app has no primary purpose.

**Independent Test**: Can be fully tested by creating a test URL (e.g., `mauiapp://check-in?userId=12345&location=NYC`) in Safari or a test app, triggering it, and verifying the MAUI app opens and displays the passed parameters correctly. Delivers immediate value as a standalone deep-link receiver.

**Acceptance Scenarios**:

1. **Given** the MAUI app is not running, **When** user taps a link with scheme `mauiapp://check-in?userId=123&location=Store5`, **Then** the app launches, navigates to the check-in screen, and displays userId "123" and location "Store5"
2. **Given** the MAUI app is already running in the background, **When** user taps a link with scheme `mauiapp://check-in?userId=456&event=Meeting`, **Then** the app comes to foreground, navigates to the check-in screen, and displays userId "456" and event "Meeting"
3. **Given** user taps a deep link with minimal parameters `mauiapp://check-in`, **When** the app launches, **Then** the check-in screen displays with empty or default values for attributes

---

### User Story 2 - View Check-In Details Screen (Priority: P2)

Once the app is launched (either via deep link OR standalone launch), the user sees a check-in screen. When launched via deep link, it displays the attributes (parameters) passed from the calling application. When launched standalone, it provides a form to manually enter check-in details.

**Why this priority**: This provides the user interface for the check-in workflow, supporting both orchestrator-invoked and standalone usage modes. It's dependent on P1 for deep link scenarios but can function independently for standalone usage.

**Independent Test**: Can be tested by (1) manually launching the app from home screen and entering check-in data, OR (2) triggering via deep link and verifying parameters display correctly in a user-friendly layout.

**Acceptance Scenarios**:

1. **Given** the app receives parameters via deep link, **When** the check-in screen loads, **Then** all received parameters are displayed in labeled fields (e.g., "User ID: 123", "Location: Store5")
2. **Given** the check-in screen is displaying parameters, **When** the user views the screen, **Then** the information is clearly readable with appropriate formatting (labels, spacing, font sizes)
3. **Given** the app is launched standalone (no deep link), **When** the check-in screen loads, **Then** the screen displays an input form for entering check-in details manually
4. **Given** the user completes the check-in workflow, **When** they tap "Done" or "Submit", **Then** the app returns control to the orchestrator (if launched via deep link) OR shows a confirmation (if standalone)

---

### User Story 3 - App Installation and Initial Setup (Priority: P3)

A developer can create and configure the MAUI project for iOS with the custom URL scheme properly registered, ensuring the project builds and deploys to an iOS device or simulator successfully.

**Why this priority**: This is the foundation that enables P1 and P2, but from a testing perspective, it's the setup/configuration story rather than a user-facing feature. It's independently testable as "does the project build and run."

**Independent Test**: Can be tested by building the project in Visual Studio or Visual Studio for Mac, deploying to an iOS simulator or device, and verifying the app launches without errors. The URL scheme can be verified by checking the Info.plist configuration.

**Acceptance Scenarios**:

1. **Given** a developer has the MAUI project source code, **When** they build the project for iOS, **Then** the build completes successfully without errors
2. **Given** the app is deployed to an iOS device/simulator, **When** the app is launched normally, **Then** the app opens successfully showing a default or home screen
3. **Given** the Info.plist is configured, **When** examining the URL schemes section, **Then** `mauiapp` is registered as a URL scheme with the identifier properly set

---

### Edge Cases

**Implementation Note**: Edge case handling is addressed in tasks.md Phase 7 (Error Handling & Edge Cases, tasks T056-T063).

- What happens when a deep link is triggered with malformed or extremely long parameter values (e.g., strings exceeding typical length limits)? → **Implementation**: T056-T059 (try-catch wrapper, ErrorState creation, user-friendly alert), T062 (test long values)
- How does the system handle deep links with special characters or encoding issues in parameter values (e.g., spaces, Unicode characters, URL-encoded strings)? → **Implementation**: T038 (test special characters), HttpUtility.ParseQueryString handles URL decoding
- What occurs when the app is in the middle of a different operation and a new deep link is triggered (should it interrupt current flow or queue the request)? → **Implementation**: T063 (test rapid successive triggers), T024 (test foreground state)
- How does the app behave if the same deep link is triggered multiple times rapidly in succession? → **Implementation**: T063 (graceful handling test)
- What happens when the iOS system denies the app permissions or the URL scheme conflicts with another app? → **Implementation**: T060-T061 (test wrong scheme/host), iOS system-level handling (MAUI app receives no notification if scheme conflict exists)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The mobile application MUST be built using the latest stable version of .NET MAUI framework available at time of development
- **FR-002**: The application MUST register and respond to the custom URL scheme `mauiapp://check-in`
- **FR-003**: The application MUST extract and parse query parameters from deep link URLs (e.g., `mauiapp://check-in?param1=value1&param2=value2`)
- **FR-004**: The application MUST provide a dedicated check-in screen that displays received parameters
- **FR-005**: The check-in screen MUST display all parameters passed via the deep link in a user-readable format with appropriate labels
- **FR-006**: The application MUST handle deep link activation when the app is not running (cold start)
- **FR-007**: The application MUST handle deep link activation when the app is running in the background (warm start)
- **FR-008**: The application MUST handle deep link activation when the app is already in the foreground
- **FR-009**: The application MUST be deployable to iOS devices and simulators
- **FR-010**: The application MUST configure the iOS Info.plist file correctly to register the URL scheme for deep linking
- **FR-011**: The application MUST navigate to the check-in screen automatically when a deep link is triggered
- **FR-012**: The check-in screen MUST gracefully handle cases where no parameters are provided in the deep link
- **FR-013**: The application MUST support standalone launch mode where users can manually perform check-in without external invocation
- **FR-014**: The application MUST detect whether it was launched via deep link or standalone mode
- **FR-015**: The application MUST send a handoff response back to the orchestrator when the check-in workflow completes AND the app was launched via deep link (if orchestrator URL scheme is not registered on device, application MUST display error alert to user per tasks T049)
- **FR-016**: When launched standalone, the application MUST NOT attempt to invoke the orchestrator's return URL

### Key Entities

- **Check-In Request**: Represents the data received from a calling application via deep link, containing arbitrary key-value pairs (parameters) such as user identifiers, location information, event names, timestamps, or other contextual attributes needed for the check-in process
- **URL Scheme Handler**: Represents the application component responsible for intercepting and parsing deep link URLs with the `mauiapp://check-in` scheme, extracting parameters, and triggering navigation to the check-in screen

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The application successfully launches and displays the check-in screen within 3 seconds when triggered by a deep link on a standard iOS device
- **SC-002**: 100% of deep link activations (cold start, warm start, foreground) result in successful navigation to the check-in screen with parameters correctly displayed
- **SC-003**: The check-in screen correctly displays all parameter types (text, numbers, special characters) without data loss or corruption in 100% of test cases
- **SC-004**: Developers can build and deploy the MAUI iOS project successfully on the first attempt following standard MAUI setup documentation
- **SC-005**: The app handles at least 10 different parameter combinations without errors or crashes
