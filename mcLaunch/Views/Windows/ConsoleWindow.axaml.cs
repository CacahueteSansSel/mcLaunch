using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class ConsoleWindow : Window
{
    Process? process;
    
    public ConsoleWindow()
    {
        InitializeComponent();

        ConsoleText.Text = "No process associated with this window";
    }

    public ConsoleWindow(Process process)
    {
        InitializeComponent();
        this.process = process;
        
        ReadProcessOutput();
    }

    async void ReadProcessOutput()
    {
        if (process == null) return;
        
        while (!process.HasExited)
        {
            string line = await process.StandardOutput.ReadLineAsync();
            ConsoleText.Text += $"{line}\n";
            ConsoleText.ScrollToLine(ConsoleText.Text.Split('\n').Length - 1);
        }

        ConsoleText.Text += $"Minecraft exited with code {process.ExitCode}";
        ConsoleText.ScrollToLine(ConsoleText.Text.Split('\n').Length - 1);
    }
}