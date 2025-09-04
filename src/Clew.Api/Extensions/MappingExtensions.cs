using System.Collections.Immutable;
using Clew.Api.Contracts;
using Clew.Domain.Enums;
using Clew.Domain.Models;

namespace Clew.Api.Extensions;

internal static class MappingExtensions
{
    internal static ProjectListResolveParameters ToDomainProjectListParams(this ProjectListResolveParametersDto projectListDto)
    {
        return new ProjectListResolveParameters
        {
            ProjectsParameters = projectListDto.Projects.Select(projectDto => projectDto.ToDomainProjectParams(projectListDto)),
            
            DefaultProjectVersionFilters = new ProjectVersionFilters
            {
                GameVersions = projectListDto.DefaultGameVersions.ToImmutableList(),
                Platforms = projectListDto.DefaultPlatforms.ToImmutableList(),
                ReleaseChannel = projectListDto.DefaultReleaseChannel ?? ReleaseChannelFilter.Any
            },

            ExcludedProjects = (projectListDto.ExcludedProjects ?? Array.Empty<ProjectIdentifierDto>())
                .Select(projectIdDto => new GlobalProjectIdentifier 
                {
                    ContentSourceName = projectIdDto.ContentSourceName,
                    Id = projectIdDto.Id
                })
        };
    }

    internal static ProjectResolveParameters ToDomainProjectParams(
        this ProjectResolveParametersDto projectDto, ProjectListResolveParametersDto projectListDto)
    {
        return new ProjectResolveParameters
        {
            Identifier = new()
            {
                ContentSourceName = projectDto.ContentSourceName,
                Id = projectDto.Id,
            },
            IsInitial = true,
            ProjectVersionFilters = projectDto.GetDomainVersionFilters(projectListDto)
        };
    }

    internal static ProjectVersionFilters GetDomainVersionFilters(
        this ProjectResolveParametersDto projectDto, ProjectListResolveParametersDto projectListDto)
    {
        return new ProjectVersionFilters
        {
            GameVersions = (projectDto.GameVersions ?? Array.Empty<string>())
                .Union(projectListDto.DefaultGameVersions)
                .ToImmutableList(),
            
            Platforms = (projectDto.Platforms ?? Array.Empty<string>())
                .Union(projectListDto.DefaultPlatforms)
                .ToImmutableList(),
            
            ReleaseChannel = projectDto.ReleaseChannel ?? projectListDto.DefaultReleaseChannel ?? ReleaseChannelFilter.Any
        };
    }
}