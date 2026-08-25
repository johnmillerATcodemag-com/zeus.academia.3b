using MediatR;

namespace Zeus.Academia.Features.ReferenceData.ManageUniversities.ListUniversities;

public sealed record ListUniversitiesQuery() : IRequest<IReadOnlyList<ListUniversitiesResponse>>;
