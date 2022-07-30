using System.Text;

namespace LibProcess2;

public class ExternalApplication
{
    private readonly IProcessRunner _processRunner;
    private readonly string _fileName;
    private readonly string? _defaultWorkingDirectory;
    private readonly Action<string?>? _onOut;
    private readonly Action<string?>? _onErr;
    private readonly Func<int, bool>? _defaultIsSuccess;

    public ExternalApplication(
        IProcessRunner processRunner,
        string fileName,
        string? defaultWorkingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        Func<int, bool>? defaultIsSuccess = null)
    {
        _processRunner = processRunner;
        _fileName = fileName;
        _defaultWorkingDirectory = defaultWorkingDirectory;
        _onOut = onOut;
        _onErr = onErr;
        _defaultIsSuccess = defaultIsSuccess;
    }

    public async Task<int> Run(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    ) =>
        await _processRunner.Run(
            _fileName,
            arguments,
            workingDirectory ?? _defaultWorkingDirectory,
            _onOut,
            _onErr,
            cancellationToken,
            isSuccess ?? _defaultIsSuccess);

    public async Task<(int exitCode, string stdOut, string stdErr)> RunWithResult(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
    {
        var sbOut = new StringBuilder();
        var sbErr = new StringBuilder();

        var exitCode = await _processRunner.Run(
            _fileName,
            arguments,
            workingDirectory ?? _defaultWorkingDirectory,
            s =>
            {
                _onOut?.Invoke(s);
                sbOut.AppendLine(s);
            },
            s =>
            {
                _onErr?.Invoke(s);
                sbErr.AppendLine(s);
            },
            cancellationToken,
            isSuccess ?? _defaultIsSuccess);
        return (exitCode, sbOut.ToString(), sbErr.ToString());
    }
}