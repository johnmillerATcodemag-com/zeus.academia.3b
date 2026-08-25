---
ai_generated: true
model: "github/copilot@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "78bccef1-7df6-4b32-99b6-2cc4a743aecc"
prompt: |
  Create a new implementation prompt that follows the shared kernel prompt that creates the Application Host and Persistence Composition.
started: "2026-08-24T16:25:00-07:00"
ended: "2026-08-24T16:35:00-07:00"
task_durations:
  - task: "define Phase 0 host and persistence boundary"
    duration: "00:04:00"
  - task: "define context and migration ownership"
    duration: "00:04:00"
  - task: "define verification and handoff gates"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/08/24/78bccef1-7df6-4b32-99b6-2cc4a743aecc/conversation.md"
source: ".github/prompts/academia-implementation/ep-0-1-shared-kernel-implementation.prompt.md"
name: implement-application-host-and-persistence-composition
description: Create the application host, dependency-injection composition, endpoint registration, SQL Server configuration, and migration execution boundary after Shared Kernel
author: John Miller
tags: [academia, implementation, application-host, persistence, migrations]
context: "Zeus Academia Phase 0 application composition and isolated feature persistence"
expected_output: "An implementation-ready work plan for the application host and persistence composition with explicit ownership, migration, verification, and handoff rules"
tools: ["read", "search", "edit", "agent"]
mode: agent
---

# Implement Application Host and Persistence Composition

## Slice Summary and Business Value

- Slice: Application Host and Persistence Composition
- Business outcome: provide the executable composition root that registers feature-local DbContexts and handlers, exposes feature endpoints, configures SQL Server, and applies the correct migrations.
- Required predecessor: Shared Kernel.
- Out of scope: business behavior inside ManageRanks, ManageDegrees, ManageUniversities, ProvisionExtension, or later academic slices.

## Context Files to Review First

- `.github/prompts/academia-implementation/ep-0-1-shared-kernel-implementation.prompt.md`
- `.github/models/workflows/academia-execution-plan.md`
- `.github/models/workflows/academia-implementation-plan.md`
- `.github/instructions/project-overview.instructions.md`
- `.github/instructions/vertical-slice-implementation.instructions.md`
- `.github/instructions/aspnetcore-implementation.instructions.md`
- `.github/instructions/cqrs-mediatr-efcore.instructions.md`
- `.github/instructions/csharp-implementation.instructions.md`
- Existing feature projects under `src/features/ReferenceData/`
- Existing Shared Kernel project under `src/features/SharedKernel/Foundation/`

## Prerequisites and Dependency Checks

- Shared Kernel builds and its invariant tests pass.
- The repository target is .NET 8 and SQL Server.
- The current repository may not yet contain an API host, so confirm whether to create the host project or adopt an existing equivalent before editing.
- Existing feature-local DbContext patterns are authoritative for ManageRanks and ManageDegrees.
- No two DbContexts may own migrations for the same table.

## Assigned Agents and Role Boundaries

| Role                    | Agent                  | Responsibility                                                                                                        | Inputs                                          | Outputs                                                    | Escalate when                                                   |
| ----------------------- | ---------------------- | --------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ---------------------------------------------------------- | --------------------------------------------------------------- |
| Scope and composition   | `slice-coordinator`    | Confirm host project, Phase 0 boundary, context inventory, migration owners, and non-overlap rules                    | Shared Kernel, feature projects, execution plan | Approved composition map and blocker list                  | No host project or table owner can be identified                |
| Host implementation     | `backend-domain`       | Implement `Program.cs`, service registration, endpoint registration, middleware, and host health/configuration wiring | Approved composition map, ASP.NET standards     | Executable host and DI composition                         | Required framework or authentication configuration is undefined |
| Persistence composition | `data-persistence`     | Register feature-local contexts, configure SQL Server, create design-time factories, and apply migration ownership    | Context inventory, SQL Server rules             | DbContext registration, migration roots, design-time setup | Multiple contexts claim one table or migration output conflicts |
| Verification            | `testing-verification` | Verify startup, routes, context registration, migration application, and failure diagnostics                          | Implemented host and persistence composition    | Tests, command output, and residual-risk report            | Environment prerequisites prevent meaningful verification       |

## Ordered Implementation Steps

1. Confirm host and context inventory.
   Targets: solution, existing project files, `src/features/**`, tests, and workflow plans.
   Owner: `slice-coordinator`.
   Validation before next step: the host project path, endpoint registration location, every DbContext, table ownership, and migration owner are recorded.
2. Create or complete the application host.
   Targets: host project, `Program.cs`, configuration files, and solution registration.
   Owner: `backend-domain`.
   Validation before next step: the host builds, uses explicit SQL Server configuration, and has no feature behavior embedded in the composition root.
3. Register feature-local contexts and handlers.
   Targets: host DI composition, MediatR/FluentValidation registration, and feature project references.
   Owner: `data-persistence` with `backend-domain`.
   Validation before next step: ManageRanks, ManageDegrees, ManageUniversities, and ProvisionExtension each resolve through their own feature-local context; Shared Kernel types remain reusable dependencies rather than host-owned behavior.
4. Establish migration execution and design-time ownership.
   Targets: each owning feature migration root, design-time factories, host startup/migration command, and SQL Server configuration.
   Owner: `data-persistence`.
   Validation before next step: one migration owner is recorded per table; `Extensions` is owned by `ProvisionExtensionDbContext`; no Shared Kernel migration competes for `Extensions`; non-Windows execution requires `ZEUS_SQLSERVER_CONNECTION`.
5. Register endpoint groups.
   Targets: host route registration and feature endpoint aggregators.
   Owner: `backend-domain`.
   Validation before next step: route registration is explicit for each available feature and does not modify unrelated slice behavior.
6. Verify composition end to end.
   Targets: host tests, model tests, migration checks, route checks, and verification scripts.
   Owner: `testing-verification`.
   Validation before next step: startup, dependency resolution, route discovery, migration generation/application, and actionable setup failures are proven.

## Context and Migration Ownership Matrix

| Concern                                            | Owner                                                   |
| -------------------------------------------------- | ------------------------------------------------------- |
| Shared domain types and reusable mapping semantics | Shared Kernel                                           |
| Application startup and DI composition             | Application Host                                        |
| Rank table and migrations                          | ManageRanks feature context                             |
| Degree table and migrations                        | ManageDegrees feature context                           |
| University table and migrations                    | ManageUniversities feature context                      |
| Extensions table and migrations                    | `ProvisionExtensionDbContext`                           |
| Migration application orchestration                | Application Host or explicitly named deployment command |

## Verification and Acceptance Criteria

- The host starts with explicit SQL Server configuration and fails with actionable diagnostics when required non-Windows configuration is absent.
- Each persistence-bearing feature has an explicitly named feature-local DbContext.
- `ProvisionExtensionDbContext` maps the Shared Kernel `Extension` entity and reuses its configuration semantics.
- `ProvisionExtensionDbContext` is the sole migration owner for `Extensions`.
- Shared Kernel does not create competing feature migrations and does not contain host startup code.
- Endpoint registration is located in the host composition boundary and invokes feature endpoint aggregators.
- Design-time and runtime DbContext configuration use the same SQL Server and platform-guard rules.
- Migration artifacts are complete as a set: migration class, Designer metadata, and model snapshot.
- No duplicate project declarations or duplicate migration ownership exists.
- Tests and verification commands fail explicitly when SQL Server prerequisites are unavailable; they do not silently skip.
- Existing Shared Kernel tests and all touched feature tests pass.

## Human Showcase Steps

1. Start the application host with the documented SQL Server configuration.
2. Confirm dependency resolution for each feature-local DbContext.
3. Apply or validate migrations and inspect that each table has one migration owner.
4. Discover the registered reference-data routes.
5. Confirm that `Extensions` is mapped through `ProvisionExtensionDbContext` while the domain entity remains in Shared Kernel.

## Completion Checklist

- [ ] Shared Kernel remains free of application-host startup concerns.
- [ ] Host project and route-registration location are documented.
- [ ] Every feature-local DbContext and migration owner is documented.
- [ ] `ProvisionExtensionDbContext` exclusively owns `Extensions` migrations.
- [ ] Runtime and design-time SQL Server configuration agree.
- [ ] Complete migration metadata is committed for each schema-changing context.
- [ ] No duplicate DbContext migration ownership exists.
- [ ] Host, route, migration, and prerequisite verification evidence is captured.
