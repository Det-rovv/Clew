using Clew.Application.Enums;

namespace Clew.Infrastructure.Settings;

internal sealed class ContentSourceNamingsSettings : 
    Dictionary<TranslatableNamesCategory, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>;