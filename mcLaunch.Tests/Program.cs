// See https://aka.ms/new-console-template for more information

using System.Reflection;
using mcLaunch.Tests;
using mcLaunch.Tests.Tests;
using mcLaunch.Utilities;

Console.WriteLine("mcLaunch Tester");
Console.WriteLine($"Running on mcLaunch {CurrentBuild.Version} (commit {CurrentBuild.Commit})");

TestRunner runner = new();
runner.RegisterTests(Assembly.GetExecutingAssembly().GetTypes()
    .Where(type => type.IsSubclassOf(typeof(TestBase)))
    .Select(type => (TestBase) Activator.CreateInstance(type)!)
    .ToArray());

await runner.RunAsync();