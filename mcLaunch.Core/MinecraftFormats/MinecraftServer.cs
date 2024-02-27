using Avalonia.Media.Imaging;
using SharpNBT;

namespace mcLaunch.Core.MinecraftFormats;

public class MinecraftServer
{
    private readonly string ip;

    public MinecraftServer()
    {
    }

    public MinecraftServer(CompoundTag nbt)
    {
        IsHidden = nbt["hidden"] == null ? false : ((ByteTag) nbt["hidden"]).Value == 1;
        IconData = Convert.FromBase64String(((StringTag) nbt["icon"]).Value);
        ip = ((StringTag) nbt["ip"]).Value;
        Name = ((StringTag) nbt["name"]).Value;

        string[] tokens = ip.Split(':');
        Address = tokens[0];
        Port = tokens.Length == 1 ? "25565" : tokens[1];
    }

    public bool IsHidden { get; set; }
    public byte[] IconData { get; set; }
    public string Address { get; set; }
    public string Port { get; set; }
    public string Name { get; set; }
    public Bitmap? Icon { get; set; }

    public async Task LoadIconAsync()
    {
        MemoryStream imageStream = new(IconData);

        Icon = await Task.Run(() =>
        {
            try
            {
                return Bitmap.DecodeToWidth(imageStream, 400);
            }
            catch (Exception e)
            {
                return null;
            }
        });
    }
}