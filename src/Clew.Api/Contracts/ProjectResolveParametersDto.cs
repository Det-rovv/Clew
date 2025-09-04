using Clew.Domain.Enums;

namespace Clew.Api.Contracts;

public sealed record ProjectResolveParametersDto
{
    public required string ContentSourceName { get; init; }
    public required string Id { get; init; }
    public IReadOnlyList<string>? GameVersions { get; init; }
    public IReadOnlyList<string>? Platforms { get; init; }
    public ReleaseChannelFilter? ReleaseChannel { get; init; }
}