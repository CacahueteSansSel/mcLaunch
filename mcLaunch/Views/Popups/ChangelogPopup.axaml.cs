using System;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Contents;
using mcLaunch.Utilities;
using ReverseMarkdown;

namespace mcLaunch.Views.Popups;

public partial class ChangelogPopup : UserControl
{
    private readonly Action cancelCallback;
    private readonly Action confirmCallback;

    public ChangelogPopup()
    {
        InitializeComponent();
    }

    public ChangelogPopup(MinecraftContent mod, Action confirm, Action cancel)
    {
        InitializeComponent();

        confirmCallback = confirm;
        cancelCallback = cancel;

        TitleText.Text = TitleText.Text
            .Replace("$MOD", mod.Name);

        Regex isHtmlRegex = new("<\\/\\w+>");
        if (isHtmlRegex.IsMatch(mod.Changelog))
        {
            Converter converter = new();
            MarkdownArea.Markdown = HttpUtility.HtmlDecode(converter.Convert(mod.Changelog));
        }
        else
            MarkdownArea.Markdown = mod.Changelog;
    }

    private void InstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        confirmCallback?.Invoke();
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        cancelCallback?.Invoke();
    }
}