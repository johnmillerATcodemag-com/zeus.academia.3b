---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-24-project-overview-zeus-academia"
prompt: |
  Submit create-technology-instructions.prompt.md with arguments:
  technology_name: FluentValidation
  technology_category: validation
  primary_language: C#
  project_context: Request validation in MediatR pipeline
  version_target: 11.0+
started: "2026-02-24T01:15:00Z"
ended: "2026-02-24T01:25:00Z"
task_durations:
  - task: "analyze validation patterns"
    duration: "00:03:00"
  - task: "document validation rules"
    duration: "00:05:00"
  - task: "create examples"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/02/24/2026-02-24-project-overview-zeus-academia/conversation.md"
source: ".github/prompts/create-technology-instructions.prompt.md"
name: "FluentValidation Standards"
description: "FluentValidation standards for command/query validation in feature-domain folders"
applyTo: "src/**/*Validator.cs"
tags: [fluentvalidation, validation, backend, csharp, mediatr]
---

# FluentValidation Implementation Standards

**Role**: Request validation in MediatR CQRS pipeline
**Version**: 11.0+
**Language**: C#

## Core Principles

- **One Validator Per Request**: Each command/query has its own validator
- **Fail Fast**: Stop on first failure (default) or collect all errors
- **Declarative**: Chain validation rules fluently
- **Async Support**: Use async validators for database checks
- **Reusability**: Extract common rules to shared validators
- **Registration gate**: A validator must be registered in DI or the MediatR validation pipeline before the slice is considered complete; a validator file alone is not enough
- **Single-source-of-truth rule**: Do not duplicate the same normalization, allowed-values, or range logic in both the validator and the handler or mapping layer

## File Organization

- `src/features/<Feature>/<UseCase>/` - Keep the validator with the request it protects
- `src/features/<Feature>/Shared/Validation/` - Feature-scoped validators reused by multiple use-cases in one feature domain
- `src/shared/Validation/` - Cross-cutting validators and reusable rules
- Naming: `<Command>Validator.cs`, `<Query>Validator.cs`
- Location: Same folder as the command/query unless multiple validators in the same use-case justify a `Validation/` subfolder

## Standard Patterns

### Basic Command Validator

```csharp
// src/features/Students/CreateStudent/CreateStudentCommandValidator.cs
using FluentValidation;
using Zeus.Academia.Features.Students.CreateStudent;

namespace Zeus.Academia.Features.Students.CreateStudent;

public sealed class CreateStudentCommandValidator
    : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Date of birth is not valid");
    }
}
```

### Validator with Conditional Rules

```csharp
// src/features/Enrollments/EnrollStudent/EnrollStudentCommandValidator.cs
using FluentValidation;
using Zeus.Academia.Features.Enrollments.EnrollStudent;

namespace Zeus.Academia.Features.Enrollments.EnrollStudent;

public sealed class EnrollStudentCommandValidator
    : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .Must(BeValidGuid).WithMessage("Student ID must be a valid GUID");

        RuleFor(x => x.CourseId)
            .NotEmpty()
            .Must(BeValidGuid).WithMessage("Course ID must be a valid GUID");

        RuleFor(x => x.EnrollmentDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Enrollment date cannot be in the past");

        // Conditional validation
        When(x => x.PaymentMethod == PaymentMethod.Installment, () =>
        {
            RuleFor(x => x.InstallmentCount)
                .NotNull().WithMessage("Installment count is required for installment payments")
                .GreaterThan(0).WithMessage("Installment count must be greater than 0")
                .LessThanOrEqualTo(12).WithMessage("Maximum 12 installments allowed");

            RuleFor(x => x.DownPaymentAmount)
                .NotNull()
                .GreaterThan(0).WithMessage("Down payment is required for installment payments");
        });

        When(x => x.PaymentMethod == PaymentMethod.FullPayment, () =>
        {
            RuleFor(x => x.TotalAmount)
                .NotNull()
                .GreaterThan(0);
        });
    }

    private static bool BeValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
}
```

### Validator with Async Rules

```csharp
// src/features/Students/UpdateStudent/UpdateStudentCommandValidator.cs
using FluentValidation;
using Zeus.Academia.Features.Students.UpdateStudent;
using Zeus.Academia.Domain.Repositories;

namespace Zeus.Academia.Features.Students.UpdateStudent;

public sealed class UpdateStudentCommandValidator
    : AbstractValidator<UpdateStudentCommand>
{
    private readonly IStudentRepository _studentRepository;

    public UpdateStudentCommandValidator(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;

        RuleFor(x => x.Id)
            .NotEmpty()
            .MustAsync(StudentExists)
                .WithMessage("Student not found");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(BeUniqueEmail)
                .WithMessage("Email is already in use");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);
    }

    private async Task<bool> StudentExists(
        string studentId,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            studentId,
            cancellationToken);
        return student is not null;
    }

    private async Task<bool> BeUniqueEmail(
        UpdateStudentCommand command,
        string email,
        CancellationToken cancellationToken)
    {
        var existing = await _studentRepository.GetByEmailAsync(
            email,
            cancellationToken);

        // Email is unique if no one has it, or the current student has it
        return existing is null || existing.Id == command.Id;
    }
}
```

### Nested Object Validation

```csharp
// src/features/Courses/CreateCourse/CreateCourseCommand.cs
public sealed record CreateCourseCommand(
    string Title,
    string Code,
    CourseSchedule Schedule,
    List<CourseModule> Modules
) : IRequest<CourseDto>;

public sealed record CourseSchedule(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Location
);

public sealed record CourseModule(
    string Title,
    string Description,
    int OrderIndex
);

// src/features/Courses/CreateCourse/CreateCourseCommandValidator.cs
public sealed class CreateCourseCommandValidator
    : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^[A-Z]{3}\d{3}$")
                .WithMessage("Course code must be 3 letters followed by 3 digits (e.g., CSC101)");

        RuleFor(x => x.Schedule)
            .NotNull()
            .SetValidator(new CourseScheduleValidator());

        RuleFor(x => x.Modules)
            .NotEmpty().WithMessage("At least one module is required")
            .Must(HaveUniqueOrderIndices).WithMessage("Module order indices must be unique");

        RuleForEach(x => x.Modules)
            .SetValidator(new CourseModuleValidator());
    }

    private static bool HaveUniqueOrderIndices(List<CourseModule> modules)
    {
        return modules.Select(m => m.OrderIndex).Distinct().Count() == modules.Count;
    }
}

public sealed class CourseScheduleValidator : AbstractValidator<CourseSchedule>
{
    public CourseScheduleValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum();

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time");

        RuleFor(x => x.EndTime)
            .NotEmpty();

        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public sealed class CourseModuleValidator : AbstractValidator<CourseModule>
{
    public CourseModuleValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0);
    }
}
```

### Common Validation Rules

```csharp
// src/shared/Validation/CommonRules.cs
using FluentValidation;

namespace Zeus.Academia.Shared.Validation;

public static class CommonRules
{
    public static IRuleBuilderOptions<T, string> IsValidGuid<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage("'{PropertyName}' must be a valid GUID");
    }

    public static IRuleBuilderOptions<T, string> IsAlphanumeric<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^[a-zA-Z0-9]+$")
            .WithMessage("'{PropertyName}' must contain only letters and numbers");
    }

    public static IRuleBuilderOptions<T, decimal> IsPositiveAmount<T>(
        this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("'{PropertyName}' must be greater than 0")
            .PrecisionScale(10, 2, false)
            .WithMessage("'{PropertyName}' must have at most 2 decimal places");
    }
}

// Usage
public class ExampleValidator : AbstractValidator<ExampleCommand>
{
    public ExampleValidator()
    {
        RuleFor(x => x.Id).IsValidGuid();
        RuleFor(x => x.Code).IsAlphanumeric();
        RuleFor(x => x.Amount).IsPositiveAmount();
    }
}
```

### Collection Validation

```csharp
public sealed class BatchEnrollCommandValidator
    : AbstractValidator<BatchEnrollCommand>
{
    public BatchEnrollCommandValidator()
    {
        RuleFor(x => x.Enrollments)
            .NotEmpty().WithMessage("At least one enrollment is required")
            .Must(x => x.Count <= 100).WithMessage("Maximum 100 enrollments per batch");

        RuleForEach(x => x.Enrollments)
            .ChildRules(enrollment =>
            {
                enrollment.RuleFor(x => x.StudentId).IsValidGuid();
                enrollment.RuleFor(x => x.CourseId).IsValidGuid();
            });

        RuleFor(x => x.Enrollments)
            .Must(HaveUniqueStudentCoursePairs)
            .WithMessage("Duplicate student-course combinations found");
    }

    private static bool HaveUniqueStudentCoursePairs(
        List<EnrollmentRequest> enrollments)
    {
        return enrollments
            .Select(e => (e.StudentId, e.CourseId))
            .Distinct()
            .Count() == enrollments.Count;
    }
}
```

## Error Message Customization

```csharp
public sealed class StudentValidator : AbstractValidator<CreateStudentCommand>
{
    public StudentValidator()
    {
        // Default message
        RuleFor(x => x.FirstName).NotEmpty();

        // Custom message
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required");

        // Message with property name
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("'{PropertyName}' is not a valid email address");

        // Message with property value
        RuleFor(x => x.Age)
            .InclusiveBetween(18, 100)
            .WithMessage("'{PropertyName}' must be between 18 and 100. You entered {PropertyValue}");

        // Message with error code
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("Student ID is required")
            .WithErrorCode("STUDENT_ID_REQUIRED");
    }
}
```

## Integration with MediatR

Pipeline behavior automatically validates (see MediatR instructions):

```csharp
// src/shared/Behaviors/ValidationBehavior.cs
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
```

## Performance Considerations

```csharp
// Cache expensive validators
private static readonly Regex EmailRegex = new(@"^[^@]+@[^@]+\.[^@]+$", RegexOptions.Compiled);

RuleFor(x => x.Email)
    .Must(email => EmailRegex.IsMatch(email))
    .WithMessage("Invalid email format");

// Limit async validator calls
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress() // Run cheap validation first
    .MustAsync(BeUniqueEmail) // Only run expensive check if email is valid
        .WithMessage("Email already exists");
```

## Required String Validation Semantics

When a string field treats whitespace-only values as missing input, do not rely on `NotEmpty()` alone.
Use an explicit whitespace-aware required rule first, then run format or allowed-values rules.

```csharp
RuleFor(x => x.Code)
    .Cascade(CascadeMode.Stop)
    .Must(code => !string.IsNullOrWhiteSpace(code))
        .WithMessage("Code is required.")
    .Must(code => RankCatalog.TryParseCode(code, out _))
        .WithMessage($"Code must be one of: {RankCatalog.AllowedCodesDisplay}.");
```

Testing expectation:

- Include `null`, `""`, and `"  "` inputs in validator tests when required-message semantics matter.
- Assert that whitespace-only input produces the required-field message, not a downstream format or allowed-values message.

## Validation Checklist

- [ ] One validator per command/query
- [ ] Validators inherit from `AbstractValidator<T>`
- [ ] Async validators use `MustAsync` for database checks
- [ ] Custom error messages for user-facing validations
- [ ] Conditional validation with `When()` where needed
- [ ] Nested object validation with `SetValidator()`
- [ ] Collection validation with `RuleForEach()`
- [ ] Extracted common rules to shared methods
- [ ] Performance considered for regex and async validators
- [ ] Required string fields that treat whitespace as missing input use explicit `IsNullOrWhiteSpace`-equivalent checks before downstream rules
- [ ] Validator tests cover `null`, empty, and whitespace-only required-field inputs where applicable

## Anti-Patterns

❌ Validation logic in handlers
✅ FluentValidation validators

❌ Mixing business rules with validation
✅ Validators check input validity, handlers enforce business rules

❌ Throwing custom exceptions from validators
✅ Return validation failures via FluentValidation

❌ Synchronous database checks (`Must` with sync call)
✅ Async database checks (`MustAsync`)

❌ Complex logic in `Must` predicates
✅ Extract to named methods for readability

❌ Validating multiple concerns in one rule
✅ Separate rules for separate concerns

❌ Generic error messages
✅ Specific, actionable error messages

❌ Using `NotEmpty()` alone when whitespace should trigger the required-field message
✅ Use a whitespace-aware required rule first, then downstream format/allowed-values rules

❌ Validators with mutable state
✅ Stateless validators (dependencies via DI only)
