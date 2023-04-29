using System.Runtime.Intrinsics.Arm;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cacahuete.MinecraftLib.Cache;

public class CredentialsCache
{
    public string RootPath { get; private set; }
    private string key;

    public CredentialsCache(string rootPath, string key)
    {
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        
        RootPath = rootPath;
        this.key = key;
    }

    public T? Get<T>(string name)
    {
        using SHA256 sha = SHA256.Create();

        string filename = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(name)))
            .ToLower();

        string ivFilename = Convert
            .ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(filename)))
            .ToLower();

        if (!File.Exists($"{RootPath}/{filename}") || !File.Exists($"{RootPath}/{ivFilename}")) 
            return default;

        byte[] iv = File.ReadAllBytes($"{RootPath}/{ivFilename}");
        byte[] encData = File.ReadAllBytes($"{RootPath}/{filename}");

        byte[] decData = Encryption.Decrypt(key, iv, encData);

        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(decData));
    }

    public void Set<T>(string name, T data)
    {
        using SHA256 sha = SHA256.Create();

        string filename = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(name)))
            .ToLower();

        string ivFilename = Convert
            .ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(filename)))
            .ToLower();

        var result = Encryption.Encrypt(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)));
        
        File.WriteAllBytes($"{RootPath}/{filename}", result.data);
        File.WriteAllBytes($"{RootPath}/{ivFilename}", result.iv);
    }
}