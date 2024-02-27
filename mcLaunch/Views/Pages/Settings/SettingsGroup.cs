using System.Reflection;

namespace mcLaunch.Views.Pages.Settings;

public class SettingsGroup
{
    public SettingsGroup(string name, Setting[]? settings = null)
    {
        Name = name;
        if (settings != null) Settings = settings;
    }

    public string Name { get; }
    public Setting[] Settings { get; set; }
}

public class Setting
{
    public Setting(string name, PropertyInfo property, string groupName = null)
    {
        Name = name;
        Property = property;
        GroupName = groupName;

        if (property.PropertyType == typeof(bool)) Type = SettingType.Boolean;
        else Type = SettingType.Unknown;
    }

    public string Name { get; }
    public string GroupName { get; }
    public SettingType Type { get; }
    public PropertyInfo Property { get; }
}

public enum SettingType
{
    Unknown,
    Boolean
}