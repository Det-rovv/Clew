using Clew.Domain.Enums;

namespace Clew.Domain.Models;

public sealed record RelatedProject
{
    public required GlobalProjectIdentifier Identifier { get; init; }
    public required ProjectRelationType RelationType { get; init; }
}