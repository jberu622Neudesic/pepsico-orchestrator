<!--
  SYNC IMPACT REPORT
  ==================
  Version: 1.0.0 (Initial Constitution)
  Date: 2026-01-26
  
  Changes Made:
  - Initial constitution established for PepsiCo Delivery Orchestrator POC
  - Defined 5 core principles for multi-platform orchestration architecture
  - Established POC constraints (4-week timeline, iOS-only)
  - Defined platform handoff protocol requirements
  
  Principles Added:
  1. Platform Orchestration Pattern
  2. Native Handoff Protocol
  3. POC-First Development
  4. Platform Version Currency
  5. Driver-Centric UX
  
  Template Compliance Status:
  ✅ plan-template.md - Compatible (uses constitution gates)
  ✅ spec-template.md - Compatible (user story format aligns with driver workflows)
  ✅ tasks-template.md - Compatible (phase structure supports multi-platform builds)
  ⚠️ No command templates found in .specify/templates/commands/ - will create if needed
  
  Follow-up Actions:
  - None required for initial ratification
-->

# PepsiCo Delivery Orchestrator Constitution

## Core Principles

### I. Platform Orchestration Pattern

**React Native MUST serve as the central orchestration layer; platform-specific apps (MAUI for chips, Swift for beverages) handle only domain-specific workflows.**

- The React Native app owns: route management, schedule fetching, store check-in/out, navigation between stops, workflow state persistence
- MAUI and Swift apps are invoked ONLY for delivery-type-specific tasks (manifest completion, payment processing)
- Platform apps MUST return control to the orchestrator immediately upon task completion
- Deep linking or URL schemes MUST be used for seamless handoffs—no manual app switching by drivers
- State synchronization between orchestrator and platform apps is REQUIRED for continuity

**Rationale**: Drivers need a unified workflow hub. Context-switching between unrelated apps degrades UX and causes workflow interruptions. The orchestrator ensures a single source of truth for route progression.

### II. Native Handoff Protocol

**All inter-app communication MUST follow a contract-based handoff protocol with explicit success/failure/cancel states.**

- Handoff payloads MUST include: stop ID, delivery type, manifest data (if applicable), return URL
- Platform apps MUST respond with one of: `SUCCESS` (task completed), `FAILURE` (error details), `CANCEL` (user aborted)
- The orchestrator MUST handle all three states gracefully: success → next stop, failure → retry UI, cancel → resume route
- No silent failures: every handoff attempt MUST be logged with timestamp, payload, and response
- Timeout enforcement (30 seconds max per handoff) with user notification if exceeded

**Rationale**: In field operations, unreliable handoffs lead to incomplete deliveries or data loss. A strict protocol ensures traceability and driver confidence in the system.

### III. POC-First Development

**Optimize for rapid validation over production readiness; ruthlessly descope non-essential features to meet the 4-week delivery target.**

- Focus areas: core route flow, check-in/out, single successful handoff per delivery type
- Deferred to post-POC: offline support, advanced error recovery, analytics, multi-user login
- Hardcoded test data is ACCEPTABLE for POC if API integration is not critical-path
- Visual polish is secondary to functional workflow completion
- Every feature MUST answer: "Does this prove the orchestration concept works?"

**Rationale**: The POC objective is to validate the multi-platform orchestration architecture for PepsiCo stakeholders. Over-engineering delays validation and wastes budget.

### IV. Platform Version Currency

**All platforms MUST use latest stable versions: iOS (latest), React Native (latest stable), MAUI (latest .NET), Swift (latest Xcode default).**

- React Native: Use the latest stable release (not pre-release/RC)
- MAUI: Use latest .NET SDK (e.g., .NET 8 or .NET 9 if stable by start date)
- Swift: Use the latest Xcode stable version's default Swift (e.g., Swift 6.x if available)
- iOS Deployment Target: Minimum iOS 16+ to ensure modern API availability
- Dependency updates MUST be reviewed weekly during the POC phase to avoid security/compatibility issues

**Rationale**: Using outdated versions introduces technical debt before launch. Latest versions provide better performance, security, and community support. POC timeline is short enough to avoid mid-project breaking changes.

### V. Driver-Centric UX

**Every UI decision MUST prioritize in-vehicle usability: large touch targets, minimal input, glanceable status, offline-tolerant design.**

- Touch targets MUST be ≥44pt (iOS standard) to accommodate gloved hands or vehicle vibration
- Critical actions (check-in, next stop) MUST be accessible within 2 taps from the main screen
- Text MUST be legible at arm's length (minimum 16pt body, 20pt+ headings)
- Loading states MUST be visible (no silent waiting); errors MUST provide actionable recovery steps
- Assume intermittent connectivity: local caching of route data, optimistic UI updates

**Rationale**: Drivers operate in challenging environments (sunlight, movement, time pressure). Poor mobile UX directly impacts delivery efficiency and safety.

## Technology Stack Requirements

**React Native Orchestrator**:
- Framework: React Native (latest stable, e.g., 0.76.x or newer)
- Navigation: React Navigation (v6+) for multi-screen route flows
- State Management: React Context or Zustand for simple POC-level state
- HTTP Client: Axios or Fetch API for route/schedule retrieval
- Storage: AsyncStorage for local route caching (upgrade to MMKV if performance issues arise)

**MAUI App (Chips Delivery)**:
- Framework: .NET MAUI (latest .NET SDK, e.g., .NET 8/9)
- Target: iOS only (Android out of scope for POC)
- Deep Linking: Configure URL scheme for orchestrator handoff
- Data Format: JSON payloads for manifest submission

**Swift App (Beverages Delivery)**:
- Language: Swift (latest Xcode default)
- Framework: UIKit or SwiftUI (team discretion—SwiftUI preferred for rapid iteration)
- Target: iOS 16+
- Deep Linking: Universal Links or custom URL scheme
- Data Format: JSON payloads for payment processing

**Deployment**:
- iOS builds via Xcode (development provisioning profiles acceptable for POC)
- TestFlight distribution for stakeholder review (not required for internal POC validation)

## Development Workflow

**Parallel Workstreams**:
- Stream 1: React Native orchestrator (route UI, check-in/out, handoff logic)
- Stream 2: MAUI app (chips manifest workflow)
- Stream 3: Swift app (beverages payment workflow)
- Integration checkpoints: Week 2 (handoff protocol test), Week 3 (end-to-end route test), Week 4 (stakeholder demo)

**Code Organization**:
- Repository structure: `/react/`, `/maui/`, `/swift/` (monorepo approach)
- Shared contracts: `/contracts/` directory with JSON schema definitions for handoff payloads
- Documentation: Feature specs in `/specs/`, implementation plans follow template structure

**Quality Gates** (relaxed for POC speed):
- Manual testing acceptable; automated tests for critical handoff logic only
- Code reviews MUST verify handoff protocol compliance
- Integration smoke tests MUST pass before each checkpoint (Week 2/3/4)

## Governance

This constitution supersedes all conflicting practices. During the 4-week POC phase:

- **Amendment Process**: Amendments require team consensus (all three platform leads + project sponsor). Documented in this file with version increment.
- **Compliance Verification**: Weekly check-ins MUST review adherence to Platform Orchestration Pattern and Native Handoff Protocol.
- **Complexity Justification**: Any architectural deviation MUST be justified in writing with sponsor approval (e.g., adding a backend service).
- **Post-POC Transition**: Upon POC approval, a production-ready constitution MUST be drafted to address scalability, offline-first, security, and multi-platform (Android) requirements.

**Version**: 1.0.0 | **Ratified**: 2026-01-26 | **Last Amended**: 2026-01-26
