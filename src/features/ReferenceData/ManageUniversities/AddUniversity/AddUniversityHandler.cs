using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public sealed class AddUniversityHandler : IRequestHandler<AddUniversityCommand, AddUniversityResponse>
{
  private readonly ManageUniversitiesDbContext _dbContext;

  public AddUniversityHandler(ManageUniversitiesDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<AddUniversityResponse> Handle(AddUniversityCommand request, CancellationToken cancellationToken)
  {
    var universityRecord = request.ToUniversityRecord();

    var codeAlreadyExists = await _dbContext.Universities
      .AsNoTracking()
      .AnyAsync(x => x.Code == universityRecord.Code, cancellationToken);

    if (codeAlreadyExists)
    {
      throw new UniversityConflictException($"University code '{universityRecord.Code}' already exists.");
    }

    _dbContext.Universities.Add(universityRecord);

    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException)
    {
      var nowExists = await _dbContext.Universities
        .AsNoTracking()
        .AnyAsync(x => x.Code == universityRecord.Code, cancellationToken);

      if (nowExists)
      {
        throw new UniversityConflictException($"University code '{universityRecord.Code}' already exists.");
      }

      throw;
    }

    return universityRecord.ToResponse();
  }
}
