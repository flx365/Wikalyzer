namespace Wikalyzer.Services
{
    /// <summary>
    /// Unterstützte Wikipedia-Sprachen.
    /// </summary>
    public enum WikiLanguage
    {
        De,
        En
    }

    public static class WikiLanguageExtensions
    {
        /// <summary>
        /// Liefert den Sprachcode für die REST-API (z.B. "de", "en").
        /// </summary>
        public static string ToCode(this WikiLanguage lang) => lang switch
        {
            WikiLanguage.De => "de",
            WikiLanguage.En => "en",
            _               => "de"
        };
    }
}