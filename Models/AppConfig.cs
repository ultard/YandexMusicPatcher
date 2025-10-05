namespace YandexMusicPatcher.Models;

public class AppConfig
{
    public string YmExePath { get; set; } = string.Empty;
    public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.Full;
    public bool KeepAsarFile { get; set; } = false;
}