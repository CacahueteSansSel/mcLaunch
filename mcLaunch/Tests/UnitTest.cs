using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using mcLaunch.Models;

namespace mcLaunch.Tests;

public abstract class UnitTest
{
    public string AssertLog { get; set; }
    public abstract Task RunAsync();

    protected void Assert(bool condition)
    {
        if (!condition)
            throw new Exception("Assertion failed");
    }
    
    protected void Assert(bool condition, string message)
    {
        AssertLog += $"{message}: {(condition ? "YES" : "NO")}\n";
        
        if (!condition)
            throw new Exception($"Assertion failed: {message}");
    }

    protected T? FindControlMain<T>(string name) where T : Control
    {
        foreach (Control control in MainWindow.Instance.GetVisualDescendants().Where(v => v is Control))
        {
            if (control.Name == name) return (T) control;
        }

        return null;
    }

    protected T? FindControlPopup<T>(string name) where T : Control
    {
        foreach (Control control in MainWindowDataContext.Instance.CurrentPopup
                     .GetVisualDescendants().Where(v => v is Control))
        {
            if (control.Name == name) return (T) control;
        }

        return null;
    }

    protected void ButtonClick(Button button)
        => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    protected bool IsPopupShown<T>() where T : UserControl
    {
        return MainWindowDataContext.Instance.CurrentPopup is T;
    }

    protected bool IsPageShown<T>() where T : UserControl
    {
        return MainWindowDataContext.Instance.CurrentPage is T;
    }
}