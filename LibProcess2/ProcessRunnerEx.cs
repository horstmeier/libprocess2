namespace LibProcess2;

public class ProcessRunnerEx
{
    private readonly IProcessRunner _processRunner;
    private readonly Action<string?>? _onOut;
    private readonly Action<string?>? _onErr;

    public ProcessRunnerEx(
        IProcessRunner processRunner,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null)
    {
        _processRunner = processRunner;
        _onOut = onOut;
        _onErr = onErr;
    }

    public async Task<int> Run(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
    {
        return await _processRunner.Run(fileName, arguments, workingDirectory, _onOut, _onErr,
            cancellationToken, isSuccess);
    }
}