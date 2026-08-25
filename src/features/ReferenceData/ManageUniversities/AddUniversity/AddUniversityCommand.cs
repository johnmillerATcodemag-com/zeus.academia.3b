using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.AddUniversity;

public sealed record AddUniversityCommand(string Code) : IRequest<AddUniversityResponse>;
