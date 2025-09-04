using Clew.Application.Enums;
using Clew.Infrastructure.Abstractions;

namespace Clew.Infrastructure.Extensions;

internal static class TranslationExtensions
{
    public static string GetCommonPlatformName(this IContentSourceNamingsTranslator translator,
        string contentSourceName, string specificPlatformName)
    {
        return translator.GetCommonName(TranslatableNamesCategory.Platforms, contentSourceName, specificPlatformName);
    }
    
    public static string GetCommonReleaseChannelName(this IContentSourceNamingsTranslator translator,
        string contentSourceName, string specificReleaseChannelName)
    {
        return translator.GetCommonName(TranslatableNamesCategory.ReleaseChannels, contentSourceName, specificReleaseChannelName);
    }
    
    public static string GetCommonRelationTypeName(this IContentSourceNamingsTranslator translator,
        string contentSourceName, string specificRelationTypeName)
    {
        return translator.GetCommonName(TranslatableNamesCategory.RelationTypes, contentSourceName, specificRelationTypeName);
    }
}