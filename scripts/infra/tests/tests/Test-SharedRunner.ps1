$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../test-shared.cake') -Raw
$start = $source.IndexOf('void RunDotNetTest(')
$end = $source.IndexOf('void RunCodeCoverage(', $start)
if ($start -lt 0 -or $end -lt 0) { throw 'The shared runner method was not found.' }
$method = $source.Substring($start, $end - $start).Replace(
    'void RunDotNetTest(', 'public static void RunDotNetTest(')

# Compile the actual argument-mapping method without restoring Cake or launching test hosts.
$harness = @'
using System;
using System.Collections.Generic;
using System.IO;

public sealed class FilePath
{
    public string FullPath { get; }
    public FilePath(string path) { FullPath = path; }
    public DirectoryPath GetDirectory() => new DirectoryPath(Path.GetDirectoryName(FullPath));
    public string GetExtension() => Path.GetExtension(FullPath);
}

public sealed class DirectoryPath
{
    public string FullPath { get; }
    public DirectoryPath(string path) { FullPath = path; }
    public DirectoryPath Combine(string path) => new DirectoryPath(Path.Combine(FullPath, path));
}

public sealed class ProcessArgumentBuilder
{
    public List<string> Values { get; } = new List<string>();
    public List<string> Quoted { get; } = new List<string>();
    public ProcessArgumentBuilder Append(string value) { Values.Add(value); return this; }
    public ProcessArgumentBuilder AppendQuoted(string value)
    {
        Values.Add(value);
        Quoted.Add(value);
        return this;
    }
}

public enum DotNetVerbosity { Normal }

public sealed class DotNetTestSettings
{
    public string Configuration { get; set; }
    public bool NoBuild { get; set; }
    public DirectoryPath WorkingDirectory { get; set; }
    public DotNetVerbosity Verbosity { get; set; }
    public Func<ProcessArgumentBuilder, ProcessArgumentBuilder> ArgumentCustomization { get; set; }
}

public static class SharedRunnerFixture
{
    public static bool COVERAGE;
    public static string CONFIGURATION = "Release";
    public static DotNetTestSettings Settings;
    public static ProcessArgumentBuilder Arguments;
    private static FilePath MakeAbsolute(FilePath path) => path;
    private static DirectoryPath MakeAbsolute(DirectoryPath path) => path;
    private static void DotNetTest(string project, DotNetTestSettings settings)
    {
        Settings = settings;
        Arguments = settings.ArgumentCustomization(new ProcessArgumentBuilder());
    }
__METHOD__
}
'@
Add-Type -TypeDefinition $harness.Replace('__METHOD__', $method)

function Assert-Argument([string] $Value, [bool] $Present = $true) {
    if ([SharedRunnerFixture]::Arguments.Values.Contains($Value) -ne $Present) {
        throw "Unexpected argument presence: $Value (expected $Present)."
    }
}

$output = [DirectoryPath]::new('/results/shared runner')
[SharedRunnerFixture]::RunDotNetTest([FilePath]::new('/tests/Console.slnx'), $output)
Assert-Argument '/p:Platform=Any CPU'
Assert-Argument '--show-live-output'
Assert-Argument '--no-ansi'
Assert-Argument '--report-trx'
Assert-Argument '--report-trx-filename' $false
Assert-Argument '--filter-trait' $false
Assert-Argument '--hangdump'
Assert-Argument '15m'
if (-not [SharedRunnerFixture]::Settings.NoBuild) { throw 'Default no-build behavior changed.' }
if (-not [SharedRunnerFixture]::Arguments.Quoted.Contains('/p:Platform=Any CPU')) {
    throw 'The solution platform argument lost its quoting.'
}

[SharedRunnerFixture]::RunDotNetTest([FilePath]::new('/tests/Console.csproj'), $output)
Assert-Argument '/p:Platform=AnyCPU'
Assert-Argument '--report-trx-filename'
Assert-Argument 'TestResults.trx'
Assert-Argument '--filter-trait' $false

$properties = [Collections.Generic.Dictionary[string, string]]::new()
$properties.Add('Platform', 'x64')
$properties.Add('TargetFramework', 'net10.0')
$properties.Add('TargetFrameworks', 'net10.0')
[SharedRunnerFixture]::RunDotNetTest(
    [FilePath]::new('/tests/Samples.csproj'), $output, $null, $properties,
    $false, $null, [string[]]@('--filter-trait', 'RunGroup=Build'))
Assert-Argument '/p:Platform=x64'
Assert-Argument '/p:TargetFramework=net10.0'
Assert-Argument '/p:TargetFrameworks=net10.0'
Assert-Argument '--hangdump' $false
Assert-Argument '--filter-trait'
Assert-Argument 'RunGroup=Build'
if ([SharedRunnerFixture]::Settings.NoBuild) { throw 'The Samples build request was ignored.' }
if ($properties.Count -ne 3) { throw 'The caller properties were modified.' }
if (-not [SharedRunnerFixture]::Arguments.Quoted.Contains('RunGroup=Build')) {
    throw 'The Samples trait argument lost its quoting.'
}
if ([SharedRunnerFixture]::Arguments.Values.IndexOf('--filter-trait') -le
    [SharedRunnerFixture]::Arguments.Values.IndexOf('--report-trx')) {
    throw 'Runner arguments were not forwarded after the common reporting arguments.'
}

[SharedRunnerFixture]::RunDotNetTest(
    [FilePath]::new('/tests/Samples.csproj'), $output, $null, $null,
    $true, $null, $null)
Assert-Argument '--filter-trait' $false
Assert-Argument '--hangdump' $false

[SharedRunnerFixture]::COVERAGE = $true
[SharedRunnerFixture]::RunDotNetTest([FilePath]::new('/tests/Console.slnx'), $output)
Assert-Argument '--coverage'
Assert-Argument '--coverage-output-format'
Assert-Argument 'cobertura'
Assert-Argument $output.Combine('Coverage').FullPath
Assert-Argument '--report-trx-filename' $false
Assert-Argument '--filter-trait' $false
[SharedRunnerFixture]::COVERAGE = $false

'Shared runner solution/project, caller properties, Samples filtering/build/hang and MTP coverage checks passed.'
