---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-24-project-overview-zeus-academia"
prompt: |
  Submit create-technology-instructions.prompt.md with arguments:
  technology_name: MediatR
  technology_category: backend
  primary_language: C#
  project_context: CQRS command/query handling in ASP.NET Core API
  version_target: 12.0+
started: "2026-02-24T01:05:00Z"
ended: "2026-02-24T01:15:00Z"
task_durations:
  - task: "analyze MediatR patterns"
    duration: "00:03:00"
  - task: "document CQRS standards"
    duration: "00:05:00"
  - task: "create examples"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/02/24/2026-02-24-project-overview-zeus-academia/conversation.md"
source: ".github/prompts/create-technology-instructions.prompt.md"
name: "MediatR CQRS Standards"
description: "MediatR implementation standards for CQRS pattern in feature-domain folders"
applyTo: "src/**/*.cs"
tags: [mediatr, cqrs, backend, csharp, commands, queries]
---

# MediatR CQRS Implementation Standards

**Role**: CQRS command/query handling in ASP.NET Core API
**Version**: 12.0+
**Language**: C#
**Related**: See [CQRS + Event Sourcing](.github/instructions/cqrs-es-csharp-mediatr.instructions.md) for full architectural guidance

## Core Principles

- **Separation**: Commands (write) separate from queries (read)
- **Single Responsibility**: One handler per command/query
- **Immutability**: Commands and queries are immutable DTOs
- **Validation**: FluentValidation pipeline before handlers
- **Registration requirement**: A request with validation must actually be registered in the application pipeline; a validator file or handler-only check does not satisfy runtime validation
- **No duplicate enforcement**: A request should not validate the same invariant in both the handler and a validator when one source of truth is enough
- A validator may invoke a canonical domain factory for early feedback, while the handler invokes that same factory before state mutation. Neither layer may maintain a second implementation of the invariant.
- **Naming**: Clear, action-based command names, question-based query names

## File Organization

- `src/features/<Feature>/<UseCase>/` - Keep the request, handler, DTOs, and mapping for one command or query together
- `src/features/<Feature>/Shared/` - Feature-scoped abstractions used by multiple use-cases in the same feature domain
- `src/shared/Behaviors/` - Cross-cutting MediatR pipeline behaviors
- Pattern: `<Action><Entity>Command.cs`, `Get<Entity>Query.cs`, `<RequestName>Handler.cs`

## Standard Patterns

### Command Pattern

```csharp
// src/features/Students/CreateStudent/CreateStudentCommand.cs
using MediatR;

namespace Zeus.Academia.Features.Students.CreateStudent;

public sealed record CreateStudentCommand(
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth
) : IRequest<StudentDto>;

// src/features/Students/CreateStudent/CreateStudentCommandHandler.cs
using MediatR;
using Zeus.Academia.Domain.Entities;
using Zeus.Academia.Domain.Repositories;

namespace Zeus.Academia.Features.Students.CreateStudent;

public sealed class CreateStudentCommandHandler
    : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStudentCommandHandler> _logger;

    public CreateStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateStudentCommandHandler> logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<StudentDto> Handle(
        CreateStudentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating student: {Email}", request.Email);

        var student = Student.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        await _studentRepository.AddAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Student created with ID: {StudentId}", student.Id);

        return StudentDto.FromEntity(student);
    }
}
```

### Query Pattern

```csharp
// src/features/Students/GetStudent/GetStudentQuery.cs
using MediatR;

namespace Zeus.Academia.Features.Students.GetStudent;

public sealed record GetStudentQuery(string Id) : IRequest<StudentDto?>;

// src/features/Students/GetStudent/GetStudentQueryHandler.cs
using MediatR;
using Zeus.Academia.Domain.Repositories;

namespace Zeus.Academia.Features.Students.GetStudent;

public sealed class GetStudentQueryHandler
    : IRequestHandler<GetStudentQuery, StudentDto?>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<GetStudentQueryHandler> _logger;

    public GetStudentQueryHandler(
        IStudentRepository studentRepository,
        ILogger<GetStudentQueryHandler> logger)
    {
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<StudentDto?> Handle(
        GetStudentQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching student: {StudentId}", request.Id);

        var student = await _studentRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        return student is not null
            ? StudentDto.FromEntity(student)
            : null;
    }
}
```

### Query with Filtering and Pagination

```csharp
// src/features/Students/GetStudents/GetStudentsQuery.cs
using MediatR;

namespace Zeus.Academia.Features.Students.GetStudents;

public sealed record GetStudentsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? SearchTerm = null
) : IRequest<PagedResult<StudentDto>>
{
    public int Skip => (Page - 1) * PageSize;
}

// Handler
public sealed class GetStudentsQueryHandler
    : IRequestHandler<GetStudentsQuery, PagedResult<StudentDto>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<PagedResult<StudentDto>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var (students, totalCount) = await _studentRepository.GetPagedAsync(
            request.Skip,
            request.PageSize,
            request.Status,
            request.SearchTerm,
            cancellationToken);

        return new PagedResult<StudentDto>(
            students.Select(StudentDto.FromEntity),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
```

### Void Command (No Return Value)

```csharp
// src/features/Students/DeleteStudent/DeleteStudentCommand.cs
using MediatR;

namespace Zeus.Academia.Features.Students.DeleteStudent;

public sealed record DeleteStudentCommand(string Id) : IRequest<Unit>;

// Handler
public sealed class DeleteStudentCommandHandler
    : IRequestHandler<DeleteStudentCommand, Unit>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStudentCommandHandler(
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        DeleteStudentCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (student is null)
            throw new NotFoundException(nameof(Student), request.Id);

        await _studentRepository.DeleteAsync(student, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
```

### Pipeline Behaviors

```csharp
// src/shared/Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;

namespace Zeus.Academia.Shared.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}

// src/shared/Behaviors/LoggingBehavior.cs
using MediatR;
using System.Diagnostics;

namespace Zeus.Academia.Shared.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

## Dependency Injection Setup

```csharp
// src/shared/DependencyInjection/MediatRRegistration.cs
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zeus.Academia.Shared.Behaviors;

namespace Zeus.Academia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
```

## Naming Conventions

### Commands (Write Operations)

- `Create<Entity>Command` - Create new entity
- `Update<Entity>Command` - Update existing entity
- `Delete<Entity>Command` - Delete entity
- `Register<Action>Command` - Business operation
- `Process<Action>Command` - Workflow step
- `Approve<Entity>Command` - Approval operation

### Queries (Read Operations)

- `Get<Entity>Query` - Single entity by ID
- `Get<Entities>Query` - Multiple entities (list/page)
- `Search<Entities>Query` - Search with filters
- `Count<Entities>Query` - Get count only
- `Exists<Entity>Query` - Boolean check
- `Calculate<Metric>Query` - Computed value

### Handlers

- `<Command>Handler` - Command handler
- `<Query>Handler` - Query handler

## Integration

- **FluentValidation**: Automatic validation via pipeline behavior
- **ASP.NET Core**: Called from controllers or minimal endpoints
- **Unit of Work**: Transaction management for commands
- **Repositories**: Data access abstraction
- **Domain Events**: Raised from entities, published via MediatR

## Performance Considerations

```csharp
// Use cancellation tokens
public async Task<StudentDto> Handle(
    GetStudentQuery request,
    CancellationToken cancellationToken)
{
    return await _repository.GetAsync(request.Id, cancellationToken);
}

// Avoid N+1 queries in handlers
public async Task<PagedResult<EnrollmentDto>> Handle(
    GetEnrollmentsQuery request,
    CancellationToken cancellationToken)
{
    // Include related entities
    var enrollments = await _repository.GetWithStudentsAndCoursesAsync(
        request.Skip,
        request.PageSize,
        cancellationToken);

    return PagedResult.From(enrollments);
}
```

## Validation Checklist

- [ ] Commands are immutable records
- [ ] Queries are immutable records
- [ ] One handler per request
- [ ] Handlers implement `IRequestHandler<TRequest, TResponse>`
- [ ] CancellationToken passed to all async calls
- [ ] Validation via FluentValidation validators
- [ ] Logging in handlers or pipeline behavior
- [ ] Unit of Work used in command handlers
- [ ] Queries don't modify state
- [ ] Return DTOs, not domain entities

## Anti-Patterns

❌ Mutable commands/queries
✅ Immutable `record` types

❌ Multiple responsibilities in one handler
✅ Single responsibility, focused handlers

❌ Business logic in pipeline behaviors
✅ Business logic only in handlers

❌ Queries modifying data
✅ Queries are read-only

❌ Returning domain entities to API
✅ Return DTOs/projections

❌ Catching exceptions in handlers without re-throwing
✅ Let exceptions bubble up or handle specifically

❌ Validation logic in handlers
✅ FluentValidation validators + pipeline

❌ Direct DbContext access in handlers
✅ Repository abstraction
