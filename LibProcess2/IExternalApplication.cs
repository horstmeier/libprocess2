namespace LibProcess2;

public interface IExternalApplication
{
    Task<int> Run(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    );

    Task<(int exitCode, string stdOut, string stdErr)> RunWithResult(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    );
}