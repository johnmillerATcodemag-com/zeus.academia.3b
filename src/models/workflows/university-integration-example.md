---
ai_generated: true
model: "anthropic/claude-haiku-4.5@2024-10-22"
operator: "johnmillerATcodemag-com"
chat_id: "2026-08-24-university-identity-reconciliation"
prompt: |
  Create integration example showing how RegisterAcademic will consume
  the ManageUniversities university resolution contract
started: "2026-08-24T19:45:00Z"
ended: "2026-08-24T20:00:00Z"
task_durations:
  - task: "design RegisterAcademic handler flow with university resolution"
    duration: "00:08:00"
  - task: "document error handling and deactivation scenarios"
    duration: "00:05:00"
  - task: "create code examples and verification steps"
    duration: "00:02:00"
total_duration: "00:15:00"
ai_log: "ai-logs/2026/08/24/2026-08-24-university-identity-reconciliation/conversation.md"
source: "Phase 0 Step 6 Reconciliation — University Identity"
---

# University Integration Example — RegisterAcademic Use Case

**Purpose**: Show how downstream slices (RegisterAcademic, RecordDegreeObtained, etc.) will resolve university codes to domain value objects using the ManageUniversities contract.

**Pattern**: This is the CANONICAL integration pattern. All slices that reference universities must follow this flow.

---

## Scenario 1: Record a Successful Qualification

### Setup

- **Academic**: Employee 12345 (existing in system)
- **Input Command**: Record a Master's degree from Boston University obtained on 2020-05-15
- **Catalog State**: "BOSTON_U" is active

### Command

```csharp
public record RecordQualificationCommand(
    string EmpNr,
    string DegreeCode,
    string UniversityCode,
    DateOnly ObtainedDate) : IRequest<Result>;
```

### Request

```json
{
  "empNr": "12345",
  "degreeCode": "MAST",
  "universityCode": "BOSTON_U",
  "obtainedDate": "2020-05-15"
}
```

### Handler Implementation

```csharp
namespace Zeus.Academia.Features.RegisterAcademic.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageDegrees.Shared.Queries;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

public class RecordQualificationCommandHandler : IRequestHandler<RecordQualificationCommand, Result>
{
    private readonly IMediator _mediator;
    private readonly RegisterAcademicDbContext _context;

    public RecordQualificationCommandHandler(
        IMediator mediator,
        RegisterAcademicDbContext context)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(context);

        _mediator = mediator;
        _context = context;
    }

    public async Task<Result> Handle(RecordQualificationCommand cmd, CancellationToken ct)
    {
        // STEP 1: Input Validation
        // ───────────────────────
        if (string.IsNullOrWhiteSpace(cmd.EmpNr))
            return Error.Create("InvalidEmpNr", "Employee number is required");
        if (string.IsNullOrWhiteSpace(cmd.DegreeCode))
            return Error.Create("InvalidDegreeCode", "Degree code is required");
        if (string.IsNullOrWhiteSpace(cmd.UniversityCode))
            return Error.Create("InvalidUniversityCode", "University code is required");
        if (cmd.ObtainedDate == default)
            return Error.Create("InvalidDate", "Obtained date is required");

        // STEP 2: Load Academic Aggregate
        // ────────────────────────────────
        var academic = await _context.Academics
            .FirstOrDefaultAsync(a => a.EmpNr == cmd.EmpNr, ct);

        if (academic is null)
            return Error.Create(
                "AcademicNotFound",
                $"Employee {cmd.EmpNr} is not registered in the system");

        // STEP 3: Resolve Degree from Catalog
        // ────────────────────────────────────
        var degreeResult = await ResolveDegreeAsync(cmd.DegreeCode, ct);
        if (!degreeResult.IsSuccess)
            return degreeResult;
        var degreeDto = degreeResult.Data;

        // STEP 4: RESOLVE UNIVERSITY FROM CATALOG ← THE CRITICAL PATTERN
        // ──────────────────────────────────────────────────────────────
        var universityQuery = new GetUniversityByCodeQuery(cmd.UniversityCode);
        var universityDto = await _mediator.Send(universityQuery, ct);

        // 4a. Validate university is in catalog
        if (!universityDto.IsFound)
        {
            return Error.Create(
                "UniversityNotFound",
                $"University code '{cmd.UniversityCode}' is not in the institutions catalog. " +
                $"Please verify the code and select from the available universities.");
        }

        // 4b. Validate university is active
        if (!universityDto.IsActive)
        {
            return Error.Create(
                "UniversityNotActive",
                $"University code '{cmd.UniversityCode}' is no longer accepting new qualifications. " +
                $"This institution is inactive in the system.");
        }

        // STEP 5: Create Domain Value Objects
        // ────────────────────────────────────
        // Both use codes (not names), normalized to uppercase
        var degree = Degree.Create(degreeDto.Code);     // From ManageDegrees
        var university = University.Create(universityDto.Code);  // From ManageUniversities

        // STEP 6: Create Qualification in Domain Aggregate
        // ──────────────────────────────────────────────────
        // Note: AcademicQualification stores codes, not objects
        var qualification = AcademicQualification.Create(
            degree.Code,       // From resolved degree catalog
            university.Code,   // From resolved university catalog (NOT universityDto.Name)
            cmd.ObtainedDate
        );

        // STEP 7: Add Qualification to Aggregate
        // ──────────────────────────────────────
        academic.AddQualification(qualification);

        // STEP 8: Persist
        // ───────────────
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // Handle database constraint violations
            if (ex.InnerException?.Message.Contains("UNIQUE") ?? false)
            {
                return Error.Create(
                    "QualificationAlreadyRecorded",
                    $"This qualification is already recorded for employee {cmd.EmpNr}");
            }

            throw;  // Re-throw if we don't recognize the error
        }

        // STEP 9: Return Success
        // ──────────────────────
        return Result.Success();
    }

    /// <summary>
    /// Helper: Resolve degree from ManageDegrees catalog.
    /// Follows the same pattern as university resolution.
    /// </summary>
    private async Task<Result<GetDegreeByCodeResponse>> ResolveDegreeAsync(
        string code,
        CancellationToken ct)
    {
        var query = new GetDegreeByCodeQuery(code);
        var dto = await _mediator.Send(query, ct);

        if (!dto.IsFound)
        {
            return Error.Create(
                "DegreeNotFound",
                $"Degree code '{code}' is not in the degrees catalog");
        }

        if (!dto.IsActive)
        {
            return Error.Create(
                "DegreeNotActive",
                $"Degree code '{code}' is no longer available");
        }

        return Result.Success(dto);
    }
}
```

### Expected Result

**Status**: 200 OK

```json
{
  "success": true,
  "message": "Qualification recorded successfully"
}
```

**Database State**:

| Table                  | EmpNr | DegreeCode | UniversityCode | ObtainedDate |
| ---------------------- | ----- | ---------- | -------------- | ------------ |
| AcademicQualifications | 12345 | MAST       | BOSTON_U       | 2020-05-15   |

---

## Scenario 2: University NOT in Catalog

### Setup

- **Input Command**: Record a qualification from university code "UNKNOWN_U"
- **Catalog State**: "UNKNOWN_U" does not exist in catalog

### Request

```json
{
  "empNr": "12345",
  "degreeCode": "MAST",
  "universityCode": "UNKNOWN_U",
  "obtainedDate": "2020-05-15"
}
```

### Handler Flow

```
STEP 1-3: Input validation and degree resolution ✅
STEP 4: Query GetUniversityByCodeQuery("UNKNOWN_U")
        → Response: IsFound=false, Code=null, Name=null, IsActive=false
STEP 4a: Check IsFound → false ❌
STEP 4b: Return error "UniversityNotFound"
```

### Expected Result

**Status**: 400 Bad Request

```json
{
  "success": false,
  "errors": {
    "code": "UniversityNotFound",
    "message": "University code 'UNKNOWN_U' is not in the institutions catalog. Please verify the code and select from the available universities."
  }
}
```

**Database State**: No change. Qualification is NOT recorded.

---

## Scenario 3: University Inactive

### Setup

- **Input Command**: Record a qualification from "CLOSED_U"
- **Catalog State**: "CLOSED_U" exists but IsActive=false

### Request

```json
{
  "empNr": "12345",
  "degreeCode": "MAST",
  "universityCode": "CLOSED_U",
  "obtainedDate": "2020-05-15"
}
```

### Handler Flow

```
STEP 1-3: Input validation and degree resolution ✅
STEP 4: Query GetUniversityByCodeQuery("CLOSED_U")
        → Response: IsFound=true, Code="CLOSED_U", Name="Closed University", IsActive=false
STEP 4a: Check IsFound → true ✅
STEP 4b: Check IsActive → false ❌
STEP 4b: Return error "UniversityNotActive"
```

### Expected Result

**Status**: 400 Bad Request

```json
{
  "success": false,
  "errors": {
    "code": "UniversityNotActive",
    "message": "University code 'CLOSED_U' is no longer accepting new qualifications. This institution is inactive in the system."
  }
}
```

**Database State**: No change. Qualification is NOT recorded.

**Important**: This does NOT affect historical qualifications from "CLOSED_U" recorded before deactivation.

---

## Scenario 4: Historical Qualifications with Deactivated University

### Timeline

**2025-05-15**: Employee 12345 records qualification from BOSTON_U (IsActive=true)

- ✅ Operation succeeds
- AcademicQualifications now contains: (12345, MAST, BOSTON_U, 2025-05-15)

**2026-01-01**: Admin deactivates BOSTON_U (IsActive=false)

- UpdateUniversityCommand("BOSTON_U") { IsActive = false }

**2026-02-01**: Another admin tries to record qualification from BOSTON_U

- ❌ Operation fails with "UniversityNotActive"
- GetUniversityByCodeQuery("BOSTON_U") returns IsActive=false

**2026-03-01**: Query historical qualifications for employee 12345

- ✅ Query succeeds, returns qualification from BOSTON_U
- Database shows: (12345, MAST, BOSTON_U, 2025-05-15)

### Why This Works

```csharp
// Database storage (immutable)
AcademicQualifications row:
{
  EmpNr: "12345",
  DegreeCode: "MAST",
  UniversityCode: "BOSTON_U",  // ← Stored as CODE, not name
  ObtainedDate: "2025-05-15"
}

// Domain value object
var qualification = AcademicQualification.Create(
    "MAST",
    "BOSTON_U",  // ← Immutable value object
    new DateOnly(2025, 5, 15)
);

// Catalog entry
UniversityRecord {
  Code: "BOSTON_U",
  Name: "Boston University",
  IsActive: false  // ← Can change
}

// Key insight:
// - Database stores the CODE, not the NAME
// - Domain value object is immutable
// - Catalog entry can deactivate without affecting historical data
// - Query "was this qualification from BOSTON_U?" always returns YES
// - Query "can I add new qualifications from BOSTON_U?" returns NO
```

---

## Scenario 5: Handling University Code Case Variations

### Setup

- **Input Command**: Record qualification with code "boston_u" (lowercase)
- **Catalog State**: "BOSTON_U" (uppercase) is in catalog

### Handler Flow

```
STEP 4: Query GetUniversityByCodeQuery("boston_u")
        → Handler normalizes to "BOSTON_U"
        → Finds record with Code="BOSTON_U"
        → Response: IsFound=true, Code="BOSTON_U", IsActive=true
STEP 5: Create University value object
        → University.Create("BOSTON_U") normalizes input
        → Returns: University with Code="BOSTON_U"
STEP 6: Create qualification
        → AcademicQualification.Create(..., "BOSTON_U", ...)
        → Stored in database as "BOSTON_U"
```

### Expected Result

**Status**: 200 OK

**Database State**:

| UniversityCode |
| -------------- |
| BOSTON_U       |

**Key Point**: Both the query handler and the value object normalize to uppercase, ensuring consistency.

---

## Integration Checklist for RegisterAcademic

When implementing RecordQualificationCommand and its handler:

- [ ] Inject `IMediator` for cross-feature queries
- [ ] Construct `GetUniversityByCodeQuery(cmd.UniversityCode)`
- [ ] Send query via mediator (do NOT inject ManageUniversitiesDbContext directly)
- [ ] Check `IsFound` flag first
- [ ] Check `IsActive` flag second
- [ ] Return error immediately if either check fails (do NOT try to create value object)
- [ ] Only create `University.Create(universityDto.Code)` after both checks pass
- [ ] Store `university.Code` in the aggregate, not `universityDto.Name`
- [ ] Do not inject `ManageUniversitiesDbContext` or reference `UniversityRecord` directly
- [ ] Handle `DbUpdateException` for duplicate qualifications
- [ ] Never throw exceptions for validation failures; return `Result.Failure(...)`

---

## Testing Strategy

### Unit Tests (RegisterAcademic)

**Test 1**: Record qualification with valid, active university

- Mock `GetUniversityByCodeQuery` to return IsFound=true, IsActive=true
- Assert qualification is recorded
- Assert AcademicQualification.UniversityCode == "BOSTON_U"

**Test 2**: Reject qualification with unknown university

- Mock `GetUniversityByCodeQuery` to return IsFound=false
- Assert handler returns error "UniversityNotFound"
- Assert qualification is NOT recorded

**Test 3**: Reject qualification with inactive university

- Mock `GetUniversityByCodeQuery` to return IsFound=true, IsActive=false
- Assert handler returns error "UniversityNotActive"
- Assert qualification is NOT recorded

**Test 4**: Handle case-insensitive university code

- Input: "boston_u" (lowercase)
- Mock returns normalized: "BOSTON_U"
- Assert stored as uppercase

**Contract shape**: unknown or malformed codes return `IsFound=false` with `Code=null` and `Name=null`; inactive records return `IsFound=true` with `IsActive=false`.

### Integration Tests (RegisterAcademic + ManageUniversities)

**Test 1**: Full flow with real database

- Set up Universities catalog with BOSTON_U (active)
- Record qualification
- Verify stored in AcademicQualifications

**Test 2**: Full flow with inactive university

- Set up Universities catalog with CLOSED_U (inactive)
- Attempt to record qualification
- Assert failure and no database change

**Test 3**: Historical data preservation

- Record qualification with BOSTON_U (active)
- Deactivate BOSTON_U
- Verify historical qualification is still queryable
- Verify new qualifications from BOSTON_U are rejected

---

## Common Mistakes to Avoid

❌ **Don't**:

```csharp
// WRONG: Storing the full UniversityRecord object
var qualification = AcademicQualification.Create(
    degree.Code,
    universityDto.Name,  // ← WRONG: Using name instead of code
    cmd.ObtainedDate
);

// WRONG: Injecting ManageUniversitiesDbContext directly
public RecordQualificationCommandHandler(ManageUniversitiesDbContext context)
{
    // ← WRONG: Creates hard coupling to another feature's private context
}

// WRONG: Skipping IsActive check
if (universityDto.IsFound)  // ← Only checks IsFound, not IsActive
{
    // ... proceed to create qualification
}

// WRONG: Throwing exception instead of returning error
throw new Exception($"University {code} not found");  // ← Use Result.Failure instead
```

✅ **Do**:

```csharp
// CORRECT: Using the code
var qualification = AcademicQualification.Create(
    degree.Code,
    university.Code,  // ← Use University.Code from value object
    cmd.ObtainedDate
);

// CORRECT: Using IMediator for cross-feature queries
public RecordQualificationCommandHandler(
    IMediator mediator,
    RegisterAcademicDbContext context)
{
    // ← Clean boundary; no direct dependency on ManageUniversities
}

// CORRECT: Checking both flags
if (!universityDto.IsFound)
    return Error.Create("UniversityNotFound", ...);
if (!universityDto.IsActive)
    return Error.Create("UniversityNotActive", ...);

// CORRECT: Using Result for validation
return Error.Create("UniversityNotFound", ...");
```

---

## References

- [University Resolution Contract](../../features/ReferenceData/ManageUniversities/UNIVERSITY_RESOLUTION_CONTRACT.md)
- [EP-1-3 Handoff Notes](../../features/ReferenceData/ManageUniversities/EP-1-3-HANDOFF-NOTES.md)
- [Shared Kernel University Value Object](../../features/SharedKernel/Foundation/Domain/University.cs)
- [Shared Kernel AcademicQualification](../../features/SharedKernel/Foundation/Domain/AcademicQualification.cs)
