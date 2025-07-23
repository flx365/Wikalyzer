// Services/WikiRestClient.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wikalyzer.Models;

namespace Wikalyzer.Services
{
    /// <summary>
    /// MediaWiki-Client auf Basis der Action-API  
    /// (action=query&generator=search)  
    /// • liefert Extract + Thumbnail (pageimages) für Artikel  
    /// • liefert Original-Bild (imageinfo) für Dateiseiten  
    /// • bewahrt die API-Suchreihenfolge (Feld <c>index</c>)  
    /// Zusätzlich:
    /// • GetArticleHtmlAsync holt den reinen HTML-Artikel  
    /// • GetArticlePlainTextAsync wandelt HTML per Regex in formatierten Text um
    /// </summary>
    public class WikiRestClient
    {
        private readonly HttpClient _http;
        private readonly string     _langCode;

        public WikiRestClient(WikiLanguage lang)
        {
            _langCode = lang.ToCode(); // z.B. "de"
            _http     = new HttpClient
            {
                BaseAddress = new Uri($"https://{_langCode}.wikipedia.org/")
            };
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Wikalyzer/1.0");
            _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        /// <summary>
        /// Führt eine Suche aus und gibt Artikel + Vorschaubilder zurück.
        /// </summary>
        public async Task<IEnumerable<ArticleSearchResult>> SearchWithThumbnailsAsync(
            string term,
            int    namespaceId = 0,
            int    limit       = 10,
            int    thumbSize   = 150)
        {
            var sb = new StringBuilder("w/api.php?action=query");
            sb.Append("&format=json")
              .Append("&generator=search")
              .Append("&gsrsearch=").Append(Uri.EscapeDataString(term))
              .Append("&gsrlimit=").Append(limit);
            if (namespaceId >= 0)
                sb.Append("&gsrnamespace=").Append(namespaceId);

            sb.Append("&prop=extracts|pageimages|imageinfo")
              .Append("&exintro=1&explaintext=1")
              .Append("&piprop=thumbnail|original")
              .Append("&pithumbsize=").Append(thumbSize)
              .Append("&iiprop=url");

            var url = sb.ToString();
            Console.WriteLine($"[WikiRestClient] GET {_http.BaseAddress}{url}");

            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[WikiRestClient] HTTP-Fehler: {ex.Message}");
                return Array.Empty<ArticleSearchResult>();
            }

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[WikiRestClient] Status {(int)response.StatusCode}: {err}");
                return Array.Empty<ArticleSearchResult>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("query",  out var q) ||
                !q.TryGetProperty("pages",                out var pages))
                return Array.Empty<ArticleSearchResult>();

            var orderedPages = pages.EnumerateObject()
                                    .Select(p => p.Value)
                                    .Select(p => new
                                    {
                                        Json  = p,
                                        Index = p.TryGetProperty("index", out var idx)
                                                ? idx.GetInt32()
                                                : int.MaxValue
                                    })
                                    .OrderBy(x => x.Index)
                                    .Select(x => x.Json);

            var results = new List<ArticleSearchResult>();

            foreach (var p in orderedPages)
            {
                var title   = p.GetProperty("title").GetString() ?? "";
                var summary = p.TryGetProperty("extract", out var ex)
                                  ? ex.GetString() ?? ""
                                  : "";
                summary = Regex.Replace(summary, "<.*?>", "");

                string? thumb = null;
                if (p.TryGetProperty("thumbnail", out var tb) &&
                    tb.TryGetProperty("source", out var tbSrc))
                {
                    thumb = tbSrc.GetString();
                }
                else if (p.TryGetProperty("imageinfo", out var ii) &&
                         ii.ValueKind == JsonValueKind.Array &&
                         ii[0].TryGetProperty("url", out var urlProp))
                {
                    thumb = urlProp.GetString();
                }
                else if (namespaceId == 6)
                {
                    var fileName = title.StartsWith("Datei:")
                        ? title["Datei:".Length..]
                        : title;
                    thumb = $"https://{_langCode}.wikipedia.org/wiki/Special:FilePath/{Uri.EscapeDataString(fileName)}";
                }

                results.Add(new ArticleSearchResult
                {
                    Title        = title,
                    Summary      = summary,
                    ThumbnailUrl = thumb
                });
            }

            Console.WriteLine($"[WikiRestClient] Parsed {results.Count} items.");
            return results;
        }

        /// <summary>
        /// Holt den reinen HTML-Artikel über die REST-API.
        /// </summary>
        public async Task<string> GetArticleHtmlAsync(string title)
        {
            var restUrl = $"api/rest_v1/page/html/{Uri.EscapeDataString(title)}";
            HttpResponseMessage resp;
            try
            {
                resp = await _http.GetAsync(restUrl);
            }
            catch
            {
                return string.Empty;
            }

            if (!resp.IsSuccessStatusCode)
                return string.Empty;

            return await resp.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Wandelt den HTML-Artikel per Regex in formatierten Plain‑Text um
        /// (Überschriften in Großbuchstaben, Absätze mit Leerzeilen).
        /// </summary>
        public async Task<string> GetArticlePlainTextAsync(string title)
        {
            var html = await GetArticleHtmlAsync(title);
            if (string.IsNullOrWhiteSpace(html))
                return "(Artikel konnte nicht geladen werden)";

            // Regex für <h1>, <h2> und <p>
            var pattern = @"<(h[12])\b[^>]*>(.*?)</\1>|<p\b[^>]*>(.*?)</p>";
            var regex   = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var sb      = new StringBuilder();

            foreach (Match m in regex.Matches(html))
            {
                // Überschrift-Gruppe
                if (m.Groups[2].Success)
                {
                    var heading = WebUtility.HtmlDecode(m.Groups[2].Value.Trim());
                    if (string.IsNullOrEmpty(heading)) continue;

                    sb.AppendLine();
                    sb.AppendLine(heading.ToUpperInvariant());
                    sb.AppendLine(new string('-', heading.Length));
                }
                // Absatz-Gruppe
                else if (m.Groups[3].Success)
                {
                    var para = WebUtility.HtmlDecode(m.Groups[3].Value.Trim());
                    if (string.IsNullOrEmpty(para)) continue;

                    sb.AppendLine(para);
                    sb.AppendLine();
                }
            }

            return sb.ToString().Trim();
        }
    }
}
