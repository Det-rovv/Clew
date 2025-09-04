using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using Clew.Domain.Enums;
using Clew.Domain.Exceptions;
using Clew.Domain.Extensions;
using Clew.Domain.Models;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Enum;

namespace Clew.Infrastructure.Services;

internal sealed class ParallelProjectListResolver : IProjectListResolver
{
    private static readonly ProjectListResolveOptions DefaultOptions = new();
    private readonly IThreadCountCalculator _threadCountCalculator;
    
    public ParallelProjectListResolver(IThreadCountCalculator threadCountCalculator)
    {
        _threadCountCalculator = threadCountCalculator;
    }
    
    public async Task<IEnumerable<ProjectResolveResult>> ResolveProjectList(
        ProjectListResolveParameters projectListParameters,
        Func<ProjectResolveParameters, CancellationToken, Task<ProjectResolveResult?>> fetchDownloadData,
        CancellationToken ct, ProjectListResolveOptions? options = null)
    {
        options ??= DefaultOptions;
        var initialSearchParams = projectListParameters.ProjectsParameters;
        
        var modsToHandle = initialSearchParams.ToHashSet();
        
        var modsCount = modsToHandle.Count;
        
        if (modsCount < 1) return Enumerable.Empty<ProjectResolveResult>();
        
        var dataFetchingThreadCount = _threadCountCalculator.Calculate(
            ConcurrentTasks.ProjectDataFetching, modsCount);
        
        var dataProcessingThreadCount = _threadCountCalculator.Calculate(
            ConcurrentTasks.ProjectDataProcessing, modsCount);

        var handledMods = new ConcurrentDictionary<GlobalProjectIdentifier, bool>();
        
        var results = Channel.CreateUnbounded<ProjectResolveResult>(
            new UnboundedChannelOptions { 
                SingleReader = true, 
                SingleWriter = false 
            });

        var unresolvedModsCounter = modsCount;
        
        var requestBlock = new TransformBlock<ProjectResolveParameters, ProjectResolveResult>(
            RequestModAsync,
            new ExecutionDataflowBlockOptions {
                EnsureOrdered = false,
                MaxDegreeOfParallelism = dataFetchingThreadCount,
                CancellationToken = ct
            });

        var processBlock = new ActionBlock<ProjectResolveResult>(
            ProcessModDownloadDataAsync, 
            new ExecutionDataflowBlockOptions {
                EnsureOrdered = false,
                MaxDegreeOfParallelism = dataProcessingThreadCount,
                CancellationToken = ct
            });
        
        requestBlock.LinkTo(processBlock, new DataflowLinkOptions { PropagateCompletion = true});

        foreach (var modParameters in modsToHandle)
            if (handledMods.TryAdd(modParameters.Identifier, true))
                await requestBlock.SendAsync(modParameters, ct);

        try
        {
            await processBlock.Completion;
        }
        catch (AggregateException e)
        {
            if (e.InnerExceptions.Count == 1) 
                throw e.InnerExceptions[0];
        }
        results.Writer.Complete();
        
        return await ToEnumerableAsync(results.Reader.ReadAllAsync(ct), modsCount, ct);


        async Task<ProjectResolveResult> RequestModAsync(ProjectResolveParameters modSearchParameters)
        {
            var modDownloadData = await fetchDownloadData(modSearchParameters, ct);

            if (modDownloadData is null || (modDownloadData.ProjectVersion is null && !options.IgnoreNoMatchingVersions))
                throw new ProjectVersionNotFoundException(modSearchParameters);
            
            return modDownloadData;
        }
        
        async Task ProcessModDownloadDataAsync(ProjectResolveResult projectResolveResult)
        {
            if (projectResolveResult.ProjectVersion is not null && options.HandleDependencies)
            {
                foreach (var dependency in projectResolveResult.ProjectVersion.RelatedProjects)
                {
                    var dependencyId = dependency.Identifier.Id;
                    
                    if (dependency.RelationType != ProjectRelationType.RequiredDependency || 
                        projectListParameters.ExcludedProjects.Contains(dependency.Identifier)) continue;
                    
                    var searchParameters = projectResolveResult.ResolveParameters;
                    
                    var dependencySearchParameters = searchParameters
                        .GetDependencySearchParameters(dependencyId, projectListParameters.DefaultProjectVersionFilters);
    
                    if (handledMods.TryAdd(dependencySearchParameters.Identifier, true))
                    {
                        Interlocked.Increment(ref unresolvedModsCounter);
                        await requestBlock.SendAsync(dependencySearchParameters, ct);
                    }
                }
            }
            

            await results.Writer.WriteAsync(projectResolveResult, ct);
            if (Interlocked.Decrement(ref unresolvedModsCounter) == 0) requestBlock.Complete();
        }
    }

    private static async Task<IEnumerable<T>> ToEnumerableAsync<T>(
        IAsyncEnumerable<T> asyncEnumerable, int baseLength, CancellationToken ct)
    {
        var list = new List<T>(baseLength);
        
        await foreach (var item in asyncEnumerable.WithCancellation(ct))
        {
            list.Add(item);
        }

        return list;
    }
}