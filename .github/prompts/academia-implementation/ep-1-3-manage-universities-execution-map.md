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

Planning only. Do not implement this slice until the identity and persistence blockers below are resolved. This track is independently reviewable and must not modify ProvisionExtension files or Shared Kernel files.

## Prerequisite Evidence

Shared Kernel is available at `src/features/SharedKernel/Foundation/`. Its focused test project passed with 26 tests and zero failures.

Relevant existing types and persistence:

- `src/features/SharedKernel/Foundation/Domain/University.cs`
- `src/features/SharedKernel/Foundation/Domain/AcademicQualification.cs`
- `src/features/SharedKernel/Foundation/Domain/SharedKernelFieldLengths.cs`
- `src/features/SharedKernel/Foundation/Persistence/SharedKernelDbContext.cs`
- `src/features/ReferenceData/ManageDegrees/` as the nearest reference-data implementation pattern

No university feature folder, university catalog record, university handler, university endpoint, university test project, or migration currently exists.

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

The route group should follow `src/features/ReferenceData/ManageDegrees/ManageDegreesEndpoints.cs`. There is currently no API host or route-registration file in the inspected source tree, so host registration is a separate integration decision and must not be invented inside this map.

## Schema Decision

Create a standalone reference-data `Universities` table owned by this slice if the repository's migration strategy confirms that feature-local reference-data contexts are persisted independently.

- Primary key: `Code`
- Value: normalized, canonical university code
- Required field with the shared maximum length
- Uniqueness supplied by the primary key; do not add a duplicate unique index on `Code`
- The catalog record is not a second Shared Kernel `University` domain type

Do not modify `University.cs`, `AcademicQualification.cs`, `SharedKernelDbContext.cs`, or any Shared Kernel persistence configuration in this track.

## Canonical Identity Blocker

The Shared Kernel currently exposes `University.Name`, while the ORM and workflow contract define `University_code`. Before implementation, the coordinator must decide whether `Name` is intentionally the persisted code or whether a future coordinated Shared Kernel change is needed. The slice must not silently create a second normalization rule or claim downstream code compatibility without that decision.

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
- One coordinated host-registration file only if the host is identified and both tracks agree before editing it

This track must not change any ProvisionExtension file, Shared Kernel file, existing ManageRanks or ManageDegrees file, or shared migration/snapshot file without an explicit coordination decision.

## Handoff Gate

Ready for implementation only after the coordinator records:

1. `University_code` versus `University.Name` identity resolution.
2. Owning DbContext and migration root.
3. Host route-registration location.
4. An explicit confirmation that the allowed file set does not overlap the ProvisionExtension track.
