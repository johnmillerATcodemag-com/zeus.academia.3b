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
source: ".github/prompts/academia-implementation/ep-1-4-provision-extension-implementation.prompt.md"
description: "Parallel execution map for the ProvisionExtension slice"
---

# EP-1-4 ProvisionExtension Execution Map

## Execution Status

Planning only. Do not implement this slice yet. This track is independently reviewable and must not duplicate the Shared Kernel extension model or modify the ManageUniversities track.

## Prerequisite Evidence

Shared Kernel is available at `src/features/SharedKernel/Foundation/`. Its focused test project passed with 20 tests and zero failures.

The authoritative extension model already exists:

- `src/features/SharedKernel/Foundation/Domain/Extension.cs`
- `src/features/SharedKernel/Foundation/Persistence/ExtensionConfiguration.cs`
- `src/features/SharedKernel/Foundation/Persistence/SharedKernelDbContext.cs`

The existing model uses `Extension.Number` as the key and `AssignedEmpNr` as assignment state. The ProvisionExtension feature project, feature-local DbContext, and service-registration helper already exist. No provisioning or deprovisioning handler, endpoint, test project, migration, or separate extension pool model exists.

## Artifact Map

| Surface                | Owned artifacts                                                                                                                                                               |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Feature root           | `src/features/Extensions/ProvisionExtension/`                                                                                                                                 |
| Provision use case     | `ProvisionExtensionCommand.cs`, validator, handler, response, endpoint, mappings                                                                                              |
| Deprovision use case   | `DeprovisionExtensionCommand.cs`, validator if required, handler, response, endpoint                                                                                          |
| Project                | `Zeus.Academia.Features.Extensions.ProvisionExtension.csproj`                                                                                                                 |
| Tests                  | `tests/Features/Extensions/ProvisionExtension/` with validator, provision, duplicate, deprovision, assignment-guard, and model/integration tests                              |
| Persistence            | Feature-local `ProvisionExtensionDbContext` maps the Shared Kernel `Extension` entity and reuses its configuration semantics; ProvisionExtension owns `Extensions` migrations |
| Prohibited persistence | No feature-local `ExtensionRecord`, duplicate `Extensions` table, duplicate index, or competing Shared Kernel migration                                                       |

## Route Decision

Use the reference-data route family:

- `POST /api/reference-data/extensions`
- `DELETE /api/reference-data/extensions/{number}`

This slice does not add an available-extension list endpoint. `ListAvailableExtensions` remains a later slice. The API host exists at `src/Zeus.Academia.Api/Program.cs`; host DI and migration registration are now present, while route-group composition remains a separate integration change and is not owned by this track during parallel execution.

## Host Composition Status

The host project now references and registers the ProvisionExtension project, its DbContext, and its MediatR assembly, and invokes `ProvisionExtensionDbContext.Database.MigrateAsync()`. The host does not yet map ProvisionExtension endpoints because this slice has not produced an endpoint aggregator. Route mapping remains coordinator-owned and must be added only after the endpoint contract is implemented.

## Schema Decision

Reuse the existing Shared Kernel extension entity and configuration semantics through a feature-local `ProvisionExtensionDbContext`:

- Table: `Extensions`
- Primary key: `Number`
- Assignment column: `AssignedEmpNr`
- Existing filtered unique index on `AssignedEmpNr` remains authoritative
- `ProvisionExtensionDbContext` is the sole migration owner for `Extensions`
- No duplicate feature-local entity, table, primary key, or assignment index

The command may accept a numeric decimal representation because the workflow contract uses `extNr` numeric decimal, but only positive whole values are valid. Normalize and range-check at the command/domain boundary, then persist the canonical `int` representation used by `Extension.Number`. Fractional values must be rejected rather than truncated.

## Required Behavior

- Provision creates one positive, whole-number extension.
- Provisioning the same number twice fails without a duplicate record.
- Deprovisioning an unassigned extension removes it from the pool.
- Deprovisioning an assigned extension fails and preserves both the extension and assignment.
- Assignment ownership remains governed by `Extension.AssignTo` and `Extension.ReleaseFrom`.
- Persistence-conflict translation is narrow: only a proven duplicate-number conflict may become a business conflict; unrelated database failures must be rethrown.
- ProvisionExtension remains pool lifecycle behavior and does not assign extensions to academics.

## Verification Map

- Validator tests: positive whole-number acceptance, fractional rejection, range handling, and actionable property failures.
- Provision-handler tests: success, duplicate rejection, and no partial persistence.
- Deprovision-handler tests: unassigned success, missing extension behavior, and assigned-extension rejection.
- Domain tests: preserve the existing Extension ownership guards; add direct boundary tests only if public APIs are changed.
- Model tests: verify the reused Shared Kernel key and filtered assignment index rather than introducing a second model.
- SQL Server verification: generated schema or migration output must be checked against the target provider.
- Integration tests that provision a database must use an isolated test database and best-effort cleanup in `finally` blocks.

## Allowed File Set

This track may change only:

- `src/features/Extensions/ProvisionExtension/**`
- `tests/Features/Extensions/ProvisionExtension/**`
- Its own project entry in the solution, if project registration is required
- Its own migration artifacts only when a confirmed schema change belongs to this slice
- No host-registration or migration-composition files; `src/Zeus.Academia.Api/Program.cs` is coordinator-owned

This track must not change `Extension.cs`, `ExtensionConfiguration.cs`, `SharedKernelDbContext.cs`, Shared Kernel tests, any ManageUniversities file, `Program.cs`, the solution file, or shared migration/snapshot files without an explicit coordination decision. It must use the existing `ProvisionExtensionDbContext` inside its own feature tree.

## Handoff Gate

Ready for implementation only after the coordinator records:

1. Host route-registration and migration-composition location in `Program.cs`.
2. Confirmation that `ProvisionExtensionDbContext` is the sole migration owner for `Extensions` and whether any existing Shared Kernel schema is already deployed.
3. The exact request representation for decimal input and its conversion to `int`.
4. An explicit confirmation that the allowed file set does not overlap the ManageUniversities track.
