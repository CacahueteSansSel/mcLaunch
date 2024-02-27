using System.Text;

namespace mcLaunch.Core.Utilities;

public static class BinaryReaderWriterExtensions
{
    public static string? ReadNullableString(this BinaryReader rd)
    {
        int length = rd.Read7BitEncodedInt();
        if (length <= 0) return null;

        byte[] data = rd.ReadBytes(length);
        return Encoding.UTF8.GetString(data);
    }

    public static void WriteNullableString(this BinaryWriter wr, string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            wr.Write7BitEncodedInt(0);
            return;
        }

        byte[] encodedString = Encoding.UTF8.GetBytes(str);

        wr.Write7BitEncodedInt(encodedString.Length);
        wr.Write(encodedString);
    }
}