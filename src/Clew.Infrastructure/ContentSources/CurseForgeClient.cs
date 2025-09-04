using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Clew.Domain.Exceptions;
using Clew.Domain.Models;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clew.Infrastructure.ContentSources;

internal sealed class CurseForgeClient : ContentSourceBase<CurseForgeSettings>
{
    private readonly IProjectListResolver _projectListResolver;

    public CurseForgeClient(
        HttpClient httpClient,
        IProjectListResolver projectListResolver,
        IContentSourceNamingsTranslator translator,
        IProjectVersionSelector projectVersionSelector,
        IOptions<CurseForgeSettings> curseForgeSettings)
        : base(httpClient, curseForgeSettings.Value, translator, projectVersionSelector)
    {
        _projectListResolver = projectListResolver;
    }

    public override async Task<ProjectResolveResult?> ResolveProjectAsync(ProjectResolveParameters projectParameters, CancellationToken ct)
    {
        var versions = await GetAllModVersionsAsync(projectParameters.Identifier.Id, ct);
        
        return versions is null ? null
            : ResolveProjectVersions(projectParameters, 
                versions.Select(DtoToProjectVersion)
                    .OrderByDescending(v => v.DatePublished)
                    .ToList());
    }
    
    private async Task<IList<CurseForgeProjectVersionDto>?> GetAllModVersionsAsync(string modId, CancellationToken ct)
    {
        var url = $"mods/{modId}/files";

        try
        {
            var modFiles = await HttpClient.GetFromJsonAsync<ModFilesResponse>(url, ct);
        
            return modFiles?.Data;
        }
        catch (Exception)
        {
            throw new ProjectNotFoundException(GetThisSourceIdentifier(modId));
        }
    }

    public override async Task<IEnumerable<ProjectResolveResult>> ResolveProjectListAsync(
        ProjectListResolveParameters projectListParameters, CancellationToken ct)
    {
        return await _projectListResolver
            .ResolveProjectList(projectListParameters, ResolveProjectAsync, ct);
    }

    private ProjectVersion DtoToProjectVersion(CurseForgeProjectVersionDto dto)
    {
        return new ProjectVersion
        {
            ProjectIdentifier = new GlobalProjectIdentifier
            {
                Id = dto.ModId.ToString(),
                ContentSourceName = ContentSourceName
            },
            
            GameVersions = dto.GameVersions,
            // curseforge api returns game versions and platforms in the same array
            Platforms = dto.GameVersions,
            
            DatePublished = dto.DatePublished,
            DownloadUrl = dto.DownloadUrl,
            ReleaseChannel = GetCommonReleaseChannel(dto.ReleaseType.ToString()),
            
            RelatedProjects = dto.Dependencies.Select(dependency =>
                new RelatedProject 
                { 
                    Identifier = new GlobalProjectIdentifier 
                    { 
                        ContentSourceName = ContentSourceName, 
                        Id = dependency.ModId.ToString() 
                    }, 
                    RelationType = GetCommonRelationType(dependency.RelationType.ToString()) 
                })
        };
    }
    
    private sealed record ModFilesResponse
    {
        public required IList<CurseForgeProjectVersionDto> Data { get; init; }
    }
    
    private sealed record CurseForgeProjectVersionDto
    {
        public required int ModId { get; init; }
        
        public required string DownloadUrl { get; init; }
        
        public required IEnumerable<string> GameVersions { get; init; }
        
        public required int ReleaseType { get; init; }
        
        [JsonPropertyName("fileDate")]
        public required DateTimeOffset DatePublished { get; init; }
        
        public IEnumerable<RelatedProject> Dependencies { get; init; } = Enumerable.Empty<RelatedProject>();
        
        public sealed record RelatedProject
        {
            public int ModId { get; init; }
            
            public int RelationType { get; init; }
        }
    }
}