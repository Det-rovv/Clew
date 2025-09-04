using Clew.Infrastructure.Enum;

namespace Clew.Infrastructure.Settings;

internal sealed record ConcurrencySettings
{
    public required IReadOnlyDictionary<ConcurrentTasks, int> ItemsPerThread { get; init; }
}