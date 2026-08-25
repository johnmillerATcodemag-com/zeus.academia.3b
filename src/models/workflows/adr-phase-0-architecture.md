---
ai_generated: false
operator: "slice-coordinator"
chat_id: "phase-0-step-6-downstream-documentation"
prompt: |
  Create an ADR (Architecture Decision Record) that documents the Phase 0 architecture
  decisions, their rationale, consequences, and alternatives considered.
started: "2026-08-24T17:00:00Z"
ended: "2026-08-24T17:30:00Z"
task_durations:
  - task: "document status and context"
    duration: "00:05:00"
  - task: "document decisions and rationale"
    duration: "00:10:00"
  - task: "document consequences and trade-offs"
    duration: "00:10:00"
  - task: "document alternatives and implementation"
    duration: "00:05:00"
total_duration: "00:30:00"
ai_log: "ai-logs/2026/08/24/phase-0-step-6-downstream-documentation/conversation.md"
source: "Phase 0 Step 6 - Architecture Decision Record"
description: "ADR documenting Phase 0 architecture decisions and rationale"
---

# ADR-001: Phase 0 Architecture — Modular CQRS with Feature-Local DbContexts

**Status**: ✅ ACCEPTED (Phase 0 Complete)
**Date**: August 24, 2026
**Author**: Architecture Team (Slice Coordinator)
**Stakeholders**: All future implementation teams, domain experts
**References**: [Phase 0 Refactoring Plan](./academia-refactoring-plan.md), [Shared Kernel Boundaries](../features/SharedKernel/PERSISTENCE_BOUNDARIES.md)

## Table of Contents

1. Context
2. Decision Overview
3. Key Architectural Decisions (5)
4. Consequences (Positive, Negative, Mitigations)
5. Alternatives Considered
6. Implementation Checklist
7. Open Questions & Follow-ups

---

## Context

The zeus.academia application manages academic institution data (faculty, degrees, ranks, extensions). The system must:

- **Prevent circular dependencies** between features (e.g., RegisterAcademic ↔ ManageUniversities)
- **Centralize domain invariants** (e.g., mutual exclusion: tenure XOR contract)
- **Allow independent persistence ownership** (each feature owns its tables)
- **Support catalog-based reference data resolution** (immutable, historical data preservation)
- **Scale to 10k+ concurrent users** with consistent business logic

The original implementation mixed host-level and feature-level concerns, tightly coupled features through shared DbContext, and lacked clear module boundaries.

## Decision Overview

We adopt a **modular CQRS architecture with feature-local DbContexts and centralized domain primitives**:

```
┌─────────────────────────────────────────────────────┐
│              Application Host (DI Composition)      │
│  - Program.cs with centralized configuration        │
│  - SQL Server setup (LocalDB fallback)              │
│  - MediatR, FluentValidation, migration registration │
└─────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────┐
│         Shared Kernel (Cross-Cutting Domain)       │
│  - Rank, Degree, University, Extension (V.O.)      │
│  - Academic, AcademicQualification (Entities)      │
│  - Configurations (reusable, single source of truth) │
│  - Result/Error types, base domain exceptions       │
│  - NO host startup code, NO IConfiguration         │
└─────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────┐
│      Feature Modules (Isolated Persistence)        │
│                                                     │
│  Reference Data (Phase 0):                          │
│  ├─ ManageRanks → RanksDbContext (Ranks table)     │
│  ├─ ManageDegrees → DegreesDbContext (Degrees)    │
│  ├─ ManageUniversities → UniversitiesDbContext    │
│  └─ ProvisionExtension → ExtensionDbContext       │
│                                                     │
│  Domain (Phase 1+):                                 │
│  ├─ RegisterAcademic → RegisterAcademicDbContext   │
│  ├─ RecordQualification → QualificationDbContext   │
│  └─ ...                                             │
│                                                     │
│  Each Feature:                                      │
│  ├─ Commands & Queries (MediatR contracts)         │
│  ├─ Handlers (consume catalogs via IMediator)      │
│  ├─ Domain Models (aggregates using SK types)      │
│  ├─ Feature-Local DbContext (owns 1-N tables)      │
│  └─ Tests (unit + integration)                      │
└─────────────────────────────────────────────────────┘
                          ↓
              Catalog Queries (via IMediator)
              e.g., GetRankByCodeQuery
              (Decouples features; allows swapping)
```

## Key Architectural Decisions

### Decision 1: One DbContext Per Feature

**Decision**: Each feature owns a single DbContext (e.g., RegisterAcademicDbContext) and owns 1-N related tables. No shared DbContext across features.

**Why**:

- **Clear ownership**: Each context owns specific migrations (no ambiguity)
- **Independent scalability**: Features can evolve persistence independently
- **Testability**: Features test in isolation without coupling to other persistence
- **Debugging clarity**: When a migration fails, it's obvious which feature is responsible
- **Parallel development**: Teams can work on features without blocking on shared context

**Trade-off**: More DbContext classes to maintain; more setup per feature.

**Mitigation**:

- Templates and code generation automate DbContext creation
- Service collection extensions (AddXxxPersistence) hide complexity
- Consistent pattern across all features (easy to learn)

**Example**:

```csharp
// ✅ CORRECT: RegisterAcademic owns its persistence
public class RegisterAcademicDbContext : DbContext
{
    public DbSet<Academic> Academics { get; set; }  // Owns this table
}

// ❌ WRONG: Shared context (ambiguity, circular deps)
public class SharedKernelDbContext : DbContext
{
    public DbSet<Academic> Academics { get; set; }  // Who owns migrations?
    public DbSet<Rank> Ranks { get; set; }          // Two features need this?
}
```

### Decision 2: Shared Kernel Provides Configurations, Not DbContext

**Decision**: Shared Kernel defines Entity Framework configurations (fluent mapping, constraints, relationships) but does NOT own DbContext and does NOT own migrations. Features reuse configurations directly.

**Why**:

- **Single source of truth**: Configuration defined once (Shared Kernel), reused everywhere
- **Prevents conflicts**: Extensions entity owned by ProvisionExtensionDbContext only (not also SharedKernelDbContext)
- **Configuration is free to reuse**: Unlike DbContext ownership, configurations can be applied by multiple contexts without claiming ownership
- **Separation of concerns**: Shared Kernel = domain knowledge; features = persistence ownership

**Trade-off**: Configuration reuse is manual (must explicitly call ApplyConfiguration).

**Mitigation**: Clear documentation of reuse pattern; templates show the pattern.

**Example**:

```csharp
// Shared Kernel defines configuration (not ownership)
public class AcademicConfiguration : IEntityTypeConfiguration<Academic>
{
    public void Configure(EntityTypeBuilder<Academic> builder)
    {
        builder.HasKey(a => a.EmpNr);
        builder.Property(a => a.RankCode).HasMaxLength(20);
        // ... constraints, relationships, etc. ...
    }
}

// Feature 1: RegisterAcademicDbContext REUSES configuration
public class RegisterAcademicDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AcademicConfiguration());
        // RegisterAcademicDbContext owns the Academic migration
    }
}

// Feature 2: RecordQualificationDbContext REUSES same configuration
public class RecordQualificationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AcademicConfiguration());
        // Would own Academic migration if first to generate it
        // But RegisterAcademic already owns it, so this context depends on it
    }
}
```

### Decision 3: Catalog Queries as Integration Point

**Decision**: Downstream features consume reference data via IMediator query contracts (GetXxxByCodeQuery), not via direct DbContext access.

**Why**:

- **Loose coupling**: Downstream features don't know how catalogs are persisted
- **Independent evolution**: Catalog implementation can change; downstream unaffected
- **Testability**: Easy to mock catalog queries in integration tests
- **CQRS alignment**: Queries are the intended read path
- **Future flexibility**: Can swap implementations (e.g., external API, cache) without changing handlers

**Trade-off**: One extra hop (IMediator) for every catalog access.

**Mitigation**: IMediator overhead negligible; benefits (decoupling, testability) far outweigh cost.

**Example**:

```csharp
// ✅ CORRECT: Query via IMediator (decoupled)
var rankQuery = new GetRankByCodeQuery("PROF");
var rankResponse = await _mediator.Send(rankQuery, ct);
if (!rankResponse.IsFound)
    return Result.Failure(...);

// ❌ WRONG: Direct DbContext (tightly coupled)
var rank = await _manageRanksDbContext.Ranks
    .FirstOrDefaultAsync(r => r.Code == "PROF", ct);
if (rank is null)
    return Result.Failure(...);
// Now RegisterAcademic depends on ManageRanks persistence!
// If ManageRanks changes, this breaks!
```

### Decision 4: Code-Based Identity for Reference Data

**Decision**: Reference data (Ranks, Degrees, Universities) are identified by immutable codes. Aggregates store codes, not references to catalog records.

**Why**:

- **Immutability**: Once a qualification is recorded with a degree code, the code is fixed forever
- **Historical integrity**: Even if a degree is renamed or deleted in the catalog, the historical record remains valid
- **Simplicity**: Value objects (Rank, Degree, University) wrap the code; no complex relationship mapping
- **Data integrity**: Prevents orphaned foreign keys (if catalog record deleted)
- **Flexibility**: Code is a natural business identifier (e.g., "MAST", "BOSTON_U")

**Trade-off**: Must query catalog to get full details (description, constraints, etc.); codes alone insufficient.

**Mitigation**: Catalog queries provide full details; handlers cache as needed.

**Example**:

```csharp
// Qualification stores CODES (not references)
public class AcademicQualification
{
    public string DegreeCode { get; set; }       // "MAST" (immutable, historical)
    public string UniversityCode { get; set; }   // "BOSTON_U" (immutable)
    public DateTime ObtainedDate { get; set; }   // When obtained (immutable)
}

// If "MAST" is deleted from Degrees catalog, this qualification is unaffected
// If "MAST" is renamed to "MASTER", the qualification still says "MAST" (correct)

// To get full details, query the catalog:
var degreeResponse = await _mediator.Send(new GetDegreeByCodeQuery("MAST"), ct);
// Response includes: Code, Description, IsActive, Level, etc.

// Aggregate creates value objects from codes (validates during command execution)
var degreeVo = Degree.Create(degreeResponse.Code!);  // Code! safe after validation
```

### Decision 5: Host Owns DI and Configuration

**Decision**: Application Host (src/Zeus.Academia.Api) owns the DI composition root, SQL Server configuration, migration execution, and route registration. Features are NOT responsible for host setup.

**Why**:

- **Centralized configuration**: One place to configure all dependencies
- **Environment-aware**: Connection strings, API keys, feature toggles managed centrally
- **Consistent setup**: All features registered the same way
- **Testing**: Test double configuration isolated from feature code
- **Operational clarity**: Infrastructure decisions made at host level, not scattered across features

**Trade-off**: Requires features to provide extension methods (AddXxxPersistence, AddXxxMediatR).

**Mitigation**: Template patterns established; extension method pattern is standard.

**Example**:

```csharp
// Feature provides extensions (Shared Kernel: ManageRanks, etc.)
public static class ManageRanksServiceCollectionExtensions
{
    public static IServiceCollection AddManageRanksPersistence(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<RanksDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        return services;
    }
}

// Host uses extensions (Program.cs)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddManageRanksPersistence(builder.Configuration);  // ← Centralized
builder.Services.AddManageDegreesPersistence(builder.Configuration);
builder.Services.AddRegisterAcademicPersistence(builder.Configuration);
// All setup in one place, easy to audit and modify
```

---

## Consequences

### Positive Consequences ✅

1. **No circular dependencies between features**
   - Each feature depends on Phase 0 (Shared Kernel, Application Host)
   - Features do NOT depend on each other (at persistence level)
   - Prevents deadlock in development (teams work in parallel)

2. **Clear module boundaries**
   - Each feature owns its DbContext, migrations, handlers
   - Shared Kernel owns domain types and configurations (not persistence)
   - Host owns DI and infrastructure

3. **Independent scalability**
   - Features can add tables, change schemas without affecting other features
   - Migrations isolated (no merging conflicts in shared migration file)
   - Parallel feature development and testing

4. **Reference data is immutable (historical integrity)**
   - Once a qualification is recorded with code "MAST", it's forever "MAST"
   - If catalog entry renamed/deleted, historical records unaffected
   - Supports auditing, compliance, historical reports

5. **Testability**
   - Features test in isolation (mock catalog queries)
   - Integration tests use in-memory database (no shared state)
   - No dependencies on external features' database state

6. **Configuration reuse without duplication**
   - AcademicConfiguration defined once (Shared Kernel)
   - Reused by RegisterAcademicDbContext, RecordQualificationDbContext, etc.
   - Single source of truth (constraint changes propagate automatically)

7. **Explicit, decoupled contracts**
   - GetRankByCodeQuery + GetRankByCodeResponse are formal contracts
   - Features don't need to know how catalogs work internally
   - Easy to mock in tests, easy to understand dependencies

### Negative Consequences ⚠️

1. **More DbContext classes to create and maintain**
   - Each feature needs its own DbContext
   - More boilerplate (though templates can reduce this)
   - More files in the codebase

2. **IMediator adds one extra hop for catalog access**
   - Not a blocking issue (overhead negligible)
   - Adds slight complexity (must define query contracts)

3. **Configuration reuse is manual (not automatic)**
   - Must explicitly call `ApplyConfiguration(new AcademicConfiguration())`
   - Not a big deal; it's clear and explicit
   - Templates establish the pattern

4. **Requires discipline to follow patterns**
   - Must remember to use IMediator (not direct DbContext access)
   - Must remember to reuse configurations (not duplicate)
   - Must remember to register feature in host
   - Mitigated by clear documentation and code reviews

### Mitigations

| Consequence                 | Mitigation                                                                                                                       |
| --------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| More DbContext classes      | Templates/code generation for new features                                                                                       |
| IMediator overhead          | Documentation explaining negligible performance cost                                                                             |
| Manual configuration reuse  | Clear examples, code review checklist                                                                                            |
| Pattern discipline required | [Phase 1+ Downstream Consumer Pattern](./phase-1-downstream-consumer-pattern.md) (canonical reference), architecture review gate |

---

## Alternatives Considered

### Alternative 1: Shared DbContext for All (Rejected)

**Approach**: All features use a single SharedKernelDbContext that owns all tables.

**Problems**:

- ❌ **Ambiguous ownership**: Who owns Extensions? Who owns Academic? Two features claim ownership, migrations conflict
- ❌ **Circular dependencies possible**: Feature A adds table, Feature B adds column, migrations become interdependent
- ❌ **Tight coupling**: All features depend on SharedKernelDbContext, hard to test independently
- ❌ **Merge conflicts**: All features update same migration file
- ❌ **Shared Kernel bloated**: SK becomes all-knowing, loses focus on domain primitives

**Why rejected**: Ownership conflicts, circular dependencies, tight coupling make this unmaintainable at scale.

### Alternative 2: Inherit from SharedKernelDbContext

**Approach**: Features inherit from SharedKernelDbContext (e.g., RegisterAcademicDbContext : SharedKernelDbContext) and add their own tables.

```csharp
// ❌ PROBLEMATIC
public class RegisterAcademicDbContext : SharedKernelDbContext
{
    public DbSet<AcademicExtension> AcademicExtensions { get; set; }  // ← New table
}

public class ProvisionExtensionDbContext : SharedKernelDbContext
{
    public DbSet<Extension> Extensions { get; set; }  // ← Same table? Or different?
}
// Conflict! Both claim ownership of Extension. Which context generates the migration?
```

**Problems**:

- ❌ **Inheritance induces ownership ambiguity**: If two contexts inherit from SK, who owns the inherited tables?
- ❌ **Configuration precedence unclear**: If both ApplyConfiguration, which wins?
- ❌ **Tight coupling to SK**: All features depend on SK DbContext, hard to change SK

**Why rejected**: Inheritance creates implicit ownership claims; doesn't actually solve the problem.

### Alternative 3: Direct DbContext Access Between Features

**Approach**: Downstream features directly access ManageRanks DbContext:

```csharp
// ❌ TIGHTLY COUPLED
public class RegisterAcademicCommandHandler
{
    private readonly RanksDbContext _ranksDbContext;  // ← Direct dependency

    public async Task Handle(RegisterAcademicCommand cmd, CancellationToken ct)
    {
        var rank = await _ranksDbContext.Ranks
            .FirstOrDefaultAsync(r => r.Code == cmd.RankCode);
        // RegisterAcademic now depends on ManageRanks persistence!
    }
}
```

**Problems**:

- ❌ **Breaks module boundaries**: Features become tightly coupled at persistence level
- ❌ **Hard to test**: Can't test RegisterAcademic without ManageRanks DbContext running
- ❌ **Hard to maintain**: Changes in ManageRanks schema break RegisterAcademic
- ❌ **Violates CQRS**: Should query, not directly access persistence

**Why rejected**: Tight coupling makes the system fragile and hard to test.

### Alternative 4: Event-Based Integration (Deferred)

**Approach**: Features communicate via domain events (e.g., RankCreated event published by ManageRanks, consumed by RegisterAcademic).

**Problems**:

- ⚠️ **Complexity**: Event store, event bus, eventual consistency, saga patterns
- ⚠️ **Overkill for phase 0**: Not needed for current scope; introduces needless complexity
- ⚠️ **Debugging harder**: Async event handling harder to trace

**Decision**: Defer event-based integration to Phase 2 if needed. Current query-based approach sufficient and simpler.

---

## Implementation Checklist

### Phase 0 (Complete ✅)

- [x] Establish Application Host (Program.cs, DI composition, SQL Server config)
- [x] Create Shared Kernel (domain types, configurations, NO host code)
- [x] Create feature-local DbContexts (ManageRanks, ManageDegrees, prepared for Phase 1)
- [x] Verify migration ownership (each table owned by one context)
- [x] Document University identity contract (code-based, historical)
- [x] Create canonical downstream consumer pattern documentation
- [x] Create handoff documents for Phase 1 slices

### Phase 1 (Upcoming)

- [ ] Implement ManageUniversities (GetUniversityByCodeQuery, UniversitiesDbContext)
- [ ] Implement ProvisionExtension (handlers, ExtensionDbContext)
- [ ] Implement RegisterAcademic (first domain slice, uses ManageRanks catalog)
- [ ] Implement RecordQualification (uses ManageDegrees + ManageUniversities catalogs)
- [ ] Verify all features follow downstream consumer pattern
- [ ] Update pattern documentation with Phase 1 learnings

### Phase 2+ (Future)

- [ ] Evaluate event-based integration if needed
- [ ] Add cross-feature domain events (e.g., AcademicRegistered)
- [ ] Consider CQRS read models if reporting complexity grows
- [ ] Optimize catalog query performance (caching, indexing)

---

## Open Questions & Follow-ups

### Q1: What if a feature needs data from two Phase 0 catalogs?

**Answer**: Use multiple queries via IMediator. Handler resolves both:

```csharp
var degreeResponse = await _mediator.Send(new GetDegreeByCodeQuery(...), ct);
var universityResponse = await _mediator.Send(new GetUniversityByCodeQuery(...), ct);
// Handle both responses, create aggregates, persist
```

**Follow-up**: Monitor performance; may need caching in Phase 2.

### Q2: Can a feature add its own catalog (e.g., ExtensionTypes)?

**Answer**: Yes. ProvisionExtension feature owns ExtensionTypesDbContext (or includes ExtensionTypes in ProvisionExtensionDbContext). Downstream features query via GetExtensionTypeByCodeQuery.

**Same pattern applies**: Query via IMediator, store codes in aggregates.

### Q3: What if two features need the same catalog? (e.g., ManageRanks)

**Answer**: They both query via GetRankByCodeQuery. ManageRanks owns the migration; other features depend on the query contract.

**No direct DbContext sharing**: Each feature has its own DbContext; catalog queries handle the integration.

### Q4: How do we handle soft deletes (IsActive flag)?

**Answer**: Catalog queries return IsActive. Handlers validate:

```csharp
if (!universityResponse.IsActive)
    return Result.Failure("UniversityNotActive", "University no longer accepting qualifications");
```

**Aggregates store codes**: Even if deleted, historical qualifications still have the code.

### Q5: How do we implement search/filtering across features?

**Answer**: Defer to Phase 2. Current design handles CRUD + catalog lookups. Complex queries (search, filtering, aggregations) may require CQRS read models.

**Potential approach**: Create separate read models (projections) from domain events or snapshots.

### Q6: Versioning of catalog queries?

**Answer**: Not yet needed. If GetRankByCodeQuery contract must change, bump to v2:

```csharp
public record GetRankByCodeQuery(string Code) : IRequest<GetRankByCodeResponse>;
public record GetRankByCodeQueryV2(string Code, int? MinLevel = null) : IRequest<GetRankByCodeResponseV2>;
```

**Deferred**: Address if compatibility becomes issue.

---

## Conclusion

This architecture provides a solid foundation for zeus.academia: **clear module boundaries, independent feature development, historical data integrity, and testability**. The patterns are explicit, documented, and enforced through code review gates.

**All downstream slices follow the [Phase 1+ Downstream Consumer Pattern](./phase-1-downstream-consumer-pattern.md) exactly.**

---

**Status**: ✅ ACCEPTED
**Approved By**: Architecture Team
**Effective Date**: August 24, 2026
**Next Review**: After Phase 1 completion (incorporate learnings, update if needed)

---

**References**:

- [Phase 0 Refactoring Plan](./academia-refactoring-plan.md)
- [Phase 1+ Downstream Consumer Pattern](./phase-1-downstream-consumer-pattern.md)
- [Shared Kernel Persistence Boundaries](../features/SharedKernel/PERSISTENCE_BOUNDARIES.md)
- [EP-2-1 RegisterAcademic Handoff](./ep-2-1-register-academic-handoff.md)
- [Migration Ownership Matrix](./migration-ownership-matrix.md)
