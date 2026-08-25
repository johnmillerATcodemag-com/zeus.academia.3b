---
ai_generated: true
model: "openai/gpt-5.4@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "616990b5-0c5d-4735-a876-23fd1ebb4ff6"
prompt: |
  Create an implementation prompt for each slice in the #file:academia-execution-plan.md
started: "2026-04-20T20:40:00Z"
ended: "2026-04-20T21:40:00Z"
task_durations:
  - task: "analyze slice dependencies"
    duration: "00:15:00"
  - task: "draft slice implementation prompt"
    duration: "00:35:00"
  - task: "traceability and review"
    duration: "00:10:00"
total_duration: "01:00:00"
ai_log: "ai-logs/2026/04/20/616990b5-0c5d-4735-a876-23fd1ebb4ff6/conversation.md"
source: ".github/models/workflows/academia-execution-plan.md"
name: implement-academia-ep-0-1-shared-kernel
description: Implement the Shared Kernel foundation for Zeus Academia before slice delivery starts
author: John Miller
tags: [academia, implementation, shared-kernel, cqrs, domain]
context: "Zeus Academia vertical-slice delivery plan and shared-kernel foundation"
expected_output: "An implementation-ready work plan for the Shared Kernel with explicit roles, ordered steps, acceptance criteria, and showcase steps"
tools: ["read", "search", "edit", "agent"]
mode: agent
---

# Implement Shared Kernel

## Slice Summary and Business Value

- Slice: Shared Kernel
- Business outcome: establish the domain primitives, invariants, result types, and persistence constraints that every later slice depends on.
- Out of scope: application host creation, endpoint registration, dependency-injection composition, authentication, migration execution, feature endpoints, UI flows, reporting queries, and seed data beyond what is needed to validate foundational constraints. These concerns are owned by the application-host setup implementation prompt.

## Context Files to Review First

- .github/models/workflows/academia-execution-plan.md
- .github/models/workflows/academia-implementation-plan.md
- .github/instructions/project-overview.instructions.md
- .github/instructions/vertical-slice-implementation.instructions.md
- Follow the vertical-slice instructions and keep the implementation in a feature/use-case folder under `src/features/` with co-located command/query, validator, endpoint, and tests instead of splitting the slice across layer-oriented folders.
- .github/instructions/csharp-implementation.instructions.md
- .github/instructions/xunit-implementation.instructions.md
- .github/instructions/cqrs-mediatr-efcore.instructions.md

## Prerequisites and Dependency Checks

- Required prior slices: none
- Blocking risks: feature-root or persistence-root naming may differ from the plan; confirm the actual backend root before creating files.
- Existing patterns to reuse: nullable-enabled C#, Result/Error wrapper, domain event abstraction, reusable EF Core configuration semantics, EF Core uniqueness constraints, and aggregate guard methods.

## Assigned Agents and Role Boundaries

| Role                 | Responsibilities                                                                | Inputs                                                   | Outputs                                   | Escalate when                                                                                        |
| -------------------- | ------------------------------------------------------------------------------- | -------------------------------------------------------- | ----------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| slice-coordinator    | confirm folder roots, final type list, and sequence                             | execution plan, implementation plan, current source tree | approved artifact map and blocker list    | current repo layout conflicts with the planned SharedKernel location                                 |
| backend-domain       | implement aggregate, value objects, result types, exceptions, and domain events | approved artifact map, business rules                    | domain types and invariant logic          | a rule cannot be expressed cleanly without clarifying the aggregate boundary                         |
| data-persistence     | implement reusable EF Core mappings and invariant backing semantics             | domain model, persistence standards                      | mappings, constraints, ownership contract | a database rule would drift from the aggregate rule                                                  |
| testing-verification | add invariant tests, mapping tests, and migration validation evidence           | implemented kernel artifacts                             | passing tests and proof of enforced rules | tests expose ambiguity in employment-rule semantics, access-level derivation, or qualification rules |

## Ordered Implementation Steps

1. Confirm the Shared Kernel boundary and file roots.
   Targets: src/backend/SharedKernel/, persistence project root, and tests/ root or current equivalents.
   Owner: slice-coordinator.
   Validation before next step: artifact list is approved for Academic, Rank, AccessLevel, Degree, University, Extension, AcademicQualification, Result<T>, Error, domain events, and common exceptions.
2. Implement the domain model and invariant methods.
   Targets: Shared Kernel aggregate and value-object files, especially Academic employment guards and Rank to AccessLevel derivation.
   Owner: backend-domain.
   Validation before next step: the aggregate enforces the employment mutual-exclusion rule (never both tenured and contracted), AccessLevel is derived only from Rank, extension assignment cannot overwrite a different existing assignment, extension release cannot clear an extension owned by a different academic, and public domain APIs that accept `empNr` enforce `SharedKernelFieldLengths.EmpNr` after normalization (including qualification create and extension assign/release paths).
3. Implement reusable persistence mappings and hard database-constraint semantics.
   Targets: EF Core entity configurations, indexes, and ownership-neutral mapping semantics for empNr uniqueness and extension assignment uniqueness. Do not create application-host startup code or claim migration ownership for feature-owned tables.
   Owner: data-persistence.
   Validation before next step: mappings align with domain rules, no persistence rule contradicts the aggregate, no unique index duplicates an existing primary key column set, check-constraint naming matches predicate semantics (do not use Xor naming unless strict exactly-one is enforced), and each mapped table has one named migration owner outside this slice.
4. Add reusable error/result plumbing and domain event contracts.
   Targets: Shared Kernel result types, error primitives, event interfaces, and common exceptions.
   Owner: backend-domain.
   Validation before next step: later slices can consume common result and exception types without redefining them, `Result<T>.Value` throws on failure access instead of exposing `default!`, factory-validated primitives do not expose public constructors that bypass invariant checks, and C# files keep one primary type per file with filename-to-type alignment.
5. Remove scaffolding leftovers and normalize file hygiene before final verification.
   Targets: any newly added source, test, project, and solution files.
   Owner: slice-coordinator.
   Validation before next step: no `Class1.cs`, `UnitTest1.cs`, `Placeholder` types, or similar starter artifacts remain, and file names match their primary type or test behavior.
6. Verify invariants and persistence behavior.
   Targets: unit tests, mapping tests, and migration validation.
   Owner: testing-verification.
   Validation before next step: all foundational tests pass, failures clearly identify which invariant broke, infrastructure/setup failures fail explicitly (no catch-and-return skip path), environment configuration is read once per value in setup helpers and verification scripts, SQL Server setup paths use explicit platform guards (no unconditional LocalDB fallback on non-Windows hosts), any touched solution file has no duplicate project declarations, and any touched solution file keeps the Visual Studio header as line 1 with no BOM-only leading line.

## Verification and Acceptance Criteria

### Review-Prevention Guardrails

- Dependency compatibility is validated for coupled tooling packages when touched (for example xUnit core and runner major versions align).
- Result-style failure factories guard non-null failure payloads in both generic and non-generic wrappers when touched.
- `Error.None` remains reserved for success only; failure results cannot use the empty success sentinel and must carry actionable details.
- Factory-validated shared primitives (for example `Error`) keep constructors non-public so callers cannot bypass `Create`/`TryCreate` invariant validation.
- C# source keeps one primary type per file and file names stay aligned with the primary type.
- Domain exceptions are split into dedicated files/types with aligned names as the exception set grows.
- Value-object parse/create APIs reject lossy coercion unless explicitly required and covered by tests.
- Domain create/update paths enforce persistence-backed field limits and normalization (for example Degree/University max length parity with EF Core mappings) before persistence.
- Public domain APIs that accept persisted identifiers reject overlong normalized values before persistence (for example `AcademicQualification.Create`, `Extension.AssignTo`, and `Extension.ReleaseFrom` enforce `SharedKernelFieldLengths.EmpNr`).
- Normalization helpers are owned by the same concept or by a neutral shared utility; unrelated concepts do not depend on each other's normalization methods (for example `University` does not call `Degree.Normalize`).
- Read-only collection properties do not leak mutable backing collections, including array-backed catalogs (use defensive copies or read-only wrappers when backing storage is mutable).
- Integration tests that provision external resources include deterministic best-effort cleanup in `finally` blocks.
- Integration-test teardown failures are non-fatal to the primary assertion signal (cleanup errors are surfaced separately and do not mask the behavioral failure under test).
- Creating or mutating an Academic cannot leave both IsTenured and ContractEndDate set at the same time; constraint names and test names must reflect this mutual-exclusion semantic unless strict XOR is explicitly required.
- Rank values map only as P -> INT, SL -> NAT, and L -> LOC, and AccessLevel is never assigned directly.
- Shared Kernel types compile with nullable reference types enabled and are reusable by later slices.
- Database constraints back up the code-level uniqueness rules for empNr and extension assignment.
- Shared Kernel defines reusable mapping semantics and migration-ownership rules; it does not execute migrations or own feature-slice migrations.
- EF Core schema changes include the required migration artifact and metadata files in the owning feature slice unless explicitly waived, and verification evidence shows migration output matches the intended model.
- Model verification checks inspect `context.Model` directly and do not depend on `IDesignTimeModel` service resolution in normal tests.
- Extension-association invariants prevent cross-academic state corruption: assignment cannot overwrite a different active assignment, and release validates ownership before clearing links.
- Foundational tests cover invariant success and failure paths for employment guards, derivation, and result handling.
- Result tests include direct coverage of both non-generic `Result` and generic `Result<T>` success/failure invariants.
- `Result<T>.Value` is accessible only for successful results and throws a clear exception for failure results.
- Model verification checks primary key shape directly and does not require a redundant unique index on the same key columns.
- SQL Server constraint verification fails with actionable diagnostics when connectivity/setup is unavailable; tests must not silently return.
- Design-time SQL Server configuration matches cross-platform verification behavior (LocalDB fallback only under explicit Windows guard; non-Windows requires `ZEUS_SQLSERVER_CONNECTION`).
- Newly created source and test files do not retain placeholder scaffolding; file names match the primary type or test behavior under review.
- Support scripts and database-test helpers read each environment variable once and reuse the parsed value instead of duplicating lookups across branches.

## Human Showcase Steps

1. Starting state: clean branch with no slice-specific code yet.
   Action: open the Shared Kernel project and inspect the Academic aggregate, value objects, and Result/Error types after implementation.
   Expected result: the domain foundation exists in one reusable location with explicit invariant methods and no feature-specific leakage.
   Value demonstrated: later slice work no longer needs to rediscover or duplicate core academic rules.
2. Starting state: test runner available.
   Action: run the Shared Kernel unit and mapping tests, including the cases for tenure/contract exclusivity and rank derivation.
   Expected result: passing tests prove the core rules are enforced before endpoint work begins.
   Value demonstrated: the highest-risk domain invariants are locked in before the backlog expands.

## Completion Checklist

- [ ] Review-prevention guardrails were evaluated and marked N/A where not applicable.
- [ ] If test packages changed, compatibility is verified (for example xUnit core and runner major versions align).
- [ ] If value-object parsing or creation changed, lossy coercion is rejected unless explicitly required and tested.
- [ ] If integration tests create external resources, teardown is enforced with best-effort `finally` cleanup.
- [ ] Shared Kernel scope is still limited to reusable domain and persistence foundations.
- [ ] Application host creation, endpoint registration, dependency-injection composition, authentication, and migration execution remain outside this slice and are owned by the application-host setup implementation prompt.
- [ ] Aggregate invariants and derived properties are enforced in code.
- [ ] Result failure paths use actionable errors and do not rely on `Error.None` for failures.
- [ ] Factory-validated shared primitives keep constructors non-public so validation cannot be bypassed.
- [ ] C# source keeps one primary type per file with filename-to-type alignment.
- [ ] Exception types are organized into dedicated files/types with aligned names.
- [ ] Domain create/update rules enforce persistence-backed field limits before persistence (including shared max-length constraints).
- [ ] Public domain create/assign/release APIs reject overlong normalized persisted identifiers before persistence (including `empNr` paths).
- [ ] Normalization logic does not introduce cross-concept coupling between unrelated domain types.
- [ ] Read-only collection members do not expose mutable backing lists or backing arrays.
- [ ] Database constraints back up the critical uniqueness rules.
- [ ] Migration ownership is explicit for each persisted table, with no competing DbContext migration owner.
- [ ] Result, error, event, and exception primitives are reusable by later slices.
- [ ] Verification evidence exists for invariant and mapping behavior.
- [ ] Result primitive tests cover both `Result` and `Result<T>` invariants after any refactor.
- [ ] Any repo-layout deviation from the plan is documented before dependent slice work begins.
- [ ] No unique index duplicates a primary key column set unless explicitly justified and documented.
- [ ] Environment/setup failures in persistence verification fail explicitly; no silent pass path remains.
- [ ] SQL Server setup uses explicit platform guards and does not rely on unconditional LocalDB fallback on non-Windows hosts.
- [ ] Newly created files were checked for leftover scaffolding placeholders before review.
