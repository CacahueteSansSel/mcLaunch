using System.Collections.Generic;
using mcLaunch.Core;
using mcLaunch.Core.Contents;

namespace mcLaunch.Models;

public class ModificationListDataContext
{
    public List<MinecraftContent> Modifications { get; } = new();

    public ModificationListDataContext()
    {
        Modifications = new List<MinecraftContent>()
        {
            new MinecraftContent {Author = "me", IconUrl = "todo", Name = "fungus mod"},
            new MinecraftContent {Author = "not me", IconUrl = "todo", Name = "dzd mod"},
            new MinecraftContent {Author = "your", IconUrl = "todo", Name = "dzdahahaHAHAHA mod"}
        };
    }
    
    public ModificationListDataContext(List<MinecraftContent> mods)
    {
        Modifications = mods;
    }
}