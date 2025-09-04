using Clew.Application.Abstractions;
using Clew.Application.Extensions;
using Clew.Domain.Models;

namespace Clew.Application.Services;

internal sealed class ProjectsService : IProjectsService
{
    private readonly IContentSourceRouter _contentSourceRouter;

    public ProjectsService(IContentSourceRouter contentSourceRouter)
    {
        _contentSourceRouter = contentSourceRouter;
    }

    public async Task<(IEnumerable<string> InitialProjects, IEnumerable<string> Dependencies)> GetModListDownloadUrlsAsync(ProjectListResolveParameters projectListParameters, CancellationToken ct)
    {
        var tasks = projectListParameters.ProjectsParameters
            .GroupBy(mod => mod.Identifier.ContentSourceName)
            .Select(group =>
            {
                return _contentSourceRouter[group.Key]
                    .ResolveProjectListAsync(projectListParameters with
                    {
                        ProjectsParameters = group,
                        ExcludedProjects =
                        projectListParameters.ExcludedProjects.Where(ex => ex.ContentSourceName == group.Key)
                    }, ct);
            });

        var resultsByInitiality = (await Task.WhenAll(tasks))
            .SelectMany(resolveResult => resolveResult)
            .RemoveDuplicateFiles()
            .ToLookup(resolveResult => resolveResult.ResolveParameters.IsInitial, 
                resolveResult => resolveResult.ProjectVersion.DownloadUrl);

        var initialMods = resultsByInitiality[true];
        var dependencies = resultsByInitiality[false];

        return (initialMods, dependencies);
    }
}