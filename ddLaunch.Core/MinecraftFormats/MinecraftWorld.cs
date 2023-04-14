using Avalonia.Media.Imaging;
using SharpNBT;

namespace ddLaunch.Core.MinecraftFormats;

public class MinecraftWorld
{
    public string Name { get; init; }
    public Bitmap Icon { get; init; }
    public MinecraftGameMode GameMode { get; init; }
    public DateTime LastPlayed { get; init; }

    public MinecraftWorld()
    {
        
    }
    
    public MinecraftWorld(string completePath)
    {
        CompoundTag levelDat = (CompoundTag)NbtFile.Read($"{completePath}/level.dat", FormatOptions.Java)["Data"];

        Name = ((StringTag) levelDat["LevelName"]).Value;
        GameMode = (MinecraftGameMode)((IntTag) levelDat["GameType"]).Value;
        long unix = ((LongTag) levelDat["LastPlayed"]).Value;
        LastPlayed = DateTimeOffset.FromUnixTimeMilliseconds(unix).LocalDateTime;
        
        if (!File.Exists($"{completePath}/icon.png")) return;
        
        Icon = new Bitmap($"{completePath}/icon.png");
    }
}