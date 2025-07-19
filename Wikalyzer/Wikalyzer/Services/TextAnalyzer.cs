using System.Linq;
using System.Text.RegularExpressions;
using Wikalyzer.Models;

namespace Wikalyzer.Services;

public class TextAnalyzer
{
    public TextStats Analyze(string text)
    {
        var sentences = Regex.Split(text, "\r\n|\r|\n");
        var words = Regex.Split(text.ToLower(), @"\W+").Where(w => w.Length > 0).ToArray();
        var topWords = words.GroupBy(w => w)
            . OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        return new TextStats()
        {
            WordCount = words.Length,
            SentenceCount = sentences.Length,
            AvgWordLength = words.Average(w => w.Length),
            AvgSentenceLength = (double)words.Length / sentences.Length,
            WordDiversity = (double)words.Distinct().Count() / words.Length,
            TopWords = topWords
        };
    }
}