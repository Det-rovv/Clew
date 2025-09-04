using System.Collections.Immutable;
using Clew.Application.Enums;
using Clew.Application.Exceptions;
using Clew.Infrastructure.Abstractions;
using Clew.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Clew.Infrastructure.Services;

internal sealed class ContentSourceNamingsTranslator : IContentSourceNamingsTranslator
{
    private readonly IReadOnlyDictionary<TranslatableNamesCategory,
        IReadOnlyDictionary<(string contentSourceName, string commonName), string>> _specificNames;
    
    private readonly IReadOnlyDictionary<TranslatableNamesCategory,
        IReadOnlyDictionary<(string contentSourceName, string specificName), string>> _commonNames;
    
    public ContentSourceNamingsTranslator(IOptions<ContentSourceNamingsSettings> namingsSettings)
    {
        var settings = namingsSettings.Value;

        var specificNames = new Dictionary<TranslatableNamesCategory,
            IReadOnlyDictionary<(string contentSourceName, string commonName), string>>();
        
        var commonNames = new Dictionary<TranslatableNamesCategory,
            IReadOnlyDictionary<(string contentSourceName, string specificName), string>>();

        foreach (var category in settings.Keys)
        {
            var (specificCategoryNames, commonCategoryNames) = TransformToSeparateDicts(settings[category]);
            
            specificNames[category] = specificCategoryNames;
            commonNames[category] = commonCategoryNames;
        }
        
        _specificNames = specificNames;
        _commonNames = commonNames;
    }

    private (IReadOnlyDictionary<(string contentSourceName, string commonName), string>,
        IReadOnlyDictionary<(string contentSourceName, string specificName), string>)
        TransformToSeparateDicts(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> source)
    {
        var specificNames = new Dictionary<(string contentSourceName, string commonName), string>(new StringTupleIgnoreCaseComparer());
        var commonNames = new Dictionary<(string contentSourceName, string specificName), string>(new StringTupleIgnoreCaseComparer());

        foreach (var pair in source)
        {
            var commonName = pair.Key;

            foreach (var sourceAndSpecific in pair.Value)
            {
                var contentSourceName = sourceAndSpecific.Key;
                var specificName = sourceAndSpecific.Value;
                
                specificNames.Add((contentSourceName, commonName), specificName);
                commonNames.Add((contentSourceName, specificName), commonName);
            }
        }
        
        return (specificNames.ToImmutableDictionary(), commonNames.ToImmutableDictionary());
    }
    
    public string GetCommonName(TranslatableNamesCategory category, string contentSourceName, string specificName)
    {
        return _commonNames.GetValueOrDefault(category)?.GetValueOrDefault((contentSourceName, specificName))
            ?? throw TranslationNotFoundException.FromSpecificToCommon(category, contentSourceName, specificName);
    }

    public string GetSpecificName(TranslatableNamesCategory category, string contentSourceName, string commonName)
    {
        return _specificNames.GetValueOrDefault(category)?.GetValueOrDefault((contentSourceName, commonName))
            ?? throw TranslationNotFoundException.FromCommonToSpecific(category, contentSourceName, commonName);
    }
    
    private sealed class StringTupleIgnoreCaseComparer : IEqualityComparer<(string, string)>
    {
        public bool Equals((string, string) x, (string, string) y)
        {
            return x.Item1.Equals(y.Item1, StringComparison.OrdinalIgnoreCase) &&
                   x.Item2.Equals(y.Item2, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode((string, string) obj)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2));
        }
    }
}