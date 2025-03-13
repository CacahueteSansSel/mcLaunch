﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Managers;

public static class BackgroundManager
{
    private static TrayIcon? icon;
    private static Process? javaProcess;

    public static bool IsInBackground { get; private set; }
    public static Box? RunningBox { get; private set; }
    public static bool IsMinecraftRunning => RunningBox != null && javaProcess != null && !javaProcess.HasExited;
    public static Func<NativeMenuItemBase[]>? LastMenuAdditionalItemsProvider { get; private set; }

    public static void EnterBackgroundState(Func<NativeMenuItemBase[]>? additionalItems = null)
    {
        if (IsInBackground) return;

        IsInBackground = true;
        LastMenuAdditionalItemsProvider = additionalItems;
        MainWindow.Instance.Hide();
        DiscordManager.Shutdown();

        icon = new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://mcLaunch/resources/icon.ico"))),
            IsVisible = true,
            ToolTipText = "mcLaunch",
            Menu = GetIconNativeMenu(additionalItems?.Invoke())
        };
    }

    public static void LeaveBackgroundState()
    {
        if (!IsInBackground) return;

        IsInBackground = false;
        MainWindow.Instance.Show();
        DiscordManager.Init();

        icon?.Dispose();
        icon = null;
        //RunningBox = null;
    }

    public static async Task<bool> RunMinecraftMonitoring(Process process, Box box)
    {
        javaProcess = process;
        RunningBox = box;
        if (icon != null)
            icon.ToolTipText = $"{box.Manifest.Name} - mcLaunch";

        await javaProcess.WaitForExitAsync();

        if (javaProcess != null && javaProcess.ExitCode != 0)
        {
            LeaveBackgroundState();
            Navigation.ShowPopup(new CrashPopup(javaProcess.ExitCode, box.Manifest.Id));

            if (box.Manifest.Type != BoxType.Temporary)
                return false;

            await Navigation.WaitPopupClosingAsync();
        }

        if (box.Manifest.Type == BoxType.Temporary)
        {
            LeaveBackgroundState();
            await ProcessFastLaunchBoxAsync(box);
        }

        RunningBox = null;

        return true;
    }

    public static bool IsBoxRunning(Box box) => RunningBox != null && RunningBox.Manifest.Id == box.Manifest.Id;

    public static void KillMinecraftProcess()
    {
        javaProcess?.Kill();

        javaProcess = null;
        RunningBox = null;
    }

    private static async Task ProcessFastLaunchBoxAsync(Box box)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Keep this FastLaunch instance ?",
            "Do you want to keep this FastLaunch instance ? If you delete it, it will be lost forever !",
            () =>
            {
                if (box == null) return;

                Navigation.ShowPopup(new EditBoxPopup(box, false));
            }, async () =>
            {
                if (box == null) return;

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Directory.Delete(box.Path, true);
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await Task.Delay(10);
                    }
                }

                if (Directory.Exists(box.Path))
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Box folder deletion",
                        "Failed to delete the box's folder, is the game still running ?", MessageStatus.Warning));
                }

                Navigation.Reset();
                Navigation.Push<MainPage>();
            })
        );

        await Task.Run(async () =>
        {
            while (MainWindowDataContext.Instance.IsPopup)
                await Task.Delay(100);
        });
    }

    private static NativeMenu GetIconNativeMenu(NativeMenuItemBase[]? additionalItems = null)
    {
        NativeMenuItem showLauncherMenu = new("Show mcLaunch");
        NativeMenuItem closeLauncherMenu = new("Close mcLaunch");
        List<NativeMenuItemBase> additionalMenu = [];
        List<NativeMenuItemBase> systemMenu =
        [
            showLauncherMenu,
            closeLauncherMenu
        ];

        showLauncherMenu.Click += (_, _) =>
        {
            Navigation.HidePopup();
            LeaveBackgroundState();
        };
        closeLauncherMenu.Click += (_, _) => { Environment.Exit(0); };

        if (additionalItems != null) additionalMenu.AddRange(additionalItems);

        return
        [
            ..additionalMenu,
            new NativeMenuItemSeparator(),
            ..systemMenu
        ];
    }
}