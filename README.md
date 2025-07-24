# Wikalyzer

Wikalyzer ist ein plattform­unabhängiges C#-Tool, das Wikipedia-Artikel automatisch abruft, statistisch auswertet (Wort-/Satzanzahl, Wortvielfalt, Top-Wörter u. a.)
und die Ergebnisse in einer mit Avalonia UI erstellten grafischen Benutzeroberfläche anzeigt. 

Eine ausführliche Projektbeschreibung und Dokumentation findest du im **[Projekt-Wiki](../../wiki)**.

---
| Einsatzzweck                         | Verwendetes Modell     | Erfahrung / Bewertung der Nützlichkeit                                       |
|-------------------------------------|-------------------------|-------------------------------------------------------------------------------|
| Anforderungsanalyse                 | ChatGPT-4o        |   Grobe Strukturierung möglich aber wenig reeler Bezug            |
| Architekturvorschläge / Designideen | ChatGPT-4o              | Hilfreiche Modularisierungsvorschläge, oft generisch, aber anpassbar.       |
| Code-Generierung (z. B. Klassen, Methoden) | ChatGPT-4o        | Nützlich bei sehr komplexen Komponenten (z. B. Markdown-Parser, REST-Client-Wrapper). Der generierte Code ist oft **funktional**, aber enthält **Redundanz**, **fehlende Fehlerbehandlung** oder **ungeeignete Namensgebung**. Für produktiven Einsatz war stets manuelle Nacharbeit nötig. Besonders hilfreich bei der **strukturellen Vorlage** oder zum Verstehen von Algortihmen – **nicht** geeignet zum vollständigen und direkten Übernehmen.                   |                                                                               |
| Testfallgenerierung (z. B. Unit Tests)     |                     |    -                                                                           |
| Refactoring-Vorschläge              |                         |       -                                                                        |
| Code Review                         | ChatGPT-03             | relativ gute Erkennung von Redundanz und Verbesserungspotenzial bzgl. Clean Code.            |
| Dokumentation (README, API-Doku etc.) | ChatGPT-4o              | Sehr nützlich für klare Struktur und Formatierung in Markdown.               |
| Fehlersuche / Debugginghilfe        | ChatGPT- 04-mini-high             | Meistens gute Hinweise auf mögliche Ursachen und API-Probleme. Teilweise völlig neue Codevorschläge die der Fuktionalität mehr schaden als nützen                |
| Versionsverwaltung (Git-Strategien) | ChatGPT-4o              | gut funktionierende Vorschläge für branch Struktur             |
