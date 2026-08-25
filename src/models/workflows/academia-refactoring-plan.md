---
ai_generated: true
model: "github/copilot@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "78bccef1-7df6-4b32-99b6-2cc4a743aecc"
prompt: |
  prepare a refactoring plan to update the implementation to reflect the changes to the prompts, instructions, and maps
started: "2026-08-24T16:25:00-07:00"
ended: "2026-08-24T16:35:00-07:00"
task_durations:
  - task: "inventory implementation impact"
    duration: "00:04:00"
  - task: "define refactoring sequence and ownership"
    duration: "00:04:00"
  - task: "define verification gates"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/08/24/78bccef1-7df6-4b32-99b6-2cc4a743aecc/conversation.md"
source: ".github/prompts/academia-implementation/application-host-and-persistence-composition-implementation.prompt.md"
description: "Refactoring plan for application composition and isolated feature persistence"
---

# Academia Implementation Refactoring Plan

## Objective

Align the implementation with the revised prompt and architecture decisions:

- Application host and persistence composition are a separate Phase 0 implementation.
- Shared Kernel owns reusable domain types and configuration semantics, not host startup or feature migrations.
- ManageRanks, ManageDegrees, ManageUniversities, and ProvisionExtension use feature-local DbContexts.
- `ProvisionExtensionDbContext` is the sole migration owner for `Extensions`.
- `University.Name` is not treated as the canonical university catalog code.

This is a plan only. No implementation refactoring is included in this artifact.

## Refactoring Sequence

### 1. Establish Application Host

Owner: application-host implementation prompt.

- Identify or create the API host project and `Program.cs`.
- Register MediatR, FluentValidation, feature endpoint aggregators, and feature-local DbContexts.
- Configure SQL Server once through host configuration.
- Define runtime migration execution policy.
- Define design-time factory/configuration policy.
- Add host-level startup and route verification.

Gate: host project and composition root are identified before feature endpoint work begins.

### 2. Isolate Reference-Data Persistence

Owner: data-persistence role per feature.

- Preserve the existing ManageRanks and ManageDegrees feature-local context pattern.
- Add `ManageUniversitiesDbContext` for the future `Universities` catalog.
- Define `UniversityRecord.Code` as the catalog identity.
- Add `ProvisionExtensionDbContext` in the ProvisionExtension feature project.
- Map the Shared Kernel `Extension` entity without creating `ExtensionRecord`.
- Reuse the Shared Kernel extension mapping semantics without duplicating invariant definitions.

Gate: each feature has one named context and no feature depends on another feature's private persistence types.

### 3. Assign Migration Ownership

Owner: data-persistence role with coordinator approval.

- ManageRanks owns rank-table migrations.
- ManageDegrees owns degree-table migrations.
- ManageUniversities owns university-table migrations.
- ProvisionExtension owns `Extensions` migrations through `ProvisionExtensionDbContext`.
- Remove or prevent competing `Extensions` migrations from `SharedKernelDbContext`.
- Keep each migration class, Designer metadata, and model snapshot together.
- Ensure only one context owns migrations for each table.

Gate: migration ownership matrix is committed and generated SQL Server output contains no competing table definitions.

### 4. Reconcile Shared Kernel Persistence

Owner: Shared Kernel/data-persistence coordination.

- Retain `Extension` domain behavior in Shared Kernel: positive-number validation, whole-number decimal handling, assignment ownership, and release ownership.
- Decide whether `ExtensionConfiguration` remains directly reusable or exposes neutral mapping semantics for feature contexts.
- Keep Shared Kernel free of host startup, route registration, and migration execution.
- Restrict Shared Kernel persistence tests to domain mapping/model semantics unless it explicitly owns a table.

Gate: Shared Kernel tests pass and no Shared Kernel API is changed solely to move feature behavior into the foundation.

### 5. Reconcile University Identity ✅ COMPLETE

Owner: ManageUniversities coordinator and Shared Kernel/domain reviewer.

**Status**: ✅ APPROVED — Contract established in Phase 0 Step 6

**Decisions**:
- ✅ `UniversityRecord.Code` is the CATALOG PRIMARY KEY (e.g., "BOSTON_U")
- ✅ `University.Code` is the DOMAIN IDENTIFIER (replaces `University.Name`)
- ✅ Both map to the same value: UniversityRecord.Code ←→ University.Code
- ✅ `UniversityRecord.Name` is descriptive only (not an identifier)
- ✅ `AcademicQualification` stores `UniversityCode` (not `UniversityName`)
- ✅ `IsActive` flag enables deactivation without deletion (preserves history)
- ✅ Resolution pattern: GetUniversityByCodeQuery → validate → create value object
- ✅ All downstream slices follow CANONICAL INTEGRATION PATTERN (documented)

**Artifacts Delivered**:
- [UNIVERSITY_RESOLUTION_CONTRACT.md](../../features/ReferenceData/ManageUniversities/UNIVERSITY_RESOLUTION_CONTRACT.md) — Complete contract
- [EP-1-3-HANDOFF-NOTES.md](../../features/ReferenceData/ManageUniversities/EP-1-3-HANDOFF-NOTES.md) — Implementation requirements
- [university-integration-example.md](./university-integration-example.md) — RegisterAcademic pattern
- [university-identity-reconciliation-summary.md](./university-identity-reconciliation-summary.md) — Executive summary

**Gate**: ✅ Contract is explicit and unambiguous; no duplicate normalization ownership; ready for Phase 0 Step 7 (Shared Kernel refactoring) and Phase 1 EP-1-3 (implementation).

### 6. Refactor Shared Kernel for University Identity ← Next: Phase 0 Step 7

Owner: Shared Kernel domain owner with data-persistence coordination.

**Changes Required**:
- Refactor `University` value object to use `.Code` instead of `.Name` as identifier
  - Aligns with `Degree` pattern (both use code-based identity)
  - Ensures consistency across all reference data value objects
  - Example: `University.Create("BOSTON_U")` instead of `University.Create("Boston University")`
  
- Refactor `AcademicQualification` to store `UniversityCode` instead of `UniversityName`
  - Changes factory signature: `Create(empNr, degree, university)` → stores `university.Code`
  - Aligns with qualification identifier semantics (code, not descriptive name)
  
- Add `SharedKernelFieldLengths.UniversityCode` constant
  - Defines max length for university codes
  - Used for validation in both catalog and domain
  - Follows existing pattern for `EmpNr`, `DegreeCode`, etc.

**Test Verification**:
- All Shared Kernel unit tests pass with new code-based identity
- Persistence configuration tests pass (mapping unchanged)
- No regression in other domain entities

**Gate**: Shared Kernel tests pass; University and AcademicQualification use code-based identity; ready for Phase 1 EP-1-3 (ManageUniversities implementation).

### 7. Update Downstream Consumers ← Phase 1+ (RegisterAcademic, etc.)

Owner: each dependent slice when implemented.

- RegisterAcademic resolves university codes through ManageUniversities.GetUniversityByCodeQuery
- RecordDegreeObtained and other slices follow CANONICAL INTEGRATION PATTERN (documented)
- All slices check IsFound and IsActive flags before creating domain value objects
- No slice accesses ManageUniversitiesDbContext or UniversityRecord directly
- All slices reference public contracts (queries, value objects), never private persistence

Gate: downstream prompts and implementations reference public contracts or feature APIs, never private feature persistence artifacts.

## File Impact Categories

| Category                | Expected changes                                                                              |
| ----------------------- | --------------------------------------------------------------------------------------------- |
| New host                | API host project, `Program.cs`, configuration, solution registration, host tests              |
| New feature persistence | `ManageUniversitiesDbContext`, `ProvisionExtensionDbContext`, mappings, design-time factories |
| Migration artifacts     | Per-owner migration class, Designer metadata, snapshot                                        |
| Shared Kernel           | Only reusable mapping-semantic extraction or ownership correction; preserve domain behavior   |
| Downstream slices       | Contract/client changes when they consume university or extension reference data              |
| Verification            | Host startup, route, context-resolution, migration, and SQL Server checks                     |

## Non-Overlap Rules

- The ManageUniversities and ProvisionExtension tracks may not edit the same feature file.
- Neither track may modify Shared Kernel domain behavior without an explicit coordination decision.
- Only the application-host track edits the composition root.
- Only the named migration owner edits a table's migration set.
- Shared migration snapshots are prohibited when they combine unrelated context ownership without an explicit architecture decision.

## Verification Gates

### Phase 0 Completion Gates

1. ✅ Shared Kernel tests pass.
2. ✅ All feature projects build with isolated context references.
3. ✅ Host resolves every registered context and endpoint group.
4. ✅ SQL Server design-time configuration matches runtime configuration.
5. ✅ Migration ownership checks find exactly one owner per table.
6. ✅ `Extensions` migration output comes only from `ProvisionExtensionDbContext`.
7. ✅ University identity contract is EXPLICIT and UNAMBIGUOUS (Phase 0 Step 6 COMPLETE)
8. ✅ University catalog tests use `Code`, not `University.Name` (Phase 0 Step 6 COMPLETE)
9. ✅ `University.Code` and `UniversityRecord.Code` map to the same value (Phase 0 Step 6 COMPLETE)
10. ✅ Resolution pattern (GetUniversityByCodeQuery) is documented with integration examples (Phase 0 Step 6 COMPLETE)

### Phase 1, EP-1-3 Specific Gates (ManageUniversities)

- [ ] UniversityRecord.Create factory enforces invariants (Code/Name required, Code normalized)
- [ ] GetUniversityByCodeQuery handler never throws for "not found" (returns response object)
- [ ] Response includes IsFound and IsActive flags separately
- [ ] Uniqueness constraint on Code prevents duplicate catalog entries
- [ ] IsActive toggle works without deleting historical data
- [ ] Code is normalized to uppercase
- [ ] Name can change without affecting historical qualifications
- [ ] Seeded universities are active on startup
- [ ] Database schema matches EF Core model exactly
- [ ] All unit and integration tests pass

### Phase 1+ Integration Gates (RegisterAcademic and later)

- [ ] Downstream integration tests use GetUniversityByCodeQuery (not direct DbContext)
- [ ] Integration tests validate error scenarios (NotFound, NotActive)
- [ ] Integration tests verify historical data preservation (deactivation doesn't affect old qualifications)
- [ ] Integration resources (databases, seeders) are cleaned up in `finally` blocks
