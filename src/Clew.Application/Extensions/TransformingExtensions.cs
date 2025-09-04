using Clew.Domain.Enums;
using Clew.Domain.Models;

namespace Clew.Application.Extensions;

public static class TransformingExtensions
{
    public static ProjectResolveParameters GetDependencyResolveParameters(
        this ProjectResolveParameters parentResolveParameters, string dependencyId, ProjectVersionFilters defaultFilters)
    {
        var parentSearchFilters = parentResolveParameters.ProjectVersionFilters;
        return new ProjectResolveParameters
        {
            Identifier = parentResolveParameters.Identifier with { Id = dependencyId },
            IsInitial = false,
            ProjectVersionFilters = new ProjectVersionFilters
            {
                GameVersions =
                    (defaultFilters.GameVersions ?? Enumerable.Empty<string>())
                    .Union(parentSearchFilters.GameVersions ?? Enumerable.Empty<string>())
                    .ToList(),

                Platforms = (defaultFilters.Platforms ?? Enumerable.Empty<string>())
                    .Union(parentSearchFilters.Platforms ?? Enumerable.Empty<string>())
                    .ToList(),

                ReleaseChannel = defaultFilters.ReleaseChannel ?? ReleaseChannelFilter.Any
            }
        };
    }
    
    public static ProjectResolveParameters GetDefaultSearchParameters(this GlobalProjectIdentifier identifier,
        ProjectVersionFilters defaultFilters, bool isInitial = false)
    {
        return new ProjectResolveParameters
        {
            Identifier = identifier,
            ProjectVersionFilters = defaultFilters,
            IsInitial = isInitial,
        };
    }
    
    public static IEnumerable<ProjectResolveResult> RemoveDuplicateFiles(this IEnumerable<ProjectResolveResult> resolveResults, bool prioritizeInitial = true)
    {
        if (prioritizeInitial)
            resolveResults = resolveResults.OrderByDescending(resolveResult => resolveResult.ResolveParameters.IsInitial);
        
        return resolveResults
            .DistinctBy(modData => Path.GetFileName(modData.ProjectVersion.DownloadUrl));
    }
}