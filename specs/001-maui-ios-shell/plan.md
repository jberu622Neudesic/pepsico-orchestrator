# Implementation Plan: .NET MAUI iOS Shell Application

**Branch**: `001-maui-ios-shell` | **Date**: January 26, 2026 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-maui-ios-shell/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Build a .NET MAUI iOS application that operates in two modes:

1. **Standalone Mode**: Launch from iOS home screen, perform check-in workflow independently with manual data entry, show local confirmation
2. **Orchestrator-Invoked Mode**: Respond to the custom URL scheme `mauiapp://check-in`, extract query parameters from deep link URLs, display received data on check-in screen, and send handoff response back to orchestrator upon completion

This dual-mode design enables independent validation (standalone) while supporting seamless integration with the React Native orchestrator for multi-platform workflow handoffs.

## Technical Context

**Language/Version**: C# with .NET 10.0 (or latest stable), MAUI workload  
**Primary Dependencies**: Microsoft.Maui (latest stable), Microsoft.Maui.Controls, Microsoft.iOS  
**Storage**: N/A (parameters received via deep link are displayed transiently, no persistence required for POC)  
**Testing**: xUnit or NUnit for unit tests, manual testing for deep link flows on iOS Simulator/device  
**Target Platform**: iOS 16+ (physical devices and simulators)  
**Project Type**: Mobile (single-platform iOS for POC)  
**Performance Goals**: App launch from deep link within 3 seconds, smooth UI rendering at 60 fps  
**Constraints**: iOS-only deployment, URL scheme must be `mauiapp://check-in`, minimal UI complexity for POC  
**Scale/Scope**: Single check-in screen, support for arbitrary query parameters, 1-2 MAUI content pages

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Platform Orchestration Pattern
**Status**: ✅ FULL COMPLIANCE (Dual-mode architecture)

This MAUI app operates in **two modes**:
1. **Orchestrator-invoked mode**: Invoked via deep link (`mauiapp://check-in`) by React Native orchestrator, performs check-in task, returns control via handoff response
2. **Standalone mode**: Launched independently from iOS home screen, performs full check-in workflow without orchestrator integration

**Compliance Notes**:
- ✅ App responds to deep link invocation (handoff protocol ready)
- ✅ Designed as a specialized workflow handler when invoked by orchestrator
- ✅ Also functions as standalone app for independent driver usage
- ✅ Detects launch mode and adjusts behavior (handoff response only sent if launched via deep link)
- ⚠️ React Native orchestrator does not exist yet (POC phase - will be built in subsequent feature)
- ✅ URL scheme enables seamless handoff as required by principle

**Justification**: Dual-mode design ensures the MAUI app can be validated independently (standalone mode) AND integrated with the orchestrator (invoked mode). This supports POC-First Development while maintaining production-ready flexibility.

**Post-Design Update**: ✅ Design supports both modes. Launch mode detection via `OnAppLinkRequestReceived` (deep link) vs. `OnStart` (standalone).

### II. Native Handoff Protocol
**Status**: ✅ CONTRACTS DEFINED (Implementation deferred to post-POC)

**Phase 1 Deliverables** (COMPLETED):
- ✅ `contracts/check-in-request.json`: Defines incoming deep link payload schema
- ✅ `contracts/handoff-response.json`: Defines return handoff schema (SUCCESS/FAILURE/CANCEL states)
- ✅ `data-model.md`: Documents HandoffResponse entity with required fields
- ✅ Timeout behavior: 30-second timeout is **orchestrator responsibility** (orchestrator must handle timeout and notify user if MAUI app doesn't respond within 30 seconds; MAUI app does not implement watchdog timer)

**Current Implementation Status**:
- ✅ Incoming handoff (orchestrator → MAUI): IMPLEMENTED via `OnAppLinkRequestReceived`
- ✅ Return handoff (MAUI → orchestrator): CONTRACT DEFINED, WILL BE IMPLEMENTED in this feature (when user completes check-in)
- ✅ Launch mode detection: App distinguishes orchestrator-invoked vs. standalone mode
- ✅ Conditional handoff: Return handoff only triggered if app was launched via deep link

**Compliance Summary**:
- ✅ Contract-based protocol: JSON schemas establish explicit structure
- ✅ Success/failure/cancel states: Defined in handoff-response.json
- ✅ Payload includes required fields: requestId, timestamps, status, message, errorDetails
- ✅ Logging support: completedTimestamp enables traceability
- 🔄 Timeout enforcement: To be implemented in orchestrator (not MAUI responsibility)

**Post-Design Update**: ✅ All handoff protocol contracts formalized. Return handoff deferred per POC-First Development.

### III. POC-First Development
**Status**: ✅ FULL COMPLIANCE

- ✅ Feature ruthlessly descoped to core need: receive deep link, show parameters
- ✅ No offline support, analytics, or advanced error recovery in scope
- ✅ Hardcoded success scenario acceptable (check-in screen just displays received data)
- ✅ Visual polish deferred (basic labels and text display sufficient)
- ✅ Validates core concept: "Can MAUI receive orchestrator handoff?"
- ✅ Minimal file count: ~5-7 C# files for POC (App.xaml.cs, CheckInPage, Models)

**Post-Design Update**: ✅ Project structure confirms minimal implementation. No unnecessary abstractions or patterns.

### IV. Platform Version Currency
**Status**: ✅ FULL COMPLIANCE

- ✅ .NET 10.0 (or latest stable SDK at implementation time)
- ✅ MAUI latest workload version (aligned with .NET SDK)
- ✅ iOS 16+ deployment target (modern API availability)
- ✅ Xcode latest stable for build process

**Post-Design Update**: ✅ Confirmed in research.md and quickstart.md. All version requirements documented.

### V. Driver-Centric UX
**Status**: ✅ FULL COMPLIANCE (within feature scope)

- ✅ Touch targets will be ≥44pt (MAUI default Button/Label sizes comply)
- ✅ Check-in screen is the landing screen (0 taps from deep link activation)
- ✅ Text will be 16pt minimum (MAUI default font sizes comply per quickstart.md)
- ✅ No loading states needed (parameters are immediate from URL)
- ⚠️ Offline-tolerant design N/A (this app receives data, doesn't fetch from network)

**Post-Design Update**: ✅ CheckInPage.xaml (in quickstart.md) uses appropriate font sizes and spacing. Complies with 44pt touch target guidance.

---

**Overall Gate Status**: ✅ PASS WITH JUSTIFICATIONS (RE-EVALUATED POST-DESIGN)

**Changes from Initial Check**:
- Principle II updated from "DEFERRED" to "CONTRACTS DEFINED" ✅
- All design artifacts (research.md, data-model.md, contracts/, quickstart.md) validate compliance
- No new violations introduced during design phase
- POC scope remains appropriately minimal

**Approval for Next Phase**: ✅ PROCEED to `/speckit.tasks` for implementation task breakdown

## Project Structure

### Documentation (this feature)

```text
specs/001-maui-ios-shell/
├── spec.md              # Feature specification (requirements, user stories)
├── plan.md              # This file (technical design and architecture)
├── research.md          # Phase 0: Deep linking research and technology decisions
├── data-model.md        # Phase 1: Entity definitions (CheckInRequest, HandoffResponse, ErrorState)
├── quickstart.md        # Phase 1: Developer setup guide
├── contracts/           # Phase 1: JSON schemas for deep linking
│   ├── check-in-request.json
│   ├── handoff-response.json
│   └── README.md
├── checklists/
│   └── requirements.md  # Specification quality validation
└── tasks.md             # Phase 2: Implementation tasks (/speckit.tasks - NOT YET CREATED)
```

### Source Code (repository root)

**Structure Decision**: Mobile application (Option 3) - Single MAUI project for iOS

```text
maui/
├── MauiShellApp/                    # Main MAUI project directory
│   ├── MauiShellApp.csproj          # Project file (.NET 10, iOS target)
│   ├── App.xaml                     # Application entry point (UI)
│   ├── App.xaml.cs                  # Deep link handler (OnAppLinkRequestReceived)
│   ├── AppShell.xaml                # Shell navigation structure (optional for POC)
│   ├── AppShell.xaml.cs
│   ├── MainPage.xaml                # Default landing page (may be unused if deep link launches first)
│   ├── MainPage.xaml.cs
│   │
│   ├── Views/                       # XAML pages and UI components
│   │   ├── CheckInPage.xaml         # Check-in screen (displays parameters)
│   │   └── CheckInPage.xaml.cs      # CheckInPage code-behind (parameter binding logic)
│   │
│   ├── Models/                      # Data entities (C# classes)
│   │   ├── CheckInRequest.cs        # CheckInRequest entity (from data-model.md)
│   │   ├── LaunchContext.cs         # LaunchContext entity (tracks DEEP_LINK vs STANDALONE mode)
│   │   ├── HandoffResponse.cs       # HandoffResponse entity (for orchestrator return)
│   │   └── ErrorState.cs            # ErrorState entity for error handling
│   │
│   ├── Services/                    # Business logic and utilities
│   │   ├── DeepLinkParser.cs        # Utility for parsing URI query parameters
│   │   └── NavigationService.cs     # Helper for app navigation (optional)
│   │
│   ├── Platforms/                   # Platform-specific code
│   │   └── iOS/
│   │       ├── Info.plist           # URL scheme registration (CFBundleURLSchemes)
│   │       ├── Entitlements.plist   # iOS entitlements (default, no custom entries needed)
│   │       └── AppDelegate.cs       # iOS app lifecycle (auto-generated by MAUI)
│   │
│   └── Resources/                   # Images, fonts, styles
│       ├── Images/
│       ├── Fonts/
│       └── Styles/
│           └── Colors.xaml          # Color resources
│
└── MauiShellApp.Tests/              # Unit tests (optional for POC)
    ├── MauiShellApp.Tests.csproj
    └── Models/
        └── CheckInRequestTests.cs   # Unit tests for parameter parsing logic
```

**Rationale**:
- **Single MAUI project**: Per constitution, this is a platform-specific shell (not a multi-platform app). iOS-only target reduces complexity.
- **Views/ folder**: Separates XAML UI from business logic (Models, Services).
- **Platforms/iOS/**: Contains Info.plist for URL scheme registration (mauiapp).
- **Models/ folder**: Mirrors entities from data-model.md (CheckInRequest, HandoffResponse, ErrorState).
- **Services/ folder**: Optional for POC; DeepLinkParser can encapsulate query parameter logic if needed.
- **No API or backend**: This app is a client-side shell; no server component required.

**Constitution Alignment**:
- Follows standard MAUI single-project structure (no custom deviations)
- Minimal complexity: ~5-7 C# files for POC (App.xaml.cs, CheckInPage, CheckInRequest model, DeepLinkParser utility)
- Aligns with POC-First Development: no over-engineering, focus on core deep link functionality

## Complexity Tracking

> **No complexity violations to justify.**

The MAUI app uses the standard single-project structure recommended by Microsoft. All constitution gates pass or have justified deferrals (handoff response to post-POC per Principle III).
