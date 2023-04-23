﻿using System;

namespace ddLaunch.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SettingAttribute : Attribute
{
    public string Name { get; set; }
    public string Group { get; set; }
}