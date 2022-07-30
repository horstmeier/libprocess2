using System.Collections.Immutable;

namespace LibProcess2;

public class BatchProcessor
{
    private readonly IProcessRunner _processRunner;
    private readonly string _comspec;

    public BatchProcessor(IProcessRunner processRunner)
    {
        _comspec = Environment.GetEnvironmentVariable("comspec")
                   ?? throw new Exception("Batch processor is not supported here");
        _processRunner = processRunner;
    }
    public async Task<(int exitCode, string stdOut, string stdErr)> Run(
        FileInfo batchFile,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null) =>
        await _processRunner.RunWithResult(
            _comspec,
            arguments.Aggregate(ImmutableList<string>.Empty.Add("/c"),
                (acc, it) => acc.Add(it)),
            workingDirectory ?? batchFile.DirectoryName,
            cancellationToken,
            null);

    public async Task<(int exitCode, string stdOut, string stdErr)> Execute(
        string command,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null)
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".cmd");
        await File.WriteAllTextAsync(file, command);
        try
        {
            return await Run(new FileInfo(file), Array.Empty<string>(), workingDirectory, cancellationToken);
        }
        finally
        {
            File.Delete(file);
        }
    }

}