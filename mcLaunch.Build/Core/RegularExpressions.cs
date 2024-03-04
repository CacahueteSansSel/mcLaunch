using System.Text.RegularExpressions;

namespace mcLaunch.Build.Core;

public static partial class RegularExpressions
{
    [GeneratedRegex("[A-z]+: .+")]
    public static partial Regex ProjectCommitFormat();
}