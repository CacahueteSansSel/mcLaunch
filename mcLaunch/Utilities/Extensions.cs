using System;
using Avalonia.Controls.Shapes;
using mcLaunch.Launchsite.Core;
using mcLaunch.Views.Popups;
using Path = System.IO.Path;

namespace mcLaunch.Utilities;

public static class Extensions
{
    public static void ShowErrorPopup(this Result result)
    {
        if (!result.IsError) return;

        Navigation.ShowPopup(new MessageBoxPopup("Error occurred", result.ErrorMessage!, MessageStatus.Error));
    }
}