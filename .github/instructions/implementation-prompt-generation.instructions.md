---
ai_generated: true
model: "openai/gpt-5.4@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-04-18-implementation-prompt-instructions"
prompt: |
  create an instruction file that describes the requirements for creating an implementation prompt. an implementation prompt is a prompt that specifies the implementation steps for a slice.  the implementation prompt should utilize custom agents specialized in the implementation roles. the implementation prompt includes acceptance criteria that agents and humans can use the verify the implementation. the implementation prompts includes step-by-step directions that a human can follow to showcase the value of the slice.
started: "2026-04-18T12:45:18.2891008-07:00"
ended: "2026-04-18T12:47:41.1104378-07:00"
task_durations:
  - task: "context analysis"
    duration: "00:00:50"
  - task: "draft instruction content"
    duration: "00:01:05"
  - task: "provenance and cross-reference updates"
    duration: "00:00:28"
total_duration: "00:02:23"
ai_log: "ai-logs/2026/04/18/2026-04-18-implementation-prompt-instructions/conversation.md"
source: "johnmillerATcodemag-com"
description: "Standards for generating slice implementation prompt files"
applyTo: ".github/prompts/**/*implementation*.prompt.md"
---

# Implementation Prompt Generation

## Application Host and Persistence Composition

- When a repository has feature-local persistence, implementation prompts must name the application host as a separate composition boundary rather than placing startup concerns in Shared Kernel.
- Prompts must name one feature-local DbContext and one migration owner for every persisted feature table.
- A feature-local DbContext may map a Shared Kernel entity and reuse its configuration semantics without reusing `SharedKernelDbContext`.
- Prompts must identify the host or deployment command responsible for applying migrations and must reject competing migration owners for the same table.

## Purpose and Scope

- An **implementation prompt** defines the execution plan for one named vertical slice.
- The prompt MUST focus on a single slice or use-case, not an epic or mixed multi-slice backlog.
- The prompt MUST produce implementation guidance that is concrete enough for both AI agents and humans to execute and verify.
- The prompt MUST align with existing repository standards before prescribing code changes.
- The prompt MUST explicitly require the implementation to follow [.github/instructions/vertical-slice-implementation.instructions.md](vertical-slice-implementation.instructions.md) and keep the slice under `src/features/<Feature>/<UseCase>/` rather than splitting it across layer-oriented folders.

## Naming and Location

- Store reusable implementation prompts in `.github/prompts/`.
- Use kebab-case filenames: `<slice-name>-implementation.prompt.md`.
- Set the prompt `name` to `implement-<slice-name>`.
- Use `description`, `context`, and `expected_output` in front matter so the prompt is discoverable and executable.

## Metadata Requirements

### AI Provenance

See [ai-assisted-output.instructions.md](ai-assisted-output.instructions.md) for required provenance fields.

### Prompt Metadata

Implementation prompts MUST include:

- `name`
- `description`
- `author`
- `tags`
- `context`
- `expected_output`

Implementation prompts SHOULD include:

- `arguments`
- `tools`
- `mode`
- `examples`

Example front matter:

```yaml
---
name: implement-course-enrollment
description: Guide delivery of the Course Enrollment vertical slice
author: John Miller
tags: [implementation, vertical-slice, backend, frontend, testing]
arguments:
  - name: slice_name
    description: Canonical slice name
  - name: context_files
    description: Optional supporting files to read before execution
context: "zeus.academia slice delivery with vertical-slice boundaries and custom agent orchestration"
expected_output: "A step-by-step implementation plan with agent assignments, acceptance criteria, verification, and demo steps"
tools: ["read", "search", "edit", "agent"]
mode: agent
---
```

## Required Context Analysis

Before drafting the prompt, gather the implementation context in this order:

1. Read `.github/instructions/project-overview.instructions.md`.
2. Read `.github/instructions/vertical-slice-implementation.instructions.md`.
3. Read `.github/instructions/custom-agents.instructions.md`.
4. Read stack-specific instruction files for every layer the slice touches.
5. Inspect existing code or workflow files for analogous slices.
6. Identify the custom agents required for the slice and record any missing agents as explicit blockers.

If the slice touches backend C#, frontend Vue 3, Pinia stores, or tests, the prompt MUST pull in the corresponding instruction files before prescribing work.

At minimum, the prompt author MUST include the matching repository instructions for applicable surfaces:

- backend C#: `.github/instructions/csharp-implementation.instructions.md`
- ASP.NET Core endpoints: `.github/instructions/aspnetcore-implementation.instructions.md`
- MediatR/CQRS handlers: `.github/instructions/mediatr-implementation.instructions.md`, `.github/instructions/cqrs-mediatr-efcore.instructions.md`, or `.github/instructions/cqrs-es-csharp-mediatr.instructions.md` as appropriate
- FluentValidation: `.github/instructions/fluentvalidation-implementation.instructions.md`
- frontend Vue 3 and TypeScript: `.github/instructions/vue3-implementation.instructions.md`, `.github/instructions/typescript-frontend-implementation.instructions.md`
- Pinia stores: `.github/instructions/pinia-implementation.instructions.md`
- backend tests: `.github/instructions/xunit-implementation.instructions.md`
- frontend tests: `.github/instructions/vitest-implementation.instructions.md`

## Agent Orchestration Requirements

- The prompt MUST use custom agents specialized in implementation roles rather than assigning the entire slice to one generic actor.
- The prompt MUST name the agent for each role and state that role's deliverable.
- The prompt MUST define handoff order so each agent knows when to start and what evidence to produce.
- The prompt MUST separate implementation from verification responsibility.

## Required Guardrails for Route Registration and Rule Reuse

Every implementation prompt MUST include explicit execution checks for the following failure modes:

- Startup registration check: every new endpoint aggregator or `Map...Endpoints()` method must be mapped from the application host or composition root before the slice is marked complete.
- Runtime reachability check: the prompt must require verification that routes are registered and reachable rather than merely compiling.
- Single-source-of-truth check: any validation, coercion, normalization, or range rule already implemented elsewhere must be reused; duplicate implementations in validators, handlers, and mappings are prohibited unless a direct review justification is recorded.
- Drift review: the prompt must instruct the implementation agent to compare new logic with neighboring slices and shared helpers before finalizing the change.

These checks are required for any slice that adds a route, API surface, persistence, or command/query validation.

Every implementation prompt MUST include an agent matrix like this:

| Role                    | Agent                  | Responsibility                                            | Inputs                                 | Outputs                                       |
| ----------------------- | ---------------------- | --------------------------------------------------------- | -------------------------------------- | --------------------------------------------- |
| Scope and acceptance    | `product-manager`      | Confirm boundaries, dependencies, and acceptance criteria | Slice request, workflows, prior specs  | Approved slice scope and acceptance checklist |
| Backend implementation  | `<backend-agent>`      | Implement API, domain, persistence, validation            | Accepted scope, backend standards      | Code changes and tests                        |
| Frontend implementation | `<frontend-agent>`     | Implement UI, state, API integration                      | Accepted scope, frontend standards     | UI changes and tests                          |
| Verification            | `<qa-or-review-agent>` | Validate behavior, tests, and demo readiness              | Implemented slice, acceptance criteria | Verification result and residual risks        |

Rules:

- Use only roles needed for the slice; do not include a frontend role for a backend-only slice.
- If a required agent does not yet exist, the prompt MUST say so explicitly and either:
  - reference the `.github/agents/<agent-name>.agent.md` file that must be created first, or
  - mark the execution blocked until that agent exists.
- Do not hide missing-agent gaps behind generic phrasing like "engineering agent".

## Required Prompt Sections

Every implementation prompt MUST contain these sections, in order:

### 1. Objective

State the slice name, the user or business value delivered, and the concrete outcome expected at completion.

### 2. Slice Boundary

Define:

- in-scope behavior
- explicit non-goals
- dependencies on shared kernel, contracts, or prerequisite slices
- interfaces or entry points touched by the slice

### 3. Required Context

List the files, workflows, standards, and existing code paths the executor must review before making changes.

### 4. Agent Plan

Include the agent matrix plus the expected handoff sequence. Each handoff must identify:

- owner
- prerequisite evidence
- output artifact
- stop condition

### 5. Implementation Steps

Provide an ordered list of concrete steps. Each step MUST identify:

- step number
- owning agent or human role
- goal
- files or directories to inspect or modify
- completion signal
- verification tied to the step

Preferred format:

| Step | Owner             | Action                                                  | Files                              | Done When                    | Verification                  |
| ---- | ----------------- | ------------------------------------------------------- | ---------------------------------- | ---------------------------- | ----------------------------- |
| 1    | `product-manager` | Confirm slice boundary and finalize acceptance criteria | `models/`, `.github/instructions/` | Scope is approved and stable | Acceptance checklist reviewed |

### 6. Acceptance Criteria

Acceptance criteria MUST be usable by both agents and humans. Each criterion MUST be:

- observable
- testable
- scoped to the slice outcome, not the implementation task list
- written as a checklist, Given/When/Then, or equivalent precise format

The section MUST cover, when applicable:

- happy path behavior
- validation and failure behavior
- persistence or side effects
- authorization or role restrictions
- user-visible feedback
- test coverage expectations
- persisted field limits at public domain APIs (factories/mutators enforce max length, precision/scale, and normalization before persistence)
- normalization ownership boundaries (do not couple unrelated domain concepts by calling one concept's normalization helper from another)
- dependency compatibility for coupled tooling packages (for example xUnit core/runner major-version alignment)
- integration-test resource lifecycle requirements (external resources must include deterministic teardown)
- C# file/type organization hygiene (one primary type per file and file names aligned with their primary type)
- exception argument-name accuracy for guard clauses and mapping failures (for example, property-specific failures use the property name rather than the enclosing command object)
- result-wrapper invariants for success/failure access patterns (for example, `Result<T>.Value` must not be consumable on failure)
- result failure-factory null guards (for example `Failure(Error error)` cannot accept null error payloads)
- result semantics that reserve `Error.None` for success only, so failed results must carry actionable error details rather than an empty success sentinel
- constructor accessibility for factory-enforced invariants (types that enforce validation in `Create`/`TryCreate` or equivalent must keep constructors non-public so callers cannot bypass guardrails)
- non-lossy parse/create behavior for constrained value objects (reject silent truncation or coercion unless explicitly required)
- persistence-backed domain field parity (domain create/update paths enforce the same max-length, precision, scale, and normalization constraints required by persistence)
- persisted identifier guard coverage at public APIs (for example `empNr` is validated for shared max-length and normalization in public create/assign/release methods, not only at EF persistence boundaries)
- normalization decoupling between unrelated concepts (for example, `University` normalization must not depend on `Degree.Normalize`; use local or neutral shared helpers)
- canonical-definition reuse for constrained codes, enums, and persisted allowed-value rules (validators, mappings, error messages, and EF Core check constraints must derive from one source of truth instead of repeating literals across layers)
- immutable read-only collection exposure (do not expose mutable backing collections through `IReadOnlyCollection`; include array-backed catalogs and require defensive copies or read-only wrappers such as `AsReadOnly()` when applicable)
- array-backed or static catalog exposure safety (public `IReadOnlyList<T>` over an array is insufficient; require wrappers or immutable snapshots that cannot be cast back to the backing array)
- required-text validator semantics for string fields (when whitespace should be treated as missing input, reject null/empty/whitespace with the required-message rule before membership/format checks; do not rely on `NotEmpty()` alone)
- database key/constraint intent without redundancy (for example, avoid unique indexes that duplicate the primary key columns)
- named check-constraint semantics (for example, use XOR naming only for strict exactly-one predicates; otherwise use mutual-exclusion naming)
- ownership-safe association mutations (for example, releasing an extension must validate it belongs to the target academic; assignment must not overwrite an existing different assignment)
- EF Core migration hygiene when schema changes are part of the slice (required migration artifacts, model snapshot, and metadata files must be part of the deliverable unless explicitly waived)
- EF Core migration artifact completeness when schema changes are part of the slice (never commit snapshot-only metadata; require migration class, Designer metadata, and snapshot as one coherent set unless explicitly waived as mapping-only)
- persistence-exception translation specificity (only translate `DbUpdateException` or equivalent persistence failures into business conflicts when the exact conflict is proven or provider handling is narrow enough to avoid masking unrelated failures)
- model metadata testing that inspects `context.Model` directly rather than relying on `IDesignTimeModel` from the service provider in normal tests
- migration metadata integrity checks that fail verification when snapshot, migration class, and Designer files are not committed together for schema-changing work
- exception organization hygiene (domain exceptions should be split into dedicated files/types so file names and type names stay aligned as the exception set grows)
- scope-to-surface alignment for prompts and PR language (claims like CRUD, get-by-id, or admin seeding must map to explicit steps, endpoints, handlers, and verification; otherwise the prompt must describe the narrower implemented scope)
- solution-file integrity when `.sln` is touched (no duplicate project name/path entries and no duplicate configuration blocks for equivalent projects)
- scaffold cleanup and naming hygiene (no leftover placeholder starter files; file names must match their primary type or test behavior)
- solution-file encoding hygiene when `.sln` is touched (no BOM-only line or blank line ahead of the required Visual Studio header)
- environment/setup helper hygiene when scripts or infrastructure-backed tests are touched (read each environment variable once and reuse the parsed value or helper result)
- cross-platform SQL Server setup behavior for scripts/factories (SQL Server LocalDB fallback allowed only with explicit Windows guard; on non-Windows require `ZEUS_SQLSERVER_CONNECTION` with actionable failure messaging)

Bad:

- "Create API endpoint"
- "Write tests"

Good:

- "Given valid enrollment data, when an authorized registrar submits the form, then the system creates the enrollment and returns the new enrollment identifier."
- "Given duplicate enrollment data, when the request is submitted, then the system rejects it with a conflict result and preserves existing data."

### 7. Verification Plan

Specify how the slice will be verified:

- automated tests to add or update
- commands to run
- manual checks
- evidence to collect
- residual-risk callouts if verification is partial
- behavior when environment prerequisites are missing (tests MUST fail explicitly with actionable diagnostics; no early return/skipped-by-default pattern)
- teardown robustness for infrastructure-backed tests (cleanup must be best-effort and must not mask primary assertion failures if teardown encounters transient errors)
- platform guard consistency for infrastructure helpers (design-time factories, scripts, and tests must share the same non-Windows behavior for SQL Server configuration)
- argument/parameter-name accuracy for thrown guard exceptions when mappings or validation helpers reject a specific property value
- validator error-message intent for required string inputs (required-message assertions include whitespace-only inputs and run before downstream allowed-values/format rules)
- persistence-exception translation checks (verify the intended conflict translation path and verify unrelated persistence failures are not reported as duplicate/business conflicts)
- public catalog immutability checks when supported-value lists are exposed from shared helpers or domain extensions

The prompt MUST distinguish between required verification and optional follow-up checks.

When the slice adds new source, test, or project files, the verification plan MUST also require a quick scaffold audit so placeholder starter artifacts are removed or renamed before review.

### 8. Showcase Steps

This section is mandatory. It explains how a human demonstrates the value of the slice after implementation.

The showcase script MUST include:

- prerequisites or seed data
- environment setup steps
- step-by-step user actions
- expected result after each action
- one failure-path demonstration when relevant
- the specific value proven by the demo

Preferred format:

```markdown
1. Start the API and frontend for the target environment.
2. Sign in as a registrar test user.
3. Navigate to Course Enrollment.
4. Submit a new enrollment for Student A into Course B.
   Expected: Enrollment confirmation appears and the new row is visible in the enrollment list.
5. Repeat the same request.
   Expected: The UI shows a duplicate-enrollment error and no second row is created.

Value demonstrated: The slice supports successful enrollment while protecting data integrity.
```

### 9. Output Artifacts

List the expected outputs from execution:

- code changes
- tests
- updated docs or prompt files
- verification summary
- demo notes if they are part of the deliverable

### 10. Validation Checklist

End the prompt with a checklist that verifies prompt quality before use.

## Prompt Validation Checklist

- [ ] The prompt targets exactly one slice.
- [ ] Required repo instructions are listed in pre-work.
- [ ] Custom agents are named by role, not implied.
- [ ] Missing custom agents are called out explicitly.
- [ ] Each implementation step has an owner and completion signal.
- [ ] Acceptance criteria describe outcomes, not task completion.
- [ ] Verification covers both automated and manual checks where applicable.
- [ ] Showcase steps can be executed by a human without hidden knowledge.
- [ ] The value of the slice is demonstrated explicitly.
- [ ] Non-goals and dependency constraints are stated.
- [ ] Persistence rules avoid redundant uniqueness definitions (no PK + duplicate unique index on same columns unless explicitly justified).
- [ ] Verification instructions require explicit failure for missing infrastructure prerequisites (no silent pass/early return).
- [ ] Shared result contracts include invariant access rules for success/failure payloads.
- [ ] Result semantics explicitly reserve `Error.None` for success and forbid empty-error failures.
- [ ] Prompted factory-enforced invariants keep constructors non-public so validation cannot be bypassed.
- [ ] Schema-changing prompts require migration artifacts and metadata hygiene for EF Core work.
- [ ] Model metadata checks use the EF Core model directly rather than a design-time service lookup in normal tests.
- [ ] Domain exception types are organized into dedicated files/types with aligned names.
- [ ] Prompted C# changes keep one primary type per file and preserve filename-to-type alignment.
- [ ] Prompted domain create/update flows enforce persistence-backed field limits before persistence (max length, precision, scale, normalization).
- [ ] Prompted read-only collection exposure prevents mutable backing-list or backing-array escape.
- [ ] Prompted required string validation covers null/empty/whitespace and preserves the required-message path before format/allowed-values checks.
- [ ] Prompted SQL Server setup behavior forbids unconditional LocalDB fallback on non-Windows hosts.

## Anti-Patterns

- A single prompt covering multiple unrelated slices.
- Generic instructions such as "implement the feature" with no file targets, roles, or checkpoints.
- Agent roles listed without named agents or handoffs.
- Acceptance criteria that restate implementation tasks instead of behavior.
- Demo steps that omit expected outcomes.
- Verification sections that say "run tests" without naming the test scope or commands.
- Prompts that assume a missing custom agent already exists.
- Prompting persistence work that adds a unique index on the same columns as an existing primary key.
- Prompting integration/constraint tests to catch-and-return on connection/setup errors instead of failing explicitly.

## Output Standard

The generated implementation prompt should be concise, executable, and structured for delegation. Prefer tables and ordered steps over long prose. The result should let a human reviewer answer three questions quickly:

1. Which slice is being delivered?
2. Which agent or person owns each part of the work?
3. How do we prove the slice works and show why it matters?
