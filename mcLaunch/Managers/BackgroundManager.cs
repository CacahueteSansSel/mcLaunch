using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using mcLaunch.Core.Boxes;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

namespace mcLaunch.Managers;

public static class BackgroundManager
{
    public static bool IsInBackground { get; private set; }

    public static void EnterBackgroundState()
    {
        if (IsInBackground) return;
        
        IsInBackground = true;
        MainWindow.Instance.Hide();
    }

    public static void LeaveBackgroundState()
    {
        if (!IsInBackground) return;
        
        IsInBackground = false;
        MainWindow.Instance.Show();
    }
    
    public static async Task<bool> RunMinecraftMonitoring(Process javaProcess, Box box)
    {
        if (!IsInBackground) 
            return false;
        
        await javaProcess.WaitForExitAsync();

        if (javaProcess.ExitCode != 0)
        {
            Navigation.ShowPopup(new CrashPopup(javaProcess.ExitCode, box.Manifest.Id));
            return false;
        }

        if (box.Manifest.Type == BoxType.Temporary)
        {
            LeaveBackgroundState();
            await ProcessFastLaunchBoxAsync(box);
        }
        
        return true;
    }
    
    static async Task ProcessFastLaunchBoxAsync(Box box)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup("Keep this FastLaunch instance ?",
            "Do you want to keep this FastLaunch instance ? If you delete it, it will be lost forever !",
            () =>
            {
                if (box == null) return;

                Navigation.ShowPopup(new EditBoxPopup(box, false));
            }, () =>
            {
                if (box == null) return;

                Directory.Delete(box.Path, true);
                
                Navigation.Reset();
                Navigation.Push<MainPage>();
            })
        );

        await Task.Run(async () =>
        {
            while (MainWindowDataContext.Instance.IsPopup)
                await Task.Delay(100);
        });
    }
}