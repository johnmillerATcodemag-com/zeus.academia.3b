---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-24-project-overview-zeus-academia"
prompt: |
  Submit create-technology-instructions.prompt.md with arguments:
  technology_name: ASP.NET Core
  technology_category: backend
  primary_language: C#
  project_context: Backend API for Academic Management System
  version_target: 8.0+
started: "2026-02-24T00:55:00Z"
ended: "2026-02-24T01:05:00Z"
task_durations:
  - task: "analyze ASP.NET Core patterns"
    duration: "00:03:00"
  - task: "document API standards"
    duration: "00:05:00"
  - task: "create examples"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/02/24/2026-02-24-project-overview-zeus-academia/conversation.md"
source: ".github/prompts/create-technology-instructions.prompt.md"
name: "ASP.NET Core Standards"
description: "ASP.NET Core API development standards and best practices within feature-domain folders"
applyTo: "src/**/*.cs"
tags: [aspnetcore, backend, csharp, api, rest]
---

# ASP.NET Core Implementation Standards

**Role**: Backend API framework for Academic Management System
**Version**: 8.0+
**Language**: C#

## Core Principles

- **Minimal APIs**: Prefer minimal APIs for simple endpoints, controllers for complex logic
- **Dependency Injection**: Constructor injection for all dependencies
- **Async/Await**: All I/O operations must be async
- **Nullable Reference Types**: Enabled project-wide
- **MediatR Integration**: Use CQRS pattern via MediatR
- **Host Wiring**: New feature route groups and startup migrations must be mapped/invoked by the application host before the slice is considered complete
- **Runtime reachability**: A new route is not complete until the host actually invokes the endpoint mapper and startup verification confirms it is reachable
- **Contract parity**: If an endpoint advertises `.ProducesValidationProblem()`, `.Produces(409)`, or other specific status codes, the runtime must emit the matching response instead of a generic 500 or raw exception

## File Organization

- `src/features/<Feature>/<UseCase>/` - Co-locate endpoints, request models, handlers, and mappings for one use-case
- `src/features/<Feature>/Shared/` - Feature-scoped backend components reused by multiple use-cases in the same feature domain
- `src/shared/` - Cross-cutting middleware, primitives, and infrastructure abstractions
- Keep controllers or minimal API extensions close to the use-case they expose
- Naming: PascalCase for all files matching class names
- When a minimal API advertises a validation-problem response, catch parse/mapping `ArgumentException`, `ValidationException`, and business-conflict failures and convert them to `Results.ValidationProblem(...)`, `Results.Conflict(...)`, or another explicit 4xx/409 result instead of letting them surface as a 500
- Do not leave a route `Map...Endpoints()` method defined in a feature file unless the app host actually calls it.

## Standard Patterns

### Minimal API Endpoints

```csharp
// src/features/Students/GetStudents/StudentEndpoints.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zeus.Academia.Features.Students.CreateStudent;
using Zeus.Academia.Features.Students.DeleteStudent;
using Zeus.Academia.Features.Students.GetStudent;
using Zeus.Academia.Features.Students.GetStudents;
using Zeus.Academia.Features.Students.UpdateStudent;

namespace Zeus.Academia.Features.Students.GetStudents;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students")
            .WithTags("Students")
            .WithOpenApi();

        group.MapGet("/", GetStudents)
            .WithName("GetStudents")
            .Produces<PagedResult<StudentDto>>();

        group.MapGet("/{id}", GetStudent)
            .WithName("GetStudent")
            .Produces<StudentDto>()
            .Produces(404);

        group.MapPost("/", CreateStudent)
            .WithName("CreateStudent")
            .Produces<StudentDto>(201)
            .Produces<ValidationProblemDetails>(400);

        group.MapPut("/{id}", UpdateStudent)
            .WithName("UpdateStudent")
            .Produces<StudentDto>()
            .Produces<ValidationProblemDetails>(400)
            .Produces(404);

        group.MapDelete("/{id}", DeleteStudent)
            .WithName("DeleteStudent")
            .Produces(204)
            .Produces(404);

        return app;
    }

    private static async Task<IResult> GetStudents(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? status,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentsQuery(page, pageSize, status);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetStudent(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentQuery(id);
        var result = await mediator.Send(query, cancellationToken);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateStudent(
        CreateStudentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Results.Created($"/api/students/{result.Id}", result);
    }

    private static async Task<IResult> UpdateStudent(
        string id,
        UpdateStudentCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        var result = await mediator.Send(command, cancellationToken);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> DeleteStudent(
        string id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteStudentCommand(id);
        await mediator.Send(command, cancellationToken);
        return Results.NoContent();
    }
}
```

### Controller Pattern (for complex logic)

```csharp
// src/features/Enrollment/GetEnrollment/EnrollmentController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zeus.Academia.Features.Enrollment.EnrollStudent;
using Zeus.Academia.Features.Enrollment.GetEnrollment;
using Zeus.Academia.Features.Enrollment.GetEnrollments;

namespace Zeus.Academia.Features.Enrollment.GetEnrollment;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EnrollmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnrollmentController> _logger;

    public EnrollmentController(
        IMediator mediator,
        ILogger<EnrollmentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets enrollments with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EnrollmentDto>>> GetEnrollments(
        [FromQuery] GetEnrollmentsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving enrollments with filters: {@Query}", query);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Enrolls a student in a course
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrollmentDto>> EnrollStudent(
        EnrollStudentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Enrolling student {StudentId} in course {CourseId}",
            command.StudentId,
            command.CourseId);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(
            nameof(GetEnrollment),
            new { id = result.Id },
            result);
    }

    /// <summary>
    /// Gets a specific enrollment
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(
        string id,
        CancellationToken cancellationToken)
    {
        var query = new GetEnrollmentQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }
}
```

### Program.cs Configuration

```csharp
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Zeus.Academia.Features.Students.GetStudents;
using Zeus.Academia.Application;
using Zeus.Academia.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>()!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Application layers
builder.Services.AddApplication(); // MediatR, FluentValidation
builder.Services.AddInfrastructure(builder.Configuration); // DbContext, repositories

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AcademiaDbContext>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapStudentEndpoints();
app.MapEnrollmentEndpoints();
app.MapHealthChecks("/health");

app.Run();

// For testing
public partial class Program { }
```

### Middleware Pattern

```csharp
// src/shared/Middleware/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Zeus.Academia.Shared.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred: {Message}",
            exception.Message);

        var (statusCode, problem) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = validationEx.Message,
                    Extensions = new Dictionary<string, object?>
                    {
                        ["errors"] = validationEx.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                    }
                }),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message
                }),
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred"
                })
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
```

## Dependency Injection

### Service Registration

```csharp
// Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zeus.Academia.Infrastructure.Persistence;

namespace Zeus.Academia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AcademiaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AcademiaDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IStorageService, AzureBlobStorageService>();

        return services;
    }
}
```

## Integration

- **MediatR**: All business logic through commands/queries
- **FluentValidation**: Automatic validation via MediatR pipeline
- **Entity Framework Core**: Data persistence
- **Azure AD B2C**: Authentication
- **Azure Services**: Storage, monitoring, deployment

## Validation Checklist

- [ ] All I/O operations are async
- [ ] Nullable reference types enabled
- [ ] Dependency injection via constructor
- [ ] Proper HTTP status codes returned
- [ ] Logging at appropriate levels
- [ ] CancellationToken passed to async methods
- [ ] Exception handling middleware configured
- [ ] Authorization attributes on protected endpoints
- [ ] XML documentation on public APIs
- [ ] Health checks configured

## Anti-Patterns

❌ Synchronous I/O (`File.ReadAllText`, `HttpClient` without await)
✅ Async I/O (`File.ReadAllTextAsync`, `await httpClient.GetAsync`)

❌ `Task.Result` or `Task.Wait()`
✅ `await` for all async operations

❌ Service locator pattern
✅ Constructor dependency injection

❌ Business logic in controllers
✅ Thin controllers, logic in handlers via MediatR

❌ Catching generic `Exception` without re-throwing
✅ Specific exception handling or middleware

❌ Returning `null` for not found
✅ Return `NotFound()` or appropriate result

❌ Magic strings for configuration
✅ Strongly-typed configuration classes

❌ Nullable warnings suppressed (`!` operator overuse)
✅ Proper null checking or non-nullable design

❌ Feature endpoints or DbContexts assumed to be wired by convention
✅ Host code explicitly maps each feature route group and applies each feature migration set
