namespace Clew.Api.Contracts;

public sealed record ProjectListDownloadUrlsDto
{
    public required IEnumerable<string> InitialProjectsDownloadUrls { get; init; }
    public required IEnumerable<string> DependenciesDownloadUrls { get; init; }
}