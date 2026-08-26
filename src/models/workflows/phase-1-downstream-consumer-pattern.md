---
ai_generated: false
operator: "slice-coordinator"
chat_id: "phase-0-step-6-downstream-documentation"
prompt: |
  Document the canonical downstream consumer pattern for all Phase 1+ slices.
  All downstream slices that consume reference data or domain primitives from Phase 0
  follow this pattern exactly.
started: "2026-08-24T17:00:00Z"
ended: "2026-08-24T17:30:00Z"
task_durations:
  - task: "establish canonical pattern"
    duration: "00:10:00"
  - task: "document handler pattern"
    duration: "00:08:00"
  - task: "provide code examples"
    duration: "00:08:00"
  - task: "document constraints and anti-patterns"
    duration: "00:04:00"
total_duration: "00:30:00"
ai_log: "ai-logs/2026/08/24/phase-0-step-6-downstream-documentation/conversation.md"
source: "Phase 0 Step 6 - Downstream Consumer Documentation"
description: "Canonical integration pattern for all Phase 1+ downstream slices"
---

# Phase 1+ Downstream Consumer Pattern

**Status**: ✅ FINALIZED (Phase 0 Step 6)
**Last Updated**: August 24, 2026
**Applies To**: RegisterAcademic, RecordQualification, AssignExtension, and all future slices

## Overview

This document defines the **single canonical pattern** for all downstream slices that consume reference data or domain primitives from Phase 0 (Shared Kernel, ManageRanks, ManageDegrees, ManageUniversities, ProvisionExtension).

Every downstream slice follows these patterns exactly. Deviations require architectural review and escalation.

## Architecture Pattern

```
Downstream Slice (e.g., RegisterAcademic)
├── Commands & Queries (MediatR contracts)
├── Handlers (consume catalogs via IMediator)
├── Domain Models (aggregates, use Shared Kernel types)
├── Persistence (feature-local DbContext, reuses Shared Kernel configs)
└── Tests (unit + integration)

↓ (depends on, via IMediator queries)

Phase 0 Foundation
├── Shared Kernel
│   ├── Rank (value object)
│   ├── Degree (value object)
│   ├── University (value object)
│   ├── Extension (entity)
│   ├── Academic (aggregate root)
│   ├── AcademicQualification (entity)
│   └── Configurations (reusable EF)
├── ManageRanks
│   ├── GetRankByCodeQuery
│   ├── GetRankByCodeResponse
│   └── RanksDbContext (owns Ranks table)
├── ManageDegrees
│   ├── GetDegreeByCodeQuery
│   ├── GetDegreeByCodeResponse
│   └── DegreesDbContext (owns Degrees table)
├── ManageUniversities
│   ├── GetUniversityByCodeQuery
│   ├── GetUniversityByCodeResponse
│   └── UniversitiesDbContext (owns Universities table)
└── ProvisionExtension
    ├── GetExtensionByEmpNrQuery
    ├── GetExtensionByEmpNrResponse
    └── ProvisionExtensionDbContext (owns Extensions table)
```

`GetUniversityByCodeQuery` is an implemented public MediatR contract owned by `ManageUniversities`. Downstream slices must resolve university codes through this contract and must not reference `ManageUniversitiesDbContext` or `UniversityRecord` directly.

## Canonical Integration Steps (All Downstream Slices)

### Step 1: Define Commands

Follow MediatR request/response pattern. Commands are **intent**, not implementation.

```csharp
namespace Zeus.Academia.Features.Qualification.RecordQualification;

/// <summary>
/// Record a new qualification for an academic.
/// </summary>
public record RecordQualificationCommand(
    int EmpNr,
    string DegreeCode,
    string UniversityCode,
    DateTime ObtainedDate
) : IRequest<Result>;
```

### Step 2: Resolve Catalogs via IMediator (NOT Direct DbContext)

When a handler needs reference data, query Phase 0 catalogs through IMediator. This decouples your feature from Phase 0 persistence details.

```csharp
namespace Zeus.Academia.Features.Qualification.RecordQualification;

public class RecordQualificationCommandHandler : IRequestHandler<RecordQualificationCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly QualificationDbContext _context;

    public RecordQualificationCommandHandler(
        IMediator mediator,
        QualificationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task<Result> Handle(RecordQualificationCommand cmd, CancellationToken ct)
    {
        // PATTERN: For each reference data needed, query the catalog

        // 1. Resolve degree from ManageDegrees catalog
        var degreeQuery = new GetDegreeByCodeQuery(cmd.DegreeCode);
        var degreeResponse = await _mediator.Send(degreeQuery, ct);
        if (!degreeResponse.IsFound)
            return Result.Failure(
                Error.Create("InvalidDegree", $"Degree {cmd.DegreeCode} not found in catalog"));

        // 2. Resolve university from ManageUniversities catalog
        var universityQuery = new GetUniversityByCodeQuery(cmd.UniversityCode);
        var universityResponse = await _mediator.Send(universityQuery, ct);
        if (!universityResponse.IsFound)
            return Result.Failure(
                Error.Create("InvalidUniversity", $"University {cmd.UniversityCode} not found in catalog"));

        // 3. Verify university is still active (accepting qualifications)
        if (!universityResponse.IsActive)
            return Result.Failure(
                Error.Create("UniversityNotActive",
                    $"University {cmd.UniversityCode} is no longer accepting qualifications"));

        // 4. Resolve rank if needed (same pattern)
        // (Omitted for brevity; follow steps 1-3)

        // 5. Create domain value objects from resolved catalog responses.
        // The catalog provides the CODE; use it rather than the display name.
        var degreeVo = Degree.Create(degreeResponse.Code!);  // Code is guaranteed non-null after validation
        var universityVo = University.Create(universityResponse.Code!);

        // 6. Load aggregate and apply domain logic
        var academic = await _context.Academics
            .FirstOrDefaultAsync(a => a.EmpNr == cmd.EmpNr, ct);

        if (academic is null)
            return Result.Failure(
                Error.Create("AcademicNotFound", $"Employee {cmd.EmpNr} not found"));

        // 7. Create qualification entity using resolved value objects
        var qualification = new AcademicQualification(
            degreeVo.Code,
            universityVo.Code,
            cmd.ObtainedDate
        );

        // 8. Add to aggregate and persist
        try
        {
            academic.AddQualification(qualification);
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
        {
            // PK/unique constraint violation
            return Result.Failure(
                Error.Create("DuplicateQualification",
                    $"Academic already holds this degree from this university"));
        }

        return Result.Success();
    }
}
```

### Step 3: Never Reference Catalogs Directly ❌

**ANTI-PATTERN**: Direct DbContext access to other feature's catalogs.

```csharp
// ❌ WRONG: This couples your feature to ManageDegrees persistence
public async Task Handle(RecordQualificationCommand cmd, CancellationToken ct)
{
    var degree = await _manageDegreesDbContext.Degrees
        .FirstOrDefaultAsync(d => d.Code == cmd.DegreeCode);

    // Problems:
    // - Tight coupling to ManageDegrees DbContext
    // - Breaks module boundaries
    // - If ManageDegrees changes, your code breaks
    // - Hard to test independently
    // - Violates CQRS pattern
}
```

**CORRECT**: Query via IMediator (loose coupling, testable, maintainable).

```csharp
// ✅ CORRECT: IMediator abstracts the catalog implementation
public async Task Handle(RecordQualificationCommand cmd, CancellationToken ct)
{
    var degreeQuery = new GetDegreeByCodeQuery(cmd.DegreeCode);
    var degreeResponse = await _mediator.Send(degreeQuery, ct);

    // Benefits:
    // - Loose coupling; catalog implementation can change
    // - Easy to mock in tests
    // - Follows CQRS pattern
    // - Catalog contract is explicit
}
```

### Step 4: Aggregate Owns Domain State

Aggregates store **codes** (from value objects), not catalog records. The aggregate is the authority for its own data.

```csharp
namespace Zeus.Academia.Features.SharedKernel;

/// <summary>
/// Academic aggregate root: manages employment state and qualifications.
/// </summary>
public class Academic
{
    public int EmpNr { get; private set; }  // Employee number (immutable, PK)

    private readonly List<AcademicQualification> _qualifications = [];
    public IReadOnlyList<AcademicQualification> Qualifications => _qualifications.AsReadOnly();

    /// <summary>
    /// Create a new academic with initial employment state.
    /// </summary>
    public static Academic Create(int empNr, string? rankCode = null, bool isTenured = false)
    {
        if (empNr <= 0)
            throw new ArgumentException("EmpNr must be positive", nameof(empNr));

        return new Academic { EmpNr = empNr };
    }

    /// <summary>
    /// Add a qualification (code-based, immutable once added).
    /// </summary>
    public void AddQualification(AcademicQualification qualification)
    {
        if (qualification is null)
            throw new ArgumentNullException(nameof(qualification));

        // Domain invariant: no duplicate degree+university per academic
        if (_qualifications.Any(q =>
            q.DegreeCode == qualification.DegreeCode &&
            q.UniversityCode == qualification.UniversityCode))
        {
            throw new InvalidOperationException(
                $"Academic {EmpNr} already holds degree {qualification.DegreeCode} " +
                $"from university {qualification.UniversityCode}");
        }

        _qualifications.Add(qualification);
    }

    // Private constructor (use factory methods)
    private Academic() { }
}

/// <summary>
/// Academic qualification: immutable record of degree+university.
/// Stores CODES, not references to catalog records.
/// </summary>
public class AcademicQualification
{
    public string DegreeCode { get; private set; }          // From Degree.Code
    public string UniversityCode { get; private set; }      // From University.Code
    public DateTime ObtainedDate { get; private set; }      // When the degree was obtained

    public AcademicQualification(string degreeCode, string universityCode, DateTime obtainedDate)
    {
        if (string.IsNullOrWhiteSpace(degreeCode))
            throw new ArgumentException("DegreeCode is required", nameof(degreeCode));

        if (string.IsNullOrWhiteSpace(universityCode))
            throw new ArgumentException("UniversityCode is required", nameof(universityCode));

        if (obtainedDate > DateTime.Today)
            throw new ArgumentException("ObtainedDate cannot be in the future", nameof(obtainedDate));

        DegreeCode = degreeCode;
        UniversityCode = universityCode;
        ObtainedDate = obtainedDate;
    }
}
```

### Step 5: Feature-Local DbContext

Create a feature-local DbContext that owns tables for your feature's aggregates. Reuse Shared Kernel configurations for domain types.

```csharp
namespace Zeus.Academia.Features.Qualification;

/// <summary>
/// Feature-local DbContext for qualification persistence.
/// Owns: Academics, AcademicQualifications tables.
/// Uses: Shared Kernel configurations and value objects.
/// </summary>
public class QualificationDbContext : DbContext
{
    public QualificationDbContext(DbContextOptions<QualificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Academic> Academics { get; set; } = default!;
    public DbSet<AcademicQualification> Qualifications { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PATTERN: Reuse Shared Kernel configurations (single source of truth)
        // Do NOT duplicate configuration logic
        modelBuilder.ApplyConfiguration(new AcademicConfiguration());
        modelBuilder.ApplyConfiguration(new AcademicQualificationConfiguration());
    }
}

/// <summary>
/// Service collection extension for feature-local persistence registration.
/// Follows the pattern established in ManageRanks, ManageDegrees, etc.
/// </summary>
public static class QualificationServiceCollectionExtensions
{
    public static IServiceCollection AddQualificationPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<QualificationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(QualificationDbContext).Assembly.GetName().Name)
            ));

        return services;
    }

    public static IServiceCollection AddQualificationMediatR(
        this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(QualificationDbContext).Assembly));

        return services;
    }
}
```

### Step 6: Feature-Specific Validation

Validation rules are **feature-specific** and live in your feature, not in Shared Kernel.

```csharp
namespace Zeus.Academia.Features.Qualification.RecordQualification;

public class RecordQualificationCommandValidator : AbstractValidator<RecordQualificationCommand>
{
    public RecordQualificationCommandValidator()
    {
        RuleFor(cmd => cmd.EmpNr)
            .GreaterThan(0)
            .WithMessage("Employee number must be positive");

        RuleFor(cmd => cmd.DegreeCode)
            .NotEmpty()
            .WithMessage("Degree code is required")
            .MaximumLength(10)
            .WithMessage("Degree code cannot exceed 10 characters");

        RuleFor(cmd => cmd.UniversityCode)
            .NotEmpty()
            .WithMessage("University code is required")
            .MaximumLength(50)
            .WithMessage("University code cannot exceed 50 characters");

        RuleFor(cmd => cmd.ObtainedDate)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Qualification date cannot be in the future");
    }
}
```

### Step 7: Register in Application Host

Add your feature to the Application Host DI composition. Follow the established pattern.

```csharp
// In Program.cs (src/Zeus.Academia.Api)

var builder = WebApplication.CreateBuilder(args);

// Phase 0: Core infrastructure
builder.Services.AddSharedKernel();  // (already registered)

// Phase 0: Reference data features
builder.Services.AddManageRanksPersistence(builder.Configuration);
builder.Services.AddManageDegreesPersistence(builder.Configuration);
builder.Services.AddManageUniversitiesPersistence(builder.Configuration);
builder.Services.AddProvisionExtensionPersistence(builder.Configuration);

// Phase 1+: Domain features (follow same pattern)
builder.Services.AddQualificationPersistence(builder.Configuration);        // ← Your feature
builder.Services.AddQualificationMediatR();                                 // ← Your feature
builder.Services.AddRegisterAcademicPersistence(builder.Configuration);     // ← Another feature
builder.Services.AddRegisterAcademicMediatR();                              // ← Another feature

// ... other features ...

var app = builder.Build();
app.Run();
```

## Error Handling Pattern

All catalog queries return DTOs with nullable fields. Handlers must validate before using.

```csharp
/// <summary>
/// Response from catalog query (from ManageDegrees feature).
/// </summary>
public record GetDegreeByCodeResponse(
    bool IsFound,
    string? Code,           // Nullable until validated
    string? Description,
    bool IsActive          // Degree may be deprecated
);

/// <summary>
/// Handler validation pattern (in your feature).
/// </summary>
public async Task<Result> Handle(RecordQualificationCommand cmd, CancellationToken ct)
{
    var response = await _mediator.Send(new GetDegreeByCodeQuery(cmd.DegreeCode), ct);

    // Validation step 1: Degree exists
    if (!response.IsFound)
        return Result.Failure(Error.Create("DegreeNotFound", $"Degree {cmd.DegreeCode} not in catalog"));

    // Validation step 2: Code is guaranteed non-null after IsFound check
    var degreeVo = Degree.Create(response.Code!);  // Code! is safe here

    // Validation step 3: May have additional constraints (e.g., not deprecated)
    if (!response.IsActive)
        return Result.Failure(Error.Create("DegreeDeprecated", $"Degree {cmd.DegreeCode} is no longer active"));

    // ... continue with business logic ...
}
```

## Feature Isolation Constraints

### ✅ **Allowed**

- Query Phase 0 catalogs **via IMediator** (GetXxxByCodeQuery pattern)
- Import Shared Kernel **value objects** (Degree, University, Rank, Extension)
- Import Shared Kernel **entities** (Academic, AcademicQualification, Extension)
- Reuse Shared Kernel **EF configurations** (AcademicConfiguration, ExtensionConfiguration)
- Define feature-local **aggregates and entities** (your domain logic)
- Create feature-local **DbContext** (owns your tables, registers your handlers)
- Implement feature-specific **handlers and validators**
- Define feature-specific **commands, queries, responses** (MediatR contracts)
- Create feature-specific **endpoints or controllers** (route registration)
- Define feature-specific **domain exceptions** (inheriting from base types if needed)

### ❌ **Prohibited**

- Direct DbContext access to **other feature's catalogs** (breaks boundaries, breaks tests)
- Storing **catalog records** in your aggregates (store codes instead)
- Creating **new Shared Kernel types** without architecture review (SK is centralized)
- Host-level logic in **feature projects** (DI, configuration, migrations belong to host)
- Circular dependencies **between features** (F1 → F2 → F1 is invalid)
- Feature-specific persistence **in Shared Kernel** (SK is cross-cutting, not feature-specific)
- Duplicating **Shared Kernel configurations** (reuse, don't copy)
- Accessing private members of **Phase 0 persistence classes** (use public contracts)

## Testing Pattern

### Unit Tests: Domain Logic Only (No Persistence)

```csharp
namespace Zeus.Academia.Tests.Features.Qualification;

public class AcademicQualificationTests
{
    [Fact]
    public void Create_WithValidInputs_Succeeds()
    {
        var qual = new AcademicQualification("MAST", "BOSTON_U", DateTime.Now);

        Assert.Equal("MAST", qual.DegreeCode);
        Assert.Equal("BOSTON_U", qual.UniversityCode);
    }

    [Fact]
    public void Create_WithEmptyDegreeCode_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new AcademicQualification("", "BOSTON_U", DateTime.Now));

        Assert.Contains("DegreeCode is required", ex.Message);
    }

    [Fact]
    public void AddQualification_WithDuplicateDegreeUniversity_Throws()
    {
        var academic = Academic.Create(12345);
        var qual1 = new AcademicQualification("MAST", "BOSTON_U", DateTime.Now);
        var qual2 = new AcademicQualification("MAST", "BOSTON_U", DateTime.Now);

        academic.AddQualification(qual1);

        var ex = Assert.Throws<InvalidOperationException>(
            () => academic.AddQualification(qual2));

        Assert.Contains("already holds", ex.Message);
    }
}
```

### Integration Tests: Handler + Persistence + Mocked Catalogs

```csharp
namespace Zeus.Academia.Tests.Features.Qualification.Integration;

public class RecordQualificationCommandHandlerTests
{
    private readonly QualificationDbContext _context;
    private readonly IMediator _mediator;
    private readonly RecordQualificationCommandHandler _handler;

    public RecordQualificationCommandHandlerTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<QualificationDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;

        _context = new QualificationDbContext(options);

        // Mock IMediator to return test catalog responses
        _mediator = new Mock<IMediator>();
        _handler = new RecordQualificationCommandHandler(_mediator, _context);
    }

    [Fact]
    public async Task Handle_WithValidQualification_PersistsAndReturnsSuccess()
    {
        // Setup: Create academic in test database
        var academic = Academic.Create(12345);
        _context.Academics.Add(academic);
        await _context.SaveChangesAsync();

        // Setup: Mock catalog responses
        _mediator
            .Setup(m => m.Send(
                It.IsAny<GetDegreeByCodeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDegreeByCodeResponse(true, "MAST", "Master's", true));

        _mediator
            .Setup(m => m.Send(
                It.IsAny<GetUniversityByCodeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUniversityByCodeResponse(true, "BOSTON_U", "Boston University", true));

        // Act
        var cmd = new RecordQualificationCommand(12345, "MAST", "BOSTON_U", DateTime.Now);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = await _context.Academics.FirstAsync();
        Assert.Single(updated.Qualifications);
        Assert.Equal("MAST", updated.Qualifications[0].DegreeCode);
    }

    [Fact]
    public async Task Handle_WithInvalidDegree_ReturnsDegreeNotFoundError()
    {
        // Setup
        var academic = Academic.Create(12345);
        _context.Academics.Add(academic);
        await _context.SaveChangesAsync();

        _mediator
            .Setup(m => m.Send(
                It.IsAny<GetDegreeByCodeQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDegreeByCodeResponse(false, null, null, false));

        // Act
        var cmd = new RecordQualificationCommand(12345, "INVALID", "BOSTON_U", DateTime.Now);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("DegreeNotFound", result.Error.Code);
    }
}
```

## When to Escalate

Contact the Shared Kernel coordinator or architecture team if:

- **Catalog query doesn't return needed data**: The GetXxxByCodeQuery response is missing fields you need
  - ➜ Request enhancement to catalog query/response
  - ➜ Do NOT add workarounds or direct DbContext access

- **Reference data constraint isn't in the catalog**: You need a constraint enforced that the catalog doesn't check
  - ➜ Escalate to catalog feature owner
  - ➜ May require adding validation to GetXxxByCodeQuery

- **New shared value object needed**: Your domain logic needs a new primitive type
  - ➜ Escalate with business case to architecture team
  - ➜ Add to Shared Kernel if cross-feature use confirmed
  - ➜ Do NOT create local versions of shared types

- **Shared Kernel domain rule needs clarification**: Academic aggregate behavior, validation, or configuration is unclear
  - ➜ Request review with Shared Kernel owner
  - ➜ Document assumption temporarily in your handler
  - ➜ Follow up with formal clarification

- **Circular dependency between features**: You find yourself needing to query a feature that depends on you
  - ➜ STOP immediately; this is an architecture error
  - ➜ Escalate for refactoring
  - ➜ May require introducing intermediate feature or query contract

## Summary

All downstream slices follow this one pattern:

1. **Define Commands** → intent, MediatR contracts
2. **Query Catalogs via IMediator** → GetXxxByCodeQuery pattern
3. **Create Value Objects** → from resolved catalog codes
4. **Load Aggregates** → from feature-local DbContext
5. **Apply Domain Logic** → in handlers and aggregates
6. **Persist Changes** → to feature-local DbContext (migrations owned by feature)
7. **Register in Host** → AddXxxPersistence + AddXxxMediatR extensions
8. **Test Everything** → unit tests (domain), integration tests (handler + persistence)

Follow this pattern **exactly**. Deviations require architectural escalation.
