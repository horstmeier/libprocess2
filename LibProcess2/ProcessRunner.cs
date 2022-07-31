using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LibProcess2;
public class ProcessRunner : IProcessRunner
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
        foreach (var argument in arguments)
        {
            psi.ArgumentList.Add(argument);
        }
        var process = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        var argText = psi.Arguments.Aggregate(new StringBuilder(), (acc, it) => acc.Append(it).Append(' '));
        _log?.LogDebug("Starting process {Cmd} {CmdLine}", psi.FileName, argText);
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
        if (psi.RedirectStandardError) process.BeginErrorReadLine();
        if (psi.RedirectStandardOutput) process.BeginOutputReadLine();
        _log?.LogDebug("Waiting for process to terminate");
        try
        {
            await process.WaitForExitAsync(ct);
            if (psi.RedirectStandardError) await semaError.WaitAsync(ct);
            if (psi.RedirectStandardOutput) await semaOutput.WaitAsync(ct);
        }
        catch (Exception e)
        {
            _log?.LogWarning(e, "Received exception while waiting for process");
            if (!process.HasExited) process.Kill();
            throw;
        }
        _log?.LogDebug("Process exited with {ExitCode}", process.ExitCode);
        if (isSuccess != null && !isSuccess(process.ExitCode))
        {
            throw new Exception($"{fileName} returned error {process.ExitCode}");
        }

        return process.ExitCode;
    }
}
