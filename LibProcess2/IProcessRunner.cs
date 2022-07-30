namespace LibProcess2;

public interface IProcessRunner
{
    Task<int> Run(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    );
}