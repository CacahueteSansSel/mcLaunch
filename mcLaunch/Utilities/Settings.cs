using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities.Attributes;
using mcLaunch.Views.Pages.Settings;

namespace mcLaunch.Utilities;

public class Settings
{
    public Settings()
    {
        Instance = this;
    }

    public static Settings? Instance { get; private set; }
    public static List<string> SeenVersionsList { get; private set; } = [];

    [Setting(Name = "Expose launcher name to Minecraft", Group = "Minecraft")]
    public bool ExposeLauncherNameToMinecraft { get; set; } = true;

    [Setting(Name = "Enable snapshots versions of Minecraft", Group = "Minecraft")]
    public bool EnableSnapshots { get; set; }

    [Setting(Name = "Force dedicated graphics", Group = "Minecraft")]
    public bool ForceDedicatedGraphics { get; set; } = true;

    [Setting(Name = "Anonymize box name & icons", Group = "Display")]
    public bool AnonymizeBoxIdentity { get; set; }

    [Setting(Name = "Show advanced features", Group = "Display")]
    public bool ShowAdvancedFeatures { get; set; }

    [Setting(Name = "Use Discord's Rich Presence", Group = "Discord")]
    public bool UseDiscordRpc { get; set; } = true;

    [Setting(Name = "Show box infos on Discord's Rich Presence", Group = "Discord")]
    public bool ShowBoxInfosOnDiscordRpc { get; set; }

    public Settings WithDefaults()
    {
        ExposeLauncherNameToMinecraft = true;
        ForceDedicatedGraphics = true;
        UseDiscordRpc = true;

        return this;
    }

    public Setting[] GetAll()
    {
        List<Setting> settings = new();

        foreach (PropertyInfo property in Instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            SettingAttribute? attribute = property.GetCustomAttribute<SettingAttribute>();
            if (attribute == null) continue;

            settings.Add(Get(property.Name));
        }

        return settings.ToArray();
    }

    public SettingsGroup[] GetAllGroups()
    {
        Setting[] all = GetAll();
        Dictionary<string, List<Setting>> groups = new();

        foreach (Setting setting in all)
        {
            if (!groups.ContainsKey(setting.GroupName)) groups.Add(setting.GroupName, new List<Setting>());

            groups[setting.GroupName].Add(setting);
        }

        return groups.Select(g => new SettingsGroup(g.Key, g.Value.ToArray())).ToArray();
    }

    public Setting Get(string name)
    {
        PropertyInfo? prop = Instance.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null) return null;

        SettingAttribute? attribute = prop.GetCustomAttribute<SettingAttribute>();
        if (attribute == null) return null;

        return new Setting(attribute.Name, prop, attribute.Group);
    }

    public static void MarkCurrentVersionAsSeen()
    {
        if (SeenVersionsList.Contains(CurrentBuild.Version.ToString())) return;

        SeenVersionsList.Add(CurrentBuild.Version.ToString());
        Save();
    }

    public static void Save()
    {
        File.WriteAllText(AppdataFolderManager.GetPath("settings.json"), JsonSerializer.Serialize(Instance));
        File.WriteAllText(AppdataFolderManager.GetPath("seen_versions.json"),
            JsonSerializer.Serialize(SeenVersionsList));
    }

    public static void Load()
    {
        if (File.Exists(AppdataFolderManager.GetPath("seen_versions.json")))
            SeenVersionsList = JsonSerializer.Deserialize<List<string>>(
                File.ReadAllText(AppdataFolderManager.GetPath("seen_versions.json")))!;

        if (File.Exists(AppdataFolderManager.GetPath("settings.json")))
            Instance = JsonSerializer.Deserialize<Settings>(
                File.ReadAllText(AppdataFolderManager.GetPath("settings.json")))!;
        else Instance = new Settings().WithDefaults();
    }
}