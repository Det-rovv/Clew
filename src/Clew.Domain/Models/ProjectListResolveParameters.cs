namespace Clew.Domain.Models;

public sealed record ProjectListResolveParameters
{
    public required ProjectVersionFilters DefaultProjectVersionFilters { get; init; }
    public required IEnumerable<ProjectResolveParameters> ProjectsParameters { get; init; }
    public IEnumerable<GlobalProjectIdentifier> ExcludedProjects { get; init; } = Enumerable.Empty<GlobalProjectIdentifier>();
}