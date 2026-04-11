using System;
using System.IO;
using System.Threading.Tasks;

namespace mcLaunch.Core.Utilities;

public class FileAccessFailSafe
{
    private FailSafeIOOperationDelegate _callback;

    public FileAccessFailSafe(FailSafeIOOperationDelegate callback)
    {
        _callback = callback;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            try
            {
                await _callback.Invoke();
                break;
            }
            catch (IOException e)
            {
                if (!e.Message.Contains("used by another process"))
                    throw;
                
                Console.WriteLine("IOException in FileAccessFailSafe, retrying");

                await Task.Delay(500);
            }
        }
    }

    public delegate Task FailSafeIOOperationDelegate();
}