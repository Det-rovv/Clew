using Clew.Domain.Models;

namespace Clew.Application.Abstractions;

public interface IProjectsService
{
    Task<(IEnumerable<string> InitialProjects, IEnumerable<string> Dependencies)>
        GetModListDownloadUrlsAsync(ProjectListResolveParameters projectListParameters, CancellationToken ct);
}