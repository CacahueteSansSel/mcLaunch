using System;
using System.Diagnostics;
using Avalonia.Controls;
using mcLaunch.Launchsite.Http;
using mcLaunch.Utilities;

namespace mcLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();

        MacosButtonsMargin.IsVisible = OperatingSystem.IsMacOS();

        Api.OnNetworkError += OnApiNetworkError;
        Api.OnNetworkSuccess += OnApiNetworkSuccess;

        if (CurrentBuild.Branch == "dev" || Debugger.IsAttached)
        {
            LogoDevelopment.IsVisible = true;
            LogoBeta.IsVisible = false;
        }
        else
        {
            LogoDevelopment.IsVisible = false;
            LogoBeta.IsVisible = true;
        }
    }

    public void SetTitle(string title)
    {
        TitleText.Text = title;
    }

    private void OnApiNetworkSuccess(string url)
    {
        OfflineBadge.IsVisible = false;
    }

    private void OnApiNetworkError(string url)
    {
        OfflineBadge.IsVisible = true;
    }
}