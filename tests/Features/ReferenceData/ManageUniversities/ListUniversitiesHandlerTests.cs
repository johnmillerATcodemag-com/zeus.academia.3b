using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class ListUniversitiesHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsStableSortedUniversities()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Universities.AddRange(
      UniversityRecord.Create("STANFORD", "Stanford University"),
      UniversityRecord.Create("MIT", "Massachusetts Institute of Technology"),
      UniversityRecord.Create("BOSTON_U", "Boston University"));
    await dbContext.SaveChangesAsync();

    var handler = new ListUniversitiesHandler(dbContext);

    var response = await handler.Handle(new ListUniversitiesQuery(), CancellationToken.None);

    Assert.Equal(3, response.Count);
    Assert.Equal(["BOSTON_U", "MIT", "STANFORD"], response.Select(x => x.Code).ToArray());
    Assert.Equal("Boston University", response[0].Name);
  }

  private static ManageUniversitiesDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseInMemoryDatabase($"ManageUniversitiesListTests-{Guid.NewGuid():N}")
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}