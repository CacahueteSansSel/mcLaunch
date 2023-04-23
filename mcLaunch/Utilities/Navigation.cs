using Avalonia.Controls;
using mcLaunch.Models;

namespace mcLaunch.Utilities;

public class Navigation
{
    public static void Push<T>() where T : Control, new() => MainWindowDataContext.Instance.Push<T>();

    public static void Push(Control value) => MainWindowDataContext.Instance.Push(value);

    public static void Pop() => MainWindowDataContext.Instance.Pop();

    public static void ShowPopup(Control value) => MainWindowDataContext.Instance.ShowPopup(value);

    public static void HidePopup() => MainWindowDataContext.Instance.HidePopup();
}