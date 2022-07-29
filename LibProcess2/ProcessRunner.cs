using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LibProcess2;
public class ProcessRunner
{
    private readonly ILogger<ProcessRunner>? _log;

    public ProcessRunner(ILogger<ProcessRunner>? log = null)
    {
        _log = log;
    }

    public async Task<int> Run(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
    {
        var ct = cancellationToken ?? CancellationToken.None;
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            RedirectStandardError = onErr != null,
            RedirectStandardOutput = onOut != null,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        var sb = new StringBuilder().Append(fileName).Append(' ');
        foreach (var argument in arguments)
        {
            sb.Append(argument).Append(' ');
            psi.ArgumentList.Add(argument);
        }
        _log?.LogDebug("Starting process {CmdLine}", sb);
        var process = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        var semaOutput = new SemaphoreSlim(0);
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                semaOutput.Release();
            }
            else
            {
                onOut?.Invoke(e.Data);
            }
        };

        var semaError = new SemaphoreSlim(0);
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data == null)
            {
                semaError.Release();
            }
            else
            {
                onErr?.Invoke(e.Data);
            }
        };

        if (!process.Start())
        {
            throw new Exception("Failed to start process");
        }

        _log?.LogDebug("Waiting for process to terminate");
        try
        {
            await process.WaitForExitAsync(ct);
            await semaError.WaitAsync(ct);
            await semaOutput.WaitAsync(ct);
        }
        catch (Exception e)
        {
            _log?.LogWarning(e, "Received exception while waiting for process");
            if (!process.HasExited) process.Kill();
            throw;
        }
        _log?.LogDebug("Process exited with {ExitCode}", process.ExitCode);
        return process.ExitCode;
    }
}
