using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.GetUniversityByCode;

public sealed record GetUniversityByCodeQuery(string Code) : IRequest<GetUniversityByCodeResponse>;
