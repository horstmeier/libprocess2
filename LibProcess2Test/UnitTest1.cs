using System.Text;
using LibProcess2;

namespace LibProcess2Test;

public class TestProcessRunner
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task TestSimpleRun()
    {
        var scriptFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
        await File.WriteAllTextAsync(scriptFile, "Write-Host 'Hello'");
        try
        {
            var processRunner = new ProcessRunner();
            var sb = new StringBuilder();
            await processRunner.Run("C:/program files/powershell/7/pwsh.exe", new[]
            {
                "/c",
                scriptFile
            }, null, s => sb.AppendLine(s));
            Assert.That("Hello", Is.EqualTo(sb.ToString().Trim()));
        }
        finally
        {
            File.Delete(scriptFile);
        }
    }

    [Test]
    public async Task TestCancelRun()
    {
        var scriptFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
        await File.WriteAllTextAsync(scriptFile, "while($true) { [System.Threading.Thread]::Sleep(100) }");
        try
        {
            var processRunner = new ProcessRunner();
            var sb = new StringBuilder();
            var cts = new CancellationTokenSource();
            var task = processRunner.Run("C:/program files/powershell/7/pwsh.exe", new[]
            {
                "/c",
                scriptFile
            }, null, s => sb.AppendLine(s), null, cts.Token);

            await Task.Delay(300);

            cts.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () => await task);
        }
        finally
        {
            File.Delete(scriptFile);
        }
    }
}