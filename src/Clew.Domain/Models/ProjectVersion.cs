using Clew.Domain.Abstractions;
using Clew.Domain.Enums;

namespace Clew.Domain.Models;

public sealed record ProjectVersion : IProjectVersion
{
    public required GlobalProjectIdentifier ProjectIdentifier { get; init; }
    public required IEnumerable<string> GameVersions { get; init; }
    public required IEnumerable<string> Platforms { get; init; }
    public required ReleaseChannel ReleaseChannel { get; init; }
    public required string DownloadUrl { get; init; }
    public DateTimeOffset? DatePublished { get; init; }
    public required IEnumerable<RelatedProject> RelatedProjects { get; init; } = Enumerable.Empty<RelatedProject>();
}