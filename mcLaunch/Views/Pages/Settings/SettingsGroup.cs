using System.Collections.Generic;
using System.Reflection;

namespace mcLaunch.Views.Pages.Settings;

public class SettingsGroup
{
    public string Name { get; }
    public Setting[] Settings { get; set; }

    public SettingsGroup(string name, Setting[]? settings = null)
    {
        Name = name;
        if (settings != null) Settings = settings;
    }
}

public class Setting
{
    public string Name { get; }
    public string GroupName { get; }
    public SettingType Type { get; }
    public PropertyInfo Property { get; }

    public Setting(string name, PropertyInfo property, string groupName = null)
    {
        Name = name;
        Property = property;
        GroupName = groupName;

        if (property.PropertyType == typeof(bool)) Type = SettingType.Boolean;
        else Type = SettingType.Unknown;
    }
}

public enum SettingType
{
    Unknown,
    Boolean
}