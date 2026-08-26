---
ai_generated: true
model: "anthropic/claude-haiku-4.5@2024-10-22"
operator: "johnmillerATcodemag-com"
chat_id: "2026-08-24-university-identity-reconciliation"
prompt: |
  Create handoff notes for EP-1-3 (ManageUniversities implementation)
  with explicit requirements for UniversityRecord and GetUniversityByCodeQuery
started: "2026-08-24T19:30:00Z"
ended: "2026-08-24T19:45:00Z"
task_durations:
  - task: "extract EP-1-3 requirements from contract"
    duration: "00:05:00"
  - task: "document UniversityRecord structure and constraints"
    duration: "00:05:00"
  - task: "document GetUniversityByCodeQuery handler requirements"
    duration: "00:03:00"
  - task: "document invariants and verification gates"
    duration: "00:02:00"
total_duration: "00:15:00"
ai_log: "ai-logs/2026/08/24/2026-08-24-university-identity-reconciliation/conversation.md"
source: "Phase 0 Step 6 Reconciliation — University Identity"
---

# EP-1-3: ManageUniversities Implementation — Handoff Notes

**Status**: ✅ APPROVED for Phase 1 Execution Planning
**Target**: Implement in EP-1-3 (Reference Data — ManageUniversities)
**Dependency**: Phase 0 Step 6 (University Identity Reconciliation) — COMPLETE
**Predecessor**: Application Host (Phase 0 Step 1) and Shared Kernel (Phase 0 Steps 2-4)

---

## Overview

You are implementing the ManageUniversities feature, which:

1. Owns the `Universities` catalog table in the database
2. Provides `UniversityRecord` entity with Code as the primary identifier
3. Implements `GetUniversityByCodeQuery` handler for downstream resolution
4. Seeds initial universities and provides basic CRUD operations

**Critical Constraint**: You do NOT change Shared Kernel domain types or behavior. Only catalog data access and the resolution query.

---

## Scope: What You Implement

### 1. UniversityRecord Entity

**Location**: `src/features/ReferenceData/ManageUniversities/Shared/UniversityRecord.cs`

**Definition**:

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

/// <summary>
/// Represents an entry in the institutions catalog.
/// Owned by ManageUniversitiesDbContext; migrations managed by ManageUniversities.
/// </summary>
public class UniversityRecord
{
    /// <summary>
    /// The institutional code — e.g., "BOSTON_U", "MIT", "STANFORD".
    /// PRIMARY KEY: One code maps to exactly one catalog entry.
    /// Maps directly to University.Code in Shared Kernel domain model.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// The full, legal name of the institution — e.g., "Boston University".
    /// Not the primary identifier (Code is).
    /// Can change if the institution rebrands; historical qualifications retain the original code.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Flag indicating whether this institution is available for new qualifications.
    /// When false, new qualifications from this institution are rejected.
    /// Historical qualifications remain valid and queryable.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Factory method for creating a UniversityRecord.
    /// Enforced invariants: Code and Name are required, non-empty.
    /// All normalization and validation happens here.
    /// </summary>
    public static UniversityRecord Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("University code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("University name is required.", nameof(name));

        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedName = name.Trim();

        if (normalizedCode.Length > SharedKernelFieldLengths.UniversityCode)
            throw new ArgumentException(
                $"University code cannot exceed {SharedKernelFieldLengths.UniversityCode} characters.",
                nameof(code));

        if (normalizedName.Length > 256)  // Or appropriate max length for institution name
            throw new ArgumentException(
                "University name cannot exceed 256 characters.",
                nameof(name));

        return new UniversityRecord
        {
            Code = normalizedCode,
            Name = normalizedName,
            IsActive = true
        };
    }

    /// <summary>
    /// Deactivates this university without deletion.
    /// Preserves historical data; prevents new qualifications from this institution.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates this university.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
    }
}
```

**Invariants to Enforce**:

- [ ] `Code` is required; cannot be null, empty, or whitespace
- [ ] `Code` is normalized to uppercase
- [ ] `Code` does not exceed `SharedKernelFieldLengths.UniversityCode` characters
- [ ] `Name` is required; cannot be null, empty, or whitespace
- [ ] `Name` is trimmed of leading/trailing whitespace
- [ ] `Name` does not exceed 256 characters (adjust as needed)
- [ ] Private setter on all properties (factory method gates creation)
- [ ] `IsActive` defaults to `true` on creation

---

### 2. ManageUniversitiesDbContext

**Location**: `src/features/ReferenceData/ManageUniversities/Shared/ManageUniversitiesDbContext.cs`

**Requirements**:

```csharp
public class ManageUniversitiesDbContext : DbContext
{
    public ManageUniversitiesDbContext(DbContextOptions<ManageUniversitiesDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Universities table - maps UniversityRecord entities.
    /// Configuration applied from UniversityRecordConfiguration.
    /// </summary>
    public DbSet<UniversityRecord> Universities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageUniversitiesDbContext).Assembly);
    }
}
```

**Key Points**:

- ✅ One feature context per feature (isolated persistence)
- ✅ Apply configurations via `ApplyConfigurationsFromAssembly`
- ✅ Do NOT configure any Shared Kernel entities (they own their own mappings)

---

### 3. UniversityRecordConfiguration (EF Core Mapping)

**Location**: `src/features/ReferenceData/ManageUniversities/Shared/Configurations/UniversityRecordConfiguration.cs`

**Requirements**:

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core configuration for UniversityRecord.
/// Defines table mapping, constraints, indexes, and relationships.
/// </summary>
public class UniversityRecordConfiguration : IEntityTypeConfiguration<UniversityRecord>
{
    public void Configure(EntityTypeBuilder<UniversityRecord> builder)
    {
        // Table name and schema
        builder.ToTable("Universities", schema: "dbo");

        // Primary key
        builder.HasKey(u => u.Code)
            .HasName("PK_Universities");

        // Property mappings
        builder.Property(u => u.Code)
            .HasColumnName("Code")
            .HasColumnType("NVARCHAR(20)")  // Adjust per SharedKernelFieldLengths.UniversityCode
            .IsRequired();

        builder.Property(u => u.Name)
            .HasColumnName("Name")
            .HasColumnType("NVARCHAR(256)")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .HasColumnType("BIT")
            .HasDefaultValue(true)
            .IsRequired();

        // Uniqueness constraints
        // Note: Code is already the PK (unique by definition)
        // Name is NOT unique (multiple institutions can have similar names)

        // Indexes
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Universities_IsActive")
            .IsUnique(false);  // Non-unique; supports filtering active universities

        // Check constraints (database-level validation)
        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Universities_CodeNotEmpty",
                $"[Code] <> N''"));

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Universities_NameNotEmpty",
                $"[Name] <> N''"));
    }
}
```

**Mapping Decisions**:

| Property    | Type     | Length | Constraints                             |
| ----------- | -------- | ------ | --------------------------------------- |
| `Code` (PK) | NVARCHAR | 20     | NOT NULL; Unique (PK); Check: not empty |
| `Name`      | NVARCHAR | 256    | NOT NULL; Check: not empty              |
| `IsActive`  | BIT      | —      | NOT NULL; Default=1 (true)              |

**Key Points**:

- ✅ Code is PRIMARY KEY (unique constraint)
- ✅ Name is NOT unique (informational only)
- ✅ IsActive index supports filtering
- ✅ Check constraints enforce non-empty strings
- ✅ All properties are NOT NULL

---

### 4. GetUniversityByCodeQuery & Handler

**Location**: `src/features/ReferenceData/ManageUniversities/GetUniversityByCode/GetUniversityByCodeQuery.cs`

**Definition**:

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

using MediatR;

/// <summary>
/// Query to resolve a university catalog entry by code.
/// Called by downstream slices to fetch catalog data before creating domain value objects.
/// Designed for downstream resolution: input code → validate in catalog → return response.
/// </summary>
public sealed record GetUniversityByCodeQuery(string Code) : IRequest<GetUniversityByCodeResponse>;

/// <summary>
/// Response from GetUniversityByCodeQuery.
/// Always returns a response object (never throws exceptions for "not found").
/// Allows callers to distinguish "not found" from "inactive".
/// </summary>
public sealed record GetUniversityByCodeResponse(
    /// <summary>True if the code exists in the catalog.</summary>
    bool IsFound,

    /// <summary>The institutional code (if found; otherwise null).</summary>
    string? Code,

    /// <summary>The institutional name (if found; otherwise null).</summary>
    string? Name,

    /// <summary>Whether this institution is available for new qualifications. False if not found.</summary>
    bool IsActive
);
```

**Handler Implementation**:

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

using MediatR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handler for GetUniversityByCodeQuery.
/// Queries the catalog and returns the university record (if found).
/// Never throws exceptions; all outcomes are communicated via response object.
/// </summary>
public sealed class GetUniversityByCodeHandler : IRequestHandler<GetUniversityByCodeQuery, GetUniversityByCodeResponse>
{
    private readonly ManageUniversitiesDbContext _dbContext;

    public GetUniversityByCodeHandler(ManageUniversitiesDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<GetUniversityByCodeResponse> Handle(
        GetUniversityByCodeQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Query database by Code (case-insensitive, since codes are normalized)
        string normalizedCode;

        try
        {
            normalizedCode = University.Create(request.Code).Code;
        }
        catch (ArgumentException)
        {
            return new GetUniversityByCodeResponse(
                IsFound: false,
                Code: null,
                Name: null,
                IsActive: false);
        }

        var record = await _dbContext.Universities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Code == normalizedCode,
                cancellationToken);

        // If not found, return IsFound=false with null data
        if (record is null)
        {
            return new GetUniversityByCodeResponse(
                IsFound: false,
                Code: null,
                Name: null,
                IsActive: false);
        }

        // If found, return all details (including IsActive flag)
        return new GetUniversityByCodeResponse(
            IsFound: true,
            Code: record.Code,
            Name: record.Name,
            IsActive: record.IsActive);
    }
}
```

**Handler Requirements**:

- [ ] Accept `string Code` (nullable or empty is OK; handle gracefully)
- [ ] Normalize input code to uppercase before query
- [ ] Query `ManageUniversitiesDbContext.Universities`
- [ ] Use `SingleOrDefaultAsync` to find matching record
- [ ] Return response object (never throw for "not found")
- [ ] Return `IsFound=true` with data if record exists
- [ ] Return `IsFound=false` with nulls if record does not exist
- [ ] Include `IsActive` flag in response regardless of `IsFound`
- [ ] Use `AsNoTracking()` (no tracking needed for queries)

---

### 5. Initial Seeding

**Location**: `src/features/ReferenceData/ManageUniversities/Shared/Seeding/UniversitySeeder.cs`

**Seeded Universities** (minimum set for testing):

```csharp
public static class UniversitySeeder
{
    public static void Seed(ManageUniversitiesDbContext context)
    {
        if (context.Universities.Any())
            return;  // Already seeded

        var universities = new[]
        {
            UniversityRecord.Create("BOSTON_U", "Boston University"),
            UniversityRecord.Create("MIT", "Massachusetts Institute of Technology"),
            UniversityRecord.Create("STANFORD", "Stanford University"),
            UniversityRecord.Create("HARVARD", "Harvard University"),
            UniversityRecord.Create("YALE", "Yale University"),
            UniversityRecord.Create("PRINCETON", "Princeton University"),
        };

        foreach (var university in universities)
        {
            context.Universities.Add(university);
        }

        context.SaveChanges();
    }
}
```

**Seeding Requirements**:

- [ ] All seeded universities have `IsActive=true` on startup
- [ ] Codes are normalized to uppercase
- [ ] Names are trimmed and descriptive
- [ ] Seeding is idempotent (check if data already exists)
- [ ] Seeding happens during application startup or migration

---

### 6. Migration Artifacts

**Ownership**: ManageUniversities owns all Universities table migrations.

**Migration Files to Create**:

1. **`Migrations/[Timestamp]_CreateUniversitiesTable.cs`**
   - Create `Universities` table
   - Define PK on `Code`
   - Define check constraints
   - Define indexes
   - Seed initial universities

2. **`Migrations/[Timestamp]_CreateUniversitiesTable.Designer.cs`**
   - Auto-generated; contains metadata

3. **`Migrations/[Timestamp]_CreateUniversitiesTable.sql`** (optional; for documentation)
   - SQL Server output showing exact schema

4. **`Migrations/ManageUniversitiesContextModelSnapshot.cs`**
   - EF Core model snapshot (auto-generated)

**Key Points**:

- ✅ Each migration class owns exactly one logical change
- ✅ Migration Designer files stay with the migration class
- ✅ Model snapshot is generated by `dotnet ef migrations add`
- ✅ Never manually edit Designer or snapshot files
- ✅ Seed data can go in migration `Up()` method or in a seeder

---

### 7. Feature Integration

**MediatR Registration** (in Application Host):

The host (Phase 0 Step 1) should register:

```csharp
// In Program.cs or Startup.cs
var assembly = typeof(GetUniversityByCodeQuery).Assembly;
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
});
```

**Dependency Injection**:

```csharp
// ManageUniversitiesDbContext must be registered
services.AddDbContext<ManageUniversitiesDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

---

## Scope: What You Do NOT Implement

❌ **Do NOT**:

- Modify `University` value object (Shared Kernel owns it)
- Modify `AcademicQualification` (Shared Kernel owns it)
- Create endpoints or controllers (no HTTP handlers in EP-1-3)
- Implement business logic for creating/updating qualifications (downstream slices own that)
- Change any Shared Kernel domain behavior

✅ **DO**:

- Implement `UniversityRecord` entity
- Implement `GetUniversityByCodeQuery` & handler
- Implement EF Core configuration
- Seed initial universities
- Create migrations
- Write tests for queries and constraints

---

## Testing Requirements

### Unit Tests: UniversityRecord

**File**: `tests/Features/ReferenceData/ManageUniversities/UniversityRecordTests.cs`

```csharp
public class UniversityRecordTests
{
    [Fact]
    public void Create_WithValidCode_Succeeds()
    {
        // Arrange & Act
        var record = UniversityRecord.Create("BOSTON_U", "Boston University");

        // Assert
        Assert.NotNull(record);
        Assert.Equal("BOSTON_U", record.Code);
        Assert.Equal("Boston University", record.Name);
        Assert.True(record.IsActive);
    }

    [Fact]
    public void Create_WithNullCode_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            UniversityRecord.Create(null!, "Boston University"));
    }

    [Fact]
    public void Create_WithEmptyCode_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            UniversityRecord.Create("   ", "Boston University"));
    }

    [Fact]
    public void Create_NormalizesCodeToUppercase()
    {
        // Arrange & Act
        var record = UniversityRecord.Create("boston_u", "Boston University");

        // Assert
        Assert.Equal("BOSTON_U", record.Code);
    }

    [Fact]
    public void Create_WithNameExceedingLength_Throws()
    {
        // Act & Assert
        var longName = new string('X', 257);
        Assert.Throws<ArgumentException>(() =>
            UniversityRecord.Create("BOSTON_U", longName));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var record = UniversityRecord.Create("BOSTON_U", "Boston University");

        // Act
        record.Deactivate();

        // Assert
        Assert.False(record.IsActive);
    }

    [Fact]
    public void Reactivate_SetsIsActiveTrue()
    {
        // Arrange
        var record = UniversityRecord.Create("BOSTON_U", "Boston University");
        record.Deactivate();

        // Act
        record.Reactivate();

        // Assert
        Assert.True(record.IsActive);
    }
}
```

### Integration Tests: GetUniversityByCodeQuery

**File**: `tests/Features/ReferenceData/ManageUniversities/GetUniversityByCodeQueryTests.cs`

```csharp
public class GetUniversityByCodeQueryTests : IAsyncLifetime
{
    private ManageUniversitiesDbContext _context = null!;
    private GetUniversityByCodeQueryHandler _handler = null!;

    public async Task InitializeAsync()
    {
        // Set up in-memory database or test database
        var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ManageUniversitiesDbContext(options);
        _handler = new GetUniversityByCodeQueryHandler(_context);

        // Seed test data
        _context.Universities.Add(UniversityRecord.Create("BOSTON_U", "Boston University"));
        _context.Universities.Add(UniversityRecord.Create("MIT", "MIT"));

        var inactive = UniversityRecord.Create("CLOSED_U", "Closed University");
        inactive.Deactivate();
        _context.Universities.Add(inactive);

        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WithExistingCode_ReturnsFound()
    {
        // Arrange
        var query = new GetUniversityByCodeQuery("BOSTON_U");

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsFound);
        Assert.Equal("BOSTON_U", response.Code);
        Assert.Equal("Boston University", response.Name);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task Handle_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        var query = new GetUniversityByCodeQuery("NONEXISTENT");

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(response.IsFound);
        Assert.Null(response.Code);
        Assert.Null(response.Name);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task Handle_WithInactiveCode_ReturnsIsActiveFalse()
    {
        // Arrange
        var query = new GetUniversityByCodeQuery("CLOSED_U");

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsFound);
        Assert.Equal("CLOSED_U", response.Code);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task Handle_NormalizesInputCodeToUppercase()
    {
        // Arrange
        var query = new GetUniversityByCodeQuery("boston_u");

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(response.IsFound);
        Assert.Equal("BOSTON_U", response.Code);
    }

    [Fact]
    public async Task Handle_WithNullCode_HandlesGracefully()
    {
        // Arrange
        var query = new GetUniversityByCodeQuery(null!);

        // Act
        var response = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(response.IsFound);  // Should return "not found", not throw
    }
}
```

### Database Constraint Tests

**File**: `tests/Features/ReferenceData/ManageUniversities/UniversityRecordConstraintTests.cs`

```csharp
public class UniversityRecordConstraintTests
{
    [Fact]
    public async Task UniqueConstraintOnCode_PreventsInsertingDuplicate()
    {
        // Arrange: Create context and insert a record
        using var context = CreateTestContext();
        var record1 = UniversityRecord.Create("BOSTON_U", "Boston University");
        context.Universities.Add(record1);
        await context.SaveChangesAsync();

        // Act: Try to insert duplicate code
        var record2 = UniversityRecord.Create("BOSTON_U", "Another Name");
        context.Universities.Add(record2);

        // Assert: Throws SQL constraint violation
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await context.SaveChangesAsync());

        Assert.Contains("PK_Universities", ex.InnerException?.Message ?? "");
    }

    [Fact]
    public async Task CheckConstraint_PreventsEmptyCode()
    {
        // This may require raw SQL to bypass domain validation
        // Demonstrates that database also enforces the rule
        using var context = CreateTestContext();

        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            // Attempt direct SQL insert (bypassing UniversityRecord.Create)
            await context.Database.ExecuteSqlAsync(
                "INSERT INTO Universities (Code, Name, IsActive) VALUES ('', 'Test', 1)");
        });

        Assert.NotNull(ex);
    }
}
```

---

## Verification Gates (Before Handoff to EP-1-3)

- [ ] UniversityRecord.Create factory enforces all invariants
- [ ] GetUniversityByCodeQuery handler never throws for "not found"
- [ ] Response includes IsFound and IsActive flags separately
- [ ] Uniqueness constraint prevents duplicate codes
- [ ] IsActive toggle works without deletion
- [ ] Code is normalized to uppercase
- [ ] Name can change without affecting historical references
- [ ] Seeded universities are active on startup
- [ ] Database schema matches EF Core model
- [ ] All unit and integration tests pass
- [ ] Migration artifacts are generated and committed

---

## Downstream Integration (For Reference)

### RegisterAcademic Will Use This Pattern:

```csharp
public class RecordQualificationCommandHandler : IRequestHandler<RecordQualificationCommand, Result>
{
    private readonly IMediator _mediator;

    public async Task<Result> Handle(RecordQualificationCommand cmd, CancellationToken ct)
    {
        // Your query — defined and implemented here in EP-1-3
        var universityQuery = new GetUniversityByCodeQuery(cmd.UniversityCode);
        var universityDto = await _mediator.Send(universityQuery, ct);

        // Validation (done by downstream slice)
        if (!universityDto.IsFound)
            return Error.Create("UniversityNotFound", ..);
        if (!universityDto.IsActive)
            return Error.Create("UniversityNotActive", ..);

        // Creation of domain value object (done by downstream slice)
        var university = University.Create(universityDto.Code);

        // Usage in aggregate (done by downstream slice)
        var qualification = AcademicQualification.Create(
            degree.Code,
            university.Code,  // ← Uses the Code
            obtainedDate
        );
        // ...
    }
}
```

---

## References

- [University Resolution Contract](./UNIVERSITY_RESOLUTION_CONTRACT.md)
- [Shared Kernel University Value Object](../../SharedKernel/Foundation/Domain/University.cs)
- [Shared Kernel Persistence Boundaries](../../SharedKernel/PERSISTENCE_BOUNDARIES.md)
- [Academia Refactoring Plan — Phase 0 Step 5](../../../models/workflows/academia-refactoring-plan.md)
