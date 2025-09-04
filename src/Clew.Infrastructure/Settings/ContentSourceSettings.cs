namespace Clew.Infrastructure.Settings;

internal abstract record ContentSourceSettings
{
    public required string ApiName { get; init; }
    public required string BaseUrl { get; init; }
    public string? ApiKey { get; init; }
}