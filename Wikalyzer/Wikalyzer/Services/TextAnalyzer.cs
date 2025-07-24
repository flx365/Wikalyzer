using System.Linq;
using System.Text.RegularExpressions;
using Wikalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wikalyzer.Models;

namespace Wikalyzer.Services;

public class TextAnalyzer
{
    public TextStats Analyze(string text)
    {
        var stats = new TextStats();

        var sentences = Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var words = Regex.Matches(text, @"\b[\wäöüÄÖÜß]{2,}\b").Select(m => m.Value).ToList();

        stats.WordCount = words.Count;
        stats.SentenceCount = sentences.Count;
        stats.AverageWordLength = words.Any() ? words.Average(w => w.Length) : 0;
        stats.AverageSentenceLength = sentences.Any() ? words.Count / (double)sentences.Count : 0;

        stats.LexicalDiversity = words.Distinct(StringComparer.OrdinalIgnoreCase).Count() / (double)(words.Count + 1) * 100;

        stats.LongestSentence = sentences.OrderByDescending(s => s.Length).FirstOrDefault() ?? "";
        stats.ShortestSentence = sentences.OrderBy(s => s.Length).FirstOrDefault() ?? "";

        stats.ReadingTimeMinutes = Math.Round(words.Count / 200.0, 2); // 200 WpM (Wörter pro Minute)

        stats.FleschReadingEase = CalculateFleschEase(words, sentences);

        stats.TopWords = words
            .GroupBy(w => w.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(10)
            .Select(g => $"{g.Key} ({g.Count()})")
            .ToList();

        return stats;
    }

    private double CalculateFleschEase(List<string> words, List<string> sentences)
    {
        // Flesch-Wert für Deutsch: 180 - ASL - (58.5 * ASW)
        // ASL = Durchschnittliche Satzlänge, ASW = Durchschnittliche Silbenanzahl pro Wort

        double asl = sentences.Any() ? words.Count / (double)sentences.Count : 0;
        double asw = words.Any() ? words.Average(w => CountSyllables(w)) : 0;

        return Math.Round(180 - asl - (58.5 * asw), 2);
    }

    private int CountSyllables(string word)
    {
        // Grobe Heuristik für deutsche Silbenzählung:
        return Regex.Matches(word.ToLower(), @"[aeiouyäöü]+").Count;
    }
}
