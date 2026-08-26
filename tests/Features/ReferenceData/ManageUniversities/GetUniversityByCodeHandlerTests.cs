using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class GetUniversityByCodeHandlerTests
{
  [Fact]
  public async Task Handle_WhenCodeMatchesIgnoringCaseAndWhitespace_ReturnsCanonicalRecord()
  {
    await using var dbContext = CreateInMemoryContext();
    dbContext.Universities.Add(UniversityRecord.Create("MIT", "Massachusetts Institute of Technology"));
    await dbContext.SaveChangesAsync();

    var response = await new GetUniversityByCodeHandler(dbContext)
      .Handle(new GetUniversityByCodeQuery(" mit "), CancellationToken.None);

    Assert.True(response.IsFound);
    Assert.Equal("MIT", response.Code);
    Assert.Equal("Massachusetts Institute of Technology", response.Name);
    Assert.True(response.IsActive);
  }

  [Theory]
  [InlineData("UNKNOWN")]
  [InlineData(" ")]
  public async Task Handle_WhenCodeIsUnknownOrMalformed_ReturnsNotFoundResponse(string code)
  {
    await using var dbContext = CreateInMemoryContext();

    var response = await new GetUniversityByCodeHandler(dbContext)
      .Handle(new GetUniversityByCodeQuery(code), CancellationToken.None);

    Assert.False(response.IsFound);
    Assert.Null(response.Code);
    Assert.Null(response.Name);
    Assert.False(response.IsActive);
  }

  [Fact]
  public async Task Handle_WhenUniversityIsInactive_ReturnsFoundWithInactiveState()
  {
    await using var dbContext = CreateInMemoryContext();
    var university = UniversityRecord.Create("MIT", "Massachusetts Institute of Technology");
    university.Deactivate();
    dbContext.Universities.Add(university);
    await dbContext.SaveChangesAsync();

    var response = await new GetUniversityByCodeHandler(dbContext)
      .Handle(new GetUniversityByCodeQuery("MIT"), CancellationToken.None);

    Assert.True(response.IsFound);
    Assert.Equal("MIT", response.Code);
    Assert.False(response.IsActive);
  }

  private static ManageUniversitiesDbContext CreateInMemoryContext()
  {
    var options = new DbContextOptionsBuilder<ManageUniversitiesDbContext>()
      .UseInMemoryDatabase($"ManageUniversitiesResolutionTests-{Guid.NewGuid():N}")
      .Options;

    return new ManageUniversitiesDbContext(options);
  }
}
