using Clew.Application.Abstractions;

namespace Clew.Application.Services;

internal sealed class ContentSourceRouter : IContentSourceRouter
{
    private readonly Dictionary<string, IContentSource> _contentSourcesByName = new();
    
    public ContentSourceRouter(IEnumerable<IContentSource> contentSources)
    {
        foreach (var contentSource in contentSources)
        {
            if (!_contentSourcesByName.TryAdd(contentSource.ContentSourceName, contentSource))
            {
                throw new Exception($"Registered multiple mod sources with name: {contentSource.ContentSourceName}");
            }
        }
    }

    public IContentSource this[string contentSourceName]
    {
        get
        {
            if (_contentSourcesByName.TryGetValue(contentSourceName, out var source)) return source;
            
            throw new KeyNotFoundException($"No mod source with name {contentSourceName} found");
        }
    }
}