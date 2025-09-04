using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clew.Application.Extensions;
using Clew.Domain.Enums;
using Clew.Domain.Exceptions;
using Clew.Domain.Extensions;
using Clew.Domain.Models;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clew.Infrastructure.ContentSources;

internal sealed class ModrinthClient : ContentSourceBase<ModrinthSettings>
{
    private readonly IProjectListResolver _projectListResolver;
    private readonly IModrinthUrlFormatter _urlFormatter;
    private readonly ModIdentifiersStorage _modIdentifiersStorage = new();

    private static readonly JsonSerializerOptions JsonOptions = new() 
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
    
    public ModrinthClient(
        HttpClient httpClient,
        IOptions<ModrinthSettings> modrinthSettings,
        IModrinthUrlFormatter urlFormatter,
        IContentSourceNamingsTranslator translator,
        IProjectVersionSelector projectVersionSelector,
        IProjectListResolver projectListResolver)
        : base(httpClient, modrinthSettings.Value, translator, projectVersionSelector)
    {
        _projectListResolver = projectListResolver;
        _urlFormatter = urlFormatter;
    }
    
    public override async Task<ProjectResolveResult?> ResolveProjectAsync(ProjectResolveParameters projectParameters, CancellationToken ct)
    {
        var filters = projectParameters.ProjectVersionFilters;
        
        var versions = await GetAllProjectVersionsAsync(projectParameters.Identifier.Id,
            filters.GameVersions, filters.Platforms, ct);
        
        return versions is null ? null
            : ResolveProjectVersions(projectParameters,
                versions
                    .Select(DtoToProjectVersion)
                    .OrderByDescending(v => v.DatePublished)
                    .ToImmutableList());
    }

    public override async Task<IEnumerable<ProjectResolveResult>> ResolveProjectListAsync(
        ProjectListResolveParameters projectListParameters, CancellationToken ct)
    {
        IEnumerable<ProjectResolveResult> results = new List<ProjectResolveResult>(projectListParameters.ProjectsParameters.Count());

        var refreshedExcludedProjects = projectListParameters.ExcludedProjects.Any()
            ? await BatchRequestProjects(projectListParameters.ExcludedProjects.Select(ex => ex.Id), ct)
            : Enumerable.Empty<ModrinthProjectDto>();
        
        var refreshedExcludedIdentifiers = projectListParameters.ExcludedProjects
            .Union(refreshedExcludedProjects.Select(project => GetThisSourceIdentifier(project.Identifier.Id)));

        projectListParameters = projectListParameters with {
            ExcludedProjects = projectListParameters.ExcludedProjects.Union(refreshedExcludedIdentifiers) };

        while (!ct.IsCancellationRequested && projectListParameters.ProjectsParameters.Any())
        {
            if (projectListParameters.ProjectsParameters.Count() >= Settings.BatchRequestItemsCountThreshold)
            {
                var currentStepResults =
                    await BatchResolveModList(projectListParameters, ct);
                
                results = results.Concat(currentStepResults);

                var nextStepExcludedProjects = projectListParameters.ExcludedProjects.Union(currentStepResults.Select(res => res.ResolveParameters.Identifier));
                
                var dependenciesSearchParameters =
                    currentStepResults.SelectMany(result => 
                            result.ProjectVersion.RelatedProjects
                                .Where(relation => relation.RelationType == ProjectRelationType.RequiredDependency)
                                .Select(dependency => result.ResolveParameters
                                    .GetDependencySearchParameters(dependency.Identifier.Id, projectListParameters.DefaultProjectVersionFilters)))
                    .Where(searchParams => !nextStepExcludedProjects.Contains(searchParams.Identifier));
                
                if (!dependenciesSearchParameters.Any()) return results;
                
                projectListParameters = projectListParameters with
                {
                    ProjectsParameters = dependenciesSearchParameters,
                    ExcludedProjects = nextStepExcludedProjects
                };
            }
            else
            {
                return results.Concat(await _projectListResolver.ResolveProjectList(projectListParameters, ResolveProjectAsync, ct));
            }
        }
        return results;
    }

    private async Task<IEnumerable<ProjectResolveResult>> BatchResolveModList(ProjectListResolveParameters projectListParameters, CancellationToken ct)
    {
        var handledResolveParametersById = new ConcurrentDictionary<string, ProjectResolveParameters>();
        
        var excludedProjects = projectListParameters.ExcludedProjects.ToHashSet();
        
        var modsToHandle = projectListParameters.ProjectsParameters
            .Concat(projectListParameters.ExcludedProjects
                .Where(ex => _modIdentifiersStorage.GetIdentifierByOne(ex.Id) is null)
                .Select(a => a.GetDefaultSearchParameters(projectListParameters.DefaultProjectVersionFilters)))
            .ToHashSet();
        
        var projects = await BatchRequestProjects(modsToHandle.Select(m => m.Identifier.Id), ct);
        
        var searchParametersAndProjects =
            JoinSearchParametersAndProjects(modsToHandle, projects);
        
        foreach (var pair in searchParametersAndProjects)
        {
            var (searchParameters, project) = pair;

            _modIdentifiersStorage.Add(project.Identifier);

            handledResolveParametersById.TryAdd(project.Id, searchParameters);
            handledResolveParametersById.TryAdd(project.Slug, searchParameters);
        }
        
        searchParametersAndProjects = searchParametersAndProjects
            .Where(pair => !excludedProjects
                .Any(ex => pair.project.Identifier.IsSameProject(ex.Id)));

        var versionResponses = await ChunkAndRequestAsync(searchParametersAndProjects
                .Where(project =>
                {
                    var modrinthIdentifier = _modIdentifiersStorage
                        .GetIdentifierByOne(project.resolveParameters.Identifier.Id);
                        
                        return !excludedProjects.Contains(GetThisSourceIdentifier(modrinthIdentifier.Id)) &&
                               !excludedProjects.Contains(GetThisSourceIdentifier(modrinthIdentifier.Slug));
                })
                .SelectMany(project => project.project.Versions),
            versions => GetMultipleVersionsDataAsync(versions, ct),
            Settings.MaxItemsPerRequest);

        var results = new ConcurrentBag<ProjectResolveResult>();
        var relatedProjects = new ConcurrentBag<RelatedProject>();
        
        Parallel.ForEach(versionResponses
                .GroupBy(ver => ver.ProjectId)
                .Where(group => !excludedProjects.Contains(new GlobalProjectIdentifier {ContentSourceName = ContentSourceName, Id = group.Key})),
            new ParallelOptions { CancellationToken = ct },
            group => 
        {
            var versionsByDate =
                group.Select(DtoToProjectVersion)
                    .OrderByDescending(ver => ver.DatePublished);

            handledResolveParametersById.TryGetValue(group.Key, out var searchParameters);
            if (searchParameters is null)
                throw new ArgumentException($"No search parameters found for project '{group.Key}'. Something went wrong.");

            var versionResolveResult = ResolveProjectVersions(searchParameters, versionsByDate.ToImmutableList());
            
            if (versionResolveResult.ProjectVersion is null)
                throw new ProjectVersionNotFoundException(searchParameters);

            foreach (var relatedProject in versionResolveResult.ProjectVersion.RelatedProjects.Where(proj => !excludedProjects.Contains(proj.Identifier)))
                relatedProjects.Add(relatedProject);
            
            results.Add(versionResolveResult);
        });
        
        return results;
    }

    private async Task<IEnumerable<ModrinthProjectDto>> BatchRequestProjects(IEnumerable<string> ids, CancellationToken ct)
    {
        var fetchedProjects = await ChunkAndRequestAsync(ids,
            modIds => GetMultipleProjectsDataAsync(modIds, ct), 
            Settings.MaxItemsPerRequest);

        var returnedSlugs = fetchedProjects
            .Select(project => project.Slug)
            .ToImmutableHashSet();
        
        var returnedIds = fetchedProjects
            .Select(project => project.Id)
            .ToImmutableHashSet();

        var missingProject = ids.FirstOrDefault(id => !returnedSlugs.Contains(id) && !returnedIds.Contains(id));
        
        if (missingProject != null) throw new ProjectNotFoundException(GetThisSourceIdentifier(missingProject));

        return fetchedProjects;
    }

    private IEnumerable<(ProjectResolveParameters resolveParameters, ModrinthProjectDto project)> JoinSearchParametersAndProjects(
        IEnumerable<ProjectResolveParameters> resolveParameters, IEnumerable<ModrinthProjectDto> projects)
    {
        return from resolveParam in resolveParameters 
            from project in projects 
            where project.Identifier.IsSameProject(resolveParam.Identifier.Id)
            select (resolveParam, project);
    }

    private async Task<IEnumerable<ModrinthProjectDto>> GetMultipleProjectsDataAsync(IEnumerable<string> modIds, CancellationToken ct)
    {
        var url = _urlFormatter.GetMultipleProjectsUrl(modIds);
        
        return await HttpClient.GetFromJsonAsync<IEnumerable<ModrinthProjectDto>>(url, JsonOptions, ct)
               ?? Enumerable.Empty<ModrinthProjectDto>();
    }
    
    private async Task<IEnumerable<ModrinthProjectVersionDto>> GetMultipleVersionsDataAsync(IEnumerable<string> versionIds, CancellationToken ct)
    {
        var url = _urlFormatter.GetMultipleVersionsUrl(versionIds);
        
        return await HttpClient.GetFromJsonAsync<IEnumerable<ModrinthProjectVersionDto>>(url, JsonOptions, ct)
               ?? Enumerable.Empty<ModrinthProjectVersionDto>();
    }

    private async Task<IList<ModrinthProjectVersionDto>?> GetAllProjectVersionsAsync(string modId, IReadOnlyList<string>? gameVersions,
        IReadOnlyList<string>? modLoaders, CancellationToken ct)
    {
        var url = _urlFormatter.GetModVersionsUrl(modId, gameVersions, modLoaders);
        
        return await HttpClient.GetFromJsonAsync<IList<ModrinthProjectVersionDto>>(url, JsonOptions, ct);
    }

    private ProjectVersion DtoToProjectVersion(ModrinthProjectVersionDto dto)
    {
        return new ProjectVersion
        {
            ProjectIdentifier = new GlobalProjectIdentifier
            {
                ContentSourceName = ContentSourceName,
                Id = dto.ProjectId
            },
            
            GameVersions = dto.GameVersions,
            Platforms = dto.Platforms.Select(specificPlatformName => GetCommonPlatformName(specificPlatformName)),
            
            DatePublished = dto.DatePublished,
            ReleaseChannel = GetCommonReleaseChannel(dto.ReleaseChannel),
            
            DownloadUrl = dto.Files.FirstOrDefault()?.Url
                          ?? throw new Exception(
                              $"'{dto.Id}' version of '{dto.ProjectId}' project on '{ContentSourceName}' does not contains any files"),
            
            RelatedProjects = dto.Dependencies.Select(proj => new RelatedProject
            {
                Identifier = new GlobalProjectIdentifier { ContentSourceName = ContentSourceName, Id = proj.Id },
                RelationType = GetCommonRelationType(proj.DependencyType)
            })
        };
    }
    
    private sealed record ModrinthProjectDto
    {
        public ModrinthIdentifier Identifier => new() { Id = Id, Slug = Slug };
        public required string Id { get; init; }
        public required string Slug { get; init; }
        public required IReadOnlyList<string> Versions { get; init; }
    }

    private sealed record ModrinthProjectVersionDto
    {
        public required string Id { get; init; }
        public required string ProjectId { get; init; }
        
        [JsonPropertyName("version_type")]
        public required string ReleaseChannel { get; init; }
        
        public required IReadOnlyList<FileResponse> Files { get; init; }
        
        [JsonPropertyName("dependencies")]
        public required IEnumerable<Dependency> Dependencies { get; init; }
        
        public required IEnumerable<string> GameVersions { get; init; }
        
        [JsonPropertyName("loaders")]
        public required IEnumerable<string> Platforms { get; init; }
        
        public required DateTimeOffset DatePublished { get; init; }
        
        
        public sealed record FileResponse
        {
            public required string Url { get; init; }
        }
        
        public sealed record Dependency
        {
            [JsonPropertyName("project_id")]
            public required string Id { get; init; }
            public required string DependencyType { get; init; }
        }
    }
    
    private sealed record ModrinthIdentifier
    {
        public required string Id { get; init; }
        public required string Slug { get; init; }
        
        public bool IsSameProject(string projectId)
        {
            return Id == projectId || Slug == projectId;
        }
        
        public bool IsSameProject(ModrinthIdentifier other)
        {
            return other.Id == Id && other.Slug == Slug;
        }
    }

    private sealed class ModIdentifiersStorage
    {
        private readonly ConcurrentDictionary<string, ModrinthIdentifier> _byId = new();
        private readonly ConcurrentDictionary<string, ModrinthIdentifier> _bySlug = new();

        public void Add(ModrinthIdentifier identifiers)
        {
            _byId[identifiers.Id] = identifiers;
            _bySlug[identifiers.Slug] = identifiers;
        }
        
        public ModrinthIdentifier? GetIdentifierByOne(string identifier)
        {
            return _byId.GetValueOrDefault(identifier) ?? _bySlug.GetValueOrDefault(identifier);
        }
    }

}