namespace Clew.Domain.Models;

public sealed record ProjectResolveParameters
{
    public required GlobalProjectIdentifier Identifier { get; init; }
    public required ProjectVersionFilters ProjectVersionFilters { get; init; } 
    public required bool IsInitial { get; init; }
    
    public override int GetHashCode()
    {
        return Identifier.GetHashCode();
    }
    
    public bool Equals(ProjectResolveParameters? other)
    {
        return other is not null &&
               Identifier.ContentSourceName == other.Identifier.ContentSourceName &&
               Identifier.Id == other.Identifier.Id;
    }
}