using Clew.Domain.Enums;
using Clew.Domain.Models;

namespace Clew.Domain.Extensions;

public static class ProjectResolveParametersExtensions
{
    public static ProjectResolveParameters GetDependencySearchParameters(
        this ProjectResolveParameters parentParameters, string dependencyId, ProjectVersionFilters defaultFilters)
    {
        var parentFilters = parentParameters.ProjectVersionFilters;
        
        return new ProjectResolveParameters
        {
            Identifier = parentParameters.Identifier with { Id = dependencyId },
            IsInitial = false,
            ProjectVersionFilters = new ProjectVersionFilters
            {
                GameVersions =
                    (defaultFilters.GameVersions ?? Enumerable.Empty<string>())
                    .Union(parentFilters.GameVersions ?? Enumerable.Empty<string>())
                    .ToList(),

                Platforms = (defaultFilters.Platforms ?? Enumerable.Empty<string>())
                    .Union(parentFilters.Platforms ?? Enumerable.Empty<string>())
                    .ToList(),

                ReleaseChannel = defaultFilters.ReleaseChannel ?? ReleaseChannelFilter.Any
            }
        };
    }
}