using System.Text.Json;
using Clew.Infrastructure.Abstractions;
using Microsoft.AspNetCore.WebUtilities;

namespace Clew.Infrastructure.Services;

internal sealed class ModrinthUrlFormatter : IModrinthUrlFormatter
{
    public string GetModVersionsUrl(string modId, IEnumerable<string>? gameVersions, IEnumerable<string>? platforms)
    {
        var route = $"project/{modId}/version";

        var queryParameters = new Dictionary<string, string?>
        {
            ["game_versions"] = gameVersions is not null
                ? JsonSerializer.Serialize(gameVersions)
                : null,
            
            ["loaders"] = platforms is not null
                ? JsonSerializer.Serialize(platforms)
                : null
        };
        
        return QueryHelpers.AddQueryString(route, queryParameters);
    }

    public string GetMultipleProjectsUrl(IEnumerable<string> projectIds)
    {
        return QueryHelpers.AddQueryString("projects", "ids", JsonSerializer.Serialize(projectIds));
    }

    public string GetMultipleVersionsUrl(IEnumerable<string> versionIds)
    {
        return QueryHelpers.AddQueryString("versions", "ids", JsonSerializer.Serialize(versionIds));
    }
}