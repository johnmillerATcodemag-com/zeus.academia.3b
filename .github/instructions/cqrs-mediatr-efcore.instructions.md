---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-01-27-cqrs-csharp-mediatr"
prompt: |
  Generate CQRS instructions for C# with MediatR and EF Core
started: "2026-01-27T00:15:00Z"
ended: "2026-01-27T00:35:00Z"
task_durations:
  - task: "analyze requirements"
    duration: "00:02:00"
  - task: "draft instruction sections"
    duration: "00:12:00"
  - task: "add examples"
    duration: "00:04:00"
  - task: "optimize tokens"
    duration: "00:02:00"
total_duration: "00:20:00"
ai_log: "ai-logs/2026/01/27/2026-01-27-cqrs-csharp-mediatr/conversation.md"
source: ".github/prompts/generate-cqrs-instructions.prompt.md"
description: "CQRS implementation rules for C# with MediatR and EF Core"
applyTo: "src/**/*.cs"
---

# CQRS Architecture - C# + MediatR + EF Core

Implementation rules for Command Query Responsibility Segregation pattern using MediatR library with Entity Framework Core persistence.

## Command Pattern

### Structure

| Component | Purpose                         | Location                                         |
| --------- | ------------------------------- | ------------------------------------------------ |
| Command   | Immutable DTO with intent       | `Commands/<Aggregate>/<Name>Command.cs`          |
| Handler   | Validates → executes → persists | `Commands/<Aggregate>/<Name>CommandHandler.cs`   |
| Validator | FluentValidation rules          | `Commands/<Aggregate>/<Name>CommandValidator.cs` |
| Result    | Success/failure outcome         | Return `Unit`, `Result<T>`, or custom type       |

### Rules

- One command = one handler implementing `IRequestHandler<TCommand, TResponse>`
- Command names: `{Verb}{Noun}Command` (e.g., `CreateOrderCommand`, `UpdateInventoryCommand`)
- Commands are `record` types (immutable by default)
- No return of domain data; only confirmation/error
- Validate via pipeline behavior or explicit validator call
- Publish `IDomainEvent` after `SaveChangesAsync()`
- Persistence-bearing entities and DbContext mappings must define a real primary key before any migration is applied or startup is treated as success.
- Model validation must fail explicitly when a required key is missing; placeholder or stub persistence models are not acceptable in committed code.
- SQL Server/LocalDB availability checks must be explicit and environment-aware; LocalDB fallback is allowed only on confirmed Windows setups, while non-Windows hosts require a concrete connection string.

### Template

```csharp
// Command
public record CreateOrderCommand(Guid CustomerId, List<OrderItemDto> Items) : IRequest<Result<Guid>>;

// Handler
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateOrderCommand> _validator;

    public async Task<Result<Guid>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // 1. Validate
        var validationResult = await _validator.ValidateAsync(cmd, ct);
        if (!validationResult.IsValid)
            return Result<Guid>.Failure(validationResult.Errors);

        // 2. Execute domain logic
        var order = Order.Create(cmd.CustomerId, cmd.Items);

        // 3. Persist
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);

        // 4. Publish events (if using domain events)
        // await _mediator.DispatchDomainEventsAsync(order, ct);

        return Result<Guid>.Success(order.Id);
    }
}

// Validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().Must(HaveValidItems);
    }
}
```

## Query Pattern

### Structure

| Component | Purpose                  | Location                                  |
| --------- | ------------------------ | ----------------------------------------- |
| Query     | Criteria for retrieval   | `Queries/<Context>/<Name>Query.cs`        |
| Handler   | Reads from DB/read model | `Queries/<Context>/<Name>QueryHandler.cs` |
| DTO       | Response data shape      | `Models/Queries/<Name>Dto.cs`             |

### Rules

- One query = one handler implementing `IRequestHandler<TQuery, TResponse>`
- Query names: `Get{Entity}Query`, `List{Entities}Query` (e.g., `GetOrderByIdQuery`, `ListActiveUsersQuery`)
- NEVER modify state (no `SaveChanges`)
- Use `.AsNoTracking()` for read-only queries
- Return DTOs, not domain entities
- Return `null` or empty collection for not-found

### Template

```csharp
// Query
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

// Handler
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly AppDbContext _context;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Total = o.Total,
                Status = o.Status.ToString()
            })
            .FirstOrDefaultAsync(ct);
    }
}

// DTO
public record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
}
```

## Project Structure

```
src/
├── Application/
│   ├── Commands/
│   │   ├── Orders/
│   │   │   ├── CreateOrder/
│   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   │   └── CreateOrderCommandValidator.cs
│   │   │   └── UpdateOrder/
│   │   │       ├── UpdateOrderCommand.cs
│   │   │       └── UpdateOrderCommandHandler.cs
│   │   └── Customers/
│   ├── Queries/
│   │   ├── Orders/
│   │   │   ├── GetOrderById/
│   │   │   │   ├── GetOrderByIdQuery.cs
│   │   │   │   └── GetOrderByIdQueryHandler.cs
│   │   │   └── ListOrders/
│   │   │       ├── ListOrdersQuery.cs
│   │   │       └── ListOrdersQueryHandler.cs
│   │   └── Customers/
│   ├── Models/
│   │   ├── Commands/          # Write models (if needed)
│   │   └── Queries/            # DTOs
│   │       ├── OrderDto.cs
│   │       └── CustomerDto.cs
│   ├── Behaviors/              # MediatR pipeline behaviors
│   │   ├── ValidationBehavior.cs
│   │   └── LoggingBehavior.cs
│   └── Common/
│       └── Result.cs           # Result pattern implementation
├── Domain/
│   ├── Entities/
│   └── Events/                 # Domain events
└── Infrastructure/
    └── Persistence/
        └── AppDbContext.cs
```

## Validation Rules

### Command Validation

- Use FluentValidation with `AbstractValidator<TCommand>`
- Register validators: `services.AddValidatorsFromAssembly(assembly)`
- Implement `ValidationBehavior<TRequest, TResponse>` in pipeline
- Fail-fast: Return errors before handler execution
- Include business rule validation in domain layer

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Query Validation

- Validate input criteria (IDs, filters, pagination)
- Return `null` for single-item not-found
- Return empty collection for list not-found
- Use simple property checks or FluentValidation

## EF Core Integration

### Unit of Work

- DbContext acts as unit of work
- Command handlers: Inject `AppDbContext`, call `SaveChangesAsync()`
- Query handlers: Use `AsNoTracking()` for performance
- Transaction per command (implicit in `SaveChangesAsync`)

### Repository Pattern (Optional)

- Not required with EF Core (DbContext is repository+UoW)
- If used: Thin abstraction over `DbSet<T>`
- Keep repositories in Infrastructure layer

```csharp
// Direct DbContext usage (recommended)
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly AppDbContext _context;

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        return order.Id;
    }
}
```

### Change Tracking

- Commands: Default tracking enabled
- Queries: Use `.AsNoTracking()`
- Updates: Load entity, modify properties, `SaveChangesAsync()`

### Key and Index Rules

- Treat the primary key as the canonical uniqueness constraint for its key columns.
- Do not create an additional unique index on the exact same column set as a primary key.
- Add unique indexes only for alternate keys or query patterns that differ from the primary key.
- Model tests must verify the intended primary key shape directly, not infer it from a redundant unique index.

## Cross-Cutting Concerns

### Transaction Boundaries

- Each command handler = one transaction
- Explicit transactions for multi-operation commands:

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync(ct);
try
{
    // Multiple operations
    await _context.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

### Error Handling

- Commands: Return `Result<T>` or throw domain exceptions
- Queries: Return `null`/empty or use `Result<T>`
- Global exception handler for API layer

```csharp
public class Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T Value =>
        IsSuccess
            ? _value ?? throw new InvalidOperationException("Successful result must include a value.")
            : throw new InvalidOperationException("Cannot access Value when result is a failure.");
    public string Error { get; }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(true, value, string.Empty);
    }

    public static Result<T> Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new(false, default, error);
    }
}
```

### Logging

Use `LoggingBehavior<TRequest, TResponse>` in pipeline:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

## MediatR Registration

```csharp
// Program.cs or Startup.cs
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

## Anti-Patterns

| ❌ DON'T                                       | ✅ DO                                                       |
| ---------------------------------------------- | ----------------------------------------------------------- |
| Return domain entities from queries            | Return DTOs mapped from entities                            |
| Modify state in query handlers                 | Use `.AsNoTracking()` and read-only logic                   |
| Return business data from commands             | Return only operation result (ID, success)                  |
| Share models between commands/queries          | Separate write models and read DTOs                         |
| Cross-aggregate transactions in single command | Use domain events + eventual consistency                    |
| Inject `IMediator` into domain entities        | Keep domain pure, use from handlers                         |
| Add unique index duplicating a primary key     | Keep PK as canonical uniqueness and test key shape directly |

## Testing

### Command Tests

```csharp
[Fact]
public async Task Handle_ValidCommand_CreatesOrder()
{
    // Arrange
    var context = CreateInMemoryContext();
    var handler = new CreateOrderCommandHandler(context);
    var command = new CreateOrderCommand(Guid.NewGuid(), new List<OrderItemDto>());

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Single(context.Orders);
}
```

### Query Tests

```csharp
[Fact]
public async Task Handle_ExistingOrder_ReturnsDto()
{
    // Arrange
    var context = CreateInMemoryContext();
    var order = Order.Create(Guid.NewGuid());
    context.Orders.Add(order);
    await context.SaveChangesAsync();

    var handler = new GetOrderByIdQueryHandler(context);
    var query = new GetOrderByIdQuery(order.Id);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(order.Id, result.Id);
}
```

### Integration Tests

```csharp
[Fact]
public async Task CommandToQuery_FullFlow_WorksEndToEnd()
{
    // Arrange: Create via command
    var createCmd = new CreateOrderCommand(customerId, items);
    var orderId = await _mediator.Send(createCmd);

    // Act: Retrieve via query
    var query = new GetOrderByIdQuery(orderId);
    var dto = await _mediator.Send(query);

    // Assert
    Assert.NotNull(dto);
    Assert.Equal(customerId, dto.CustomerId);
}
```

## Success Criteria

- [x] Command/query boundaries clearly defined
- [x] Naming conventions specified (`{Verb}{Noun}Command`, `Get/List{Entity}Query`)
- [x] Validation requirements included (FluentValidation + pipeline)
- [x] Structure templates with MediatR interfaces
- [x] Error handling documented (`Result<T>` pattern)
- [x] C#/MediatR-specific examples provided
- [x] EF Core integration patterns shown
- [x] Testing approach defined (unit + integration)
- [x] Anti-patterns listed
- [x] Token-optimized (tables, bullets, concise code)
