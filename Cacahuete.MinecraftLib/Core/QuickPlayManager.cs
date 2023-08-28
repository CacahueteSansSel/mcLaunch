using System.Text.Json;
using Cacahuete.MinecraftLib.Models.QuickPlay;

namespace Cacahuete.MinecraftLib.Core;

public class QuickPlayManager
{
    MinecraftFolder folder;
    string path;

    public string[] Profiles 
        => Directory.GetFiles(path, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToArray();

    public QuickPlayManager(MinecraftFolder folder)
    {
        this.folder = folder;
        path = $"{folder.CompletePath}/quickPlay/java";

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    public string Create(QuickPlayWorldType worldType, QuickPlayGameMode gamemode, string worldName)
    {
        QuickPlayProfile profile = new QuickPlayProfile
        {
            Type = worldType.ToString().ToLower(),
            Id = worldName,
            Name = worldName,
            LastPlayedTime = DateTime.Now,
            Gamemode = gamemode.ToString().ToLower()
        };

        string path = $"{this.path}/{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.json";
        
        File.WriteAllText(path, JsonSerializer.Serialize(new [] {profile}));

        return path;
    }

    public void Delete(string name) => File.Delete($"{path}/{name}.json");

    public QuickPlayProfile[]? Get(string name)
        => JsonSerializer.Deserialize<QuickPlayProfile[]>(File.ReadAllText($"{path}/{name}"));
}

public enum QuickPlayWorldType
{
    Singleplayer,
    Multiplayer,
    Realms
}

public enum QuickPlayGameMode
{
    Survival = 0,
    Creative = 1,
    Adventure = 2,
    Spectator = 3
}