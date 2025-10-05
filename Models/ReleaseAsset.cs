using System.Text.Json.Serialization;

namespace YandexMusicPatcher.Models;

public class ReleaseAsset
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("browser_download_url")]
    public required string BrowserDownloadUrl { get; set; }
}