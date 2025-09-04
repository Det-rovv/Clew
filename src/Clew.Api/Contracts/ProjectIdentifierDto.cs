namespace Clew.Api.Contracts;

public sealed record ProjectIdentifierDto
{
    public required string ContentSourceName { get; init; }
    public required string Id { get; init; }
}