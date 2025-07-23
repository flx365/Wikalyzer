namespace Wikalyzer.Models
{
    /// <summary>
    /// Ergebnis einer einzelnen Suchseite:
    /// Titel, kurzer Extract und URL zum Thumbnail.
    /// </summary>
    public class ArticleSearchResult
    {
        public string  Title        { get; set; } = "";
        public string  Summary      { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
    }
}