using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class ListUniversitiesHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsStableSortedCodesWithResponseShape()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Universities.AddRange(
      UniversityRecord.Create("mit", "Massachusetts Institute of Technology"),
      UniversityRecord.Create("stanford", "Stanford University"),
      UniversityRecord.Create("boston_u", "Boston University"));
    await dbContext.SaveChangesAsync();

    var handler = new ListUniversitiesHandler(dbContext);

    var response = await handler.Handle(new ListUniversitiesQuery(), CancellationToken.None);

    Assert.Equal(3, response.Count);
    Assert.Equal(["BOSTON_U", "MIT", "STANFORD"], response.Select(x => x.Code).ToArray());
    Assert.Equal("Boston University", response[0].Name);
    Assert.Equal("Massachusetts Institute of Technology", response[1].Name);
    Assert.Equal("Stanford University", response[2].Name);
    Assert.All(response, x => Assert.True(x.IsActive));
  }

  private static ManageUniversitiesDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseInMemoryDatabase($"ManageUniversitiesListTests-{Guid.NewGuid():N}")
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}