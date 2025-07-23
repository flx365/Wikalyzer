// Services/HistoryService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Wikalyzer.Services;

/// <summary>
/// Speichert die Historie von Offline‑Analysen und Online‑Suchen persistent im Dateisystem.
/// </summary>
public class HistoryService
{
    private const string FolderName = "Wikalyzer";
    private const string FileName   = "history.json";

    public static HistoryService Instance { get; } = new();

    private readonly string _filePath;
    private readonly List<string> _offlineHistory = new();
    private readonly List<string> _onlineHistory  = new();

    public event Action<string>? OfflineAdded;
    public event Action<string>? OnlineAdded;

    private HistoryService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder  = Path.Combine(appData, FolderName);
        Directory.CreateDirectory(folder);
        _filePath   = Path.Combine(folder, FileName);

        LoadFromDisk();
    }

    public IReadOnlyList<string> OfflineHistory => _offlineHistory;
    public IReadOnlyList<string> OnlineHistory  => _onlineHistory;

    public void AddOfflineAnalysis(string summary)
    {
        _offlineHistory.Insert(0, summary);
        OfflineAdded?.Invoke(summary);
        SaveToDisk();
    }

    public void AddOnlineSearch(string query)
    {
        _onlineHistory.Insert(0, query);     // ← korrekte Einfügung ganz oben
        OnlineAdded?.Invoke(query);
        SaveToDisk();
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var json = File.ReadAllText(_filePath);
            var dto  = JsonSerializer.Deserialize<HistoryDto>(json);
            if (dto?.Offline != null)
                _offlineHistory.AddRange(dto.Offline);
            if (dto?.Online != null)
                _onlineHistory.AddRange(dto.Online);
        }
        catch
        {
            // Falls Deserialisierung fehlschlägt, Liste einfach leer lassen
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var dto = new HistoryDto
            {
                Offline = _offlineHistory,
                Online  = _online_history_clone()
            };
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Fehler beim Speichern ignorieren oder optional loggen
        }
    }

    private record HistoryDto
    {
        public List<string>? Offline { get; init; }
        public List<string>? Online  { get; init; }
    }

    // Hilfsmethode, um Immutable Copy für DTO zu erstellen
    private List<string> _online_history_clone() => new(_onlineHistory);
}
