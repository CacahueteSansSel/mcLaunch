using System.Diagnostics;

if (args.Length != 2) return;

string pid = args[0];
string boxId = args[1];

if (!int.TryParse(pid, out int javaPid))
{
    Console.WriteLine("Invalid PID argument. Usage : ddLaunch.MinecraftGuard <process-pid>");
    return;
}

Process java = Process.GetProcessById(javaPid);
Console.WriteLine($"ddLaunch box id {boxId}");
Console.WriteLine($"Attached to process id {java.Id}");

await java.WaitForExitAsync();

// If we're here, Minecraft exited
// We need to launch back ddLaunch if the Minecraft process has crashed

if (java.ExitCode != 0)
    Process.Start("ddLaunch", $"--from-guard --exit-code {java.ExitCode} --box-id {boxId}");