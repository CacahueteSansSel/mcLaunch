using System.Diagnostics;
using mcLaunch.Tests.Tests;

namespace mcLaunch.Tests;

public class TestRunner
{
    public List<TestBase> Tests { get; } = [];

    public TestRunner()
    {
        
    }

    public void RegisterTests(TestBase[] tests)
        => Tests.AddRange(tests);

    public async Task<TestResult[]> RunAsync()
    {
        List<TestResult> results = new();

        Logging.Log(LogType.Info, $"Now running {Tests.Count} test(s)...");

        int index = 1;
        foreach (TestBase test in Tests)
        {
            TestResult result;
            
            Logging.Log(LogType.Pending, $"Test {index}/{Tests.Count} : {test.Name}");
            
            try
            {
                result = await test.RunAsync();
                result.Test = test;
            }
            catch (Exception e)
            {
                result = new TestResult(false, e.ToString(), test);
                if (Debugger.IsAttached) throw;
            }
            
            Logging.Log(result.IsSuccess ? LogType.Success : LogType.Failure, 
                $"{test.Name}: {result.Message ?? "OK"}");
                
            results.Add(result);
            index++;
        }

        TestResult[] succeededResults = results.Where(result => result.IsSuccess).ToArray();
        TestResult[] failedResults = results.Where(result => !result.IsSuccess).ToArray();

        Console.WriteLine($"Test Running Results:");
        Console.WriteLine($"    Succeeded tests: {succeededResults.Length} test(s)");
        Console.WriteLine($"    Failed tests: {failedResults.Length} test(s)");

        index = 0;
        foreach (TestResult failedTest in failedResults)
        {
            Console.Write("        [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Failure #{index}");
            Console.ResetColor();
            Console.Write("] ");
            
            Console.WriteLine($"{failedTest.Test.Name}: {failedTest.Message}");

            index++;
        }
        
        return results.ToArray();
    }
}

public class TestResult(bool isSuccess, string? message, TestBase? test = null)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? Message { get; } = message;
    public TestBase? Test { get; set; } = test;
}