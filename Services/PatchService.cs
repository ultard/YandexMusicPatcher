using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using YandexMusicPatcher.Constants;

namespace YandexMusicPatcher.Services;

public static class PatchService
{
    public static bool InstallPatch(string modedAsarPath)
    {
        if (
            !File.Exists(modedAsarPath) || 
            !File.Exists(Paths.AsarPath) ||
            !File.Exists(Paths.YmExePath)
            ) return false;

        var oldHash = CalcAsarHeaderHash(Paths.AsarPath);
        var newHash = CalcAsarHeaderHash(modedAsarPath);
        
                Trace.WriteLine($"{oldHash} {newHash} {oldHash.Length} {newHash.Length}");
        if (oldHash.Length != newHash.Length) return false;
        if (oldHash == newHash) return true;
        
        CloseClients();
        
        if (!File.Exists(Path.Combine(Paths.TmpPath, "app.asar.bak")))
            File.Copy(Paths.AsarPath, Path.Combine(Paths.TmpPath, "app.asar.bak"), false);
        
        if (!File.Exists(Paths.BackupYmExePath))
            File.Copy(Paths.YmExePath, Paths.BackupYmExePath, false);
        
        if (!BypassAsarIntegrity(oldHash, newHash)) return false;
        
        File.Copy(modedAsarPath, Paths.AsarPath, true);
        OpenClient();
        return true;
    }
    
    public static bool UninstallPatch()
    {
        var appAsarBackupPath = Path.Combine(Paths.TmpPath, "app.asar.bak");
        
        if (
            !File.Exists(Paths.BackupYmExePath) || 
            !File.Exists(appAsarBackupPath)
            )
            return false;

        CloseClients();
        
        File.Copy(Paths.BackupYmExePath, Paths.YmExePath, true);
        File.Copy(appAsarBackupPath, Paths.AsarPath, true);
        
        OpenClient();
        return true;
    }

    private static void OpenClient()
    {
        var psi = new ProcessStartInfo("yandexmusic://")
        {
            UseShellExecute = true
        };

        Process.Start(psi);
    }
    
    private static void CloseClients()
    {
        foreach (var process in Process.GetProcessesByName("Яндекс Музыка"))
        {
            try { process.Kill(); process.WaitForExit(); } catch { }
        }
    }

    private static bool BypassAsarIntegrity(string oldHash, string newHash)
    {
        var fileBytes = File.ReadAllBytes(Paths.YmExePath);
        var oldBytes = Encoding.ASCII.GetBytes(oldHash);
        var newBytes = Encoding.ASCII.GetBytes(newHash);
        
        var count = 0;
        var offset = 0;
        while (true)
        {
            var idx = fileBytes.AsSpan(offset).IndexOf(oldBytes);
            if (idx == -1) break;
            idx += offset;
            Array.Copy(newBytes, 0, fileBytes, idx, newBytes.Length);
            count++;
            offset = idx + oldBytes.Length;
        }
        
        if (count == 0) return false;

        File.WriteAllBytes(Paths.YmExePath, fileBytes);
        return true;
    }

    private static string CalcAsarHeaderHash(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        var sizeBuf = new byte[8];
        if (fs.Read(sizeBuf, 0, 8) != 8)
        {
            throw new Exception("Unable to read header size");
        }
        
        var headerSize = BitConverter.ToUInt32(sizeBuf, 4);
        var headerPickleBuf = new byte[headerSize];
        if (fs.Read(headerPickleBuf, 0, (int)headerSize) != headerSize)
        {
            throw new Exception("Unable to read header");
        }
        
        var headerStringSize = BitConverter.ToUInt32(headerPickleBuf, 4);
        var headerString = Encoding.UTF8.GetString(headerPickleBuf, 8, (int)headerStringSize);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(headerString));
        return Convert.ToHexStringLower(hashBytes);
    }
}