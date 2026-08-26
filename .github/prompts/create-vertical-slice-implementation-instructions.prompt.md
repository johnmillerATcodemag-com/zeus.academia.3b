---
ai_generated: true
model: "anthropic/claude-sonnet-4.6@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-26-vertical-slices-prompt"
prompt: |
  create a new prompt file, that creates an instruction file, that provides guidance
  for implementing applications in vertical slices
started: "2026-02-26T00:00:00Z"
ended: "2026-02-26T00:15:00Z"
task_durations:
  - task: "design prompt structure"
    duration: "00:05:00"
  - task: "draft content"
    duration: "00:08:00"
  - task: "validate metadata"
    duration: "00:02:00"
total_duration: "00:15:00"
ai_log: "ai-logs/2026/02/26/2026-02-26-vertical-slices-prompt/conversation.md"
source: "johnmillerATcodemag-com"
name: implement-vertical-slice
description: Generate an instruction file for implementing full-stack feature domains as vertical slices composed of use-case folders
author: John Miller
tags: [vertical-slice, architecture, csharp, mediatr, instructions]
arguments:
  - name: stack
    description: "Target stack abbreviation: fullstack | csharp-mediatr | csharp-minimal-api (default: fullstack)"
    required: false
context: "Full-stack Academic Management System using ASP.NET Core + MediatR + EF Core and Vue 3 + Pinia in a unified vertical-slice architecture rooted at src/features"
expected_output: "Complete .instructions.md file governing full-stack vertical-slice structure, file layout, and implementation patterns under a unified src/features tree"
tools: ["create_file", "read_file", "semantic_search"]
mode: agent
---

# Generate Vertical Slice Architecture Instruction File

Create a comprehensive `.instructions.md` file that guides AI assistants in implementing full-stack feature domains as collections of self-contained use-case folders, following conventions established in this project.

## Context Analysis

Before generating the instruction file, gather context:

1. Read `#file:.github/instructions/project-overview.instructions.md` — confirm tech stack and key patterns.
2. Read `#file:.github/instructions/cqrs-mediatr-efcore.instructions.md` — understand existing CQRS conventions to align slice structure.
3. Read `#file:.github/instructions/csharp-implementation.instructions.md` — apply C# coding standards inside slices.
4. Read `#file:.github/instructions/vue3-implementation.instructions.md` and `#file:.github/instructions/pinia-implementation.instructions.md` — align frontend implementation with project standards.
5. Read `#file:.github/instructions/typescript-frontend-implementation.instructions.md` — align TypeScript strictness and naming.
6. Scan `src/features/` for existing feature-domain folders and use-case folders to infer naming and layout conventions already in use.

**Stack argument** (default `fullstack`):

| Value                | Meaning                                                       |
| -------------------- | ------------------------------------------------------------- |
| `fullstack`          | ASP.NET Core + MediatR + EF Core + Vue 3 + Pinia + TypeScript |
| `csharp-mediatr`     | ASP.NET Core + MediatR + EF Core                              |
| `csharp-minimal-api` | ASP.NET Core Minimal APIs + MediatR                           |

## Output File

**Path**: `.github/instructions/vertical-slice-implementation.instructions.md`

**Metadata** (AI provenance + Copilot fields):

```yaml
---
ai_generated: true
model: "<provider>/<model>@<version>"
operator: "<operator>"
chat_id: "<chat-id>"
prompt: |
  <exact prompt used>
started: "<ISO8601>"
ended: "<ISO8601>"
task_durations:
  - task: "context analysis"
    duration: "<hh:mm:ss>"
  - task: "draft instruction content"
    duration: "<hh:mm:ss>"
total_duration: "<hh:mm:ss>"
ai_log: "ai-logs/<yyyy>/<mm>/<dd>/<chat-id>/conversation.md"
source: "<chat-id>"
description: "Vertical slice architecture implementation standards"
applyTo: ["src/features/**/*.cs", "src/features/**/*.{vue,ts}"]
---
```

## Required Instruction Sections

The generated instruction file MUST include all of the following sections:

### 1. Core Principle

Define vertical slice in one paragraph:

- Distinguish the terms explicitly: a `feature` is the domain folder under `src/features/`; a `use-case` is the child folder that implements one action or query.
- A slice = one cohesive use-case spanning all layers (UI → state/composable → HTTP contract → handler → persistence).
- A slice owns its implementation end-to-end inside a single use-case folder tree; do not split the same use-case across layer folders.
- No cross-slice dependencies; shared kernel only for primitives.
- Prefer duplication within a slice over coupling between slices.
- Backend and frontend artifacts for the same use-case should live together under the same feature boundary in `src/features/`.

### 2. Folder Structure

Provide the canonical folder layout for one feature domain with several use-case folders. Use `{{feature_name}}` for the feature-domain folder name (default: `Enrollment`):

```
src/features/
└── {{feature_name}}/
  ├── Create{{feature_name}}/
  │   ├── Create{{feature_name}}Command.cs           # IRequest<Result>
  │   ├── Create{{feature_name}}Handler.cs           # IRequestHandler
  │   ├── Create{{feature_name}}CommandValidator.cs  # AbstractValidator
  │   ├── Create{{feature_name}}Response.cs          # DTO
  │   ├── Create{{feature_name}}Endpoint.cs          # Minimal API or Controller mapping
  │   ├── Create{{feature_name}}Mappings.cs          # AutoMapper / manual mapping for this use-case
  │   ├── Create{{feature_name}}Form.vue             # slice UI
  │   ├── useCreate{{feature_name}}.ts               # orchestrates UI behavior
  │   ├── useCreate{{feature_name}}Store.ts          # Pinia slice store
  │   ├── create{{feature_name}}Api.ts               # typed HTTP client
  │   ├── create{{feature_name}}Types.ts             # request/response contracts
  │   └── create{{feature_name}}Route.ts             # route record for this use-case
  ├── Get{{feature_name}}ById/
  │   └── ...
  └── List{{feature_name}}s/
    └── ...
```

Rules:

- `{{feature_name}}` refers to the feature-domain folder, not to a single use-case.
- One folder per use-case (e.g., `CreateEnrollment`, `GetEnrollmentById`).
- Do not introduce separate layer roots as the primary organizational split for slice code.
- Do not introduce `Commands/`, `Queries/`, `api/`, `components/`, `stores/`, `types/`, or `routes/` as the primary organizational axis for a slice.
- File names match class names exactly.
- Validators live beside their command/query — never in a shared `Validators/` folder.
- Response DTOs are slice-private; never reuse across features without explicit shared-kernel promotion.
- Frontend types and store state are use-case-private; avoid global shared state unless required by 3+ slices.
- Keep all artifacts for a use-case inside that use-case folder.

### 3. Naming Conventions

State explicitly in this section that `Feature` means the feature-domain name and `Verb<Feature>` names a use-case folder inside that feature domain.

| Artifact       | Pattern                     | Example                            |
| -------------- | --------------------------- | ---------------------------------- |
| Command        | `<Verb><Feature>Command`    | `CreateEnrollmentCommand`          |
| Query          | `<Verb><Feature>Query`      | `GetEnrollmentByIdQuery`           |
| Handler        | `<CommandOrQuery>Handler`   | `CreateEnrollmentHandler`          |
| Validator      | `<CommandOrQuery>Validator` | `CreateEnrollmentCommandValidator` |
| Response DTO   | `<CommandOrQuery>Response`  | `CreateEnrollmentResponse`         |
| Endpoint class | `<CommandOrQuery>Endpoint`  | `CreateEnrollmentEndpoint`         |
| Mapping helper | `<CommandOrQuery>Mappings`  | `CreateEnrollmentMappings`         |
| API client     | `<verb><feature>Api.ts`     | `createEnrollmentApi.ts`           |
| Pinia store    | `use<Verb><Feature>Store`   | `useCreateEnrollmentStore`         |
| Composable     | `use<Verb><Feature>`        | `useCreateEnrollment`              |
| Vue component  | `<Verb><Feature><View>.vue` | `CreateEnrollmentForm.vue`         |
| Route module   | `<verb><feature>Route.ts`   | `createEnrollmentRoute.ts`         |

### 4. Implementation Templates

Provide minimal, copy-paste-ready templates for artifacts co-located in the same use-case folder:

#### Command

```csharp
public sealed record Create{{feature_name}}Command(/* properties */) : IRequest<Result<Create{{feature_name}}Response>>;
```

#### Handler

```csharp
public sealed class Create{{feature_name}}Handler(AppDbContext db)
    : IRequestHandler<Create{{feature_name}}Command, Result<Create{{feature_name}}Response>>
{
    public async Task<Result<Create{{feature_name}}Response>> Handle(
        Create{{feature_name}}Command request, CancellationToken cancellationToken)
    {
        // 1. Validate domain rules
        // 2. Create/update aggregate
        // 3. Persist
        // 4. Return response
    }
}
```

#### Validator

```csharp
public sealed class Create{{feature_name}}CommandValidator : AbstractValidator<Create{{feature_name}}Command>
{
    public Create{{feature_name}}CommandValidator()
    {
        RuleFor(x => x./* property */).NotEmpty();
    }
}
```

#### Minimal API Endpoint

```csharp
public static class Create{{feature_name}}Endpoint
{
    public static RouteGroupBuilder MapCreate{{feature_name}}(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (Create{{feature_name}}Command cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return result.IsSuccess ? Results.Created($"/api/{{feature_name | kebab-case}}/{result.Value.Id}", result.Value)
                                    : Results.Problem(result.Error);
        })
        .WithName("Create{{feature_name}}")
        .Produces<Create{{feature_name}}Response>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        return group;
    }
}
```

#### Frontend API Client

```ts
import type { Create{{feature_name}}Request, Create{{feature_name}}Response } from "./create{{feature_name}}Types";
import { apiClient } from "@/shared/apiClient";

export const create{{feature_name}}Api = {
  async create(payload: Create{{feature_name}}Request): Promise<Create{{feature_name}}Response> {
    const { data } = await apiClient.post<Create{{feature_name}}Response>("/api/{{feature_name | kebab-case}}", payload);
    return data;
  }
};
```

#### Pinia Store

```ts
import { defineStore } from "pinia";
import { create{{feature_name}}Api } from "./create{{feature_name}}Api";
import type { Create{{feature_name}}Request, Create{{feature_name}}Response } from "./create{{feature_name}}Types";

export const useCreate{{feature_name}}Store = defineStore("create{{feature_name | camelCase}}", {
  state: () => ({
    isSaving: false,
    created: null as Create{{feature_name}}Response | null
  }),
  actions: {
    async create(payload: Create{{feature_name}}Request) {
      this.isSaving = true;
      try {
        this.created = await create{{feature_name}}Api.create(payload);
      } finally {
        this.isSaving = false;
      }
    }
  }
});
```

#### Vue Component

```vue
<script setup lang="ts">
import { reactive } from "vue";
import { useCreate{{feature_name}}Store } from "./useCreate{{feature_name}}Store";

const store = useCreate{{feature_name}}Store();
const form = reactive({});

async function onSubmit() {
  await store.create(form as never);
}
</script>

<template>
  <form @submit.prevent="onSubmit">
    <button :disabled="store.isSaving" type="submit">Save</button>
  </form>
</template>
```

### 5. Shared Kernel Rules

- Allowed in shared kernel: primitive value objects, domain events, `Result<T>`, common exceptions, base entity.
- Prohibited in shared kernel: feature-specific DTOs, validators, or business logic.
- Shared kernel path: `src/shared/` or `src/features/Shared/` if the repository keeps shared primitives alongside slices.
- Add to shared kernel only after a concept appears in ≥3 slices.

### 6. Registration Pattern

Describe how slices self-register:

- Use one route group per feature domain and map one endpoint extension per use-case from inside that use-case folder under `src/features/`.
- MediatR handlers auto-discovered via assembly scanning — no manual registration per handler.
- Validators registered via `AddValidatorsFromAssembly` — no manual registration per validator.
- Route modules registered via per-use-case route modules imported by a central router aggregator.
- Stores are used from slice composables/components; avoid a global monolithic store.

### 7. Testing Conventions

- One test class per handler: `Create{{feature_name}}HandlerTests`.
- Test file path mirrors source: `tests/features/{{feature_name}}/Create{{feature_name}}/`.
- Use a SQL Server-backed test environment (for example SQL Server LocalDB or a SQL Server test container) — never mock `DbContext` and never substitute SQLite in-memory providers for this repository.
- Validate the full slice (command → handler → db round-trip) in integration tests; unit-test validators separately.
- For every persistence-bearing slice, require a provider-backed SQL Server integration harness in the feature test project. The harness must isolate a unique database, apply migrations, assert through a fresh DbContext, and clean up best-effort; InMemory tests alone are insufficient.
- Add UI tests under `src/features/{{feature_name}}/Create{{feature_name}}/__tests__/` and sibling use-case folders.
- Test component rendering, store actions, and API error handling with Vitest.
- Validate contract alignment between UI request/response types and endpoint DTOs.

### 8. Anti-Patterns

| Anti-Pattern                                                                        | Instead                                                                   |
| ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Shared `Services/` folder across features                                           | Move logic into the handler; extract to shared kernel only when warranted |
| Generic `BaseHandler<T>`                                                            | Concrete, explicit handler per use-case                                   |
| Cross-slice handler calls                                                           | Publish domain events; let each slice's handler react independently       |
| Reusing response DTOs across features                                               | Keep DTOs slice-private; promote to shared kernel consciously             |
| Putting validators in a global `Validators/` folder                                 | Co-locate validator with its command/query                                |
| Splitting slices between separate layer roots                                       | Keep the full slice under `src/features/<Feature>/<UseCase>/...`          |
| Splitting one use-case across `Commands/Queries` or `api/components/stores` folders | Keep every artifact for the use-case in one folder tree                   |
| Anemic domain model with all logic in handlers                                      | Encapsulate invariants in the domain entity                               |
| Reusing one global Pinia store for all use-cases                                    | Use one store per use-case                                                |
| Components calling HTTP directly everywhere                                         | Centralize transport in per-slice API clients                             |
| UI models drifting from endpoint DTO contracts                                      | Define and validate shared contract mapping per slice                     |

### 9. Quality Checklist

Include a per-slice checklist at the end of the instruction file:

- [ ] Feature-domain folder matches the domain name exactly
- [ ] Use-case folder matches the use-case name exactly
- [ ] Command/Query is a `sealed record`
- [ ] Handler registered via assembly scan (not manually)
- [ ] Validator co-located with command/query
- [ ] Response DTO is slice-private
- [ ] Endpoint lives in the same use-case folder and maps to a distinct HTTP verb + route
- [ ] Integration test covers success and at least one failure path
- [ ] Persistence-bearing slices include SQL Server provider, migration-backed harness, fresh-context read-back, isolated database cleanup, and recorded integration-test evidence
- [ ] No direct dependency on another feature domain's namespace
- [ ] Use-case folder includes component, API client, store/composable, route module, and typed contracts
- [ ] UI tests cover success and error flow
- [ ] Route registration for the slice is present in router aggregation

## Validation

Before saving the output file, verify:

- [ ] All nine sections present
- [ ] `applyTo` glob covers files under `src/features/`
- [ ] Templates compile (no missing using directives)
- [ ] Naming table is consistent with templates
- [ ] Anti-patterns list addresses real pitfalls in this stack
- [ ] AI provenance metadata complete and accurate
- [ ] Canonical structure uses `src/features/<Feature>/<UseCase>/...` rather than separate layer roots
- [ ] Terminology consistently distinguishes feature domains from use-case folders
