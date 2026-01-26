# Contract Documentation: MAUI Shell Deep Linking

**Feature**: [001-maui-ios-shell](../spec.md)  
**Date**: January 26, 2026  
**Phase**: Phase 1 - Design

## Overview

This directory contains JSON schemas defining the data contracts for the MAUI iOS shell app's deep linking integration. These contracts ensure consistent communication between the React Native orchestrator and the MAUI platform-specific app.

## Contracts

### 1. check-in-request.json

**Purpose**: Defines the query parameters that can be passed when invoking the MAUI app via `mauiapp://check-in`.

**Usage**: External apps (orchestrator) construct deep link URLs using these parameters.

**Example Deep Link**:
```
mauiapp://check-in?userId=driver_001&location=Warehouse%20B&event=Delivery&requestId=req-12345
```

**Implementation Status**: ✅ **ACTIVE** - This contract is implemented in the current POC.

**Validation**:
- All parameters are optional (MAUI app handles empty parameter sets)
- Values MUST be URL-encoded by caller
- Maximum recommended URL length: 2000 characters (iOS system limit)

**See**: [check-in-request.json](check-in-request.json)

---

### 2. handoff-response.json

**Purpose**: Defines the response data that the MAUI app sends BACK to the orchestrator when the check-in workflow completes.

**Usage**: MAUI app constructs a deep link to the orchestrator's URL scheme (e.g., `reactnativeapp://handoff-complete`) with this data.

**Example Response Deep Link**:
```
reactnativeapp://handoff-complete?status=SUCCESS&completedTimestamp=2026-01-26T14:35:00Z&originalRequestId=req-12345&message=Check-in%20completed
```

**Implementation Status**: ✅ **ACTIVE** - Implemented when app is launched via deep link; skipped for standalone launches.

**Launch Mode Behavior**:
- **Deep link launch**: HandoffResponse is constructed and orchestrator is invoked upon completion
- **Standalone launch**: No handoff response sent; local confirmation shown instead

**Business Rules**:
- `status` MUST be one of: SUCCESS, FAILURE, CANCEL
- If `status = FAILURE`, `errorDetails` is required
- If `status = SUCCESS`, `errorDetails` MUST be null
- `completedTimestamp` is always required
- Orchestrator MUST implement 30-second timeout for handoff responses
- MAUI app detects launch mode via `LaunchContext` entity and only sends handoff if `LaunchMode = DEEP_LINK`

**See**: [handoff-response.json](handoff-response.json)

---

## Constitution Compliance

These contracts satisfy **Constitution Principle II: Native Handoff Protocol**:

✅ **Contract-based handoff protocol**: JSON schemas define explicit structure  
✅ **Success/failure/cancel states**: `handoff-response.json` includes all three states  
✅ **Handoff payloads include required fields**: requestId, timestamps, return URL (via orchestrator deep link)  
✅ **Logging requirement**: `completedTimestamp` enables traceability  
🔄 **Timeout enforcement (30s)**: To be implemented in orchestrator (not MAUI app responsibility)

**Partial Compliance Note**: The **return handoff** (MAUI → orchestrator) is deferred to post-POC. The **incoming handoff** (orchestrator → MAUI) is fully implemented via `check-in-request.json`.

## Integration Workflow

```
┌─────────────────────┐
│  React Native       │
│  Orchestrator       │
└──────────┬──────────┘
           │
           │ 1. Construct deep link using check-in-request.json schema
           │    mauiapp://check-in?userId=123&location=Store5
           │
           ▼
┌─────────────────────┐
│  iOS System         │
│  (Deep Link Router) │
└──────────┬──────────┘
           │
           │ 2. Route to MAUI app based on URL scheme
           │
           ▼
┌─────────────────────┐
│  MAUI Shell App     │
│  OnAppLinkRequested │
└──────────┬──────────┘
           │
           │ 3. Parse query params per check-in-request.json
           │    Create CheckInRequest entity (data-model.md)
           │
           ▼
┌─────────────────────┐
│  CheckInPage        │
│  (Display UI)       │
└──────────┬──────────┘
           │
           │ 4. User completes workflow (FUTURE)
           │
           ▼
┌─────────────────────┐
│  MAUI Shell App     │
│  (Construct Response)│
└──────────┬──────────┘
           │
           │ 5. Build handoff-response.json data (FUTURE)
           │    Construct deep link: reactnativeapp://handoff-complete?...
           │
           ▼
┌─────────────────────┐
│  iOS System         │
│  (Deep Link Router) │
└──────────┬──────────┘
           │
           │ 6. Route back to orchestrator (FUTURE)
           │
           ▼
┌─────────────────────┐
│  React Native       │
│  Orchestrator       │
│  (Process Response) │
└─────────────────────┘
```

## Testing

### Test Case 1: Valid Request (All Parameters)

**Deep Link**:
```
mauiapp://check-in?userId=driver_001&location=Warehouse%20B&event=Delivery&requestId=req-001&timestamp=2026-01-26T14:30:00Z
```

**Expected MAUI Behavior**:
- App launches (or comes to foreground)
- `OnAppLinkRequestReceived` parses all 5 parameters
- `CheckInPage` displays:
  - User ID: driver_001
  - Location: Warehouse B
  - Event: Delivery
  - Request ID: req-001
  - Timestamp: 2026-01-26T14:30:00Z

**Schema Validation**: ✅ All parameters match `check-in-request.json` types

---

### Test Case 2: Minimal Request (No Parameters)

**Deep Link**:
```
mauiapp://check-in
```

**Expected MAUI Behavior**:
- App launches with empty parameter set
- `CheckInPage` displays:
  - User ID: (empty)
  - Location: (empty)
  - Event: (empty)
  - Message: "No parameters received" or similar

**Schema Validation**: ✅ Empty object is valid per `check-in-request.json` (all properties optional)

---

### Test Case 3: Custom Fields

**Deep Link**:
```
mauiapp://check-in?userId=ABC&customField1=Value1&notes=Test%20note&priority=high
```

**Expected MAUI Behavior**:
- Parse `userId` as known field
- Parse `customField1`, `notes`, `priority` as custom fields
- `CheckInPage` displays:
  - User ID: ABC
  - Custom Fields:
    - customField1: Value1
    - notes: Test note
    - priority: high

**Schema Validation**: ✅ `additionalProperties: string` allows arbitrary custom fields

---

### Test Case 4: Future Response (Not Implemented)

**Trigger**: User taps "Done" button on CheckInPage (future feature)

**Expected MAUI Behavior** (future):
- Construct `HandoffResponse`:
  ```json
  {
    "status": "SUCCESS",
    "completedTimestamp": "2026-01-26T14:35:00Z",
    "originalRequestId": "req-001",
    "message": "Check-in completed successfully",
    "returnData": {
      "confirmationId": "CHK-789"
    }
  }
  ```
- Serialize to query parameters and invoke:
  ```
  reactnativeapp://handoff-complete?status=SUCCESS&completedTimestamp=2026-01-26T14:35:00Z&originalRequestId=req-001&message=Check-in%20completed&confirmationId=CHK-789
  ```

**Schema Validation**: ✅ Matches `handoff-response.json` structure

**Current Status**: 🚧 NOT IMPLEMENTED - Requires orchestrator app to exist

---

## Versioning

**Current Version**: 1.0.0

**Change Policy**:
- Breaking changes (removing required fields, changing types) require major version bump
- Adding optional fields is backward-compatible (minor version bump)
- Constitution amendments may trigger contract updates

**Future Considerations**:
- Add version parameter to deep links (e.g., `?version=1.0.0`) for schema negotiation
- Implement fallback behavior if orchestrator sends newer schema version than MAUI supports
