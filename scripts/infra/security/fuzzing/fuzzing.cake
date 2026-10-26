DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../../.."));

#load "../../shared/shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// FUZZING - prepare the drop shared by every fuzzing submission, or fuzz a target locally
//   dotnet cake --target=fuzzing-download
//   dotnet cake --target=fuzzing-prepare
//   dotnet cake --target=fuzzing --fuzzerTarget=managed-streams [--fuzzerMaxTime=60] [--fuzzerRuns=1000] [--fuzzerTimeout=60] [--fuzzerInput=<path>]
////////////////////////////////////////////////////////////////////////////////////////////////////

const string LibFuzzerDotnetUrl = "https://github.com/Metalnem/libfuzzer-dotnet/releases/download/v2025.05.02.0904/";

var FUZZER_EXE = Argument("fuzzer", EnvironmentVariable("FUZZER_EXE"));

DirectoryPath tools = ROOT_PATH.Combine("externals/fuzzing");
DirectoryPath drop = ROOT_PATH.Combine("output/fuzzing/drop");
DirectoryPath local = ROOT_PATH.Combine("output/fuzzing/local");
DirectoryPath bin = local.Combine("bin");
FilePath harnessProject = ROOT_PATH.CombineWithFilePath("tests/SkiaSharp.Fuzzing/SkiaSharp.Fuzzing.csproj");

Task("fuzzing-download")
    .Description("Download libfuzzer-dotnet into ./externals/fuzzing.")
    .Does(() =>
{
    if (!string.IsNullOrWhiteSpace(FUZZER_EXE))
        return;

    EnsureDirectoryExists(tools);

    string asset;
    if (IsRunningOnWindows())
        asset = "libfuzzer-dotnet-windows.exe";
    else if (IsRunningOnLinux())
        asset = "libfuzzer-dotnet-debian";
    else
        throw new Exception("libfuzzer-dotnet only supports Windows and Linux. On macOS use scripts/infra/security/fuzzing/docker/run-local.sh.");

    var exePath = tools.CombineWithFilePath(asset);
    if (!FileExists(exePath))
    {
        DownloadFile(LibFuzzerDotnetUrl + asset, exePath);
        if (!IsRunningOnWindows())
            System.IO.File.SetUnixFileMode(exePath.FullPath, System.IO.File.GetUnixFileMode(exePath.FullPath) | System.IO.UnixFileMode.UserExecute);
    }

    FUZZER_EXE = exePath.FullPath;
});

Task("fuzzing-prepare")
    .Description("Build the fuzzing harness and stage the per-platform drops into ./output/fuzzing/drop/<platform>.")
    .Does(() =>
{
    DotNetRun(
        harnessProject.FullPath,
        new ProcessArgumentBuilder()
            .Append("--prepare")
            .AppendQuoted(drop.FullPath),
        new DotNetRunSettings
        {
            Configuration = "Release",
            Framework = "net10.0",
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("AllTargetFrameworks", "net10.0")
        });
});

Task("fuzzing-run")
    .Description("Fuzz one target locally: --fuzzerTarget=<name> [--fuzzerMaxTime=<sec>] [--fuzzerRuns=<count>] [--fuzzerTimeout=<sec>] [--fuzzerInput=<path>].")
    .IsDependentOn("fuzzing-download")
    .Does(() =>
{
    var targetName = Argument<string>("fuzzerTarget", null) ?? throw new Exception("Specify the target with --fuzzerTarget=<name>.");

    DotNetRun(
        harnessProject.FullPath,
        new ProcessArgumentBuilder()
            .Append("--prepare")
            .AppendQuoted(bin.FullPath)
            .Append("--instrument"),
        new DotNetRunSettings
        {
            Configuration = "Release",
            Framework = "net10.0",
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("AllTargetFrameworks", "net10.0")
        });

    var crashes = local.Combine("crashes");
    var corpus = local.Combine("corpus").Combine(targetName);
    var seedDir = bin.Combine("seeds").Combine(targetName);
    EnsureDirectoryExists(crashes);
    EnsureDirectoryExists(corpus);

    var runner = bin.CombineWithFilePath(IsRunningOnWindows() ? "SkiaSharp.Fuzzing.exe" : "SkiaSharp.Fuzzing");
    var args = new ProcessArgumentBuilder()
        .AppendSwitchQuoted("--target_path", "=", runner.FullPath)
        .AppendSwitchQuoted("--target_arg", "=", targetName)
        .Append($"-timeout={Argument("fuzzerTimeout", 60)}")
        .AppendSwitchQuoted("-artifact_prefix", "=", $"{crashes.FullPath}/");

    if (Argument<int?>("fuzzerMaxTime", null) is { } maxTime)
        args.Append($"-max_total_time={maxTime}");

    if (Argument<int?>("fuzzerRuns", null) is { } runs)
        args.Append($"-runs={runs}");

    var input = Argument<string>("fuzzerInput", null);
    if (!string.IsNullOrWhiteSpace(input))
    {
        var inputPath = ROOT_PATH.CombineWithFilePath(input);
        if (DirectoryExists(inputPath.FullPath) && Argument<int?>("fuzzerRuns", null) is null)
            args.Append("-runs=0");
        args.AppendQuoted(inputPath.FullPath);
    }
    else
    {
        args.AppendQuoted(corpus.FullPath);
        args.AppendQuoted(seedDir.FullPath);
    }

    RunProcess(FUZZER_EXE, new ProcessSettings { Arguments = args });
});

Task("Default")
    .IsDependentOn("fuzzing-download")
    .IsDependentOn("fuzzing-run");

RunTarget(TARGET);
