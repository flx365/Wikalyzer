using System;
using System.Globalization;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Wikalyzer.Converters;

/// <summary>
/// Lädt eine HTTP/HTTPS-URL herunter und liefert eine <see cref="Bitmap"/>
/// für <c>Image.Source</c>.  Bei Fehlern → <c>null</c>.
/// </summary>
public class UrlToBitmapConverter : IValueConverter
{
    // global geteilter HttpClient
    private static readonly HttpClient Http = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            // Download synchron (kleine Thumbnails → OK)
            var stream = Http.GetStreamAsync(url)
                .GetAwaiter()
                .GetResult();

            return new Bitmap(stream);
        }
        catch
        {
            // Bild konnte nicht geladen werden → kein Crash, kein Binding-Fehler
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}