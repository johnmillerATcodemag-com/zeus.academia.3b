---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2025-12-28-ai-dev-process-generation"
prompt: |
  submit the #file:create-ai-dev-process.prompt.md
started: "2025-12-28T19:30:00Z"
ended: "2025-12-28T19:40:00Z"
task_durations:
  - task: "design workflow structure"
    duration: "00:03:00"
  - task: "draft directives"
    duration: "00:05:00"
  - task: "optimize tokens"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2025/12/28/2025-12-28-ai-dev-process-generation/conversation.md"
source: ".github/prompts/create-ai-dev-process.prompt.md"
description: "AI-assisted software development workflow and review process"
applyTo: "**"
---

# AI-Assisted Software Development Process

## Application Composition and Persistence Ownership

- Application host creation, endpoint registration, dependency-injection composition, authentication, SQL Server configuration, and migration execution are owned by the application-host setup implementation prompt, not the Shared Kernel slice.
- Each persistence-bearing feature uses an explicitly named feature-local DbContext and owns migrations for its tables; no two DbContexts may own migrations for the same table.
- A feature-local DbContext may map a Shared Kernel entity and reuse its configuration semantics without reusing `SharedKernelDbContext` or introducing a duplicate domain entity.

## AI Code Generation

**When to Use:**

- Boilerplate, repetitive patterns, standard implementations
- Initial scaffolding, test skeletons, documentation templates
- Code transformations, refactoring, format conversions

**Requirements:**

- MUST include complete provenance metadata (see ai-assisted-output.instructions.md)
- MUST pass existing tests or include new passing tests
- MUST follow project style, patterns, and conventions
- MUST follow [.github/instructions/vertical-slice-implementation.instructions.md](vertical-slice-implementation.instructions.md) for all slice implementation work (feature-domain first, use-case folder ownership, no layer-root splitting)
- MUST document non-obvious logic, edge cases, assumptions
- MUST reference source prompt or instruction file

**Pre-PR Review-Prevention Checks (Required):**

- MUST verify reference integrity for every documented command/path before commit:
  - If documentation or agent guidance references a file, that file must be committed in the same change.
  - If guidance references an editor task (for example a VS Code task), the corresponding task file must exist in the repository; otherwise reference a committed script command instead.
- MUST validate platform assumptions for runtime tooling:
  - SQL Server local-development fallbacks (for example SQL Server LocalDB) must be explicitly guarded.
  - On non-Windows, require explicit environment configuration instead of silent fallback.
  - Design-time DbContext factories and verification scripts must use the same SQL Server-specific platform guard behavior as runtime verification (no unconditional LocalDB fallback on non-Windows hosts).
- MUST run a focused self-review for common correctness regressions before opening PR:
  - Vertical slice layout and boundaries match [.github/instructions/vertical-slice-implementation.instructions.md](vertical-slice-implementation.instructions.md).
  - Placeholder scaffolding artifacts are removed or renamed before review; do not leave `Class1.cs`, `UnitTest1.cs`, `Placeholder` types, or similar starter files in committed slices.
  - C# file names match their primary type name and xUnit test files contain real test classes named for the behavior under test.
  - C# source keeps one primary type per file; do not colocate unrelated primary types in the same `.cs` file.
  - Types that enforce invariants through `Create`/`TryCreate` (or equivalent factory methods) do not expose public constructors that can bypass those checks; constructor visibility must enforce the intended guardrails.
  - Null argument validation for non-nullable API inputs.
  - Dependency package families remain version-compatible (for example xUnit core package major version aligned with its runner package major version).
  - No mutable collection escape through read-only interfaces.
  - Array-backed catalogs and static arrays are not exposed directly through `IReadOnlyList<T>` or similar interfaces; return immutable snapshots or read-only wrappers that cannot be down-cast and mutated.
  - Backing `List<T>` collections are not exposed directly; read-only members return immutable/read-only wrappers (for example `AsReadOnly()`).
  - Database exception translation is narrow and evidence-based; do not collapse every `DbUpdateException` into a duplicate/conflict result unless a targeted post-failure existence check proves that specific conflict.
  - No duplicate uniqueness enforcement on the same database key path (for example PK + duplicate unique index).
  - Any EF Core model, configuration, or `DbSet` addition that changes schema ships with the matching migration artifacts and updated model snapshot unless the change is explicitly documented as mapping-only.
  - Do not commit a standalone EF Core model snapshot; when migrations are in scope, include the migration class plus its Designer metadata alongside the snapshot (or omit all migration artifacts when explicitly waived as mapping-only).
  - If a slice introduces or changes route groups, verify the application host maps them explicitly before review; do not rely on implicit discovery.
  - If a feature defines a `Map*Endpoints(this IEndpointRouteBuilder app)` aggregator, maintain an automated host-composition guard test that fails when `Program.cs` does not call the matching `app.Map*Endpoints()` method.
  - If an endpoint advertises `ProducesValidationProblem()` or equivalent validation responses, verify every normalization/argument exception from the handler, mapper, or factory is converted into `Results.ValidationProblem(...)` (or equivalent) rather than leaking as a 500.
  - If a numeric/date/enum value is normalized or range-checked in multiple layers, centralize the rule in one shared helper or domain primitive; do not duplicate equivalent logic across validators, handlers, and mapping code.
  - Do not keep unreachable `catch` blocks for impossible exceptions; if the code path cannot throw due to earlier guards, remove the dead catch and rely on the actual validation path.
  - If the application host calls `Database.MigrateAsync()` for a feature DbContext, ensure the feature includes matching migration artifacts or the migration owner is explicitly documented.
  - When a canonical domain helper normalizes input before validation, run length/shape checks on the normalized value rather than the raw string.
  - Do not use `null!` to satisfy failing lookup helpers; nullable out values are required when the failure path is real.
  - No duplicate project declarations in solution files; project name/path pairs must appear once with one GUID and one configuration block.
  - Any touched solution file must keep the required Visual Studio header as the first line with no leading blank line or stray BOM-only line.
  - Test/setup helpers and verification scripts read environment configuration once per value and reuse the parsed result instead of duplicating environment-variable lookups across branches.
  - Database constraint names must match predicate semantics; reserve "Xor" naming for strict exactly-one rules and use explicit mutual-exclusion naming when both-false is allowed.
  - Method and type naming remains compliant with language conventions (for example PascalCase in C#).
  - Exception and failure messages never include secrets (connection strings, credentials, tokens, keys).
  - Result-style failure factories enforce non-null failure payloads (for example guard `Failure(Error error)` inputs against nulls in both non-generic and generic result types).
  - Shared foundational primitives (for example Result/Error base types) retain direct tests for both non-generic and generic invariants when touched.
  - Value-object parse/creation APIs reject lossy coercion (for example silently truncating fractional inputs) unless the behavior is explicitly required and tested.
  - Domain creation/update paths enforce persistence-backed field limits (for example max length, precision, scale) at creation-time so invalid objects are rejected before persistence.
  - Integration tests that provision external resources (databases, containers, queues, files) perform best-effort cleanup in `finally` blocks.
  - Public/shared parse or mapping APIs retain direct acceptance tests when touched; do not remove only-path coverage without replacement.
  - Constrained-code parse/validation failures remain actionable by including allowed values (prefer constants over inline literals).
  - Validation messages must derive allowed values from a single source of truth rather than duplicating hard-coded literals across exception messages.
  - Database-backed tests must use unique test-scoped database names and safe connection-string handling; never use a provided connection string verbatim against a shared or non-test database.
  - PR titles, descriptions, and slice prompts describe only the operations actually implemented; do not claim CRUD, get-by-id, or other endpoint coverage unless those handlers and routes are present in the diff.

**Prohibited Without Review:**

- Security-critical code (auth, crypto, permissions)
- Database schema changes, data migrations
- API contract modifications, breaking changes
- Production configuration, environment variables

## AI Code Reviews

**Required For:**

- All AI-generated code before commit
- All human code in PR before merge
- Any security-sensitive changes
- Cross-cutting refactors

**Review Focus:**

- **Correctness**: Logic errors, edge cases, type safety
- **Security**: Injection risks, auth bypasses, data exposure
- **Performance**: N+1 queries, memory leaks, blocking operations
- **Style**: Naming, structure, idioms, conventions
- **Tests**: Coverage, quality, edge cases

**Tools:**

- Use `review` tool for branch comparisons
- Use `reviewUnstaged` for working directory changes
- Use `reviewStaged` for pre-commit validation

**Interpretation:**

- High severity → MUST fix before commit
- Medium severity → Fix or document reason to skip
- Low severity → Consider for future improvement

## Human Code Reviews

**Mandatory For:**

- First use of new patterns, libraries, or architectures
- Security-critical changes (auth, permissions, data access)
- API contracts, database schemas, breaking changes
- Code flagged by AI review as high-risk or complex
- Any change touching >500 lines or >5 files

**Review Criteria:**

- [ ] Solves stated problem completely
- [ ] No unintended side effects or regressions
- [ ] Tests validate success and failure cases
- [ ] Documentation updated (README, API docs, comments)
- [ ] Follows project conventions and standards
- [ ] No security vulnerabilities introduced
- [ ] Performance implications acceptable

**Process:**

- AI review first → address findings → human review
- Reviewer requests changes → author updates → re-review
- Approved → ready for merge (pending PR approval)

## PR Approval Workflow

**Approval Gates:**

1. **AI Review Pass** (automated)
   - No high-severity issues unresolved
   - All required tests passing
   - Provenance metadata complete (AI-generated code)

2. **Human Approval** (1+ required)
   - Maintainer or code owner review
   - Approval indicates: correct, safe, maintainable, tested

**Who Can Approve:**

- Project maintainers: all PRs
- Code owners: files in their domain
- Senior devs: routine changes in their area

**Merge Requirements:**

- [ ] **Technical Gates**: All checks in [Git Workflow](git-workflow.instructions.md) passed
- [ ] **AI Review**: Completed with no unresolved high-severity issues
- [ ] **Human Approval**: ≥1 approval from qualified reviewer
- [ ] **Comments**: No unresolved review comments

**Special Cases:**

- Hotfix: 1 approval, AI review optional if critical
- Docs-only: AI review optional, 1 approval sufficient
- Bot/automation: Requires maintainer approval

## Quality Gates

**Technical Gates:**
Refer to [Git Workflow](git-workflow.instructions.md) for all CI, testing, and metadata requirements.

**Process Gates:**

- [ ] AI Review completed
- [ ] Human Review completed
- [ ] PR Approval granted

## Exceptions

**Emergency Hotfix:**

- May skip AI review if immediate deployment critical
- Requires post-merge review and follow-up PR if needed
- Document exception reason in commit message

**Experimental Branches:**

- Relaxed review requirements
- Must not merge to main/production branches
- Label as `experimental` or `wip`

**Automated Updates:**

- Dependency bumps: AI review + 1 approval
- Generated code (schemas, clients): Validate generation, spot-check output

## Integration

After creating this file, update `.github/instructions/project-overview.instructions.md`:

- Add reference in "Standards" or "Key Patterns" section
- Link as: `[AI-Assisted Development Process](.github/instructions/ai-dev-process.instructions.md)`
- Note: Defines code generation, review, and approval workflows
