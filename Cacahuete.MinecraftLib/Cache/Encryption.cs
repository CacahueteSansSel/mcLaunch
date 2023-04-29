using System.Security.Cryptography;
using System.Text;

namespace Cacahuete.MinecraftLib.Cache;

public static class Encryption
{
    public static (byte[] data, byte[] iv) Encrypt(string key, byte[] data)
    {
        byte[] iv = Guid.NewGuid().ToByteArray();

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using MemoryStream stream = new MemoryStream();
        using CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
        using BinaryWriter writer = new BinaryWriter(cryptoStream);
        
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

        using MemoryStream stream = new MemoryStream(data);
        using CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);

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