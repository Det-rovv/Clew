using Clew.Domain.Models;

namespace Clew.Infrastructure.Abstractions;

internal interface IProjectListResolver
{
    Task<IEnumerable<ProjectResolveResult>> ResolveProjectList(
        ProjectListResolveParameters projectListParameters,
        Func<ProjectResolveParameters, CancellationToken, Task<ProjectResolveResult?>> fetchDownloadData,
        CancellationToken ct, ProjectListResolveOptions? options = null);
}

internal sealed record ProjectListResolveOptions
{
    public bool IgnoreNoMatchingVersions { get; init; } = false;
    public bool HandleDependencies { get; init; } = true;
}