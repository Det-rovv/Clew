using System.Collections.Immutable;
using Clew.Application.Abstractions;
using Clew.Application.Enums;
using Clew.Domain.Abstractions;
using Clew.Domain.Enums;
using Clew.Domain.Models;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Extensions;
using Clew.Infrastructure.Settings;

namespace Clew.Infrastructure.ContentSources;

internal abstract class ContentSourceBase<TSettings> : IContentSource where TSettings : ContentSourceSettings
{
    public string ContentSourceName { get; }
    protected HttpClient HttpClient { get; }
    protected TSettings Settings { get; }
    protected IContentSourceNamingsTranslator Translator { get; }
    protected IProjectVersionSelector ProjectVersionSelector { get; }
    
    public ContentSourceBase(HttpClient httpClient, TSettings settings,
        IContentSourceNamingsTranslator translator,
        IProjectVersionSelector projectVersionSelector)
    {
        ContentSourceName = settings.ApiName;
        HttpClient = httpClient;
        Settings = settings;
        Translator = translator;
        ProjectVersionSelector = projectVersionSelector;
    }

    public abstract Task<ProjectResolveResult?> ResolveProjectAsync(
        ProjectResolveParameters projectParameters, CancellationToken ct);

    public abstract Task<IEnumerable<ProjectResolveResult>> ResolveProjectListAsync(
        ProjectListResolveParameters projectListParameters, CancellationToken ct);
    
    protected virtual ProjectResolveResult ResolveProjectVersions<TProjectVersion>(
        ProjectResolveParameters projectParameters, IList<TProjectVersion> projectVersions)
        where TProjectVersion : class, IProjectVersion
    {
        var filters = projectParameters.ProjectVersionFilters;

        var latestVersion = projectVersions.Any()
            ? ProjectVersionSelector.FindMatchingProjectVersion(projectVersions.ToImmutableList(), filters)
            : null;

        return new ProjectResolveResult
        {
            ProjectVersion = latestVersion,
            ResolveParameters = projectParameters,
        };
    }

    protected virtual ProjectRelationType GetCommonRelationType(string specificRelationType)
    {
        var commonRelationTypeName =
            Translator.GetCommonRelationTypeName(ContentSourceName, specificRelationType);

        return System.Enum.Parse<ProjectRelationType>(commonRelationTypeName, true);
    }
    
    protected virtual ReleaseChannel GetCommonReleaseChannel(string specificReleaseChannel)
    {
        var commonReleaseChannelName =
            Translator.GetCommonReleaseChannelName(ContentSourceName, specificReleaseChannel);

        return System.Enum.Parse<ReleaseChannel>(commonReleaseChannelName, true);
    }
    
    protected virtual string GetCommonPlatformName(string specificPlatformName)
    {
        return Translator.GetCommonPlatformName(ContentSourceName, specificPlatformName);
    }
    
    protected static async Task<IEnumerable<TResult>> ChunkAndRequestAsync<TInput, TResult>(
        IEnumerable<TInput> inputs,
        Func<IEnumerable<TInput>, Task<IEnumerable<TResult>>> request,
        int maxItemsPerRequest)
    {
        if (!inputs.Any()) return Enumerable.Empty<TResult>();
        if (maxItemsPerRequest <= 0) throw new ArgumentOutOfRangeException(nameof(maxItemsPerRequest));
        
        var chunkLength = GetItemsPerRequest(inputs.Count(), maxItemsPerRequest);

        var requests = inputs
            .Chunk(chunkLength)
            .Select(input => request(input));
        
        return (await Task.WhenAll(requests)).SelectMany(result => result);
    }
    
    protected static int GetItemsPerRequest(int itemCount, int maxChunkLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxChunkLength);

        var numberOfChunks = (itemCount + maxChunkLength - 1) / maxChunkLength;
        
        return (itemCount + numberOfChunks - 1) / numberOfChunks;
    }

    protected GlobalProjectIdentifier GetThisSourceIdentifier(string id)
    {
        return new GlobalProjectIdentifier
        {
            ContentSourceName = ContentSourceName,
            Id = id
        };
    }
}