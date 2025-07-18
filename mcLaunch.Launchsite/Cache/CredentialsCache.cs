﻿using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace mcLaunch.Launchsite.Cache;

public class CredentialsCache
{
    private readonly string key;

    public CredentialsCache(string rootPath, string key)
    {
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

        RootPath = rootPath;
        this.key = key;
    }

    public string RootPath { get; }

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

        (byte[] data, byte[] iv) result =
            Encryption.Encrypt(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)));

        File.WriteAllBytes($"{RootPath}/{filename}", result.data);
        File.WriteAllBytes($"{RootPath}/{ivFilename}", result.iv);
    }

    public void ClearAll()
    {
        foreach (string filename in Directory.GetFiles(RootPath))
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception e)
            {
            }
        }
    }

    public bool Clear(string name)
    {
        using SHA256 sha = SHA256.Create();

        string filename = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(name)))
            .ToLower();

        string ivFilename = Convert
            .ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(filename)))
            .ToLower();

        try
        {
            File.Delete($"{RootPath}/{filename}");
            File.Delete($"{RootPath}/{ivFilename}");

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}