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
using HtmlAgilityPack;
using Markdig;

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
        private readonly string _langCode;

        public WikiRestClient(WikiLanguage lang)
        {
            _langCode = lang.ToCode(); // z.B. "de"
            _http = new HttpClient
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
            int namespaceId = 0,
            int limit = 10,
            int thumbSize = 150)
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

            if (!doc.RootElement.TryGetProperty("query", out var q) ||
                !q.TryGetProperty("pages", out var pages))
                return Array.Empty<ArticleSearchResult>();

            var orderedPages = pages.EnumerateObject()
                .Select(p => p.Value)
                .Select(p => new
                {
                    Json = p,
                    Index = p.TryGetProperty("index", out var idx)
                        ? idx.GetInt32()
                        : int.MaxValue
                })
                .OrderBy(x => x.Index)
                .Select(x => x.Json);

            var results = new List<ArticleSearchResult>();

            foreach (var p in orderedPages)
            {
                var title = p.GetProperty("title").GetString() ?? "";
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
                    Title = title,
                    Summary = summary,
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
        private static string FormatToPlainText(string markdown)
        {
            var lines = markdown.Split('\n');
            var sb = new StringBuilder();
            bool inList = false;

            foreach (var line in lines.Select(l => l.Trim()))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (inList)
                    {
                        sb.AppendLine(); // Liste beenden
                        inList = false;
                    }

                    sb.AppendLine(); // Absatztrennung
                    continue;
                }

                // == Formatierungen ==
                // Fett (Überschriften als Markdown-Fett → fett + unterstrichen)
                if (line.StartsWith("**") && line.EndsWith("**"))
                {
                    var heading = line.Trim('*').Trim();
                    sb.AppendLine(heading);
                    sb.AppendLine(new string('─', heading.Length)); // Unicode-Strich
                }
                // Aufzählung
                else if (line.StartsWith("• "))
                {
                    inList = true;
                    sb.AppendLine("  • " + line[2..].Trim());
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString().TrimEnd();
        }


        /// <summary>
        /// Wandelt den HTML-Artikel per Regex in formatierten Plain‑Text um
        /// (Überschriften in Großbuchstaben, Absätze mit Leerzeilen).
        /// </summary>
       public async Task<string> GetArticleMarkdownAsync(string title)
        {
            var html = await GetArticleHtmlAsync(title);
            if (string.IsNullOrWhiteSpace(html))
                return "(Artikel konnte nicht geladen werden)";

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sb = new StringBuilder();
            var body = doc.DocumentNode.SelectSingleNode("//body");
            if (body == null)
                return "(Kein lesbarer Inhalt)";

            foreach (var node in body.Descendants())
            {
                switch (node.Name)
                {
                    case "h1":
                    case "h2":
                    case "h3":
                        var heading = HtmlEntity.DeEntitize(node.InnerText.Trim());
                        if (heading.Length > 0)
                        {
                            sb.AppendLine();
                            sb.AppendLine("## " + heading);  // Markdown Heading
                            sb.AppendLine();
                        }
                        break;

                    case "p":
                        var para = ProcessMarkdownInline(node);
                        if (!string.IsNullOrWhiteSpace(para))
                        {
                            sb.AppendLine(para);
                            sb.AppendLine();
                        }
                        break;

                    case "ul":
                        foreach (var li in node.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            var item = ProcessMarkdownInline(li);
                            sb.AppendLine("- " + item);
                        }
                        sb.AppendLine();
                        break;
                }
            }

            return sb.ToString().Trim();
        }

        private static string ProcessMarkdownInline(HtmlNode node)
        {
            var sb = new StringBuilder();

            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    sb.Append(HtmlEntity.DeEntitize(child.InnerText));
                }
                else if (child.Name is "b" or "strong")
                {
                    sb.Append("**").Append(ProcessMarkdownInline(child)).Append("**");
                }
                else if (child.Name is "i" or "em")
                {
                    sb.Append("*").Append(ProcessMarkdownInline(child)).Append("*");
                }
                else if (child.Name == "a")
                {
                    // Text anzeigen, Link auslassen
                    sb.Append(ProcessMarkdownInline(child));
                }
                else
                {
                    sb.Append(ProcessMarkdownInline(child));
                }
            }

            return sb.ToString();
        }

        private static string ProcessInlineNodes(HtmlNode node)
        {
            var sb = new StringBuilder();

            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    sb.Append(HtmlEntity.DeEntitize(child.InnerText));
                }
                else if (child.Name is "b" or "strong")
                {
                    sb.Append("**").Append(ProcessInlineNodes(child)).Append("**");
                }
                else if (child.Name is "i" or "em")
                {
                    sb.Append("_").Append(ProcessInlineNodes(child)).Append("_");
                }
                else if (child.Name == "a")
                {
                    // LINKTEXT JA, aber URL NEIN – also nur den Textinhalt übernehmen
                    var text = ProcessInlineNodes(child).Trim();
                    if (!string.IsNullOrEmpty(text))
                        sb.Append(text);
                }
                else
                {
                    sb.Append(ProcessInlineNodes(child));
                }
            }

            return sb.ToString();
        }

    }
}
