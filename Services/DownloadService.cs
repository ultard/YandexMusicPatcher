using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using YandexMusicPatcher.Constants;
using YandexMusicPatcher.Models;

namespace YandexMusicPatcher.Services;

public static class DownloadService
{
    public static async Task DownloadFileAsync(string url, string destinationPath)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var sourceStream = await response.Content.ReadAsStreamAsync();
        await using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await sourceStream.ReadAsync(buffer)) > 0)
        {
            await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead));
        }
    }

    public static async Task<ReleaseMetadata?> GetMetadata()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YandexMusicPatcher", "1.0"));
        
        using var response = await client.GetAsync(Urls.LatestModReleaseUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ReleaseMetadata>(json)!;
    }

    public static void DecompressGz(string gzPath, string outPath)
    {
        using var inputFile = new FileStream(gzPath, FileMode.Open);
        using var gzipStream = new GZipStream(inputFile, CompressionMode.Decompress);
        using var outputFile = new FileStream(outPath, FileMode.Create);

        gzipStream.CopyTo(outputFile);
    }
}