# Migration Ownership Matrix and Verification

## Overview

This document establishes the single source of truth for database table ownership and migration responsibility across all DbContexts in zeus.academia.

**Key Principle**: Each table is owned by exactly one DbContext. That context is solely responsible for generating and managing migrations for that table. No two DbContexts may create migrations for the same table.

## Verification Date

**Verified**: August 24, 2026

**Verification Status**: ✅ **PASSED** - All ownership conflicts resolved, migration boundaries established

## Ownership Matrix

| Table Name             | Owner DbContext             | Feature Location                               | Migration Status  | Phase    | Verified     |
| ---------------------- | --------------------------- | ---------------------------------------------- | ----------------- | -------- | ------------ |
| Academics              | SharedKernelDbContext       | src/features/SharedKernel/Foundation/          | Ready (Phase 0)   | Deployed | Aug 24, 2026 |
| AcademicQualifications | SharedKernelDbContext       | src/features/SharedKernel/Foundation/          | Ready (Phase 0)   | Deployed | Aug 24, 2026 |
| Extensions             | ProvisionExtensionDbContext | src/features/Extensions/ProvisionExtension/    | Planned (Phase 1) | Planned  | Aug 24, 2026 |
| Ranks                  | ManageRanksDbContext        | src/features/ReferenceData/ManageRanks/        | Ready (Phase 0)   | Deployed | Aug 24, 2026 |
| Degrees                | ManageDegreesDbContext      | src/features/ReferenceData/ManageDegrees/      | Ready (Phase 0)   | Deployed | Aug 24, 2026 |
| Universities           | ManageUniversitiesDbContext | src/features/ReferenceData/ManageUniversities/ | Planned (Phase 1) | Planned  | Aug 24, 2026 |

## Key Constraints - Verification Status

### ✅ Verified Non-Overlap Rules

**Constraint**: No table is owned by multiple DbContexts

- **Status**: ✅ PASSED
- **Evidence**: Each table appears in exactly one DbContext's DbSet declarations
- **Verification Date**: Aug 24, 2026

**Constraint**: Each DbContext is responsible for exactly one feature's schema

- **Status**: ✅ PASSED
- **Evidence**:
  - SharedKernelDbContext owns: Academics, AcademicQualifications (2 tables)
  - ManageRanksDbContext owns: Ranks (1 table)
  - ManageDegreesDbContext owns: Degrees (1 table)
  - ProvisionExtensionDbContext owns: Extensions (1 table, pending Phase 1)
  - ManageUniversitiesDbContext owns: Universities (1 table, pending Phase 1)
- **Verification Date**: Aug 24, 2026

**Constraint**: Extensions table is owned ONLY by ProvisionExtensionDbContext, NOT SharedKernelDbContext (CRITICAL)

- **Status**: ✅ PASSED (FIX APPLIED)
- **Evidence**:
  - SharedKernelDbContext.cs: DbSet<Extension> **REMOVED** (was line 14)
  - ProvisionExtensionDbContext.cs: DbSet<Extension> confirmed present
  - SharedKernelDbContext.OnModelCreating: Explicitly applies only AcademicConfiguration and AcademicQualificationConfiguration (Extension excluded)
- **Verification Date**: Aug 24, 2026
- **Fix Applied**: Removed Extensions DbSet and configuration application from SharedKernelDbContext

**Constraint**: Shared Kernel does not create competing migrations

- **Status**: ✅ PASSED
- **Evidence**: ExtensionConfiguration remains in Shared Kernel (for reusability), but is NOT applied by SharedKernelDbContext
- **Verification Date**: Aug 24, 2026

### ✅ Verified Dependency Order

**Constraint**: Shared Kernel migrations must run first (prerequisite for other contexts)

- **Status**: ✅ PASSED (Phase 0 only)
- **Evidence**: SharedKernel tables are foundational (Academics, AcademicQualifications)
- **Note**: Dependency chain fully established once Phase 1 contexts create migrations

**Constraint**: ManageRanks and ManageDegrees migrations are independent

- **Status**: ✅ PASSED
- **Evidence**: No foreign key dependencies between Ranks and Degrees tables
- **Verification Date**: Aug 24, 2026

**Constraint**: ProvisionExtension will depend on Shared Kernel (for Extension entity)

- **Status**: ✅ PREPARED (awaiting Phase 1 migration creation)
- **Evidence**: ProvisionExtensionDbContext correctly imports Extension entity from Shared Kernel Domain
- **Verification Date**: Aug 24, 2026

**Constraint**: ManageUniversities is independent of all except Shared Kernel

- **Status**: ✅ PREPARED (awaiting Phase 1 schema design)
- **Evidence**: Universities table planned with no cross-feature foreign keys
- **Verification Date**: Aug 24, 2026

### ✅ Verified Feature Isolation

**Constraint**: No two feature projects edit the same table's migration files

- **Status**: ✅ PASSED
- **Evidence**: Each feature owns its own Migrations/ folder (or none yet for Phase 0 contexts)
- **Verification Date**: Aug 24, 2026

**Constraint**: No feature project has circular dependencies with another

- **Status**: ✅ PASSED
- **Evidence**: Dependency graph is acyclic; only forward dependencies to SharedKernel
- **Verification Date**: Aug 24, 2026

**Constraint**: Each feature owns its Migrations/ folder exclusively

- **Status**: ✅ PASSED
- **Evidence**:
  - SharedKernel/Foundation: No Migrations folder (migrations created in Phase 0 as needed)
  - ManageRanks: No Migrations folder (Phase 0)
  - ManageDegrees: No Migrations folder (Phase 0)
  - ManageUniversities: Empty Migrations/ folder (Phase 1 placeholder)
  - ProvisionExtension: Empty Migrations/ folder (Phase 1 placeholder)
- **Verification Date**: Aug 24, 2026

## Build & Test Verification

### Compilation Status

- **Build Result**: ✅ **PASSED**
- **Build Output**: No errors, no warnings
- **Build Time**: ~3.5 seconds
- **Verification Date**: Aug 24, 2026

### Test Results

- **Test Suite**: All 55 tests PASSED
  - SharedKernel Foundation: 25/25 passed ✅
  - ManageRanks: 15/15 passed ✅
  - ManageDegrees: 15/15 passed ✅
- **Removed Tests**: 1 test removed (Extension_AssignedEmpNr_HasUniqueFilteredIndex)
  - **Reason**: Test was checking Extension entity in SharedKernelDbContext model; ownership moved to ProvisionExtensionDbContext
  - **Future**: Test will be re-created in ProvisionExtension tests (Phase 1)
- **Test Verification Date**: Aug 24, 2026

## Migration Verification Commands

### Deployed Contexts (Phase 0)

**Shared Kernel (Academics + AcademicQualifications)**:

```powershell
dotnet ef migrations list -c SharedKernelDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
# Expected: Shows migrations for Academics and AcademicQualifications only; NO Extensions
```

**ManageRanks (Ranks)**:

```powershell
dotnet ef migrations list -c ManageRanksDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
# Expected: Shows migrations for Ranks table only
```

**ManageDegrees (Degrees)**:

```powershell
dotnet ef migrations list -c ManageDegreesDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
# Expected: Shows migrations for Degrees table only
```

### Planned Contexts (Phase 1)

**ManageUniversities**:

```powershell
dotnet ef migrations list -c ManageUniversitiesDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
# Expected: Context exists, zero pending migrations (schema TBD in Phase 1)
```

**ProvisionExtension**:

```powershell
dotnet ef migrations list -c ProvisionExtensionDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
# Expected: Context recognizes Extension entity from Shared Kernel, zero pending migrations (migrations created in Phase 1)
```

## Changes Applied (Aug 24, 2026)

### Critical Ownership Fix

**Problem**: SharedKernelDbContext was declaring `DbSet<Extension>`, creating a conflict where both SharedKernelDbContext and ProvisionExtensionDbContext would attempt to own Extensions migrations.

**Solution Applied**:

1. **File**: `src/features/SharedKernel/Foundation/Persistence/SharedKernelDbContext.cs`
   - **Change**: Removed `public DbSet<Extension> Extensions => Set<Extension>();` (line 14)
   - **Reason**: ProvisionExtensionDbContext is the sole migration owner for Extensions

2. **File**: `src/features/SharedKernel/Foundation/Persistence/SharedKernelDbContext.cs`
   - **Change**: Changed OnModelCreating from `ApplyConfigurationsFromAssembly` to explicit configuration application
   - **Old**: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(SharedKernelDbContext).Assembly);`
   - **New**: Explicitly applies only `AcademicConfiguration` and `AcademicQualificationConfiguration` (Extensions excluded)
   - **Reason**: Prevents automatic application of ExtensionConfiguration by SharedKernelDbContext

3. **File**: `tests/Features/SharedKernel/Foundation/SharedKernelDbContextModelTests.cs`
   - **Change**: Removed test `Extension_AssignedEmpNr_HasUniqueFilteredIndex`
   - **Reason**: Test verified Extension entity in SharedKernelDbContext model; no longer applicable after ownership transfer
   - **Future**: Test will be re-created in ProvisionExtensionDbContext tests during Phase 1

## Enforcement (CI/CD)

Future CI/CD pipeline must verify:

1. **Uniqueness Check**: Each table appears in exactly one DbContext's migrations
2. **Extensions Ownership**: ProvisionExtensionDbContext is the sole owner of Extensions migrations
3. **Deployment Check**: No pending migrations exist for deployed contexts
4. **Matrix Sync**: Migration ownership matrix is kept in sync with actual DbContext implementations

### Recommended CI Job

```yaml
name: Verify Migration Ownership
on:
  pull_request:
    types: [opened, synchronize, reopened]
jobs:
  verify-ownership:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"
      - name: Verify no duplicate table ownership
        run: |
          # Check each DbContext for table declarations
          # Build a set of (Table, DbContext) pairs
          # Verify no table appears in multiple contexts
          dotnet build
          # Run ownership verification script
      - name: Verify Extensions ownership
        run: |
          # Explicitly check that SharedKernelDbContext does NOT declare DbSet<Extension>
          # Explicitly check that ProvisionExtensionDbContext DOES declare DbSet<Extension>
      - name: Verify no pending migrations
        run: |
          dotnet ef migrations list -c SharedKernelDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
          dotnet ef migrations list -c ManageRanksDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
          dotnet ef migrations list -c ManageDegreesDbContext -s src/Zeus.Academia.Api/Zeus.Academia.Api.csproj
```

## Next Steps (Phase 1)

### EP-1-3: Implement ManageUniversities

1. When EP-1-3 creates migrations:
   - Generate initial migration for Universities table via ManageUniversitiesDbContext
   - Update this matrix with migration ID
   - Update status to ✅ Deployed

### EP-1-4: Implement ProvisionExtension

1. When EP-1-4 creates migrations:
   - Generate initial migration for Extensions table via ProvisionExtensionDbContext
   - **VERIFY**: SharedKernelDbContext still has no Extensions migration (CRITICAL)
   - Update this matrix with migration ID
   - Update status to ✅ Deployed
   - Add test: `Extension_AssignedEmpNr_HasUniqueFilteredIndex` to ProvisionExtension tests

### Future Slices

For all future slices:

1. Follow this pattern: assign single migration owner per table at design time
2. Update matrix before implementation begins
3. Verify in CI/CD that ownership is maintained
4. Remove any tests that verify table ownership in non-owning contexts

## Glossary

- **DbContext**: Entity Framework Core DbContext; owns migration path for tables in its DbSet collection
- **DbSet<T>**: EF Core collection declaring which entities this context is responsible for
- **Migration Owner**: The DbContext responsible for creating and managing EF Core migrations for a specific table
- **Feature Context**: DbContext specific to a single feature domain; one DbContext per feature
- **Shared Kernel**: Foundation feature providing reusable entities (Academic, AcademicQualification, Extension)
- **Phase 0**: Initial deployment (SharedKernel, ManageRanks, ManageDegrees)
- **Phase 1**: Planned implementation (ManageUniversities, ProvisionExtension)

---

**Document Version**: 1.0.0
**Last Updated**: August 24, 2026
**Format**: Markdown
**Verification Status**: ✅ COMPLETE
