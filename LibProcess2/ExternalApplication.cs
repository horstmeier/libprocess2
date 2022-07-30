namespace LibProcess2;

public class ExternalApplication : IExternalApplication
{
    private readonly ExternalApplicationEx _externalApplication;

    public ExternalApplication(
        IProcessRunner processRunner,
        string fileName,
        string? defaultWorkingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        Func<int, bool>? defaultIsSuccess = null)
    {
        _externalApplication = new ExternalApplicationEx(
            processRunner,
            fileName,
            x => x,
            defaultWorkingDirectory,
            onOut,
            onErr,
            defaultIsSuccess);
    }

    public async Task<int> Run(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    ) => await _externalApplication.Run(arguments, workingDirectory, cancellationToken, isSuccess);

    public async Task<(int exitCode, string stdOut, string stdErr)> RunWithResult(IEnumerable<string> arguments, string? workingDirectory = null, CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null) =>
        await _externalApplication.RunWithResult(arguments, workingDirectory, cancellationToken, isSuccess);
}