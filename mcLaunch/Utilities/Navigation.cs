using Avalonia.Controls;
using mcLaunch.Models;
using mcLaunch.Views;

namespace mcLaunch.Utilities;

public class Navigation
{
    public static void Push<T>() where T : ITopLevelPageControl, new()
    {
        MainWindowDataContext.Instance.Push<T>();
    }

    public static void Push(ITopLevelPageControl value)
    {
        MainWindowDataContext.Instance.Push(value);
    }

    public static void Pop()
    {
        MainWindowDataContext.Instance.Pop();
    }

    public static void Reset()
    {
        MainWindowDataContext.Instance.Reset();
    }

    public static void ShowPopup(Control value)
    {
        MainWindowDataContext.Instance.ShowPopup(value);
    }

    public static void HidePopup()
    {
        MainWindowDataContext.Instance.HidePopup();
    }
}