namespace mcLaunch.Tests.Tests;

public class SampleTest : TestBase
{
    public override string Name => "Sample test";
    
    public override async Task<TestResult> RunAsync()
    {
        if (!await Test("Check if the hour is 9", async () => DateTime.Now.Hour == 9))
            return TestFailure();

        return new TestResult(true, null);
    }
}