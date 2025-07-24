using System;
using System.Globalization;
using System.Net.Http;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Wikalyzer.Converters
{
    /// <summary>
    /// Lädt ein Bild von einer HTTP/HTTPS-URL herunter und gibt eine Bitmap zurück,
    /// die direkt als Image.Source verwendet werden kann. Bei Problemen wird einfach null zurückgegeben.
    /// </summary>
    public class UrlToBitmapConverter : IValueConverter
    {
        // Ein einziger HttpClient für alle Aufrufe
        private static readonly HttpClient HttpClient = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string url || string.IsNullOrWhiteSpace(url))
                return null; // keine gültige URL, also kein Bild

            try
            {
                // Lade das Bild-Stream synchron – Thumbnails sind klein, daher in Ordnung
                using var stream = HttpClient.GetStreamAsync(url).GetAwaiter().GetResult();
                return new Bitmap(stream);
            }
            catch
            {
                // Falls etwas schiefgeht (Netzwerk, ungültiges Format etc.),
                // einfach null zurückgeben und weiterlaufen
                return null;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Rückwärtskonvertierung wird nicht unterstützt.");
        }
    }
}