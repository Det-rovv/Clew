using Clew.Domain.Models;

namespace Clew.Application.Abstractions;

public interface IContentSource
{
    string ContentSourceName { get; }

    Task<ProjectResolveResult?> ResolveProjectAsync(ProjectResolveParameters projectParameters, CancellationToken ct);
    
    Task<IEnumerable<ProjectResolveResult>> ResolveProjectListAsync(ProjectListResolveParameters projectListParameters, CancellationToken ct);
}