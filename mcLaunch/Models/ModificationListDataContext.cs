using System.Collections.Generic;
using mcLaunch.Core.Contents;

namespace mcLaunch.Models;

public class ModificationListDataContext
{
    public ModificationListDataContext()
    {
        Modifications = new List<MinecraftContent>
        {
            new() { Author = "me", IconUrl = "todo", Name = "fungus mod" },
            new() { Author = "not me", IconUrl = "todo", Name = "dzd mod" },
            new() { Author = "your", IconUrl = "todo", Name = "dzdahahaHAHAHA mod" }
        };
    }

    public ModificationListDataContext(List<MinecraftContent> mods)
    {
        Modifications = mods;
    }

    public List<MinecraftContent> Modifications { get; } = new();
}