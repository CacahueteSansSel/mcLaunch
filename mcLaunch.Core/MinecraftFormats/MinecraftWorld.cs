using Avalonia.Media.Imaging;
using SharpNBT;

namespace mcLaunch.Core.MinecraftFormats;

public class MinecraftWorld
{
    public MinecraftWorld()
    {
    }

    public MinecraftWorld(string path)
    {
        CompoundTag levelDat = (CompoundTag)NbtFile.Read($"{path}/level.dat", FormatOptions.Java)["Data"];

        WorldPath = path;
        FolderName = Path.GetFileNameWithoutExtension(path);
        Name = ((StringTag)levelDat["LevelName"]).Value;
        GameMode = (MinecraftGameMode)((IntTag)levelDat["GameType"]).Value;
        long unix = ((LongTag)levelDat["LastPlayed"]).Value;
        LastPlayed = DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime;
        IsCheats = ((ByteTag)levelDat["allowCommands"]).Value == 1;
        Version = ((StringTag)((CompoundTag)levelDat["Version"])["Name"]).Value;

        if (!File.Exists($"{path}/icon.png")) return;

        Icon = new Bitmap($"{path}/icon.png");
    }

    public string WorldPath { get; init; }
    public string Name { get; init; }
    public string FolderName { get; init; }
    public Bitmap Icon { get; init; }
    public MinecraftGameMode GameMode { get; init; }
    public DateTime LastPlayed { get; init; }
    public bool IsCheats { get; init; }
    public string Version { get; init; }
}