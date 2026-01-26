# Tasks: .NET MAUI iOS Shell Application

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)  
**Generated**: January 26, 2026

**Input**: Design documents from `/specs/001-maui-ios-shell/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Manual testing only for POC - no automated test tasks included per POC-First Development principle

**Organization**: Tasks are grouped by user story to enable independent implementation and testing

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Project uses MAUI single-project structure:
- `maui/MauiShellApp/` - Main project directory
- `maui/MauiShellApp/Views/` - XAML pages
- `maui/MauiShellApp/Models/` - C# entities
- `maui/MauiShellApp/Platforms/iOS/` - iOS-specific configuration

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic MAUI structure

- [X] T001 Create MAUI project using `dotnet new maui -n MauiShellApp -f net10.0` in maui/ directory (use latest stable .NET SDK available: net10.0 or newer)
- [X] T002 Configure iOS deployment target to 16.0 in maui/MauiShellApp/MauiShellApp.csproj
- [X] T003 [P] Add System.Web reference for HttpUtility in maui/MauiShellApp/MauiShellApp.csproj
- [X] T004 [P] Create Views/ folder in maui/MauiShellApp/
- [X] T005 [P] Create Models/ folder in maui/MauiShellApp/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before user story implementation

**⚠️ CRITICAL**: No user story work can begin until URL scheme registration is complete

- [X] T006 Configure URL scheme "mauiapp" in maui/MauiShellApp/Platforms/iOS/Info.plist (add CFBundleURLTypes)
- [X] T007 Create LaunchMode enum in maui/MauiShellApp/App.xaml.cs
- [X] T008 Add static properties for launch mode tracking in maui/MauiShellApp/App.xaml.cs (CurrentLaunchMode, OrchestratorReturnUrl, OriginalRequestId)
- [X] T009 Override OnStart() to set STANDALONE mode in maui/MauiShellApp/App.xaml.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 3 - App Installation and Initial Setup (Priority: P3) 🎯 MVP FOUNDATION

**Goal**: Developer can create and configure the MAUI project for iOS with URL scheme registered, build successfully, and deploy to simulator/device

**Independent Test**: Build project (`dotnet build -f net10.0-ios`), deploy to iOS simulator, verify app launches without errors, check Info.plist has `mauiapp` URL scheme

### Implementation for User Story 3

- [X] T010 [US3] Verify build succeeds with `dotnet build -f net10.0-ios` in maui/MauiShellApp/ (use latest stable .NET SDK: net10.0 or newer)
- [ ] T011 [US3] Test deployment to iOS simulator with `dotnet build -t:Run -f net10.0-ios` (SKIPPED: Simulator runtime not available)
- [ ] T012 [US3] Verify app launches and displays default AppShell/MainPage (SKIPPED: Simulator runtime not available)
- [X] T013 [US3] Open Platforms/iOS/Info.plist and verify CFBundleURLSchemes contains "mauiapp"

**Checkpoint**: Project builds, deploys, and launches successfully. URL scheme is registered.

---

## Phase 4: User Story 1 - Deep Link Navigation from External App (Priority: P1) 🎯 CORE MVP

**Goal**: App responds to `mauiapp://check-in` deep links, launches (cold/warm/foreground), extracts parameters, and navigates to check-in screen

**Independent Test**: 
1. Run app on simulator
2. Trigger deep link via Safari (`mauiapp://check-in?userId=123&location=Store5`) or Terminal (`xcrun simctl openurl booted "mauiapp://check-in?userId=test"`)
3. Verify app launches/comes to foreground and displays check-in screen with parameters

### Models for User Story 1

- [X] T014 [P] [US1] Create CheckInRequest.cs model in maui/MauiShellApp/Models/ (properties: RawUri, Parameters, ReceivedTimestamp, UserId, Location, Event, CustomFields)
- [X] T015 [P] [US1] Create LaunchContext.cs model in maui/MauiShellApp/Models/ (properties: LaunchMode, OrchestratorReturnUrl, OriginalRequestId, LaunchTimestamp)

### Deep Link Handler for User Story 1

- [X] T016 [US1] Override OnAppLinkRequestReceived() in maui/MauiShellApp/App.xaml.cs
- [X] T017 [US1] Validate URI scheme and host (must be "mauiapp://check-in") in OnAppLinkRequestReceived()
- [X] T018 [US1] Set CurrentLaunchMode to DEEP_LINK in OnAppLinkRequestReceived()
- [X] T019 [US1] Parse query parameters using HttpUtility.ParseQueryString() in OnAppLinkRequestReceived()
- [X] T020 [US1] Extract returnUrl and requestId parameters and store in static properties in OnAppLinkRequestReceived()
- [X] T021 [US1] Navigate to CheckInPage with parsed parameters in OnAppLinkRequestReceived()

### Testing User Story 1

- [ ] T022 [US1] Test cold start: App not running, trigger deep link `mauiapp://check-in?userId=123&location=Store5`
- [ ] T023 [US1] Test warm start: App in background, trigger deep link `mauiapp://check-in?userId=456&event=Meeting`
- [ ] T024 [US1] Test foreground: App already open, trigger deep link `mauiapp://check-in?userId=789`
- [ ] T025 [US1] Test minimal parameters: Trigger `mauiapp://check-in` with no query string
- [ ] T026 [US1] Test custom fields: Trigger `mauiapp://check-in?userId=ABC&customData=Value1&notes=Test`

**Checkpoint**: Deep link handling works across all app states (cold/warm/foreground). Parameters are extracted correctly.

---

## Phase 5: User Story 2 - View Check-In Details Screen (Priority: P2) 🎯 UI MVP

**Goal**: Check-in screen displays parameters from deep link OR provides manual entry form for standalone mode. Clear, readable layout with appropriate formatting.

**Independent Test**:
1. **Deep link mode**: Trigger `mauiapp://check-in?userId=123&location=Store5&event=Delivery`, verify all parameters display correctly
2. **Standalone mode**: Launch app from home screen, verify UI is usable (can show empty/default state or input form)

### UI Implementation for User Story 2

- [X] T027 [P] [US2] Create CheckInPage.xaml in maui/MauiShellApp/Views/ (VerticalStackLayout with labels for userId, location, event, customFields)
- [X] T028 [P] [US2] Create CheckInPage.xaml.cs code-behind in maui/MauiShellApp/Views/
- [X] T029 [US2] Add constructor accepting NameValueCollection parameters in CheckInPage.xaml.cs
- [X] T030 [US2] Implement DisplayParameters() method to populate labels in CheckInPage.xaml.cs
- [X] T031 [US2] Set label text for UserId, Location, Event (handle null/empty values) in CheckInPage.xaml.cs
- [X] T032 [US2] Extract and display custom fields (exclude known keys: userId, location, event, timestamp, requestId, returnUrl) in CheckInPage.xaml.cs
- [X] T033 [US2] Add "Done" button to CheckInPage.xaml with OnDoneClicked handler
- [X] T034 [US2] Ensure font sizes are 16pt minimum (per Driver-Centric UX principle) in CheckInPage.xaml
- [X] T035 [US2] Ensure button height is 50pt (≥44pt touch target) in CheckInPage.xaml

### Testing User Story 2

- [ ] T036 [US2] Test with full parameters: Verify all fields display correctly with proper labels
- [ ] T037 [US2] Test with no parameters: Verify empty/default values display appropriately
- [ ] T038 [US2] Test with special characters: Trigger deep link with URL-encoded values (spaces, Unicode)
- [ ] T039 [US2] Test with long parameter values: Verify text wrapping and readability
- [ ] T040 [US2] Test custom fields display: Verify unlabeled parameters appear in "Additional Parameters" section

**Checkpoint**: Check-in screen displays parameters correctly with readable formatting. Standalone and deep link modes both work.

---

## Phase 6: Handoff Response Implementation (Return to Orchestrator)

**Goal**: When user completes check-in AND app was launched via deep link, send HandoffResponse back to orchestrator. If standalone, show local confirmation only.

**Independent Test**:
1. Launch via deep link with returnUrl: `mauiapp://check-in?userId=123&returnUrl=reactnativeapp://handoff-complete&requestId=req-001`
2. Tap "Done" button
3. Verify orchestrator deep link is invoked (if orchestrator doesn't exist, iOS will show error - expected for POC)
4. Launch standalone, tap "Done", verify local alert displays (no deep link invoked)

### Models for Handoff Response

- [X] T041 [P] Create HandoffResponse.cs model in maui/MauiShellApp/Models/ (properties: Status, CompletedTimestamp, OriginalRequestId, Message, ErrorDetails, ReturnData)
- [X] T042 [P] Create ErrorState.cs model in maui/MauiShellApp/Models/ (properties: ErrorCode, UserMessage, TechnicalDetails, OccurredAt, RecoveryAction)

### Handoff Logic Implementation

- [X] T043 Implement OnDoneClicked handler in maui/MauiShellApp/Views/CheckInPage.xaml.cs
- [X] T044 Check App.CurrentLaunchMode in OnDoneClicked - branch to handoff vs. local confirmation
- [X] T045 Implement SendHandoffResponse() method in CheckInPage.xaml.cs (constructs deep link URL with status, timestamp, message)
- [X] T046 Get OrchestratorReturnUrl from App static property (use default "reactnativeapp://handoff-complete" if null) in SendHandoffResponse()
- [X] T047 Build query string with status=SUCCESS, completedTimestamp, message, originalRequestId in SendHandoffResponse()
- [X] T048 Invoke orchestrator using Launcher.CanOpenAsync() and Launcher.OpenAsync() in SendHandoffResponse()
- [X] T049 Handle error if orchestrator URL scheme not registered (show alert to user) in SendHandoffResponse()
- [X] T050 Implement standalone confirmation: DisplayAlert("Success", "Check-in completed!", "OK") in OnDoneClicked

### Testing Handoff Response

- [ ] T051 Test deep link mode with returnUrl: Verify handoff response is sent (check iOS console for URL invocation)
- [ ] T052 Test deep link mode without returnUrl: Verify default orchestrator URL is used
- [ ] T053 Test standalone mode: Verify local alert displays, no deep link invoked
- [ ] T054 Test with originalRequestId: Verify it's included in handoff response URL
- [ ] T055 Test handoff failure: Orchestrator not installed, verify user sees error alert

**Checkpoint**: Handoff response logic works. App correctly detects launch mode and sends response only when appropriate.

---

## Phase 7: Error Handling & Edge Cases

**Purpose**: Handle malformed deep links, parsing errors, and edge cases gracefully

- [X] T056 Wrap OnAppLinkRequestReceived in try-catch in maui/MauiShellApp/App.xaml.cs
- [X] T057 Create ErrorState when URI parsing fails in OnAppLinkRequestReceived
- [X] T058 Show error alert with user-friendly message when deep link is invalid
- [X] T059 Log technical error details to console/debug output (not shown to user)
- [ ] T060 Test malformed URI: Trigger `http://check-in` (wrong scheme), verify error handling
- [ ] T061 Test invalid host: Trigger `mauiapp://wrong-host`, verify error handling
- [ ] T062 Test extremely long parameter values (>500 chars), verify app doesn't crash
- [ ] T063 Test rapid successive deep link triggers, verify app handles gracefully

**Checkpoint**: Error handling prevents crashes. Users see helpful messages when errors occur.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final touches and validation

- [ ] T064 [P] Review all XAML files for Driver-Centric UX compliance (font sizes ≥16pt, touch targets ≥44pt)
- [ ] T065 [P] Add XML documentation comments to public methods in Models/ classes
- [ ] T066 [P] Add XML documentation comments to App.xaml.cs methods
- [ ] T067 Verify quickstart.md steps work end-to-end (build, deploy, test deep link)
- [ ] T068 Test on physical iOS device (if available) to verify deep link behavior matches simulator
- [ ] T069 Review Constitution compliance: Platform Orchestration Pattern, Native Handoff Protocol, POC-First Development, Platform Version Currency, Driver-Centric UX
- [ ] T070 Code cleanup: Remove commented code, unused namespaces, debug statements

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) - BLOCKS all user stories
- **US3 - Installation/Setup (Phase 3)**: Depends on Foundational (Phase 2) - Foundation validation
- **US1 - Deep Link Navigation (Phase 4)**: Depends on Foundational (Phase 2) - Core feature
- **US2 - Check-In Screen (Phase 5)**: Depends on US1 (Phase 4) - Needs deep link handler to navigate
- **Handoff Response (Phase 6)**: Depends on US2 (Phase 5) - Needs CheckInPage UI to trigger
- **Error Handling (Phase 7)**: Depends on US1 (Phase 4) - Enhances deep link handler
- **Polish (Phase 8)**: Depends on all previous phases - Final validation

### User Story Dependencies

- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Setup validation
- **User Story 1 (P1)**: Can start after US3 (Phase 3) - Requires working project
- **User Story 2 (P2)**: Can start after US1 (Phase 4) - Requires deep link handler to navigate to CheckInPage

### Suggested Implementation Order (Priority-Based)

Given the actual user story priorities and dependencies:

1. **Phase 1-2**: Setup + Foundational (T001-T009)
2. **Phase 3**: US3 - Project Setup (T010-T013) - Validates foundation works
3. **Phase 4**: US1 - Deep Link Navigation (T014-T026) - Core POC value
4. **Phase 5**: US2 - Check-In Screen (T027-T040) - UI for displaying parameters
5. **Phase 6**: Handoff Response (T041-T055) - Complete the orchestration loop
6. **Phase 7**: Error Handling (T056-T063) - Robustness
7. **Phase 8**: Polish (T064-T070) - Final touches

### MVP Delivery Strategy

**Minimum Viable Product (Demonstrates Core Concept)**:
- Complete through **Phase 5** (US1 + US2): Deep link works, parameters display
- This proves: "MAUI app can receive orchestrator handoff via deep link and display data"
- ~40 tasks (T001-T040)

**Full POC (Ready for Orchestrator Integration)**:
- Complete through **Phase 6**: Adds handoff response capability
- This proves: "MAUI app can send data back to orchestrator"
- ~55 tasks (T001-T055)

**Production-Ready (Robust Error Handling)**:
- Complete all phases including Phase 7-8
- ~70 tasks total

### Parallel Opportunities

**Within Setup (Phase 1)**:
- T003, T004, T005 can run in parallel (all [P] marked)

**Within Foundational (Phase 2)**:
- No parallelization - sequential setup required

**Within US1 (Phase 4)**:
- T014 and T015 (Models) can run in parallel (both [P])
- Tests T022-T026 can run in parallel once implementation is complete

**Within US2 (Phase 5)**:
- T027 and T028 (XAML and code-behind) can run in parallel (both [P])
- Tests T036-T040 can run in parallel once implementation is complete

**Within Handoff (Phase 6)**:
- T041 and T042 (Models) can run in parallel (both [P])
- Tests T051-T055 can run in parallel once implementation is complete

**Within Polish (Phase 8)**:
- T064, T065, T066 (Documentation and review) can run in parallel (all [P])

**Cross-Phase Parallelization** (if team has 2+ developers):
- Once Phase 2 completes, one developer can work on US3 while another prepares US1 models
- After US1 deep link handler works, one developer can build US2 UI while another starts handoff models

---

## Parallel Example: User Story 1 (Deep Link Navigation)

Assuming Foundational phase is complete, here's how to parallelize US1:

```bash
# Step 1: Create models in parallel (2 developers or sequential)
# Dev 1:
touch maui/MauiShellApp/Models/CheckInRequest.cs
# Implement CheckInRequest.cs (T014)

# Dev 2 (parallel):
touch maui/MauiShellApp/Models/LaunchContext.cs
# Implement LaunchContext.cs (T015)

# Step 2: Implement deep link handler (sequential - single file)
# Edit maui/MauiShellApp/App.xaml.cs (T016-T021)

# Step 3: Run tests in parallel (can test different scenarios concurrently)
# Terminal 1: Test cold start (T022)
# Terminal 2: Test warm start (T023)
# Terminal 3: Test foreground (T024)
# Terminal 4: Test minimal params (T025)
# Terminal 5: Test custom fields (T026)
```

---

## Validation Checklist

Before marking the feature complete, verify:

- [ ] All user stories (US1, US2, US3) are independently testable
- [ ] Deep link works in all three app states (cold, warm, foreground)
- [ ] Parameters display correctly with proper formatting
- [ ] Handoff response sends when launched via deep link
- [ ] Local confirmation shows when launched standalone
- [ ] Error handling prevents crashes on malformed deep links
- [ ] Constitution principles are satisfied (see plan.md Constitution Check)
- [ ] Quickstart.md instructions work end-to-end
- [ ] Code is clean and documented

---

## Implementation Strategy Summary

**Total Tasks**: 70  
**MVP Tasks**: 40 (Phase 1-5)  
**Full POC Tasks**: 55 (Phase 1-6)  

**Estimated Effort** (single developer, POC pace):
- Setup + Foundational: 2-3 hours
- US3 (Setup validation): 1 hour
- US1 (Deep link navigation): 4-6 hours
- US2 (Check-in screen): 3-4 hours
- Handoff response: 3-4 hours
- Error handling: 2-3 hours
- Polish: 2-3 hours
- **Total**: ~17-26 hours (2-3 days for experienced MAUI developer)

**Key Success Criteria**:
1. Can trigger `mauiapp://check-in?userId=123&location=Store5` and see parameters displayed
2. Tapping "Done" invokes orchestrator deep link (when launched via deep link)
3. Standalone mode shows local confirmation (when launched from home screen)
4. All Constitution gates pass (see plan.md)
