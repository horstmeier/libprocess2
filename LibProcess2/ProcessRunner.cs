using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LibProcess2;

internal class ProcessArguments
{
    private readonly string? _arguments;
    private readonly IEnumerable<string>? _argumentsList;
    public ProcessArguments(string arguments)
    {
        _arguments = arguments;
        _argumentsList = null;
    }

    public ProcessArguments(IEnumerable<string> argumentsList)
    {
        _arguments = null;
        _argumentsList = argumentsList;
    }

    public void Fill(ProcessStartInfo startInfo)
    {
        if (_arguments != null)
        {
            startInfo.Arguments = _arguments;
        }
        else if (_argumentsList != null)
        {
            foreach (var arg in _argumentsList)
            {
                startInfo.ArgumentList.Add(arg);
            }
        }
    }
}
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
        return await InternalRun(fileName,
            new ProcessArguments(arguments),
            workingDirectory,
            onOut,
            onErr,
            cancellationToken,
            isSuccess);
    }
    public async Task<int> Run(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
    {
        return await InternalRun(fileName,
            new ProcessArguments(arguments),
            workingDirectory,
            onOut,
            onErr,
            cancellationToken,
            isSuccess);
    }

    private async Task<int> InternalRun(
        string fileName,
        ProcessArguments arguments,
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
        arguments.Fill(psi);
        var process = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = psi
        };
        _log?.LogDebug("Starting process {Cmd} {CmdLine}", psi.FileName, psi.Arguments);
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
            process.Close();
        }
        catch (Exception e)
        {
            _log?.LogError(e, "Error while waiting for process to terminate");
            if (!process.HasExited) process.Kill();
            throw;
        }
        _log?.LogDebug("Process terminated with exit code {ExitCode}", process.ExitCode);
        if (isSuccess != null && !isSuccess(process.ExitCode))
        {
            
            throw new Exception($"{fileName} returned error {process.ExitCode}");
        }

        return process.ExitCode;
    }
}