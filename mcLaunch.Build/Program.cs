using mcLaunch.Build;
using mcLaunch.Build.Steps;

Console.WriteLine("mcLaunch Build System");
if (args.Length == 0)
{
    Console.WriteLine("Usage: mcLaunch.Build <solution-directory>");
    return;
}

string solutionDirectory = args[0];
BuildSystem buildSystem = new BuildSystem(solutionDirectory)
    .With<CleanOutputDirectoryStep>()
    .With<UpdateLatestCommitStep>()
    .With<BuildMcLaunchWindows64Step>()
    .With<BuildMcLaunchWindowsArm64Step>()
    .With<BuildMcLaunchMacOS64Step>()
    .With<BuildMcLaunchMacOSArm64Step>()
    .With<BuildMcLaunchLinux64Step>()
    .With<BuildMcLaunchLinuxArm64Step>()
    .With<BuildInstallerWindows64Step>()
    .With<BuildInstallerWindowsArm64Step>()
    .With<BuildInstallerLinux64Step>()
    .With<BuildInstallerLinuxArm64Step>()
    .With<CreateMacOSBundle64Step>()
    .With<CreateMacOSBundleArm64Step>();

bool success = await buildSystem.BuildAsync();

if (success) Console.WriteLine("mcLaunch built successfully !");
else Console.WriteLine("mcLaunch failed to build");

Environment.Exit(success ? 0 : 1);