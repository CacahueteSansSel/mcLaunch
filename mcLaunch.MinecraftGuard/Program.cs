using System.Diagnostics;

if (args.Length < 2) return;

string pid = args[0];
string boxId = args[1];
string type = args.Length == 2 ? "default" : args[2].ToLower();

if (!int.TryParse(pid, out int javaPid))
{
    Console.WriteLine("Invalid PID argument. Usage : mcLaunch.MinecraftGuard <process-pid>");
    return;
}

Process java = Process.GetProcessById(javaPid);
Console.WriteLine($"mcLaunch box id {boxId} {type}");
Console.WriteLine($"Attached to process id {java.Id}");

await java.WaitForExitAsync();

// If we are here, Minecraft exited

// If this is a FastLaunch temporary box, we launch back mcLaunch
// We need to launch back mcLaunch if the Minecraft process has crashed

if (java.ExitCode != 0 || type == "temporary")
    Process.Start("mcLaunch", $"--from-guard --exit-code {java.ExitCode} --box-id {boxId}");
else Console.WriteLine("mcLaunch process not restarted");

Thread.Sleep(500);