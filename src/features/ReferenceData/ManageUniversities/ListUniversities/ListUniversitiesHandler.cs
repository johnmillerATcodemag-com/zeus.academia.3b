using MediatR;
using Microsoft.EntityFrameworkCore;
using Zeus.Academia.Features.ReferenceData.ManageUniversities.Shared;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

public sealed class ListUniversitiesHandler : IRequestHandler<ListUniversitiesQuery, IReadOnlyList<ListUniversitiesResponse>>
{
  private readonly ManageUniversitiesDbContext _dbContext;

  public ListUniversitiesHandler(ManageUniversitiesDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<IReadOnlyList<ListUniversitiesResponse>> Handle(ListUniversitiesQuery request, CancellationToken cancellationToken)
  {
    return await _dbContext.Universities
      .AsNoTracking()
      .OrderBy(x => x.Code)
      .Select(x => new ListUniversitiesResponse(x.Code, x.Name, x.IsActive))
      .ToListAsync(cancellationToken);
  }
}