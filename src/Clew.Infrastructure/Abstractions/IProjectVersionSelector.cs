using Clew.Domain.Abstractions;
using Clew.Domain.Models;

namespace Clew.Infrastructure.Abstractions;

internal interface IProjectVersionSelector
{
    TVersion? FindMatchingProjectVersion<TVersion>(
        IReadOnlyList<TVersion> projectVersions,
        ProjectVersionFilters filters) where TVersion : class, IProjectVersion;
}