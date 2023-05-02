using System;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ChangelogPopup : UserControl
{
    private Action confirmCallback;
    private Action cancelCallback;
    
    public ChangelogPopup()
    {
        InitializeComponent();
    }

    public ChangelogPopup(Modification mod, Action confirm, Action cancel)
    {
        InitializeComponent();
        
        confirmCallback = confirm;
        cancelCallback = cancel;

        TitleText.Text = TitleText.Text
            .Replace("$MOD", mod.Name);

        Regex isHtmlRegex = new Regex("<\\/\\w+>");
        if (isHtmlRegex.IsMatch(mod.Changelog))
        {
            var converter = new ReverseMarkdown.Converter();
            MarkdownArea.Markdown = HttpUtility.HtmlDecode(converter.Convert(mod.Changelog));
        }
        else
        {
            MarkdownArea.Markdown = mod.Changelog;
        }
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