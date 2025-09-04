using Clew.Domain.Abstractions;

namespace Clew.Domain.Models;

public sealed record ProjectResolveResult
{
    public required ProjectResolveParameters ResolveParameters { get; init; }
    public IProjectVersion? ProjectVersion { get; init; }
}