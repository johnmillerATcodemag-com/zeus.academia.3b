using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class ListUniversitiesHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsStableSortedUniversities_WithExpectedResponseShape()
  {
    await using var dbContext = CreateInMemoryContext();

    var mit = UniversityRecord.Create("MIT", "Massachusetts Institute of Technology");
    var stanford = UniversityRecord.Create("STANFORD", "Stanford University");
    stanford.Deactivate();
    var harvard = UniversityRecord.Create("HARVARD", "Harvard University");

    dbContext.Universities.AddRange(mit, stanford, harvard);
    await dbContext.SaveChangesAsync();

    var handler = new ListUniversitiesHandler(dbContext);

    var response = await handler.Handle(new ListUniversitiesQuery(), CancellationToken.None);

    Assert.Equal(3, response.Count);
    Assert.Collection(
      response,
      item =>
      {
        Assert.Equal("HARVARD", item.Code);
        Assert.Equal("Harvard University", item.Name);
        Assert.True(item.IsActive);
      },
      item =>
      {
        Assert.Equal("MIT", item.Code);
        Assert.Equal("Massachusetts Institute of Technology", item.Name);
        Assert.True(item.IsActive);
      },
      item =>
      {
        Assert.Equal("STANFORD", item.Code);
        Assert.Equal("Stanford University", item.Name);
        Assert.False(item.IsActive);
      });
  }

  private static ManageUniversitiesDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseInMemoryDatabase($"ManageUniversitiesListTests-{Guid.NewGuid():N}")
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}