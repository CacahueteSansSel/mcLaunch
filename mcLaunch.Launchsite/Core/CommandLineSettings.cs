namespace mcLaunch.Launchsite.Core;

public class CommandLineSettings
{
    public static CommandLineSettings Default => new CommandLineSettings
    {
        MinimumAllocatedRam = 512,
        MaximumAllocatedRam = 4096,
        CustomJavaArguments = string.Empty
    };
    
    public int MinimumAllocatedRam { get; set; }
    public int MaximumAllocatedRam { get; set; }
    public string CustomJavaArguments { get; set; }

    public string BuildArguments(string nextArgs)
    {
        string args = "";
        
        args += $"-Xms{MinimumAllocatedRam}m ";
        args += $"-Xmx{MaximumAllocatedRam}m ";
        if (!string.IsNullOrWhiteSpace(CustomJavaArguments))
            args += $"{CustomJavaArguments.Trim()} ";

        if (!string.IsNullOrWhiteSpace(nextArgs))
            args += nextArgs;
        
        return args;
    }
}