namespace Clew.Application.Abstractions;

internal interface IContentSourceRouter
{
    IContentSource this[string contentSourceName] { get; }
}