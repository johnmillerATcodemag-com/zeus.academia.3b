using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

namespace Zeus.Academia.Tests.Features.ReferenceData.ManageUniversities;

public sealed class ManageUniversitiesSqlServerIntegrationTests
{
  [Fact]
  public async Task AddUniversity_PersistsCanonicalRecordToSqlServer()
  {
    await using var database = await ManageUniversitiesSqlServerTestDatabase.CreateAsync();
    await using var writeContext = database.CreateContext();
    var handler = new AddUniversityHandler(writeContext);

    var response = await handler.Handle(new AddUniversityCommand(" mit "), CancellationToken.None);

    await using var readContext = database.CreateContext();
    var persisted = await readContext.Universities.SingleAsync(x => x.Code == "MIT");

    Assert.Equal("MIT", response.Code);
    Assert.Equal("Massachusetts Institute of Technology", persisted.Name);
    Assert.True(persisted.IsActive);
  }

  [Fact]
  public async Task AddUniversity_WhenDuplicateCodeExists_RejectsWithoutSecondRecord()
  {
    await using var database = await ManageUniversitiesSqlServerTestDatabase.CreateAsync();
    await using var seedContext = database.CreateContext();
    seedContext.Universities.Add(UniversityRecord.Create("MIT", "Massachusetts Institute of Technology"));
    await seedContext.SaveChangesAsync();

    await using var writeContext = database.CreateContext();
    var handler = new AddUniversityHandler(writeContext);

    await Assert.ThrowsAsync<UniversityConflictException>(() =>
      handler.Handle(new AddUniversityCommand("mit"), CancellationToken.None));

    await using var readContext = database.CreateContext();
    Assert.Equal(1, await readContext.Universities.CountAsync(x => x.Code == "MIT"));
  }

  [Fact]
  public async Task ListUniversities_ReturnsStableSortedResultsFromSqlServer()
  {
    await using var database = await ManageUniversitiesSqlServerTestDatabase.CreateAsync();
    await using var writeContext = database.CreateContext();
    writeContext.Universities.AddRange(
      UniversityRecord.Create("stanford", "Stanford University"),
      UniversityRecord.Create("boston_u", "Boston University"),
      UniversityRecord.Create("mit", "Massachusetts Institute of Technology"));
    await writeContext.SaveChangesAsync();

    await using var firstReadContext = database.CreateContext();
    var firstResult = await new ListUniversitiesHandler(firstReadContext)
      .Handle(new ListUniversitiesQuery(), CancellationToken.None);

    await using var secondReadContext = database.CreateContext();
    var secondResult = await new ListUniversitiesHandler(secondReadContext)
      .Handle(new ListUniversitiesQuery(), CancellationToken.None);

    Assert.Equal(["BOSTON_U", "MIT", "STANFORD"], firstResult.Select(x => x.Code).ToArray());
    Assert.Equal(firstResult, secondResult);
  }

  [Fact]
  public async Task GetUniversityByCode_ReturnsCanonicalAndActiveStateFromSqlServer()
  {
    await using var database = await ManageUniversitiesSqlServerTestDatabase.CreateAsync();
    await using var writeContext = database.CreateContext();
    var university = UniversityRecord.Create("mit", "Massachusetts Institute of Technology");
    university.Deactivate();
    writeContext.Universities.Add(university);
    await writeContext.SaveChangesAsync();

    await using var readContext = database.CreateContext();
    var response = await new GetUniversityByCodeHandler(readContext)
      .Handle(new GetUniversityByCodeQuery(" MIT "), CancellationToken.None);

    Assert.True(response.IsFound);
    Assert.Equal("MIT", response.Code);
    Assert.Equal("Massachusetts Institute of Technology", response.Name);
    Assert.False(response.IsActive);
  }
}
