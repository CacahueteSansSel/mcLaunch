using System.Net.Http.Headers;
using System.Reflection;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Http;
using mcLaunch.Core.Managers;
using mcLaunch.Tests;
using mcLaunch.Tests.Tests;
using mcLaunch.Utilities;

Console.WriteLine("mcLaunch Tester");
Console.WriteLine($"Running on mcLaunch {CurrentBuild.Version} (commit {CurrentBuild.Commit})");

Api.SetUserAgent(new ProductInfoHeaderValue("mcLaunch-Tester",
    $"tester-{CurrentBuild.Version}"));

string testEnvPath = "test_env";
if (!Directory.Exists(testEnvPath)) Directory.CreateDirectory(testEnvPath);
AppdataFolderManager.SetCustomPath(Path.GetFullPath(testEnvPath));

MinecraftFolder systemFolder = new MinecraftFolder(AppdataFolderManager.GetValidPath("system"));

TestRunner runner = new(systemFolder);
runner.RegisterTests(Assembly.GetExecutingAssembly().GetTypes()
    .Where(type => type.IsSubclassOf(typeof(TestBase)))
    .Select(type => (TestBase) Activator.CreateInstance(type)!)
    .ToArray());

await runner.RunAsync();

/*
Console.WriteLine("Cleaning up test environment...");
Directory.Delete(testEnvPath, true);
*/