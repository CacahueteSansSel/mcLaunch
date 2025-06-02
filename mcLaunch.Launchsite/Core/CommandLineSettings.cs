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

    public string BuildArguments(string prefix)
    {
        string args = $"{prefix} ";

        args += $"-Xms{MinimumAllocatedRam}m ";
        args += $"-Xmw{MaximumAllocatedRam}m ";
        if (!string.IsNullOrWhiteSpace(CustomJavaArguments))
            args += $"{CustomJavaArguments.Trim()} ";
        
        return args;
    }
}