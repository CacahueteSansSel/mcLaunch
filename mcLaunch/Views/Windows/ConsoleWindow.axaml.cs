using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class ConsoleWindow : Window
{
    Process? process;
    Box? box;
    
    public ConsoleWindow()
    {
        InitializeComponent();

        ConsoleText.Text = "No process associated with this window";
    }

    public ConsoleWindow(Process process, Box box)
    {
        InitializeComponent();
        this.process = process;
        this.box = box;
        this.box.Minecraft.OnStandardOutputLineReceived += MinecraftStdOutLineReceived;

        ConsoleText.Text = string.Join("\n", this.box.Minecraft.StandardOutput);
        ConsoleText.Options.AllowScrollBelowDocument = false;

        //ReadProcessOutput();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        box.Minecraft.OnStandardOutputLineReceived -= MinecraftStdOutLineReceived;
    }

    void MinecraftStdOutLineReceived(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!IsVisible) return;
            
            ConsoleText.Text += $"{line}\n";
            ConsoleText.ScrollToEnd();
        });
    }

    async void ReadProcessOutput()
    {
        if (process == null) return;
        
        while (!process.HasExited)
        {
            string line = await process.StandardOutput.ReadLineAsync();
            ConsoleText.Text += $"{line}\n";
            ConsoleText.ScrollToEnd();
        }

        ConsoleText.Text += $"Minecraft exited with code {process.ExitCode}";
        ConsoleText.ScrollToEnd();
    }
}