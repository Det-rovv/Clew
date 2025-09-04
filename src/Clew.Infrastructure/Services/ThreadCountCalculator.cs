using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Enum;
using Clew.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clew.Infrastructure.Services;

internal sealed class ThreadCountCalculator
    (IOptions<ConcurrencySettings> concurrencySettings) : IThreadCountCalculator
{
    public int Calculate(ConcurrentTasks taskType, int itemsCount)
    {
        if (itemsCount < 1) throw new ArgumentException($"value can't be less than 1", nameof(itemsCount));

        if (!concurrencySettings.Value.ItemsPerThread.TryGetValue(taskType, out var modsPerThread))
            throw new ArgumentException($"No value found for concurrent task", nameof(taskType));

        return itemsCount / modsPerThread + Math.Min(1, itemsCount % modsPerThread);
    }
}