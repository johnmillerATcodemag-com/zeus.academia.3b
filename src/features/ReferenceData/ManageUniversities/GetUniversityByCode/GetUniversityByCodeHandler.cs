using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.SharedKernel.Foundation.Domain;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

public sealed class GetUniversityByCodeHandler
  : IRequestHandler<GetUniversityByCodeQuery, GetUniversityByCodeResponse>
{
  private readonly ManageUniversitiesDbContext _dbContext;

  public GetUniversityByCodeHandler(ManageUniversitiesDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<GetUniversityByCodeResponse> Handle(
    GetUniversityByCodeQuery request,
    CancellationToken cancellationToken)
  {
    string normalizedCode;

    try
    {
      normalizedCode = University.Create(request.Code).Code;
    }
    catch (ArgumentException)
    {
      return new GetUniversityByCodeResponse(false, null, null, false);
    }

    var university = await _dbContext.Universities
      .AsNoTracking()
      .SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

    return university is null
      ? new GetUniversityByCodeResponse(false, null, null, false)
      : new GetUniversityByCodeResponse(true, university.Code, university.Name, university.IsActive);
  }
}
