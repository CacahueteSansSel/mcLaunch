using System.Collections.Generic;
using mcLaunch.Core;
using mcLaunch.Core.Mods;

namespace mcLaunch.Models;

public class ModificationListDataContext
{
    public List<Modification> Modifications { get; } = new();

    public ModificationListDataContext()
    {
        Modifications = new List<Modification>()
        {
            new Modification {Author = "me", IconUrl = "todo", Name = "fungus mod"},
            new Modification {Author = "not me", IconUrl = "todo", Name = "dzd mod"},
            new Modification {Author = "your", IconUrl = "todo", Name = "dzdahahaHAHAHA mod"}
        };
    }
    
    public ModificationListDataContext(List<Modification> mods)
    {
        Modifications = mods;
    }
}