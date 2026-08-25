---
ai_generated: true
model: "github/copilot@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "78bccef1-7df6-4b32-99b6-2cc4a743aecc"
prompt: |
  write the maps to two markdown files. put them in the same folder as the prompts
started: "2026-08-24T16:16:14-07:00"
ended: "2026-08-24T16:22:00-07:00"
task_durations:
  - task: "review existing slice and Shared Kernel ownership"
    duration: "00:04:00"
  - task: "write independent execution maps"
    duration: "00:04:00"
  - task: "validate artifact links and scope"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/08/24/78bccef1-7df6-4b32-99b6-2cc4a743aecc/conversation.md"
source: ".github/prompts/academia-implementation/ep-1-3-manage-universities-implementation.prompt.md"
description: "Parallel execution map for the ManageUniversities slice"
---

# EP-1-3 ManageUniversities Execution Map

## Execution Status

Planning only. Do not implement this slice yet. The Shared Kernel identity prerequisite is resolved; this track remains independently reviewable and must not modify ProvisionExtension files.

## Prerequisite Evidence

Shared Kernel is available at `src/features/SharedKernel/Foundation/`. Its focused test project passed with 20 tests and zero failures.

Relevant existing types and persistence:

- `src/features/SharedKernel/Foundation/Domain/University.cs`
- `src/features/SharedKernel/Foundation/Domain/AcademicQualification.cs`
- `src/features/SharedKernel/Foundation/Domain/SharedKernelFieldLengths.cs`
- `src/features/SharedKernel/Foundation/Persistence/SharedKernelDbContext.cs`
- `src/features/ReferenceData/ManageDegrees/` as the nearest reference-data implementation pattern

The feature project, DbContext, service-registration helper, resolution contract, and handoff notes already exist. The DbContext currently contains a placeholder `UniversityRecord`; no university handlers, endpoints, tests, or migrations exist yet.

## Placeholder-Entity Blocker

This track must not proceed with a placeholder `UniversityRecord` model. A placeholder entity is not an implementation and is not considered valid for migration or startup verification.

Before implementation is considered ready:

1. `UniversityRecord` must define `Code` as the primary key.
2. `ManageUniversitiesDbContext` must configure the entity via a real `UniversityRecordConfiguration` using `HasKey(x => x.Code)`.
3. The code normalization length and casing rules must be explicitly approved and reused consistently.
4. The SQL Server or LocalDB verification path must fail fast when the environment is unavailable rather than silently passing through the host or migration step.

## Artifact Map

| Surface                   | Owned artifacts                                                                                                                                                                            |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Feature root              | `src/features/ReferenceData/ManageUniversities/`                                                                                                                                           |
| Add use case              | `AddUniversity/AddUniversityCommand.cs`, `AddUniversityCommandValidator.cs`, `AddUniversityHandler.cs`, `AddUniversityResponse.cs`, `AddUniversityEndpoint.cs`, `AddUniversityMappings.cs` |
| List use case             | `ListUniversities/ListUniversitiesQuery.cs`, `ListUniversitiesHandler.cs`, `ListUniversitiesResponse.cs`, `ListUniversitiesEndpoint.cs`                                                    |
| Slice support             | `Shared/UniversityRecord.cs`, canonical university-code catalog, conflict exception, `ManageUniversitiesDbContext.cs`, EF configuration                                                    |
| Project                   | `Zeus.Academia.Features.ReferenceData.ManageUniversities.csproj`                                                                                                                           |
| Tests                     | `tests/Features/ReferenceData/ManageUniversities/` with validator, add-handler, list-handler, model, and project files                                                                     |
| Optional schema artifacts | Migration class, Designer metadata, and model snapshot under the chosen migration root, only after migration ownership is confirmed                                                        |

## Route Decision

Use the existing reference-data route family:

- `POST /api/reference-data/universities`
- `GET /api/reference-data/universities`

The route group should follow the ManageDegrees feature pattern. The API host exists at `src/Zeus.Academia.Api/Program.cs`; host DI and migration registration are now present, while route-group composition remains a separate integration change and is not owned by this track during parallel execution.

## Host Composition Status

The host project now references and registers the ManageUniversities project, its DbContext, and its MediatR assembly, and invokes `ManageUniversitiesDbContext.Database.MigrateAsync()`. The host does not yet map ManageUniversities endpoints because this slice has not produced an endpoint aggregator. Route mapping remains coordinator-owned and must be added only after the endpoint contract is implemented.

## Schema Decision

Create a standalone reference-data `Universities` table owned by `ManageUniversitiesDbContext`; the context already exists and its project is registered in the solution. The host still needs coordinated registration and migration execution.

- Primary key: `Code`
- Value: normalized, canonical university code
- Required field with the reconciled canonical maximum length
- Uniqueness supplied by the primary key; do not add a duplicate unique index on `Code`
- The catalog record is not a second Shared Kernel `University` domain type

The Shared Kernel identity correction is complete and is outside the EP-1-3 implementation track. Do not make further Shared Kernel changes from this track without an explicit coordination decision.

## Canonical Identity Decision

The approved resolution contract is now implemented: `University.Code` is the immutable, normalized identity; `SharedKernelFieldLengths.UniversityCode` is 20; and `AcademicQualification` persists `UniversityCode`. `UniversityRecord.Code` must map to this identity, while `Name` remains descriptive metadata. EP-1-3 must reuse these Shared Kernel rules rather than define another normalization or length rule.

## Required Behavior

- Add accepts only a valid canonical university code.
- Whitespace and casing normalization has one owner and is reused by the validator, mapping, error messages, and EF configuration.
- Invalid input identifies the `Code` property.
- Duplicate codes fail without partial persistence.
- List returns stable deterministic ordering.
- The slice remains reference-data only; it does not assign universities to qualifications.

## Verification Map

- Validator tests: required, whitespace, length, normalization, and invalid-code behavior.
- Add-handler tests: success, duplicate rejection, and no duplicate persistence.
- List-handler tests: deterministic ordering and response shape.
- Model tests: primary key, field length, requiredness, and absence of duplicate PK unique index.
- SQL Server verification: generated schema or migration output must be checked against the target provider.
- Integration tests that provision a database must use an isolated test database and best-effort cleanup in `finally` blocks.

## Allowed File Set

This track may change only:

- `src/features/ReferenceData/ManageUniversities/**`
- `tests/Features/ReferenceData/ManageUniversities/**`
- Its own project entry in the solution, if project registration is required
- Its own migration artifacts, after migration ownership is confirmed
- No host-registration or migration-composition files; `src/Zeus.Academia.Api/Program.cs` is coordinator-owned

A placeholder `UniversityRecord`, empty persistence model, or stub configuration is not allowed in any committed implementation.

This track must not change any ProvisionExtension file, Shared Kernel file, existing ManageRanks or ManageDegrees file, `Program.cs`, the solution file, or shared migration/snapshot file without an explicit coordination decision.

## Handoff Gate

Ready for implementation only after the coordinator records:

1. Confirm the implementation reuses `University.Code` and the canonical length of 20.
2. Confirmation that `ManageUniversitiesDbContext` owns the `Universities` migration root.
3. Host route-registration and migration-composition location in `Program.cs`.
4. An explicit confirmation that the allowed file set does not overlap the ProvisionExtension track.
