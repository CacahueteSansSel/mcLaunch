using System.Text.Json;
using mcLaunch.Launchsite.Models.QuickPlay;
using mcLaunch.Launchsite.Utils;

namespace mcLaunch.Launchsite.Core;

public class QuickPlayManager
{
    private readonly string path;
    private MinecraftFolder folder;

    public QuickPlayManager(MinecraftFolder folder)
    {
        this.folder = folder;
        path = $"{folder.CompletePath}/quickPlay/java".FixPath();

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    public string[] Profiles
        => Directory.GetFiles(path, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

    public string Create(QuickPlayWorldType worldType, QuickPlayGameMode gamemode, string worldName)
    {
        QuickPlayProfile profile = new()
        {
            Type = worldType.ToString().ToLower(),
            Id = worldName,
            Name = worldName,
            LastPlayedTime = DateTime.Now,
            Gamemode = gamemode.ToString().ToLower()
        };

        string path = $"{this.path}/{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.json".FixPath();

        File.WriteAllText(path, JsonSerializer.Serialize(new[] { profile }));

        return path;
    }

    public void Delete(string name)
    {
        File.Delete($"{path}/{name}.json");
    }

    public QuickPlayProfile[]? Get(string name) =>
        JsonSerializer.Deserialize<QuickPlayProfile[]>(File.ReadAllText($"{path}/{name}"));
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