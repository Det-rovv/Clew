using Clew.Infrastructure.Enum;

namespace Clew.Infrastructure.Abstractions;

internal interface IThreadCountCalculator
{
    int Calculate(ConcurrentTasks taskType, int itemsCount);
}