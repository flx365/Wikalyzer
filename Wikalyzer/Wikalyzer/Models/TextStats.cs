using System.Collections.Generic;

namespace Wikalyzer.Models;


public class TextStats
{
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public double AverageWordLength { get; set; }
    public double AverageSentenceLength { get; set; }
    public double LexicalDiversity { get; set; }  // Wortvielfalt in %
    public string LongestSentence { get; set; } = "";
    public string ShortestSentence { get; set; } = "";
    public double ReadingTimeMinutes { get; set; }
    public double FleschReadingEase { get; set; }
    public List<string> TopWords { get; set; } = new();
}
