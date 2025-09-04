namespace Clew.Domain.Models;

public sealed record GlobalProjectIdentifier
{
    public required string ContentSourceName { get; init; }
    public required string Id { get; init; }
    
    public override string ToString() => $"{ContentSourceName}:{Id}";
}