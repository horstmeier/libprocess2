using Microsoft.Extensions.Logging;

namespace LibProcess2;

public static class ProcessRunnerExtensions
{
    public static async Task<int> RunLogged(
        this IProcessRunner processRunner,
        ILogger log,
        string logPrefix,
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null)
    {
        return await processRunner.Run(
            fileName,
            arguments,
            workingDirectory,
            s => log.LogDebug("{Prefix} {Message}", logPrefix, s),
            s => log.LogDebug("{Prefix} E {Message}", logPrefix, s),
            cancellationToken,
            isSuccess);
    }
}