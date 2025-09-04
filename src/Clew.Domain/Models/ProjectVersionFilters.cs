using Clew.Domain.Enums;

namespace Clew.Domain.Models;

public sealed record ProjectVersionFilters
{
    public IReadOnlyList<string>? GameVersions { get; init; }
    
    public IReadOnlyList<string>? Platforms { get; init; }
    
    public ReleaseChannelFilter? ReleaseChannel { get; init; }

    public override string ToString()
    {
        var gameVersionsString = GameVersions != null
            ? string.Join(", ", GameVersions) : "-";
        
        var platformsString = Platforms != null
            ? string.Join(", ", Platforms) : "-";
        
        return $"{{ GameVersions: [{gameVersionsString}], Platforms: [{platformsString}], ReleaseChannel: {ReleaseChannel} }}";
    }
}