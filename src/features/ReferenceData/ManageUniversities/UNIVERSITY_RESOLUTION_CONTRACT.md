---
ai_generated: true
model: "anthropic/claude-haiku-4.5@2024-10-22"
operator: "johnmillerATcodemag-com"
chat_id: "2026-08-24-university-identity-reconciliation"
prompt: |
  Establish the explicit contract between the University catalog (ManageUniversities)
  and the Shared Kernel University value object for Phase 0 Step 6
started: "2026-08-24T19:00:00Z"
ended: "2026-08-24T19:30:00Z"
task_durations:
  - task: "audit current state and identify pattern mismatch"
    duration: "00:05:00"
  - task: "define identity contract between catalog and domain"
    duration: "00:10:00"
  - task: "document resolution pattern and error handling"
    duration: "00:10:00"
  - task: "create integration examples"
    duration: "00:05:00"
total_duration: "00:30:00"
ai_log: "ai-logs/2026/08/24/2026-08-24-university-identity-reconciliation/conversation.md"
source: "Phase 0 Step 6 Reconciliation — University Identity"
---

# University Resolution Contract

## Overview

This document establishes the CANONICAL mapping between the `UniversityRecord` catalog entity (ManageUniversities) and the `University` domain value object (Shared Kernel). It ensures that:

- The catalog is the single source of truth for known institutions
- Domain aggregates reference universities by the same Code
- Resolution is deterministic, validated, and centralized
- Downstream slices (RegisterAcademic, RecordDegreeObtained) follow a consistent pattern

**Status**: ✅ APPROVED for Phase 0 Step 6
**Ownership**: ManageUniversities (catalog), Shared Kernel (value object), downstream slices (consumers)
**Effective Date**: Post-Phase-0-Step-6 (before EP-1-3 implementation)

---

## Part 1: Domain Model (Shared Kernel)

### The University Value Object

```csharp
namespace Zeus.Academia.Features.SharedKernel.Foundation.Domain;

/// <summary>
/// Represents a known institution in the academic system.
/// Identified by its institutional code (e.g., "BOSTON_U"), not by name.
/// Immutable; once created, the code never changes.
/// </summary>
public sealed record University
{
    private University(string code)
    {
        Code = code;
    }

    /// <summary>
    /// The institutional code — e.g., "BOSTON_U", "MIT", "STANFORD".
    /// This is the CANONICAL identifier (NOT the institution's legal name).
    /// Normalized to uppercase; uniqueness is enforced at the catalog level.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Factory method for creating a University value object.
    /// Validates that the code is required, non-empty, and within length limits.
    /// </summary>
    public static University Create(string code)
    {
        var normalized = Normalize(code);
        if (normalized.Length > SharedKernelFieldLengths.UniversityCode)
        {
            throw new ArgumentException(
                $"University code cannot exceed {SharedKernelFieldLengths.UniversityCode} characters.",
                nameof(code));
        }

        return new University(normalized);
    }

    /// <summary>
    /// Normalizes the code to uppercase and trims whitespace.
    /// Throws if the code is null or empty.
    /// </summary>
    internal static string Normalize(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("University code is required.", nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }
}
```

### University.Code Semantics

| Property          | Description                                                 |
| ----------------- | ----------------------------------------------------------- |
| **Identity**      | Uniquely identifies an institution across the entire system |
| **Format**        | Alphanumeric code; e.g., "BOSTON_U", "MIT", "STANFORD"      |
| **Normalization** | Uppercase; whitespace trimmed                               |
| **Length Limit**  | Defined in `SharedKernelFieldLengths.UniversityCode`        |
| **Mutability**    | Immutable (record semantics); never changes after creation  |
| **Source**        | Comes from `UniversityRecord.Code` in the catalog           |
| **Constraint**    | Required; cannot be null, empty, or whitespace              |

### Why NOT University.Name?

❌ **WRONG**: Using `University.Name` as the identifier

```csharp
// DON'T DO THIS:
var university = University.Create("Boston University");  // ← Name, not code
// Problems:
// - Multiple institutions can have identical names (e.g., "State University")
// - No guarantee of uniqueness
// - Harder to normalize and compare
// - Coupling domain to catalog naming conventions
```

✅ **CORRECT**: Using `University.Code` as the identifier

```csharp
// DO THIS:
var university = University.Create("BOSTON_U");  // ← Code, unique identifier
// Advantages:
// - Codes are inherently unique across the catalog
// - Easier to normalize (uppercase)
// - Domain logic remains independent of catalog naming changes
// - Aligns with Degree pattern (which uses .Code)
```

---

## Part 2: Catalog Model (ManageUniversities)

### The UniversityRecord Entity

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities;

/// <summary>
/// Represents an entry in the institutions catalog.
/// Owned by ManageUniversitiesDbContext; migrations managed by ManageUniversities.
/// </summary>
public class UniversityRecord
{
    /// <summary>
    /// The institutional code — e.g., "BOSTON_U", "MIT", "STANFORD".
    /// PRIMARY KEY: One code maps to exactly one catalog entry.
    /// Maps directly to University.Code in domain model.
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
    /// </summary>
    public static UniversityRecord Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("University code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("University name is required.", nameof(name));

        return new UniversityRecord
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
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

### UniversityRecord.Code Semantics

| Property                | Description                                                      |
| ----------------------- | ---------------------------------------------------------------- |
| **Database Role**       | PRIMARY KEY; unique constraint enforced                          |
| **Domain Value Object** | Maps directly to `University.Code`                               |
| **Format**              | Alphanumeric code; normalized to uppercase                       |
| **Mutability**          | Should NOT change after initial creation (immutable in practice) |
| **Constraint**          | Required; NOT NULL; unique across catalog                        |
| **Normalization**       | Uppercase; whitespace trimmed                                    |

### UniversityRecord.Name Semantics

| Property             | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **Database Role**    | Descriptive field; NOT a unique constraint                   |
| **Domain Relevance** | NOT stored in domain aggregates (informational only)         |
| **Format**           | Full institutional name; free-form string                    |
| **Mutability**       | CAN change if institution rebrands                           |
| **Examples**         | "Boston University", "Massachusetts Institute of Technology" |
| **Historical**       | Historical qualifications retain original Code, not Name     |

### UniversityRecord.IsActive Semantics

| Property               | Description                                     |
| ---------------------- | ----------------------------------------------- |
| **Purpose**            | Logical deactivation without deletion           |
| **New Qualifications** | Rejected if IsActive=false                      |
| **Historical Data**    | Not affected by deactivation; remains queryable |
| **Use Case**           | Retiring institutions, mergers, closures        |

---

## Part 3: Identity Mapping

### The Canonical Mapping

```
UniversityRecord.Code ←→ University.Code
    "BOSTON_U"       ←→ "BOSTON_U"
    "MIT"            ←→ "MIT"
    "STANFORD"       ←→ "STANFORD"
```

### Properties of the Mapping

| Property          | Guarantee                                             |
| ----------------- | ----------------------------------------------------- |
| **Directional**   | Catalog code → Domain value object (unidirectional)   |
| **Deterministic** | Same input always produces the same output            |
| **Validated**     | Catalog entry must exist and be active before mapping |
| **Immutable**     | Both sides preserve the code forever; never changes   |
| **Normalized**    | Both sides normalize to uppercase                     |
| **Unique**        | One catalog entry per code; one value object per code |

### Not Included in Mapping

❌ `UniversityRecord.Name` → NOT stored in domain aggregates
❌ `UniversityRecord.IsActive` → Checked during resolution, not stored in aggregates
❌ Other metadata → Only Code is persisted in qualifications

---

## Part 4: Resolution Pattern

### When Resolution Happens

**Trigger**: A downstream slice (e.g., RegisterAcademic) needs to record a qualification with a university.

**Input**: A university code (string), typically from a user request or command.

**Output**: Either:

- ✅ A `University` value object ready for use in domain aggregates
- ❌ An error: "UniversityNotFound" or "UniversityNotActive"

### Resolution Flow (Query-Based)

```csharp
// Step 1: Downstream slice constructs a GetUniversityByCodeQuery
var getUniversityQuery = new GetUniversityByCodeQuery("BOSTON_U");

// Step 2: ManageUniversities.GetUniversityByCodeQueryHandler executes
// (Handler implemented in EP-1-3; contract defined below)
var universityDto = await mediator.Send(getUniversityQuery, cancellationToken);

// Step 3: Validation
if (!universityDto.IsFound)
{
    // Catalog entry does not exist
    return Error.Create(
        "UniversityNotFound",
        $"University code '{universityDto.Code}' is not in the catalog");
}

if (!universityDto.IsActive)
{
    // Catalog entry exists but is inactive
    return Error.Create(
        "UniversityNotActive",
        $"University code '{universityDto.Code}' is no longer accepting qualifications");
}

// Step 4: Create domain value object from resolved code
var university = University.Create(universityDto.Code);
// university is now SAFE for use in domain aggregates

// Step 5: Use in domain aggregate
var qualification = AcademicQualification.Create(
    degree.Code,
    university.Code,  // ← Use University.Code, not universityDto.Name
    obtainedDate
);
```

### The GetUniversityByCodeQuery Contract

```csharp
namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared.Queries;

/// <summary>
/// Query to resolve a university catalog entry by code.
/// Called by downstream slices to fetch catalog data before creating domain value objects.
/// </summary>
public record GetUniversityByCodeQuery(string Code) : IRequest<GetUniversityByCodeResponse>;

/// <summary>
/// Response from GetUniversityByCodeQuery.
/// Always returns a response object (never throws exceptions).
/// Allows callers to distinguish "not found" from "inactive".
/// </summary>
public record GetUniversityByCodeResponse(
    /// <summary>True if the code exists in the catalog.</summary>
    bool IsFound,

    /// <summary>The institutional code (if found; otherwise null).</summary>
    string? Code,

    /// <summary>The institutional name (if found; otherwise null).</summary>
    string? Name,

    /// <summary>Whether this institution is available for new qualifications.</summary>
    bool IsActive
);

/// <summary>
/// Handler implementation (deferred to EP-1-3).
/// Must query UniversityRecord by Code and return the response.
/// </summary>
public class GetUniversityByCodeQueryHandler : IRequestHandler<GetUniversityByCodeQuery, GetUniversityByCodeResponse>
{
    private readonly ManageUniversitiesDbContext _context;

    public GetUniversityByCodeQueryHandler(ManageUniversitiesDbContext context)
    {
        _context = context;
    }

    public async Task<GetUniversityByCodeResponse> Handle(
        GetUniversityByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var record = await _context.Universities
            .FirstOrDefaultAsync(u => u.Code == request.Code, cancellationToken);

        if (record is null)
        {
            return new GetUniversityByCodeResponse(IsFound: false, Code: null, Name: null, IsActive: false);
        }

        return new GetUniversityByCodeResponse(
            IsFound: true,
            Code: record.Code,
            Name: record.Name,
            IsActive: record.IsActive);
    }
}
```

**Key Points**:

- ✅ Handler returns response object (never throws for "not found")
- ✅ IsFound flag allows distinguishing "not in catalog" from "inactive"
- ✅ Both IsFound and IsActive are checked by caller
- ✅ No business logic in the response; just data transfer

---

## Part 5: Error Handling

### Possible Errors

| Error                    | Cause                                | Resolution                                |
| ------------------------ | ------------------------------------ | ----------------------------------------- |
| `UniversityCodeNotFound` | Code not in `UniversityRecord` table | Validate against catalog before recording |
| `UniversityNotActive`    | Code exists but `IsActive=false`     | Ask user to select an active institution  |
| `InvalidUniversityCode`  | Code is null, empty, or malformed    | Validate input format before query        |

### Error Handling Pattern (RegisterAcademic Example)

```csharp
public class RecordQualificationCommandHandler : IRequestHandler<RecordQualificationCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly RegisterAcademicDbContext _context;

    public async Task<Result> Handle(RecordQualificationCommand cmd, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(cmd.UniversityCode))
            return Error.Create("InvalidUniversityCode", "University code is required");

        // Load Academic aggregate
        var academic = await _context.Academics.FirstOrDefaultAsync(
            a => a.EmpNr == cmd.EmpNr, ct);
        if (academic is null)
            return Error.Create("AcademicNotFound", $"Employee {cmd.EmpNr} not found");

        // Resolve Degree from ManageDegrees
        var degreeResult = await ResolveDegreeAsync(cmd.DegreeCode, ct);
        if (!degreeResult.IsSuccess)
            return degreeResult;

        // RESOLVE UNIVERSITY FROM CATALOG ← This is the pattern
        var universityQuery = new GetUniversityByCodeQuery(cmd.UniversityCode);
        var universityDto = await _mediator.Send(universityQuery, ct);

        // Validate university is available
        if (!universityDto.IsFound)
        {
            return Error.Create(
                "UniversityNotFound",
                $"University code '{cmd.UniversityCode}' is not in the institutions catalog. " +
                $"Please verify the code and try again.");
        }

        if (!universityDto.IsActive)
        {
            return Error.Create(
                "UniversityNotActive",
                $"University code '{cmd.UniversityCode}' is no longer available. " +
                $"This institution is not accepting new qualifications.");
        }

        // Create domain value objects
        var degree = Degree.Create(degreeResult.Data.Code);
        var university = University.Create(universityDto.Code);

        // Create and add qualification to aggregate
        var qualification = AcademicQualification.Create(
            degree.Code,
            university.Code,
            cmd.ObtainedDate);
        academic.AddQualification(qualification);

        // Persist
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task<Result<DegreeDto>> ResolveDegreeAsync(string code, CancellationToken ct)
    {
        var query = new GetDegreeByCodeQuery(code);
        var dto = await _mediator.Send(query, ct);
        if (!dto.IsFound)
            return Error.Create("DegreeNotFound", $"Degree code '{code}' not found");
        return Result.Success(dto);
    }
}
```

---

## Part 6: Naming Conventions for Downstream Slices

### Parameter Names

| Usage               | Convention        | Example                                                          |
| ------------------- | ----------------- | ---------------------------------------------------------------- |
| Command parameter   | `UniversityCode`  | `new RecordQualificationCommand { UniversityCode = "BOSTON_U" }` |
| Query parameter     | `Code`            | `new GetUniversityByCodeQuery(code: "MIT")`                      |
| Domain value object | `university.Code` | `qualification.UniversityCode = university.Code`                 |

### What NOT to Do

❌ **Don't use `University.Name` as identifier**

```csharp
// WRONG:
var university = University.Create("Boston University");
```

❌ **Don't store UniversityRecord directly in domain aggregates**

```csharp
// WRONG:
public class AcademicQualification
{
    public UniversityRecord University { get; }  // ← WRONG
}

// CORRECT:
public class AcademicQualification
{
    public string UniversityCode { get; }  // ← Just the code
}
```

❌ **Don't use UniversityRecord in database queries from domain logic**

```csharp
// WRONG:
var university = _context.Universities.FirstOrDefault(u => u.Code == code);

// CORRECT:
var query = new GetUniversityByCodeQuery(code);
var university = await _mediator.Send(query);
```

---

## Part 7: Historical Data & Deactivation

### Scenario: University Deactivates

**Timeline**:

1. 2025-05-15: Academic records qualification from Boston University
2. 2026-01-01: Boston University closes; admin sets `IsActive=false`
3. 2026-02-01: New qualifications from Boston University are rejected

**Result**:

| Query                                   | Response                                 |
| --------------------------------------- | ---------------------------------------- |
| GetUniversityByCodeQuery("BOSTON_U")    | `IsFound=true`, `IsActive=false`         |
| AcademicQualification for this academic | Still has `UniversityCode="BOSTON_U"`    |
| Historical data                         | Unaffected; shows original qualification |

**Why This Works**:

- ✅ Database stores Code, not Name → immutable historical reference
- ✅ Domain value object is immutable → can't retroactively change it
- ✅ Query respects IsActive flag → prevents new qualifications
- ✅ Separation of concerns: "What was the university?" vs. "Is it still active?"

---

## Part 8: Contract Verification Checklist

**For Phase 0 Step 6 Sign-Off**:

- [ ] University.Code exists in Shared Kernel (not University.Name)
- [ ] AcademicQualification stores UniversityCode (not UniversityName)
- [ ] UniversityRecord.Code is documented as primary key
- [ ] GetUniversityByCodeQuery contract is specified
- [ ] Resolution pattern is documented with examples
- [ ] Error handling (NotFound, NotActive) is specified
- [ ] Naming conventions are explicit (use Code, not Name)
- [ ] Historical data preservation is addressed
- [ ] Deactivation pattern (IsActive flag) is defined
- [ ] Downstream slices have clear guidance (see Part 4)
- [ ] No ambiguity between University.Code and UniversityRecord.Code (they're the same)

**For EP-1-3 Handoff**:

- [ ] UniversityRecord.Create factory enforces invariants
- [ ] GetUniversityByCodeQueryHandler returns correct response format
- [ ] Uniqueness constraints prevent duplicate codes
- [ ] IsActive flag can be toggled without deleting data
- [ ] Seeded universities are active on startup
- [ ] Query never throws; always returns response object

---

## References

- [EP-1-3 Handoff Notes](./EP-1-3-HANDOFF-NOTES.md)
- [University Integration Example](../../../models/workflows/university-integration-example.md)
- [Shared Kernel Domain Model](../../SharedKernel/Foundation/Domain/University.cs)
- [Refactoring Plan — Phase 0 Step 5](../../../models/workflows/academia-refactoring-plan.md)
