// Services/WikiRestClient.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wikalyzer.Models;

namespace Wikalyzer.Services;

/// <summary>
/// MediaWiki-Client auf Basis der Action-API  
/// (action=query&amp;generator=search)  
/// • liefert Extract + Thumbnail (pageimages) für Artikel  
/// • liefert Original-Bild (imageinfo) für Dateiseiten  
/// • bewahrt die API-Suchreihenfolge (Feld <c>index</c>)  
/// </summary>
public class WikiRestClient
{
    private readonly HttpClient _http;
    private readonly string     _langCode;

    public WikiRestClient(WikiLanguage lang)
    {
        _langCode = lang.ToCode();                // z.B. "de"
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
        // ───── 1) URL bauen ───────────────────────────────────────────
        var sb = new System.Text.StringBuilder("w/api.php?action=query");
        sb.Append("&format=json");
        sb.Append("&generator=search");
        sb.Append("&gsrsearch=").Append(Uri.EscapeDataString(term));
        sb.Append("&gsrlimit=").Append(limit);
        if (namespaceId >= 0)
            sb.Append("&gsrnamespace=").Append(namespaceId);

        // extracts + pageimages + imageinfo
        sb.Append("&prop=extracts|pageimages|imageinfo");
        sb.Append("&exintro=1&explaintext=1");
        sb.Append("&piprop=thumbnail|original");
        sb.Append("&pithumbsize=").Append(thumbSize);
        sb.Append("&iiprop=url"); // Bild‐URL für Datei-Seiten

        var url = sb.ToString();
        Console.WriteLine($"[WikiRestClient] GET {_http.BaseAddress}{url}");

        // ───── 2) HTTP-Anfrage ────────────────────────────────────────
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

        // ───── 3) JSON parsen ─────────────────────────────────────────
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (!doc.RootElement.TryGetProperty("query",  out var q) ||
            !q.TryGetProperty("pages",                out var pages))
            return Array.Empty<ArticleSearchResult>();

        // nach Suchreihenfolge sortieren (Feld 'index')
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
            // Titel
            var title = p.GetProperty("title").GetString() ?? "";

            // Extract
            var summary = p.TryGetProperty("extract", out var ex)
                            ? ex.GetString() ?? ""
                            : "";
            summary = Regex.Replace(summary, "<.*?>", "");

            // Bild-URL
            string? thumb = null;

            // pageimages-Thumbnail
            if (p.TryGetProperty("thumbnail", out var tb) &&
                tb.TryGetProperty("source", out var tbSrc))
            {
                thumb = tbSrc.GetString();
            }
            // imageinfo-Original (Datei-Namespace)
            else if (p.TryGetProperty("imageinfo", out var ii) &&
                     ii.ValueKind == JsonValueKind.Array &&
                     ii[0].TryGetProperty("url", out var urlProp))
            {
                thumb = urlProp.GetString();
            }
            // letzter Fallback (Special:FilePath)
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
}
