using Clew.Application.Enums;

namespace Clew.Application.Exceptions;

public sealed class TranslationNotFoundException : Exception
{
    public TranslatableNamesCategory Category { get; init; }
    public string ContentSourceName { get; init; }
    public string NameToTranslate { get; init; }

    private TranslationNotFoundException(TranslatableNamesCategory category, string contentSourceName,
        string nameToTranslate, bool fromSpecificToCommon)
        : base(BuildMessage(category, contentSourceName, nameToTranslate, fromSpecificToCommon))
    {
        Category = category;
        ContentSourceName = contentSourceName;
        NameToTranslate = nameToTranslate;
    }

    public static TranslationNotFoundException FromSpecificToCommon(TranslatableNamesCategory category,
        string contentSourceName, string nameToTranslate)
    {
        return new TranslationNotFoundException(category, contentSourceName, nameToTranslate, true);
    }
    
    public static TranslationNotFoundException FromCommonToSpecific(TranslatableNamesCategory category,
        string contentSourceName, string nameToTranslate)
    {
        return new TranslationNotFoundException(category, contentSourceName, nameToTranslate, false);
    }

    private static string BuildMessage(TranslatableNamesCategory category, string contentSourceName,
        string nameToTranslate, bool fromSpecificToCommon)
    {
        var direction = fromSpecificToCommon ? "specific to common" : "common to specific";

        return
            $"Translation from {direction} for '{nameToTranslate}' in category '{category}' for source '{contentSourceName}' not found.";
    }
}