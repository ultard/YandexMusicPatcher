using System.Text.Json.Serialization;

namespace YandexMusicPatcher.Models;

public class ReleaseMetadata
{
    [JsonPropertyName("assets")]
    public ReleaseAsset[]? Assets { get; set; }
}