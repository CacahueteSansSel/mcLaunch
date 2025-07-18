using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Windows;

public partial class ConsoleWindow : Window
{
    private readonly Box? box;
    private readonly Process? process;
    private int lastSecond;
    private int lineCountForCurrentSecond;
    private string pendingTextBuffer = "";

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

        if (box != null)
            box.Minecraft.OnStandardOutputLineReceived -= MinecraftStdOutLineReceived;
    }

    private void MinecraftStdOutLineReceived(string line)
    {
        if (lastSecond != DateTime.Now.Second)
        {
            lastSecond = DateTime.Now.Second;
            lineCountForCurrentSecond = 0;
        }
        else
        {
            lineCountForCurrentSecond++;
            if (lineCountForCurrentSecond > 10)
            {
                pendingTextBuffer += $"{line}\n";
                return;
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!IsVisible) return;

            if (!string.IsNullOrEmpty(pendingTextBuffer))
            {
                ConsoleText.Text += pendingTextBuffer;
                pendingTextBuffer = "";
            }

            ConsoleText.Text += $"{line}\n";
            ConsoleText.ScrollToEnd();
        });
    }

    private async void ReadProcessOutput()
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