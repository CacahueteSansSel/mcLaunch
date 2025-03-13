using System.Security.Cryptography;
using System.Text;

namespace mcLaunch.Launchsite.Cache;

public static class Encryption
{
    public static (byte[] data, byte[] iv) Encrypt(string key, byte[] data)
    {
        byte[] iv = Guid.NewGuid().ToByteArray();

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream stream = new();
        using CryptoStream cryptoStream = new(stream, encryptor, CryptoStreamMode.Write);
        using BinaryWriter writer = new(cryptoStream);

        writer.Write(data);

        cryptoStream.FlushFinalBlock();

        return (stream.ToArray(), iv);
    }

    public static byte[] Decrypt(string key, byte[] iv, byte[] data)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream stream = new(data);
        using CryptoStream cryptoStream = new(stream, decryptor, CryptoStreamMode.Read);

        List<byte> buffer = new();
        while (true)
        {
            int b = cryptoStream.ReadByte();
            if (b == -1) break;

            buffer.Add((byte)b);
        }

        return buffer.ToArray();
    }
}