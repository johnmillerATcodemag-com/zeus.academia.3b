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
name: implement-academia-ep-1-3-manage-universities
description: Implement the ManageUniversities reference-data slice for canonical university codes
author: John Miller
tags: [academia, implementation, reference-data, universities]
context: "Zeus Academia Phase 1 reference-data implementation"
expected_output: "A slice-scoped implementation plan for ManageUniversities"
tools: ["read", "search", "edit", "agent"]
mode: agent
---

# Implement ManageUniversities

## Slice Summary and Business Value

- Slice: ManageUniversities
- Business outcome: provide the controlled university catalog used by registration and qualification maintenance.
- Out of scope: assigning universities to academic qualifications and report aggregation.

## Context Files to Review First

- .github/models/workflows/academia-execution-plan.md
- .github/models/workflows/academia-implementation-plan.md
- .github/instructions/vertical-slice-implementation.instructions.md
- Follow the vertical-slice instructions and keep the implementation in a feature/use-case folder under `src/features/` with co-located command/query, validator, endpoint, and tests instead of splitting the slice across layer-oriented folders.
- .github/instructions/xunit-implementation.instructions.md
- .github/instructions/mediatr-implementation.instructions.md
- .github/instructions/fluentvalidation-implementation.instructions.md
- .github/instructions/aspnetcore-implementation.instructions.md

## Prerequisites and Dependency Checks

- Required prior slices: Shared Kernel
- Blocking risks: university code format may already appear in existing fixtures; reconcile those values before locking the validator.
- Existing patterns to reuse: reference-data CRUD-lite behavior, code uniqueness, and deterministic listing.
- Placeholder entity types are prohibited. `UniversityRecord` must be a real persistence model, not a stub or placeholder class.
- `UniversityRecord.Code` is the canonical primary key and must be configured explicitly as the key in the EF Core model before migration generation or startup verification.
- The model must include a `UniversityRecordConfiguration` with `builder.HasKey(x => x.Code)` and the canonical maximum length and requiredness checks.
- The code-length constant and normalization rule must be defined once and reused by validation, EF configuration, and any persistence or seed logic; do not silently introduce a new, conflicting rule.
- If SQL Server or LocalDB is unavailable, the verification path must fail explicitly with the connection issue; it must not continue as if the app were valid.

## Assigned Agents and Role Boundaries

| Role                 | Responsibilities                                            | Inputs                                                  | Outputs                                      | Escalate when                                                                                                                 |
| -------------------- | ----------------------------------------------------------- | ------------------------------------------------------- | -------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| slice-coordinator    | confirm catalog ownership and placement                     | execution plan, repo tree, existing fixtures            | approved artifact map                        | multiple university catalogs already exist                                                                                    |
| backend-domain       | implement add and list university behavior                  | Shared Kernel university type, slice conventions        | commands, queries, handlers, DTOs, endpoints | code or fixtures imply conflicting university identifiers                                                                     |
| data-persistence     | implement code normalization and durable uniqueness backing | canonical university-code definition, persistence rules | EF Core mapping, constraint decisions        | the slice would duplicate university-code normalization or uniqueness rules across validator, mapping, and persistence layers |
| testing-verification | verify uniqueness and list behavior                         | implemented slice                                       | tests and evidence                           | duplicate handling or list output is unstable                                                                                 |

## Ordered Implementation Steps

1. Confirm where university reference data belongs.
   Targets: src/features/ReferenceData/ManageUniversities/ or current equivalent, persistence setup, and seed data path.
   Owner: slice-coordinator.
   Validation before next step: one canonical catalog location and route shape are agreed.
2. Implement add-university behavior.
   Targets: AddUniversity command, validator, handler, response, endpoint, and mapping files.
   Owner: backend-domain.
   Validation before next step: valid university codes persist, code-specific failures identify the `Code` property explicitly, and duplicates fail clearly.
3. Implement persistence-backed code normalization and uniqueness rules.
   Targets: persistence configuration, any schema or model-constraint artifacts, and the canonical university-code normalization source reused by validators and mappings.
   Owner: data-persistence.
   Validation before next step: code trimming, casing, uniqueness, and any durable constraints derive from one canonical definition instead of being reimplemented separately across layers.
4. Implement list-universities behavior.
   Targets: ListUniversities query, handler, response contract, and endpoint.
   Owner: backend-domain.
   Validation before next step: query returns stable reference data for registration and qualification flows.
5. Verify the slice.
   Targets: validator tests, handler tests, integration tests.
   Owner: testing-verification.
   Validation before next step: add and list flows pass with clear failure coverage, and canonical code rules stay aligned across validator, mapping, and persistence behavior.

## Verification and Acceptance Criteria

### Review-Prevention Guardrails

- Dependency compatibility is validated for coupled tooling packages when touched (for example xUnit core and runner major versions align).
- Result-style failure factories guard non-null failure payloads in both generic and non-generic wrappers when touched.
- Value-object parse/create APIs reject lossy coercion unless explicitly required and covered by tests.
- Integration tests that provision external resources include deterministic best-effort cleanup in `finally` blocks.
- Placeholder entity types are prohibited. `UniversityRecord` must be a real persistence model, not a stub or placeholder class.
- `UniversityRecord.Code` is the canonical primary key and must be configured explicitly in EF Core using `builder.HasKey(x => x.Code)` before migration or startup validation succeeds.
- The code normalization, uniqueness, and validation rules must be defined once and reused by validators, mappings, and persistence configuration.
- Guard failures for invalid or malformed university input identify the `Code` property rather than the enclosing command object.
- Adding a university code creates one canonical reference-data record.
- Duplicate university codes are rejected without partial persistence.
- University-code normalization, uniqueness, and error messaging derive from one canonical source rather than being redefined independently in validators, mappings, and persistence rules.
- Listing universities returns stable data that downstream slices can resolve reliably.
- Validation, handler logic, and persistence behavior agree on duplicate handling.
- Automated tests cover add success, duplicate rejection, and list-query behavior.

## Human Showcase Steps

1. Starting state: Shared Kernel is available and university data is not yet loaded.
   Action: add the baseline university codes through the approved endpoint or bootstrap path.
   Expected result: universities are stored once and become available for dependent slices.
   Value demonstrated: later qualification flows can reference controlled university data.
2. Starting state: university records exist.
   Action: call the list-universities endpoint.
   Expected result: the catalog returns stable university codes suitable for API or UI consumers.
   Value demonstrated: registration and qualification updates can validate against canonical universities.

## Completion Checklist

- [ ] Review-prevention guardrails were evaluated and marked N/A where not applicable.
- [ ] If test packages changed, compatibility is verified (for example xUnit core and runner major versions align).
- [ ] If value-object parsing or creation changed, lossy coercion is rejected unless explicitly required and tested.
- [ ] If integration tests create external resources, teardown is enforced with best-effort `finally` cleanup.
- [ ] University-code guard failures point to `Code` rather than the enclosing command object.
- [ ] University-code normalization and uniqueness rules are defined once and reused by validators, mappings, messages, and persistence constraints.
- [ ] ManageUniversities remains reference-data only.
- [ ] University code uniqueness is enforced.
- [ ] Query behavior is stable for downstream lookups.
- [ ] Tests cover successful adds, duplicate rejection, and listing.
- [ ] Any bootstrap or seed mechanism is documented.
- [ ] No qualification-maintenance logic leaks into this slice.
- [ ] `UniversityRecord` is not a placeholder and defines the identity key explicitly.
- [ ] `UniversityRecord.Code` is the primary key and is validated in the EF configuration.
- [ ] Model validation and migration startup pass only when the persistence model is complete and SQL Server is available.
