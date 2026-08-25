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

### 5. Reconcile University Identity

Owner: ManageUniversities coordinator and Shared Kernel/domain reviewer.

- Treat `UniversityRecord.Code` as the catalog identifier.
- Do not use `University.Name` as `University_code`.
- Decide how future qualification flows translate a catalog code into the Shared Kernel `University` value object.
- Add an explicit contract before RegisterAcademic or RecordDegreeObtained consumes the catalog.

Gate: the catalog code, value-object representation, and qualification persistence field are documented without duplicate normalization ownership.

### 6. Update Downstream Consumers

Owner: each dependent slice when implemented.

- RegisterAcademic resolves university codes through ManageUniversities.
- AssignExtension and later extension slices resolve extensions through the feature-local ProvisionExtension context.
- Academic and qualification aggregates continue to consume Shared Kernel domain types and guards.
- No later slice references `SharedKernelDbContext` merely to access an extension.

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

1. Shared Kernel tests pass.
2. All feature projects build with isolated context references.
3. Host resolves every registered context and endpoint group.
4. SQL Server design-time configuration matches runtime configuration.
5. Migration ownership checks find exactly one owner per table.
6. `Extensions` migration output comes only from `ProvisionExtensionDbContext`.
7. University catalog tests use `Code`, not `University.Name`.
8. Downstream integration tests prove reference-data resolution without private cross-feature references.
9. Integration resources are cleaned up in `finally` blocks.
