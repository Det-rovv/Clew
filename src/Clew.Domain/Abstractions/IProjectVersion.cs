using Clew.Domain.Enums;
using Clew.Domain.Models;

namespace Clew.Domain.Abstractions;

public interface IProjectVersion
{
    GlobalProjectIdentifier ProjectIdentifier { get; }
    public IEnumerable<string> GameVersions { get; }
    public IEnumerable<string> Platforms { get; }
    public ReleaseChannel ReleaseChannel { get; }
    public string DownloadUrl { get; }
    public DateTimeOffset? DatePublished { get; }
    public IEnumerable<RelatedProject> RelatedProjects { get; }
}