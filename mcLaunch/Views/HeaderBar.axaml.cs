using System;
using Avalonia;
using Avalonia.Controls;
using Cacahuete.MinecraftLib.Http;

namespace mcLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();

        MacosButtonsMargin.IsVisible = OperatingSystem.IsMacOS();

        Api.OnNetworkError += OnApiNetworkError;
        Api.OnNetworkSuccess += OnApiNetworkSuccess;
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