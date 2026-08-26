using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class AddUniversityHandlerTests
{
  [Fact]
  public async Task Handle_WhenCodeAllowed_PersistsUniversity()
  {
    await using var dbContext = CreateInMemoryContext();
    var handler = new AddUniversityHandler(dbContext);

    var response = await handler.Handle(new AddUniversityCommand("mit"), CancellationToken.None);

    Assert.Equal("MIT", response.Code);
    Assert.Equal("Massachusetts Institute of Technology", response.Name);
    Assert.True(response.IsActive);

    var persisted = await dbContext.Universities.SingleAsync(x => x.Code == "MIT");
    Assert.Equal("Massachusetts Institute of Technology", persisted.Name);
    Assert.True(persisted.IsActive);
  }

  [Fact]
  public async Task Handle_WhenDuplicateCodeExists_ThrowsUniversityConflictException_AndDoesNotDuplicatePersistence()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Universities.Add(UniversityRecord.Create("MIT", "Massachusetts Institute of Technology"));
    await dbContext.SaveChangesAsync();

    var handler = new AddUniversityHandler(dbContext);

    var exception = await Assert.ThrowsAsync<UniversityConflictException>(async () =>
      await handler.Handle(new AddUniversityCommand("MIT"), CancellationToken.None));

    Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
    Assert.Equal(1, await dbContext.Universities.CountAsync(x => x.Code == "MIT"));
  }

  private static ManageUniversitiesDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseInMemoryDatabase($"ManageUniversitiesTests-{Guid.NewGuid():N}")
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}