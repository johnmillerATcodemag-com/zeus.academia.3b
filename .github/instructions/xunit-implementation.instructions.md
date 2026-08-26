---
ai_generated: true
model: "anthropic/claude-sonnet-4.5@unknown"
operator: "johnmillerATcodemag-com"
chat_id: "2026-02-24-project-overview-zeus-academia"
prompt: |
  Submit create-technology-instructions.prompt.md with arguments:
  technology_name: xUnit
  technology_category: testing
  primary_language: C#
  project_context: Unit and integration testing for ASP.NET Core backend
  version_target: 2.6+
started: "2026-02-24T01:25:00Z"
ended: "2026-02-24T01:35:00Z"
task_durations:
  - task: "analyze testing patterns"
    duration: "00:03:00"
  - task: "document test standards"
    duration: "00:05:00"
  - task: "create examples"
    duration: "00:02:00"
total_duration: "00:10:00"
ai_log: "ai-logs/2026/02/24/2026-02-24-project-overview-zeus-academia/conversation.md"
source: ".github/prompts/create-technology-instructions.prompt.md"
name: "xUnit Testing Standards"
description: "xUnit testing standards for C# backend application"
applyTo: "tests/**/*.cs"
tags: [xunit, testing, csharp, backend, unit-tests, integration-tests]
---

# xUnit Testing Standards

**Role**: Unit and integration testing for ASP.NET Core backend
**Version**: 2.6+
**Language**: C#
**Target Coverage**: 80%+

## Core Principles

- **Arrange-Act-Assert**: Clear test structure
- **One Assertion Per Test**: Focus on single behavior
- **Descriptive Names**: Test name explains what/when/expected
- **Isolated Tests**: No dependencies between tests
- **Fast Execution**: Unit tests <100ms, integration tests <1s
- **No Silent Passes**: Missing infrastructure or setup errors MUST fail with actionable diagnostics, not skip via `return`

## Model Metadata and EF Core Test Safety

- Model verification tests SHOULD inspect the EF Core `IModel` via `DbContext.Model` directly rather than using `context.GetService<IDesignTimeModel>()` in ordinary unit tests.
- When a test asserts on keys, indexes, or check constraints, it should target the model metadata that the context actually exposes rather than a design-time service dependency.
- Schema or migration assertions MUST verify the intended model shape and emitted migration output, not only an in-memory database configuration. When the host applies migrations for the context, verification MUST also prove that EF discovers the migration and can apply it to SQL Server.
- Every persistence-bearing feature must include a provider-backed SQL Server integration suite in its feature test project. The suite must use `Microsoft.EntityFrameworkCore.SqlServer`, `SqlConnectionStringBuilder`, a unique test database name, `Database.MigrateAsync()`, and fresh DbContext instances for persistence read-back. A feature test project containing only InMemory tests is incomplete.

## Database Test Safety

- MUST isolate database-backed tests from shared environments by using a unique test-scoped database name even when a connection string is supplied.
- MUST preserve the server/instance/credentials from the environment variable and only override the catalog/database segment with a temporary test name.
- MUST NOT execute destructive setup such as `EnsureDeleted`/`EnsureCreated` against a shared or production database unless the database name is explicitly test-scoped and temporary.
- SHOULD use helpers such as `SqlConnectionStringBuilder` to mutate `Initial Catalog` safely and fail fast with actionable diagnostics when the target is unavailable.

```csharp
var builder = new SqlConnectionStringBuilder(connectionString);
builder.InitialCatalog = $"ZeusTests_{Guid.NewGuid():N}";
```

## Integration Resource Lifecycle

- Tests that create external resources (SQL databases, containers, queues, files, temp schemas) MUST clean them up in a `finally` block.
- SQL Server tests that use per-test database names MUST run best-effort `EnsureDeletedAsync()` teardown even when assertions fail.
- Cleanup failures SHOULD surface as warnings when possible, but must not hide the original assertion failure.
- EF Core InMemory tests may support fast handler tests, but they are not persistence evidence. Persistence-bearing slices require a SQL Server-backed migration or integration check; missing provider evidence is a review blocker.
- Integration test setup failures MUST fail the test run with actionable diagnostics; tests must not skip when SQL Server is unavailable. Test output or the verification handoff must report the provider-backed integration test count.

## Validator and Endpoint Contract Coverage

- Every new validator MUST include dedicated tests for required values, invalid ranges, invalid formats, and a success case.
- When a route advertises validation failures (for example `.ProducesValidationProblem()` or equivalent), tests MUST verify the endpoint returns a validation result for invalid numbers, ranges, and malformed input instead of leaking an unhandled exception.
- Validation tests MUST assert on stable failure messages or keys so the contract does not drift when rules are refactored.
- A validator without direct coverage is a review-blocking gap, even if the command handler and mapper compile successfully.
- A mapping or handler that duplicates normalization logic already centralized in a shared helper must be flagged by tests and review before merge.

## File Organization

- `tests/Zeus.Academia.UnitTests/` - Unit tests
- `tests/Zeus.Academia.IntegrationTests/` - Integration tests
- `tests/Zeus.Academia.Api.Tests/` - API endpoint tests
- Naming: `<ClassUnderTest>Tests.cs`
- Mirror source structure: `Application/Students/` → `UnitTests/Application/Students/`

## Standard Patterns

### Unit Test Structure

```csharp
// tests/UnitTests/Application/Students/CreateStudentCommandHandlerTests.cs
using FluentAssertions;
using NSubstitute;
using Xunit;
using Zeus.Academia.Application.Students.Commands;
using Zeus.Academia.Application.Students.Handlers;
using Zeus.Academia.Domain.Entities;
using Zeus.Academia.Domain.Repositories;

namespace Zeus.Academia.UnitTests.Application.Students;

public sealed class CreateStudentCommandHandlerTests
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateStudentCommandHandler _sut; // System Under Test

    public CreateStudentCommandHandlerTests()
    {
        _studentRepository = Substitute.For<IStudentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateStudentCommandHandler(
            _studentRepository,
            _unitOfWork,
            Substitute.For<ILogger<CreateStudentCommandHandler>>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesStudent()
    {
        // Arrange
        var command = new CreateStudentCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            new DateTime(2000, 1, 1));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");

        await _studentRepository.Received(1)
            .AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_AssignsId()
    {
        // Arrange
        var command = new CreateStudentCommand(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            new DateTime(1999, 5, 15));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Id, out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidFirstName_ThrowsValidationException(
        string? firstName)
    {
        // Arrange
        var command = new CreateStudentCommand(
            firstName!,
            "Doe",
            "john@example.com",
            new DateTime(2000, 1, 1));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*FirstName*");
    }
}
```

### Theory Tests with Data

```csharp
public sealed class StudentAgeCalculationTests
{
    [Theory]
    [InlineData("2000-01-01", 24)] // Born Jan 1, 2000, today is 2024
    [InlineData("2005-12-31", 18)]
    [InlineData("1990-06-15", 34)]
    public void CalculateAge_WithDifferentBirthDates_ReturnsCorrectAge(
        string birthDateString,
        int expectedAge)
    {
        // Arrange
        var birthDate = DateTime.Parse(birthDateString);
        var student = new Student { DateOfBirth = birthDate };
        var referenceDate = new DateTime(2024, 2, 24);

        // Act
        var age = student.CalculateAge(referenceDate);

        // Assert
        age.Should().Be(expectedAge);
    }

    public static IEnumerable<object[]> GetComplexTestData()
    {
        yield return new object[]
        {
            new CreateStudentCommand("John", "Doe", "john@example.com", new DateTime(2000, 1, 1)),
            true
        };
        yield return new object[]
        {
            new CreateStudentCommand("", "Doe", "john@example.com", new DateTime(2000, 1, 1)),
            false
        };
    }

    [Theory]
    [MemberData(nameof(GetComplexTestData))]
    public async Task Handle_WithVariousCommands_ReturnsExpectedResult(
        CreateStudentCommand command,
        bool expectedSuccess)
    {
        // Test implementation
    }
}
```

### Test Fixtures for Shared Setup

```csharp
// tests/UnitTests/Fixtures/StudentTestFixture.cs
using Zeus.Academia.Domain.Entities;

namespace Zeus.Academia.UnitTests.Fixtures;

public sealed class StudentTestFixture : IDisposable
{
    public IStudentRepository StudentRepository { get; }
    public IUnitOfWork UnitOfWork { get; }

    public StudentTestFixture()
    {
        StudentRepository = Substitute.For<IStudentRepository>();
        UnitOfWork = Substitute.For<IUnitOfWork>();
    }

    public Student CreateValidStudent()
    {
        return Student.Create(
            "Test",
            "Student",
            $"test.student.{Guid.NewGuid()}@example.com",
            new DateTime(2000, 1, 1));
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

// Usage
public sealed class StudentHandlerTests : IClassFixture<StudentTestFixture>
{
    private readonly StudentTestFixture _fixture;

    public StudentHandlerTests(StudentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestMethod()
    {
        var student = _fixture.CreateValidStudent();
        // Test using fixture
    }
}
```

### Integration Tests with WebApplicationFactory

```csharp
// tests/IntegrationTests/Api/StudentEndpointsTests.cs
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Zeus.Academia.Api;
using Zeus.Academia.Application.Students.Commands;

namespace Zeus.Academia.IntegrationTests.Api;

public sealed class StudentEndpointsTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StudentEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStudents_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/students");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/json");
    }

    [Fact]
    public async Task CreateStudent_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateStudentCommand(
            "Integration",
            "Test",
            "integration.test@example.com",
            new DateTime(2000, 1, 1));

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<StudentDto>();
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Integration");
    }

    [Fact]
    public async Task GetStudent_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/students/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

### Testing with Test Database

```csharp
// tests/IntegrationTests/Fixtures/TestDatabaseFixture.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zeus.Academia.Infrastructure.Persistence;

namespace Zeus.Academia.IntegrationTests.Fixtures;

public sealed class TestDatabaseFixture : IDisposable
{
    public AcademiaDbContext DbContext { get; }

    public TestDatabaseFixture()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var options = new DbContextOptionsBuilder<AcademiaDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .UseInternalServiceProvider(serviceProvider)
            .Options;

        DbContext = new AcademiaDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}

// Usage
public sealed class StudentRepositoryTests
    : IClassFixture<TestDatabaseFixture>
{
    private readonly AcademiaDbContext _dbContext;
    private readonly StudentRepository _sut;

    public StudentRepositoryTests(TestDatabaseFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _sut = new StudentRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsStudent()
    {
        // Arrange
        var student = new Student
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Test",
            LastName = "Student",
            Email = "test@example.com"
        };
        await _dbContext.Students.AddAsync(student);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(student.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(student.Id);
    }
}
```

### Mocking with NSubstitute

```csharp
// Simple mock
var repository = Substitute.For<IStudentRepository>();
repository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult<Student?>(testStudent));

// Verify calls
await repository.Received(1).AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
await repository.DidNotReceive().DeleteAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());

// Argument matching
await repository.Received().AddAsync(
    Arg.Is<Student>(s => s.Email == "test@example.com"),
    Arg.Any<CancellationToken>());

// Callback when method is called
repository.When(x => x.AddAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>()))
    .Do(callInfo =>
    {
        var student = callInfo.ArgAt<Student>(0);
        student.Id = Guid.NewGuid().ToString();
    });
```

## Test Naming Conventions

```csharp
// Pattern: MethodName_Scenario_ExpectedResult

[Fact]
public async Task Handle_WithValidCommand_CreatesStudent() { }

[Fact]
public async Task Handle_WhenStudentExists_ThrowsDuplicateException() { }

[Fact]
public async Task GetByEmail_WithNonExistentEmail_ReturnsNull() { }

[Fact]
public async Task EnrollStudent_WhenCourseIsFull_ReturnsFailureResult() { }
```

## FluentAssertions Usage

```csharp
// Basic assertions
result.Should().NotBeNull();
result.Should().Be(expected);
result.Should().BeEquivalentTo(expected);

// String assertions
name.Should().NotBeNullOrEmpty();
email.Should().Contain("@");
code.Should().MatchRegex(@"^[A-Z]{3}\d{3}$");

// Collection assertions
list.Should().HaveCount(3);
list.Should().Contain(item);
list.Should().BeInAscendingOrder(x => x.Name);
list.Should().OnlyContain(x => x.IsActive);

// Exception assertions
var act = () => sut.ThrowingMethod();
act.Should().Throw<InvalidOperationException>()
    .WithMessage("*validation*");

// Async exception assertions
var act = async() => await sut.ThrowingMethodAsync();
await act.Should().ThrowAsync<NotFoundException>();

// Object property assertions
student.Should().Match<Student>(s =>
    s.FirstName == "John" &&
    s.LastName == "Doe" &&
    s.Email.Contains("@"));
```

## Validation Checklist

- [ ] Tests follow Arrange-Act-Assert pattern
- [ ] Test names describe scenario and expected result
- [ ] One logical assertion per test
- [ ] Mocks configured before Act phase
- [ ] Async tests use `async Task` (not `async void`)
- [ ] CancellationToken passed to async methods
- [ ] Integration tests use test database or in-memory provider
- [ ] Expensive setup shared via fixtures
- [ ] Tests don't depend on execution order
- [ ] No hardcoded waits (`Task.Delay`)
- [ ] Public/shared parsing or mapping APIs touched by a change retain direct acceptance tests for valid inputs
- [ ] Invalid-input tests verify behavior and actionable diagnostics (for constrained codes, include allowed-value hints)
- [ ] Refactors do not remove the only direct test coverage of a public/shared API without adding equivalent replacement tests

## Anti-Patterns

❌ Multiple unrelated assertions in one test
✅ Split into focused tests

❌ Test names like `Test1`, `Test2`
✅ Descriptive names: `Handle_WithValidCommand_CreatesStudent`

❌ Testing implementation details
✅ Test behavior and outcomes

❌ Tests that depend on other tests
✅ Isolated, independent tests

❌ Hardcoded test data across many tests
✅ Test data builders or factories

❌ `async void` test methods
✅ `async Task` for async tests

❌ Generic `Assert.True` without message
✅ Specific assertions with FluentAssertions

❌ Production database in tests
✅ In-memory database or test containers

❌ Catching setup exceptions and returning from a test
✅ Fail with a clear assertion/message so verification cannot silently pass
