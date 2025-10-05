using System.IO;
using System.Text.Json;
using YandexMusicPatcher.Constants;
using YandexMusicPatcher.Models;

namespace YandexMusicPatcher.Services;

public static class ConfigService
{
    public static AppConfig LoadConfig()
    {
        if (!File.Exists(Paths.ConfigPath)) return CreateDefaultConfig();

        var configJson = File.ReadAllText(Paths.ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(configJson) ?? new AppConfig();
    }
    
    public static void UpdateField<T>(string fieldName, T value)
    {
        var config = LoadConfig();
        var property = typeof(AppConfig).GetProperty(fieldName);
        if (property == null || property.PropertyType != typeof(T)) return;
        
        property.SetValue(config, value);
        SaveConfig(config);
    }
    
    private static AppConfig CreateDefaultConfig()
    {
        var defaultConfig = new AppConfig();
        SaveConfig(defaultConfig);
        return defaultConfig;
    }

    private static void SaveConfig(AppConfig config)
    {
        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Paths.ConfigPath, configJson);
    }
}