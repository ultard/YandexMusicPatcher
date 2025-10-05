using System.IO;

namespace YandexMusicPatcher.Constants;

public struct Paths
{
    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string YmPath = Path.Combine(LocalAppData, "Programs", "YandexMusic");
    public static readonly string YmExePath = Path.Combine(YmPath, "Яндекс Музыка.exe");
    public static readonly string AsarPath = Path.Combine(YmPath, "resources", "app.asar");

    public static readonly string TmpPath = Path.Combine(Path.GetTempPath(), "YandexMusicMod");
    public static readonly string BackupYmExePath = Path.Combine(TmpPath, "Яндекс Музыка.exe.bak");
    public static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
}