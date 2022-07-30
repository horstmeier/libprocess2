# libprocess2

The library helps executing external applications.

## Motivation

I have to call external applications frequently in my projects thus I wanted
to develop a library that allows to make this as easy as possible while at the
same time give me the features I need.

## ProcessRunner

The class `ProcessRunner` is the core of the project. It can be used like this:

```C#
var processRunner = new ProcessRunner();
var sb = new StringBuilder();
var retval = await processRunner.Run(
    "C:/program files/powershell/7/pwsh.exe", 
    new[] 
    {
        "/c",
        "script.ps1"
    });
```

This will call `C:/program files/powershell/7/pwsh.exe` with the arguments `/c script.ps1`

If an argument contains characters that are reserved by cmd.exe (like a whitespace for example), 
they will be escaped accordingly.

The run method is defined as follows:

```C#
Task<int> Run(
        string fileName,
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
```

The workingDirectory is the current directory when the appication executes. If this value is null,
the current working directory of the calling process will be used.

`onOut` and `onErr` can be used to retrieve the command line output of the application. `onOut` will get
the data written to `stdout`, `onErr`the output writte to `stderr`.

The `cancellationToken` can be used to stop the process. When set, the external process will be "killed".

The function returns the exit code of the process. If you prefer an exception if the exit code is invalid,
you can define the valid exit codes with `isSuccess`.

For example `n => n == 0` would throw for any exit code except 0, while for example `n => n <= 4` could be used
for robocopy (I think at least, that was how robocopy worked).

The constructor of ProcessRunner accepts an `ILogger` that will, if provided be used to add some log messages.
These log messages may or may not be what you need. They are just, what I usually want to see in the log file.

## ExternalApplication

Frequently you may want to call the same application with different arguments. This is what the class 
`ExternalApplication` is for. It has the same arguments as `ProcessRunner` but split between the 
constructor and the function call.

The constructor looks like this:
```C#
ExternalApplication(
        IProcessRunner processRunner,
        string fileName,
        string? defaultWorkingDirectory = null,
        Action<string?>? onOut = null,
        Action<string?>? onErr = null,
        Func<int, bool>? defaultIsSuccess = null)
```

with the run method

```C#
Task<int> Run(
        IEnumerable<string> arguments,
        string? workingDirectory = null,
        CancellationToken? cancellationToken = null,
        Func<int, bool>? isSuccess = null
    )
```

The `defaultWorkingDirectory` and the `defaultIsSuccess` parameters will be used if the values in the `Run` call
are null.
