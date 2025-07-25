namespace Wikalyzer.Models
{
    /// <summary>
    /// Ergebnis einer einzelnen Suchseite:
    /// Titel, kurzer Extract URL zum Thumbnail und PageUrl.
    /// </summary>
    public class ArticleSearchResult
    {
        public string  Title        { get; set; } = "";
        public string  Summary      { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public string PageUrl      { get; set; } = "";
    }
}