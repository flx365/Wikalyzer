using System.Collections.Generic;

namespace Wikalyzer.Models;


public class TextStats
{
    public int WordCount { get; set; }
    public int SentenceCount { get; set; }
    public double AvgWordLength { get; set; }
    public double AvgSentenceLength { get; set; }
    public double WordDiversity { get; set; }
    public Dictionary<string, int> TopWords { get; set; } = new();
}