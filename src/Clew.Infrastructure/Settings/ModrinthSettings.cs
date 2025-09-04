namespace Clew.Infrastructure.Settings;

internal sealed record ModrinthSettings : ContentSourceSettings
{
    public int MaxRequestsPerMinute { get; init; }
    public int MaxItemsPerRequest { get; init; }
    public int BatchRequestItemsCountThreshold { get; init; }
}