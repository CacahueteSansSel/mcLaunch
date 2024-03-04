using mcLaunch.Build;
using mcLaunch.Build.Steps;

Console.WriteLine("mcLaunch Build System");
if (args.Length == 0)
{
    Console.WriteLine("Usage: mcLaunch.Build <solution-directory>");
    return;
}

string solutionDirectory = args[0];
List<BuildStepBase> steps = [];

if (args.Contains("--windows") || args.Length == 1)
{
    steps.AddRange([
        new BuildMcLaunchWindows64Step(), new BuildMcLaunchWindowsArm64Step(), new BuildInstallerWindows64Step(),
        new BuildInstallerWindowsArm64Step()
    ]);
}

if (args.Contains("--macos") || args.Length == 1)
{
    steps.AddRange([
        new BuildMcLaunchMacOS64Step(), new BuildMcLaunchMacOSArm64Step(), new CreateMacOSBundle64Step(),
        new CreateMacOSBundleArm64Step()
    ]);
}

if (args.Contains("--linux") || args.Length == 1)
{
    steps.AddRange([
        new BuildMcLaunchLinux64Step(), new BuildMcLaunchLinuxArm64Step(), new BuildInstallerLinux64Step(),
        new BuildInstallerLinuxArm64Step()
    ]);
}

BuildSystem buildSystem = new BuildSystem(solutionDirectory)
    .With<CleanOutputDirectoryStep>()
    .With<UpdateLatestCommitStep>()
    .With(steps.ToArray());

bool success = await buildSystem.BuildAsync();

if (success) Console.WriteLine("mcLaunch built successfully !");
else Console.WriteLine("mcLaunch failed to build");

Environment.Exit(success ? 0 : 1);