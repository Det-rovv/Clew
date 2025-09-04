using Clew.Domain.Enums;

namespace Clew.Api.Contracts;

public sealed record ProjectListResolveParametersDto
{
    public required IEnumerable<ProjectResolveParametersDto> Projects { get; init; }
    public required IReadOnlyList<string> DefaultGameVersions { get; init; }
    public required IReadOnlyList<string> DefaultPlatforms { get; init; }
    public ReleaseChannelFilter? DefaultReleaseChannel { get; init; }
    public IEnumerable<ProjectIdentifierDto>? ExcludedProjects { get; init; }
}