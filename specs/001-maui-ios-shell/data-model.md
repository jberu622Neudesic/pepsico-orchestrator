# Data Model: .NET MAUI iOS Shell Application

**Feature**: [001-maui-ios-shell](spec.md)  
**Date**: January 26, 2026  
**Phase**: Phase 1 - Design

## Overview

This document defines the logical data entities for the MAUI iOS shell app. These entities represent the information flow from deep link invocation to check-in display. No persistent storage is required for POC; all data is transient (received via URL, displayed, then discarded when app closes).

## Entity: CheckInRequest

**Purpose**: Represents the data received when an external app invokes the MAUI shell via the `mauiapp://check-in` deep link.

**Source**: Query parameters from deep link URI (e.g., `mauiapp://check-in?userId=123&location=Store5&event=Meeting`)

**Lifecycle**: Created when `OnAppLinkRequestReceived` is triggered, passed to `CheckInPage`, displayed to user, discarded when user navigates away or closes app.

**Attributes**:

| Field Name | Type | Required | Description | Validation Rules | Example Value |
|------------|------|----------|-------------|------------------|---------------|
| `RawUri` | `Uri` | Yes | Full deep link URI as received by the app | Must match scheme `mauiapp` and host `check-in` | `mauiapp://check-in?userId=123` |
| `Parameters` | `Dictionary<string, string>` | Yes | All query parameters as key-value pairs | Keys are case-sensitive, values are URL-decoded | `{"userId": "123", "location": "Store5"}` |
| `ReceivedTimestamp` | `DateTime` | Yes | UTC timestamp when deep link was received | Auto-populated on creation | `2026-01-26T14:30:00Z` |

**Derived Fields** (for display convenience):

| Field Name | Type | Description | Derivation Logic |
|------------|------|-------------|------------------|
| `UserId` | `string?` | User identifier if provided | `Parameters["userId"]` or null if key missing |
| `Location` | `string?` | Location name if provided | `Parameters["location"]` or null if key missing |
| `Event` | `string?` | Event name if provided | `Parameters["event"]` or null if key missing |
| `CustomFields` | `Dictionary<string, string>` | All parameters NOT in the known set (userId, location, event) | All keys except the three above |

**Business Rules**:
- The app MUST accept deep links with zero or more parameters (empty query string is valid)
- Parameter keys are case-sensitive (e.g., `userId` ≠ `UserId`)
- If a parameter appears multiple times (e.g., `?key=val1&key=val2`), the last value wins (standard HTTP behavior)
- Special characters in values MUST be URL-encoded by the calling app; MAUI will auto-decode
- Maximum URL length is system-dependent (iOS typically supports 2000+ characters); no explicit validation needed

**Validation**:
```csharp
// Pseudo-validation logic
if (uri.Scheme != "mauiapp" || uri.Host != "check-in")
{
    throw new InvalidOperationException("Invalid deep link format");
}
```

**Example Instances**:

1. **Full parameters**:
   ```
   URI: mauiapp://check-in?userId=ABC123&location=Warehouse%20B&event=Delivery
   
   CheckInRequest:
     RawUri: mauiapp://check-in?userId=ABC123&location=Warehouse%20B&event=Delivery
     Parameters: { "userId": "ABC123", "location": "Warehouse B", "event": "Delivery" }
     ReceivedTimestamp: 2026-01-26T14:30:00Z
     UserId: "ABC123"
     Location: "Warehouse B"
     Event: "Delivery"
     CustomFields: {}
   ```

2. **Minimal parameters**:
   ```
   URI: mauiapp://check-in
   
   CheckInRequest:
     RawUri: mauiapp://check-in
     Parameters: {}
     ReceivedTimestamp: 2026-01-26T14:35:00Z
     UserId: null
     Location: null
     Event: null
     CustomFields: {}
   ```

3. **Custom fields**:
   ```
   URI: mauiapp://check-in?userId=XYZ&customData=Value1&notes=Test%20note
   
   CheckInRequest:
     RawUri: mauiapp://check-in?userId=XYZ&customData=Value1&notes=Test%20note
     Parameters: { "userId": "XYZ", "customData": "Value1", "notes": "Test note" }
     ReceivedTimestamp: 2026-01-26T14:40:00Z
     UserId: "XYZ"
     Location: null
     Event: null
     CustomFields: { "customData": "Value1", "notes": "Test note" }
   ```

---

## Entity: HandoffResponse

**Purpose**: Represents the data that is sent BACK to the calling orchestrator app when the check-in workflow completes. **IMPLEMENTED IN THIS FEATURE** when app is launched via deep link.

**Context**: Per Constitution Principle II (Native Handoff Protocol), all inter-app communication requires explicit success/failure/cancel states. This entity is used when the MAUI shell app needs to return control to the React Native orchestrator.

**Usage**: When the user completes the check-in screen (e.g., taps "Done" button) AND the app was launched via deep link, the app will construct a `HandoffResponse` and invoke the orchestrator's deep link with this data. If launched standalone, no handoff response is sent.

**Attributes**:

| Field Name | Type | Required | Description | Example Value |
|------------|------|----------|-------------|---------------|
| `Status` | `enum { SUCCESS, FAILURE, CANCEL }` | Yes | Outcome of the check-in workflow | `SUCCESS` |
| `OriginalRequestId` | `string?` | No | Identifier from original CheckInRequest (if orchestrator provided one) | `req-12345` |
| `Message` | `string?` | No | Human-readable status message | `"Check-in completed successfully"` |
| `ErrorDetails` | `string?` | No | Technical error details if Status = FAILURE | `"Network timeout after 30s"` |
| `CompletedTimestamp` | `DateTime` | Yes | UTC timestamp when workflow completed | `2026-01-26T14:35:00Z` |
| `ReturnData` | `Dictionary<string, string>?` | No | Additional data to pass back to orchestrator | `{"confirmationId": "CHK-789"}` |

**Business Rules**:
- If `Status = SUCCESS`, `ErrorDetails` MUST be null
- If `Status = FAILURE`, `ErrorDetails` SHOULD be populated with diagnostic info
- If `Status = CANCEL`, user explicitly aborted; no error details needed
- `ReturnData` is optional; orchestrator must handle missing fields gracefully

**Future Implementation Note**: This entity will be serialized to JSON and passed as query parameters in the orchestrator's deep link (e.g., `reactnativeapp://handoff-complete?status=SUCCESS&message=Done`). See [contracts/handoff-response.json](contracts/handoff-response.json) for the JSON schema.

---

## Entity: ErrorState

**Purpose**: Represents error conditions that can occur during deep link processing or parameter validation.

**Lifecycle**: Created when `OnAppLinkRequestReceived` encounters an invalid URI, when parameter parsing fails, or when navigation to `CheckInPage` throws an exception.

**Attributes**:

| Field Name | Type | Required | Description | Example Value |
|------------|------|----------|-------------|---------------|
| `ErrorCode` | `string` | Yes | Machine-readable error identifier | `INVALID_SCHEME`, `MALFORMED_URI`, `NAVIGATION_FAILED` |
| `UserMessage` | `string` | Yes | User-friendly error message | `"Sorry, this link is not valid."` |
| `TechnicalDetails` | `string?` | No | Developer-facing error info (for logging) | `"Expected scheme 'mauiapp', got 'http'"` |
| `OccurredAt` | `DateTime` | Yes | UTC timestamp when error occurred | `2026-01-26T14:45:00Z` |
| `RecoveryAction` | `string?` | No | Suggested action for user | `"Please contact support with error code INVALID_SCHEME"` |

**Error Codes**:

| Code | Trigger Condition | User Message | Recovery Action |
|------|-------------------|--------------|-----------------|
| `INVALID_SCHEME` | URI scheme is not `mauiapp` | "This link cannot be opened by this app." | "Check the link and try again." |
| `INVALID_HOST` | URI host is not `check-in` | "This link is not recognized." | "Please use a valid check-in link." |
| `MALFORMED_URI` | URI parsing throws exception | "The link format is invalid." | "Contact support if the problem persists." |
| `NAVIGATION_FAILED` | Navigation to CheckInPage throws | "Unable to load the check-in screen." | "Restart the app and try again." |

**Display Behavior**:
- When an error occurs, the app SHOULD navigate to an `ErrorPage` displaying `UserMessage` and `RecoveryAction`
- `TechnicalDetails` SHOULD be logged to console/debugging output but NOT shown to user
- For POC, a simple alert dialog with `UserMessage` is acceptable (full ErrorPage can be deferred to production)

**Example Instance**:
```
ErrorState:
  ErrorCode: "INVALID_SCHEME"
  UserMessage: "This link cannot be opened by this app."
  TechnicalDetails: "Expected scheme 'mauiapp', received 'http'. URI: http://example.com/check-in"
  OccurredAt: 2026-01-26T14:50:00Z
  RecoveryAction: "Check the link and try again."
```

---

## Entity: LaunchContext

**Purpose**: Tracks how the app was launched (deep link vs. standalone) to determine whether a handoff response should be sent upon completion.

**Lifecycle**: Created during app startup (either in `OnAppLinkRequestReceived` or `OnStart`), persisted in memory for the app session, checked when user completes check-in workflow.

**Attributes**:

| Field Name | Type | Required | Description | Example Value |
|------------|------|----------|-------------|---------------|
| `LaunchMode` | `enum { DEEP_LINK, STANDALONE }` | Yes | How the app was launched | `DEEP_LINK` |
| `OrchestratorReturnUrl` | `string?` | No | The orchestrator's deep link URL for handoff response (if provided in original deep link) | `reactnativeapp://handoff-complete` |
| `OriginalRequestId` | `string?` | No | Request ID from the original CheckInRequest (for correlation) | `req-12345` |
| `LaunchTimestamp` | `DateTime` | Yes | UTC timestamp when app was launched | `2026-01-26T14:30:00Z` |

**Business Rules**:
- If `LaunchMode = DEEP_LINK`, the app MUST attempt to send HandoffResponse upon completion
- If `LaunchMode = STANDALONE`, the app MUST NOT send HandoffResponse (just show local confirmation)
- `OrchestratorReturnUrl` can be passed as a query parameter in the original deep link (e.g., `?returnUrl=reactnativeapp://handoff-complete`) or use a hardcoded default
- If `OrchestratorReturnUrl` is null when handoff is needed, log error and show alert to user

**Example Instances**:

1. **Deep link launch**:
   ```
   LaunchContext:
     LaunchMode: DEEP_LINK
     OrchestratorReturnUrl: "reactnativeapp://handoff-complete"
     OriginalRequestId: "req-12345"
     LaunchTimestamp: 2026-01-26T14:30:00Z
   ```

2. **Standalone launch**:
   ```
   LaunchContext:
     LaunchMode: STANDALONE
     OrchestratorReturnUrl: null
     OriginalRequestId: null
     LaunchTimestamp: 2026-01-26T14:30:00Z
   ```

---

## Relationships

```
[Orchestrator-Invoked Mode]

┌─────────────────────┐
│  External App       │
│  (Orchestrator)     │
└──────────┬──────────┘
           │
           │ Invokes deep link
           │ mauiapp://check-in?params&returnUrl=...
           ▼
┌─────────────────────┐
│  LaunchContext      │◄── Created (LaunchMode = DEEP_LINK)
│  - LaunchMode       │
│  - ReturnUrl        │
└──────────┬──────────┘
           │
           │ Deep link parsed
           ▼
┌─────────────────────┐
│  CheckInRequest     │◄── Created from URI
│  - RawUri           │
│  - Parameters       │
│  - ReceivedTimestamp│
└──────────┬──────────┘
           │
           │ Passed to UI
           ▼
┌─────────────────────┐
│  CheckInPage        │◄── Displays parameters
│  (UI Layer)         │
└──────────┬──────────┘
           │
           │ User completes workflow
           ▼
┌─────────────────────┐
│  HandoffResponse    │◄── Created (Status: SUCCESS/FAILURE/CANCEL)
│  - Status           │
│  - Message          │
└──────────┬──────────┘
           │
           │ If LaunchMode = DEEP_LINK
           │ Return to orchestrator via deep link
           ▼
┌─────────────────────┐
│  External App       │
│  (Orchestrator)     │
└─────────────────────┘


[Standalone Mode]

┌─────────────────────┐
│  User Launches      │
│  from Home Screen   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  LaunchContext      │◄── Created (LaunchMode = STANDALONE)
│  - LaunchMode       │
│  - ReturnUrl = null │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  CheckInPage        │◄── User manually enters data
│  (UI Layer)         │
└──────────┬──────────┘
           │
           │ User completes workflow
           ▼
┌─────────────────────┐
│  Local Confirmation │◄── No handoff sent
│  (Success Message)  │
└─────────────────────┘


[Error Path - Both Modes]

   OnAppLinkRequestReceived
           │ exception
           ▼
┌─────────────────────┐
│  ErrorState         │◄── Created on failure
│  - ErrorCode        │
│  - UserMessage      │
└──────────┬──────────┘
           │
           │ Displayed to user
           ▼
┌─────────────────────┐
│  ErrorPage / Alert  │
└─────────────────────┘
```

## Data Flow Summary

1. **Deep Link Invocation**: External app triggers `mauiapp://check-in?params` → iOS routes to MAUI app
2. **CheckInRequest Creation**: `OnAppLinkRequestReceived` parses URI → creates `CheckInRequest` with parameters
3. **Validation**: If URI is malformed → create `ErrorState` → show error to user → STOP
4. **Navigation**: If valid → navigate to `CheckInPage` with `CheckInRequest` data
5. **Display**: `CheckInPage` binds to `CheckInRequest.Parameters` → renders labels/text
6. **Future Return** (not in POC): User taps "Done" → create `HandoffResponse` → invoke orchestrator deep link

## Storage & Persistence

**POC Decision**: NO persistent storage

- `CheckInRequest` is held in memory only (passed as navigation parameter to `CheckInPage`)
- When app is backgrounded or closed, all data is lost
- No SQLite, no local files, no cloud sync
- Future production version may add logging or analytics, but not required for POC validation

**Rationale**: The spec requires displaying parameters, not storing them. Adding persistence would violate Principle III (POC-First Development) by increasing scope unnecessarily.

---

## Validation Summary

All entities support the functional requirements:

- **FR-003** (Extract/parse query parameters): `CheckInRequest.Parameters` dictionary
- **FR-004** (Dedicated check-in screen): `CheckInRequest` is the data model for that screen
- **FR-005** (Display parameters in readable format): Derived fields (`UserId`, `Location`, `Event`, `CustomFields`) enable structured display
- **FR-012** (Gracefully handle no parameters): `Parameters` can be empty; derived fields return null

Constitution Principle II (Native Handoff Protocol) addressed via `HandoffResponse` entity (deferred to future implementation per Phase 1 contracts).
