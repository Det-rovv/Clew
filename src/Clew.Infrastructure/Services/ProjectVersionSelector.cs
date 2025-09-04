using Clew.Domain.Abstractions;
using Clew.Domain.Enums;
using Clew.Domain.Models;
using Clew.Infrastructure.Abstractions;

namespace Clew.Infrastructure.Services;

internal class ProjectVersionSelector : IProjectVersionSelector
{
    public virtual TVersion? FindMatchingProjectVersion<TVersion>(
        IReadOnlyList<TVersion> projectVersions,
        ProjectVersionFilters filters) where TVersion : class, IProjectVersion
    {
        var projectVersionsArray = projectVersions.ToArray();
        
        if (projectVersionsArray.Length == 0) return null;
        
        var requiredGameVersionsArray = filters.GameVersions?.ToArray();
        
        var requiredPlatformsArray = filters.Platforms?.ToArray();

        var scores = new int[projectVersionsArray.Length];
        
        for (int i = 0; i < projectVersionsArray.Length; i++)
        {
            var version = projectVersionsArray[i];

            if (!IsSuitableReleaseChannel(version.ReleaseChannel, filters.ReleaseChannel))
            {
                scores[i] = int.MinValue;
                continue;
            }
            
            scores[i] += requiredGameVersionsArray is not null && requiredGameVersionsArray.Length != 0
                ? version.GameVersions.Select(ver => 
                {
                    var indexOfGameVersion = Array.IndexOf(requiredGameVersionsArray, ver);
                    return indexOfGameVersion >= 0 ? requiredGameVersionsArray.Length - indexOfGameVersion : -1;
                }).DefaultIfEmpty(0).Max() * requiredPlatformsArray?.Length ?? 1
                : 0;
            
            scores[i] += requiredPlatformsArray is not null && requiredPlatformsArray.Length != 0
                ? version.Platforms.Select(platform => 
                {
                    var indexOfPlatform = Array.IndexOf(requiredPlatformsArray, platform);
                    return indexOfPlatform >= 0 ? requiredPlatformsArray.Length - indexOfPlatform : -1;
                }).DefaultIfEmpty(0).Max()
                : 0;
        }

        var bestScore = scores.Index().MaxBy(x => x.Item);
        return bestScore.Item > 0 ? projectVersionsArray[bestScore.Index] : null;
    }
    
    protected virtual bool IsSuitableReleaseChannel(ReleaseChannel releaseChannel, ReleaseChannelFilter? requiredChannel)
    {
        if (releaseChannel == ReleaseChannel.Release ||
            requiredChannel == ReleaseChannelFilter.Any ||
            requiredChannel is null)
            return true;
        
        return releaseChannel == ReleaseChannel.Beta && requiredChannel == ReleaseChannelFilter.AtLeastBeta;
    }
}