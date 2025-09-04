using Clew.Application.Enums;

namespace Clew.Infrastructure.Abstractions;

internal interface IContentSourceNamingsTranslator
{
    string GetCommonName(TranslatableNamesCategory category, string contentSourceName, string specificName);
    string GetSpecificName(TranslatableNamesCategory category, string contentSourceName, string commonName);
}