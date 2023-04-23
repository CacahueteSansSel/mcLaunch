﻿using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Views.Pages.Settings;

namespace ddLaunch.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        
        Utilities.Settings.Load();
        SetSettings(Utilities.Settings.Instance.GetAllGroups());
    }

    public void SetSettings(SettingsGroup[] groups)
    {
        SettingsRoot.Children.Clear();
        
        foreach (SettingsGroup group in groups)
        {
            SettingsSection section = new SettingsSection(group);
            SettingsRoot.Children.Add(section);
        }
    }
}