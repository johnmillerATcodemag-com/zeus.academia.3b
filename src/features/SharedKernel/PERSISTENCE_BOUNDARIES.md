# Shared Kernel Persistence Boundaries

## Overview

This document defines the scope, boundaries, constraints, and patterns for Shared Kernel persistence. It serves as the reference for all features that depend on Shared Kernel domain entities and configurations.

**Last Updated**: Phase 0 Step 5 - Reconciliation
**Status**: ✅ Finalized (No host startup code, configuration reuse verified)

## Ownership

### Owned Tables (Shared Kernel owns migrations)

- **Academics** — Academic aggregate root with employment state management
  - Primary key: EmpNr (normalized employee identifier)
  - Domain invariants: Mutual exclusion (tenure XOR contract), derived AccessLevel from Rank
  - No duplicates: Single unique index per domain entity

- **AcademicQualifications** — Academic qualification (composite relationship with Academic)
  - Composite primary key: (AcademicId, DegreeCode, UniversityCode)
  - Foreign key to Academic (cascading constraint)
  - Enforces qualification uniqueness per academic

### NOT Owned by Shared Kernel (other contexts own migrations)

- **Extensions** — Owned by `ProvisionExtensionDbContext`
  - Shared Kernel provides: Entity definition, configuration semantics (constraint definitions)
  - ProvisionExtension owns: Migrations, DbSet declaration, handler logic
  - Reuse pattern: ProvisionExtensionDbContext applies `ExtensionConfiguration` directly

- **Ranks** — Owned by `ManageRanksDbContext`
- **Degrees** — Owned by `ManageDegreesDbContext`
- **Universities** — Owned by `ManageUniversitiesDbContext`

## Scope: What Shared Kernel Provides

### ✅ Domain Layer

- **Entity Definitions**
  - `Academic` — Aggregate root with factory methods (`Create`), state management methods
  - `AcademicQualification` — Subentity with composite key enforcement
  - `Extension` — Entity with assignment lifecycle (AssignTo, ReleaseFrom)
  - **Value Objects**: `Rank`, `Degree`, `University`, `AccessLevel`

- **Factory Methods**
  - `Academic.Create(...)` — Guards against invalid employment states
  - `Extension.Create(...)` — Validates number ranges and type constraints
  - Enforce invariants at creation time; invalid objects cannot exist

- **Domain Exceptions**
  - `BusinessRuleViolationException` — Employment mutual-exclusion violation
  - `ConflictException` — Extension already assigned, release from wrong owner
  - Clear, actionable messages for domain rule violations

- **Domain Events**
  - `IDomainEvent` marker interface (for future event sourcing)

- **Value Object Behavior**
  - `Rank.ToAccessLevel()` — Derives AccessLevel from rank (canonical source of truth)
  - `Academic.NormalizeEmpNr(...)` — Consistent employee identifier normalization
  - `Academic.NormalizeEmpName(...)` — Consistent name normalization

### ✅ Persistence Layer (Mapping Semantics ONLY)

- **EF Core Configurations** (IEntityTypeConfiguration implementations)
  - `AcademicConfiguration` — Table mapping, PK, uniqueness, check constraints
  - `AcademicQualificationConfiguration` — Composite key mapping, FK relationships
  - `ExtensionConfiguration` — Table mapping, unique filtered index on AssignedEmpNr

- **Constraint Definitions**
  - **Academic table**:
    - PK: `Academic.EmpNr` (normalized, max 20 chars)
    - Check constraint: `CK_Academics_EmploymentMutualExclusion` (NOT ([IsTenured] = 1 AND [ContractEndDate] IS NOT NULL))
    - Derived column: `AccessLevel` (computed from Rank enum, no insert/update allowed)
    - Varchar constraints: EmpNr(20), EmpName(200)
    - Date constraint: ContractEndDate must be > today or null

  - **AcademicQualification table**:
    - PK: (AcademicId, DegreeCode, UniversityCode)
    - FK to Academic.EmpNr (cascade delete on parent deletion)
    - No redundant indexes (the composite PK index serves all queries)

  - **Extension table**:
    - PK: `Extension.Number` (positive integer)
    - Unique filtered index: AssignedEmpNr (non-null values only)
    - Decimal precision: Hours column defined as Decimal(4,2) when present
    - Varchar constraint: AssignedEmpNr matches Academic.EmpNr max length

- **Field Length Constants**
  - `SharedKernelFieldLengths.EmpNr` — Single source of truth for EmpNr max length
  - Used consistently across all entity configurations and domain validation

### ✅ Reusable Services

- Design-time DbContext support (for EF Core tooling)
  - Proper connection string resolution with non-Windows guard
  - Mirrors host application pattern for consistency

## Scope: What Shared Kernel Does NOT Provide

### ❌ Host-Related Concerns

- **Application Startup Registration**: No `AddDbContext`, `AddMediatR`, or similar DI registration
  - The host (Program.cs) orchestrates all service registration
  - Features provide ONLY DbContext classes and configurations, not registration methods

- **Configuration Management**: No `IConfiguration` dependencies
  - Configuration retrieval is a host concern
  - Features never read from `appsettings.json` or environment variables
  - Tests that need config use direct DbContextOptions construction

- **Environment-Specific Logic**: No `IsProduction()`, `IsDevelopment()`, or environment checking
  - All logic is environment-agnostic
  - Host determines deployment-specific behavior

- **Connection String Resolution**: Handled exclusively by the host
  - Host resolves connection strings with proper priority:
    1. Environment variable (`ZEUS_SQLSERVER_CONNECTION`)
    2. Configuration key (`ConnectionStrings:DefaultConnection`)
    3. Platform-specific fallback (LocalDB on Windows only)
  - Features never read connection strings

- **Migration Execution**: No automatic `Database.Migrate()` calls
  - Host application (Program.cs) orchestrates migration execution
  - Features provide migrations; host executes them in dependency order

### ❌ Feature-Specific Concerns

- **Handlers/Queries**: MediatR handlers belong in features, not Shared Kernel
- **Validators**: Feature-specific command validators belong in features
- **API Endpoints**: Route definitions belong in features
- **Feature-Specific Entities**: Reference data entities (Rank, Degree, University) are defined in Shared Kernel but not owned

### ❌ Duplication Risks

- **Configuration Duplication**: Each feature that uses an entity applies the same configuration
  - Shared Kernel configuration is the canonical source
  - Features reuse by applying with `modelBuilder.ApplyConfiguration(...)`
  - No inheritance of SharedKernelDbContext (would create ownership ambiguity)

## Persistence Patterns

### Pattern 1: DbContext Design (No Inheritance)

**Correct Approach** (Shared Kernel + Feature independent):

```csharp
// Shared Kernel provides entity definitions and configurations
namespace Zeus.Academia.Features.SharedKernel.Foundation.Persistence;

public sealed class ExtensionConfiguration : IEntityTypeConfiguration<Extension>
{
    public void Configure(EntityTypeBuilder<Extension> builder)
    {
        builder.ToTable("Extensions");
        builder.HasKey(x => x.Number);
        builder.Property(x => x.AssignedEmpNr).HasMaxLength(SharedKernelFieldLengths.EmpNr);
        builder.HasIndex(x => x.AssignedEmpNr)
            .IsUnique()
            .HasFilter("[AssignedEmpNr] IS NOT NULL");
    }
}

// Feature owns migration and applies configuration
namespace Zeus.Academia.Features.Extensions.ProvisionExtension;

public class ProvisionExtensionDbContext : DbContext
{
    public DbSet<Extension> Extensions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Reuse Shared Kernel configuration directly
        modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
    }
}
```

**Why NO Inheritance**:

- Avoids implicit table ownership confusion
- Each DbContext explicitly declares its tables (transparent ownership)
- Prevents accidental inclusion of unrelated table configurations
- Supports independent migration ownership

### Pattern 2: Configuration Reuse (Direct Application)

**How to Apply a Shared Configuration**:

```csharp
// In any feature's DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply configurations that this context owns migrations for
    modelBuilder.ApplyConfiguration(new AcademicConfiguration());

    // Apply configurations from other features (reuse domain definitions)
    modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
}
```

**Guarantee**: Applying a configuration multiple times is safe (idempotent); EF Core ensures the table is mapped identically in all contexts.

### Pattern 3: Entity Reuse Without Duplication

**Using a Shared Entity in Multiple Contexts**:

```csharp
// Shared Kernel defines the entity and its canonical configuration
public sealed class Extension
{
    public int Number { get; private set; }
    public string? AssignedEmpNr { get; private set; }
    // ... rest of entity definition
}

public sealed class ExtensionConfiguration : IEntityTypeConfiguration<Extension>
{
    // Defines table mapping, constraints, indexes
}

// Feature 1: ProvisionExtensionDbContext owns migrations
public class ProvisionExtensionDbContext : DbContext
{
    public DbSet<Extension> Extensions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
    }
}

// Feature 2: Another feature can query Extensions (if needed) without owning migrations
public class SomeOtherDbContext : DbContext
{
    public DbSet<Extension> Extensions { get; set; } // Read-only DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
    }
}
```

**Key Principle**: Configuration is applied consistently wherever the entity is used; migrations are owned by exactly one context.

### Pattern 4: Handoff to Features

When implementing a new slice that uses Shared Kernel entities:

1. **Import Domain Entity**

   ```csharp
   using Zeus.Academia.Features.SharedKernel.Foundation.Domain;
   ```

2. **Declare DbSet** in feature DbContext

   ```csharp
   public DbSet<Extension> Extensions { get; set; }
   ```

3. **Apply Configuration**

   ```csharp
   modelBuilder.ApplyConfiguration(new ExtensionConfiguration());
   ```

4. **Add Feature Logic** (handlers, validators, endpoints)

   ```csharp
   public class ProvisionExtensionCommand : IRequest { ... }
   public class ProvisionExtensionCommandHandler : IRequestHandler<ProvisionExtensionCommand> { ... }
   ```

5. **Generate Migrations** from feature DbContext

   ```bash
   dotnet ef migrations add ProvisionExtension_Initial -c ProvisionExtensionDbContext -p src/Zeus.Academia.Api
   ```

6. **Add Feature Tests** (persistence mapping, business logic)
   ```csharp
   [Fact]
   public void Create_WithValidNumber_ReturnsExtension() { ... }
   ```

## Constraint Enforcement

### Domain Invariants (Enforced in Entity Code)

All business rules are enforced at creation time via factory methods:

- **Extension.Create(int number)** — Validates number is positive
- **Extension.Create(decimal number)** — Validates is whole number within int range
- **Extension.AssignTo(string empNr)** — Validates not already assigned to different academic
- **Extension.ReleaseFrom(string empNr)** — Validates requester is current owner
- **Academic.Create(...)** — Validates mutual exclusion (tenure XOR contract)
- **Academic.SetContract(...)** — Validates contract date is in future

### Persistence Constraints (Enforced in Database)

Database constraints prevent bypassing domain invariants:

- **Check Constraint**: `CK_Academics_EmploymentMutualExclusion`
  - Prevents both tenure and contract date being set simultaneously
  - Complements domain factory enforcement

- **Unique Filtered Index**: `Extensions.AssignedEmpNr` (where AssignedEmpNr IS NOT NULL)
  - Prevents duplicate assigned extensions
  - Allows multiple null values (unassigned extensions)
  - Enforces uniqueness constraint: one extension per academic

- **Composite Primary Key**: `AcademicQualifications (AcademicId, DegreeCode, UniversityCode)`
  - Prevents duplicate qualifications for the same academic
  - Defines the qualification uniqueness boundary

- **Foreign Keys**
  - AcademicQualifications.EmpNr → Academics.EmpNr (cascade delete)
  - Ensures referential integrity
  - Automatic cleanup when academic is removed

## Testing Scope

### ✅ Shared Kernel Tests Verify

- **Mapping Correctness**
  - Tables are named correctly (e.g., "Academics", "AcademicQualifications", not "Academics_Copy")
  - Primary keys are defined correctly
  - Composite keys have correct column order
  - No duplicate unique indexes on primary key

- **Constraint Enforcement**
  - Check constraints are present and correct (e.g., employment mutual exclusion)
  - Unique filtered indexes exist (e.g., Extension.AssignedEmpNr)
  - Foreign keys are configured with correct cascade behavior

- **Decimal/Varchar Precision**
  - Column lengths match domain specifications
  - Decimal precision/scale match business requirements

- **Domain Invariants** (at entity level, not persistence-dependent)
  - Extension factory validates positive number
  - Academic factory prevents tenure + contract
  - Value objects are immutable

- **Result/Error Types**
  - Result<T> enforces non-null success values
  - Error types enforce non-null error payloads
  - Failure factory guards against null arguments

### ❌ Shared Kernel Tests Do NOT Verify

- Feature-specific business logic (e.g., "can only provision extension if rank >= Assistant")
- Handler behavior or query execution
- API endpoint routing
- Host startup or configuration
- Feature-owned table constraints (e.g., Rank uniqueness tested in ManageRanks, not Shared Kernel)
- Extension persistence tests (now in ProvisionExtension tests)

### Test File Organization

```
tests/Features/SharedKernel/Foundation/
├── AcademicEmploymentTests.cs           ✅ Domain invariant tests
├── RankAccessLevelTests.cs              ✅ Value object behavior
├── ResultTests.cs                       ✅ Result<T> and Error invariants
└── SharedKernelDbContextModelTests.cs   ✅ Persistence mapping and constraints
```

**Deprecated** (moved out as ownership transferred):

- `ExtensionOwnershipTests.cs.deprecated` → Should be in ProvisionExtension tests

## Quality Gates (Phase 0 Step 5 Verification)

### ✅ Build & Test Gates

- [x] Build succeeds with 0 warnings, 0 errors
- [x] All Shared Kernel tests pass (20/20)
- [x] No compilation errors after removing host startup code

### ✅ Scope Gates

- [x] No `IConfiguration` dependencies in Shared Kernel
- [x] No `AddDbContext`, `AddMediatR` in Shared Kernel
- [x] No environment checking (IsProduction, IsDevelopment) in Shared Kernel
- [x] No connection string resolution in Shared Kernel
- [x] No migration execution logic in Shared Kernel

### ✅ Ownership Gates

- [x] SharedKernelDbContext owns only: Academics, AcademicQualifications
- [x] ProvisionExtensionDbContext owns: Extensions table (reuses configuration)
- [x] ManageRanksDbContext owns: Ranks table
- [x] ManageDegreesDbContext owns: Degrees table

### ✅ Configuration Reuse Gates

- [x] ExtensionConfiguration is properly defined and reusable
- [x] ProvisionExtensionDbContext successfully applies ExtensionConfiguration
- [x] No configuration duplication across contexts
- [x] Configuration changes are single-source-of-truth (Shared Kernel)

### ✅ Test Alignment Gates

- [x] Extension ownership tests removed from Shared Kernel tests (deprecated)
- [x] Shared Kernel tests focus only on owned entities
- [x] 5 Extension tests moved out (test count: 25 → 20)

## Handoff to Future Phases

### For Phase 1 (e.g., RegisterAcademic)

When implementing RegisterAcademic slice:

1. Create `src/features/Academics/RegisterAcademic/` folder
2. Create `RegisterAcademicDbContext` that reuses `AcademicConfiguration`
3. Implement `CreateAcademicCommand` and `CreateAcademicCommandHandler`
4. Add `CreateAcademicCommandValidator` (FluentValidation)
5. Generate migrations: `dotnet ef migrations add RegisterAcademic_Initial -c RegisterAcademicDbContext`
6. Map endpoint: `POST /academics/register`
7. Add integration tests for RegisterAcademic slice

**Example Context Setup**:

```csharp
public class RegisterAcademicDbContext : DbContext
{
    public DbSet<Academic> Academics { get; set; }
    public DbSet<AcademicQualification> Qualifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AcademicConfiguration());
        modelBuilder.ApplyConfiguration(new AcademicQualificationConfiguration());
    }
}
```

### What NOT to Duplicate

- Domain entity definitions (use shared entities)
- Entity configurations (apply shared configurations)
- Domain exceptions (reuse BusinessRuleViolationException, etc.)
- Value objects (reuse Rank, Degree, University, etc.)

### What Feature Owns

- Feature-specific commands/queries
- Feature-specific handlers and validators
- Feature endpoints and routes
- Feature-specific tests
- Migrations for tables this feature writes to

## Architecture Decisions (Rationale)

### Why No SharedKernelDbContext Inheritance?

- **Transparency**: Each context explicitly declares its tables
- **Ownership clarity**: No ambiguity about which context owns which table
- **Independent evolution**: Features can change their DbContext without affecting others
- **Migration isolation**: Migrations are scoped to feature ownership

### Why Direct Configuration Application?

- **Single source of truth**: Configuration defined once in Shared Kernel
- **Reusability**: Any context can apply any configuration without duplication
- **Maintainability**: Changes to constraints are reflected everywhere automatically
- **No cyclic dependencies**: Features depend on Shared Kernel configurations, not vice versa

### Why No Service Registration in Features?

- **Clean separation**: Features provide classes, host orchestrates registration
- **Testability**: Feature DbContexts can be instantiated without host infrastructure
- **Flexibility**: Host controls registration order, dependency injection strategies
- **Configuration safety**: No risk of feature-specific configuration leaking into global state

## Verification Checklist

Run this checklist when onboarding new team members or verifying compliance:

- [ ] Build succeeds: `dotnet build --no-restore`
- [ ] Shared Kernel tests pass: `dotnet test tests/Features/SharedKernel/Foundation/`
- [ ] No host code: `Get-ChildItem -Recurse src/features/SharedKernel/ -Filter "*.cs" | Select-String "(Program|IConfiguration|AddDbContext|appsettings)"`
  - Expected result: 0 matches
- [ ] SharedKernelDbContext only contains Academics + AcademicQualifications DbSets (inspect source)
- [ ] ExtensionConfiguration exists and is reusable (check `OnModelCreating` method)
- [ ] ProvisionExtensionDbContext applies ExtensionConfiguration (inspect source)
- [ ] No configuration duplication (search for duplicate `modelBuilder.ApplyConfiguration` calls)
- [ ] All entity factories use private/protected constructors to guard invariants
- [ ] No mutable collection escapes (backing `List<T>` not exposed directly)

## References

- [AI-Assisted Development Process](.github/instructions/ai-dev-process.instructions.md)
- [Vertical Slice Implementation](.github/instructions/vertical-slice-implementation.instructions.md)
- [CQRS + MediatR Implementation](.github/instructions/cqrs-mediatr-efcore.instructions.md)
- [AI Output Policy](.github/instructions/ai-assisted-output.instructions.md)
- [EF Core Configuration Best Practices](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties)

---

**Document Version**: 1.0
**Last Reconciliation**: Phase 0 Step 5
**Status**: ✅ Finalized and Verified
