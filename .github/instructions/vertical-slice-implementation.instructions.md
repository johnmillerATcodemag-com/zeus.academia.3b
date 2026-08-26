---
ai_generated: true
model: "anthropic/claude-sonnet-4.6@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-26-vertical-slice-instructions-generation"
prompt: |
  submit #file:create-vertical-slice-implementation-instructions.prompt.md
started: "2026-02-26T00:30:00Z"
ended: "2026-02-26T00:50:00Z"
task_durations:
  - task: "context analysis"
    duration: "00:05:00"
  - task: "draft instruction content"
    duration: "00:12:00"
  - task: "validate and optimize"
    duration: "00:03:00"
total_duration: "00:20:00"
ai_log: "ai-logs/2026/02/26/2026-02-26-vertical-slice-instructions-generation/conversation.md"
source: ".github/prompts/create-vertical-slice-implementation-instructions.prompt.md"
description: "Vertical slice architecture implementation standards"
applyTo: ["src/features/**/*.cs", "src/features/**/*.{vue,ts}"]
---

# Vertical Slice Architecture — Full-Stack Implementation Standards

Standards for implementing feature domains as collections of self-contained use-case folders in the zeus.academia application.

## 1. Core Principle

### Composition and Persistence Boundary

- A persistence-bearing feature may own a feature-local DbContext and migration set. Shared Kernel domain entities and reusable mapping semantics may be referenced by that context; do not require the feature to reuse `SharedKernelDbContext`.
- Every persistence-bearing slice must explicitly name its DbContext, table ownership, migration owner, migration artifact root, and the host or deployment command that applies its migrations.
- No two DbContexts may own migrations for the same table. Shared Kernel does not own feature-slice migrations unless the architecture explicitly assigns a table to it.
- Placeholder persistence entities are prohibited. Any persistence-bearing model must define a primary key and a valid EF Core configuration before the slice is considered ready.
- Startup and migration verification must fail explicitly when SQL Server connectivity is unavailable; do not silently skip, swallow, or pass on missing LocalDB/SQL Server prerequisites.
- Model verification must inspect the actual EF Core model and primary-key shape directly before migration execution; no placeholder, no empty stubs, and no silent pass-through once the model is invalid.
- Runtime completion gate: every feature route aggregator or `Map...Endpoints()` method must be invoked from the application host or composition root before the slice is considered complete.
- Validation pipeline gate: if a command/query declares validation behavior, the validator must be registered in DI or the MediatR pipeline and verified as active before completion.
- Migration ownership gate: any schema change to a feature-local DbContext must include the migration artifacts and host evidence showing migration execution, not just code in the DbContext.
- Contract parity gate: if an endpoint advertises validation or conflict responses, the runtime must return the declared 4xx response instead of surfacing raw exceptions or 500s.

## Runtime and Integration Completion Checklist

Before a slice is considered ready for review or merge, verify all of the following:

- [ ] Every new `Map...Endpoints()` or endpoint group is called from `Program.cs` or the composition root.
- [ ] Startup or integration verification confirms the route is reachable at runtime.
- [ ] Validation is registered and active for any request that advertises validation responses.
- [ ] Feature-local DbContext schema changes include migration artifacts and explicit migration ownership.
- [ ] Shared normalization or validation logic is centralized instead of duplicated across handlers, validators, and mappings.
- [ ] Endpoint `Produces*` contracts match actual runtime responses, especially validation, conflict, and parse failures.

Use the following terms consistently:

- A `feature domain` is a top-level folder under `src/features/`, such as `Enrollment`.
- A `use-case` is a child folder under a feature domain, such as `CreateEnrollment` or `GetEnrollmentById`.

A vertical slice is one cohesive use-case that spans all relevant layers from route to persistence. Organize code by feature domain first and by use-case second, so each use-case folder owns its command or query, validation, mapping, endpoint, UI, client, store, and route registration inside one folder tree.

- Own each use-case end-to-end inside a single use-case folder.
- Allow no compile-time dependency on another feature domain's private slice artifacts.
- Use the shared kernel only for primitives and cross-cutting building blocks.
- Prefer duplication within a use-case over coupling between slices.
- Keep all artifacts for the same use-case together under the same feature-domain tree.

## 2. Canonical Folder Structure

One top-level folder per feature domain and one child folder per use-case.

```text
src/features/
└── Enrollment/
    ├── CreateEnrollment/
    │   ├── CreateEnrollmentCommand.cs
    │   ├── CreateEnrollmentCommandValidator.cs
    │   ├── CreateEnrollmentHandler.cs
    │   ├── CreateEnrollmentResponse.cs
    │   ├── CreateEnrollmentEndpoint.cs
    │   ├── CreateEnrollmentMappings.cs
    │   ├── CreateEnrollmentForm.vue
    │   ├── useCreateEnrollment.ts
    │   ├── useCreateEnrollmentStore.ts
    │   ├── createEnrollmentApi.ts
    │   ├── createEnrollmentTypes.ts
    │   └── createEnrollmentRoute.ts
    ├── GetEnrollmentById/
    │   ├── GetEnrollmentByIdQuery.cs
    │   ├── GetEnrollmentByIdHandler.cs
    │   ├── GetEnrollmentByIdResponse.cs
    │   └── GetEnrollmentByIdEndpoint.cs
    └── ListEnrollments/
        └── ...
```

Rules:

- Do not split slice code between separate layer roots.
- Do not introduce `Commands/`, `Queries/`, `api/`, `components/`, `stores/`, `types/`, or `routes/` as the primary organizational axis for a use-case.
- File names must match their primary type or exported symbol exactly.
- Validators live beside their command or query.
- Response DTOs and UI contracts are use-case-private until promoted deliberately.
- Optional feature-domain aggregators are acceptable only for composing use-case endpoints or routes; use-case behavior still lives in the use-case folders.

## 3. Naming Conventions

In the patterns below, `Feature` means the feature-domain name. `Verb<Feature>` means a specific use-case folder inside that feature domain.

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
| Test class     | `<Handler>Tests`            | `CreateEnrollmentHandlerTests`     |

## 4. Slice Templates

### 4.1 Command

```csharp
namespace Zeus.Academia.Features.Enrollment.CreateEnrollment;

public sealed record CreateEnrollmentCommand(
    Guid StudentId,
    Guid CourseId,
    DateOnly RequestedDate) : IRequest<Result<CreateEnrollmentResponse>>;
```

### 4.2 Validator

```csharp
namespace Zeus.Academia.Features.Enrollment.CreateEnrollment;

public sealed class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
{
    public CreateEnrollmentCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.RequestedDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
```

### 4.3 Handler

```csharp
namespace Zeus.Academia.Features.Enrollment.CreateEnrollment;

public sealed class CreateEnrollmentHandler(AppDbContext db)
    : IRequestHandler<CreateEnrollmentCommand, Result<CreateEnrollmentResponse>>
{
    public async Task<Result<CreateEnrollmentResponse>> Handle(
        CreateEnrollmentCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await db.Enrollments
            .AnyAsync(e => e.StudentId == request.StudentId
                        && e.CourseId == request.CourseId, cancellationToken);

        if (exists)
        {
            return Result<CreateEnrollmentResponse>.Failure("Student is already enrolled in this course.");
        }

        var enrollment = Enrollment.Create(request.StudentId, request.CourseId, request.RequestedDate);

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(cancellationToken);

        return Result<CreateEnrollmentResponse>.Success(
            new CreateEnrollmentResponse(enrollment.Id, enrollment.Status));
    }
}
```

### 4.4 Response DTO

```csharp
namespace Zeus.Academia.Features.Enrollment.CreateEnrollment;

public sealed record CreateEnrollmentResponse(Guid EnrollmentId, EnrollmentStatus Status);
```

### 4.5 Endpoint

```csharp
namespace Zeus.Academia.Features.Enrollment.CreateEnrollment;

public static class CreateEnrollmentEndpoint
{
    public static RouteGroupBuilder MapCreateEnrollment(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateEnrollmentCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/enrollments/{result.Value.EnrollmentId}", result.Value)
                : Results.Problem(result.Error);
        })
        .WithName("CreateEnrollment")
        .Produces<CreateEnrollmentResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        return group;
    }
}
```

### 4.6 Query Pattern

```csharp
namespace Zeus.Academia.Features.Enrollment.GetEnrollmentById;

public sealed class GetEnrollmentByIdHandler(AppDbContext db)
    : IRequestHandler<GetEnrollmentByIdQuery, Result<GetEnrollmentByIdResponse>>
{
    public async Task<Result<GetEnrollmentByIdResponse>> Handle(
        GetEnrollmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var response = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == request.EnrollmentId)
            .Select(e => new GetEnrollmentByIdResponse(e.Id, e.StudentId, e.CourseId, e.Status, e.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return response is not null
            ? Result<GetEnrollmentByIdResponse>.Success(response)
            : Result<GetEnrollmentByIdResponse>.Failure("Enrollment not found.");
    }
}
```

### 4.7 API Client

```ts
import { apiClient } from "@/shared/apiClient";
import type {
  CreateEnrollmentRequest,
  CreateEnrollmentResponse,
} from "./createEnrollmentTypes";

export const createEnrollmentApi = {
  async create(
    payload: CreateEnrollmentRequest,
  ): Promise<CreateEnrollmentResponse> {
    const { data } = await apiClient.post<CreateEnrollmentResponse>(
      "/api/enrollments",
      payload,
    );
    return data;
  },
};
```

### 4.8 Pinia Store

```ts
import { defineStore } from "pinia";
import { createEnrollmentApi } from "./createEnrollmentApi";
import type {
  CreateEnrollmentRequest,
  CreateEnrollmentResponse,
} from "./createEnrollmentTypes";

export const useCreateEnrollmentStore = defineStore("createEnrollment", {
  state: () => ({
    isSaving: false,
    created: null as CreateEnrollmentResponse | null,
  }),
  actions: {
    async create(payload: CreateEnrollmentRequest) {
      this.isSaving = true;
      try {
        this.created = await createEnrollmentApi.create(payload);
      } finally {
        this.isSaving = false;
      }
    },
  },
});
```

### 4.9 Composable

```ts
import { reactive } from "vue";
import { useCreateEnrollmentStore } from "./useCreateEnrollmentStore";
import type { CreateEnrollmentRequest } from "./createEnrollmentTypes";

export function useCreateEnrollment() {
  const store = useCreateEnrollmentStore();
  const form = reactive<CreateEnrollmentRequest>({
    studentId: "",
    courseId: "",
    requestedDate: "",
  });

  async function submit() {
    await store.create({ ...form });
  }

  return {
    form,
    store,
    submit,
  };
}
```

### 4.10 Vue Component

```vue
<script setup lang="ts">
import { useCreateEnrollment } from "./useCreateEnrollment";

const { form, store, submit } = useCreateEnrollment();
</script>

<template>
  <form @submit.prevent="submit">
    <button :disabled="store.isSaving" type="submit">Save</button>
  </form>
</template>
```

### 4.11 Route Module

```ts
import type { RouteRecordRaw } from "vue-router";
import CreateEnrollmentForm from "./CreateEnrollmentForm.vue";

export const createEnrollmentRoute: RouteRecordRaw = {
  path: "/enrollments/create",
  name: "create-enrollment",
  component: CreateEnrollmentForm,
};
```

## 5. Registration Pattern

- Create one route group per feature domain in `Program.cs`, then map one endpoint extension per use-case from the corresponding use-case folders under `src/features/`.
- Discover MediatR handlers via assembly scanning; do not register handlers one by one.
- Discover validators via `AddValidatorsFromAssembly`; do not register validators one by one.
- Import route modules from each use-case folder into the router aggregator.
- Keep Pinia stores local to the use-case unless three or more slices need the same state concept.

```csharp
var enrollmentGroup = app.MapGroup("/api/enrollments")
    .WithTags("Enrollment")
    .RequireAuthorization();

enrollmentGroup.MapCreateEnrollment();
enrollmentGroup.MapGetEnrollmentById();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
```

## 6. Shared Kernel Rules

Path: `src/shared/` or `src/features/Shared/`

Allowed:

- Primitive value objects
- `Result<T>` and error types
- Domain event abstractions
- Base entity and aggregate root types
- Common exceptions

Prohibited:

- Feature-domain specific DTOs or UI contracts
- Use-case specific validators or handlers
- Business rules that belong to one use-case
- Anything that imports a feature-domain private namespace

Promote a concept to the shared kernel only after it appears in at least three independent use-cases.

## 7. Cross-Slice Communication

Never call another feature domain's handler directly. Use domain events or shared infrastructure abstractions.

```csharp
enrollment.RaiseDomainEvent(new StudentEnrolledEvent(enrollment.Id, enrollment.StudentId));
await db.SaveChangesAsync(cancellationToken);

public sealed class SendEnrollmentConfirmationHandler(IEmailService email)
    : INotificationHandler<StudentEnrolledEvent>
{
    public async Task Handle(StudentEnrolledEvent notification, CancellationToken ct)
        => await email.SendEnrollmentConfirmationAsync(notification.StudentId, ct);
}
```

## 8. Testing Conventions

- Mirror the source layout in tests: `tests/features/Enrollment/CreateEnrollment/CreateEnrollmentHandlerTests.cs`.
- Never mock `DbContext`; use a real SQL Server-backed test environment or a SQL Server test container.
- Cover the full use-case path from request through persistence.
- Test validators separately for failure-path detail.
- Place UI tests under the same use-case folder, for example `src/features/Enrollment/CreateEnrollment/__tests__/CreateEnrollmentForm.spec.ts`.
- Cover component rendering, store behavior, and API error handling.
- Keep request and response types aligned with the DTO contract.

```csharp
public sealed class CreateEnrollmentHandlerTests(AppDbContextFactory factory)
    : IClassFixture<AppDbContextFactory>
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesEnrollmentAndReturnsSuccess()
    {
        using var db = factory.CreateContext();
        var handler = new CreateEnrollmentHandler(db);
        var command = new CreateEnrollmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        db.Enrollments.Should().ContainSingle(e => e.Id == result.Value.EnrollmentId);
    }
}
```

## 9. Anti-Patterns

| Anti-Pattern                                                             | Instead                                                             |
| ------------------------------------------------------------------------ | ------------------------------------------------------------------- |
| Splitting a slice between separate layer roots                           | Keep the full slice under `src/features/<Feature>/<UseCase>/...`    |
| Splitting a use-case across `Commands/Queries` folders                   | Keep every artifact for the use-case in its use-case folder         |
| Splitting a use-case across `api/components/stores/types/routes` folders | Keep every artifact for the use-case in its use-case folder         |
| Generic `BaseHandler<T>` or `CrudHandler<T>`                             | Write one explicit handler per use-case                             |
| Direct handler-to-handler calls across feature domains                   | Publish a domain event and react independently                      |
| Reusing response DTOs across feature domains                             | Keep DTOs slice-private unless deliberately promoted                |
| Shared `Validators/` folder                                              | Co-locate validators with their command or query                    |
| One monolithic Pinia store for the whole app                             | Use one store per use-case unless a shared abstraction is justified |

## 10. Per-Slice Quality Checklist

Before marking a use-case complete:

- [ ] Feature-domain folder matches the domain name.
- [ ] Use-case folder matches the use-case name exactly.
- [ ] All artifacts for the use-case live under the same `src/features/<Feature>/<UseCase>/` tree.
- [ ] Command or query is a `sealed record`.
- [ ] Handler is `sealed` and has no public mutable state.
- [ ] Validator is co-located with its command or query.
- [ ] Endpoint file lives in the same use-case folder.
- [ ] Component, API client, store, composable, route, and types live in the same use-case folder.
- [ ] Handler and validator rely on assembly scanning, not manual DI registration.
- [ ] Tests mirror the slice structure.
- [ ] No code imports another feature domain's private slice artifacts.
