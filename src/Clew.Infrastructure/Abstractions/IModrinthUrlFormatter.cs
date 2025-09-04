namespace Clew.Infrastructure.Abstractions;

internal interface IModrinthUrlFormatter
{
    string GetModVersionsUrl(string modId,
        IEnumerable<string>? gameVersions, IEnumerable<string>? platforms);

    string GetMultipleProjectsUrl(IEnumerable<string> projectIds);
    
    string GetMultipleVersionsUrl(IEnumerable<string> versionIds);
}