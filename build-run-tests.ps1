Param (
    [bool] $BuildExternals = $true,
    [bool] $BuildManaged = $true,
    [bool] $PackNuGets = $true
)

$ErrorActionPreference = 'Stop'

# Prepare the script itself
. "./build-common.ps1"

# build the bits
. "./build-managed.ps1" -BuildExternals $BuildExternals -BuildManaged $BuildManaged -AssembleDocs $false -PackNuGets $( if ($PackNuGets) { 'CurrentPlatform' } else { 'None' } )

WriteLine "$hr"
WriteLine "Running the tests..."
WriteLine ""

$xunitFull32 = Join-Path $EXTERNALS_PATH "xunit.runner.console/tools/net452/xunit.console.x86.exe"
$xunitFull64 = Join-Path $EXTERNALS_PATH "xunit.runner.console/tools/net452/xunit.console.exe"

DownloadNuGet "xunit.runner.console" (GetVersion "xunit.runner.console" "release")

# New-Item "$OUTPUT_PATH/tests/windows/x86" -itemtype "Directory" -force | Out-Null
# New-Item "$OUTPUT_PATH/tests/windows/x64" -itemtype "Directory" -force | Out-Null
# New-Item "$OUTPUT_PATH/tests/mac/AnyCPU" -itemtype "Directory" -force | Out-Null
# New-Item "$OUTPUT_PATH/tests/linux/x64" -itemtype "Directory" -force | Out-Null
# New-Item "$OUTPUT_PATH/tests/netcore" -itemtype "Directory" -force | Out-Null

if ($IsMacOS) {
    MSBuild "tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln" -target "Restore"
    MSBuild "tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln"
    # Exec $xunitFull64 -args " tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/SkiaSharp.Tests.dll -verbose -parallel none -nunit ""$OUTPUT_PATH/tests/mac/AnyCPU/TestResult.xml"" "

#     // Mac OSX (Any CPU)
#     if (IsRunningOnMac ()) {
#         EnsureDirectoryExists ("./output/tests/mac/AnyCPU");
#         RunMSBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
#         RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/SkiaSharp.Tests.dll", null, false);
#         CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/TestResult.xml", "./output/tests/mac/AnyCPU");
#     }

} elseif ($IsLinux) {

} else {
    # MSBuild "tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln" -arch "x86"
    # Exec $xunitFull32 "assemble --out=""$OUTPUT_PATH/docs/mdoc/SkiaSharp"" ""docs/en"" --debug"
}

WriteLine "Tests complete."
WriteLine "$hr"
WriteLine ""


# var RunTests = new Action<FilePath, string[], bool> ((testAssembly, skip, is32) =>
# {
#     var dir = testAssembly.GetDirectory ();
#     var settings = new XUnit2Settings {
#         NUnitReport = true,
#         ReportName = "TestResult",
#         UseX86 = is32,
#         Parallelism = ParallelismOption.Assemblies,
#         OutputDirectory = dir,
#         WorkingDirectory = dir,
#         ArgumentCustomization = args => args.Append ("-verbose"),
#     };
#     if (skip != null) {
#         settings.TraitsToExclude.Add ("Category", skip);
#     }
#     XUnit2 (new [] { testAssembly }, settings);
# });

# Task ("tests")
#     .IsDependentOn ("libs")
#     .IsDependentOn ("nuget")
#     .Does (() => 
# {
#     ClearSkiaSharpNuGetCache (VERSION_PACKAGES.Keys.ToArray ());

#     RunNuGetRestore ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");

#     // Windows (x86 and x64)
#     if (IsRunningOnWindows ()) {
#         EnsureDirectoryExists ("./output/tests/windows/x86");
#         RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", "x86");
#         RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/SkiaSharp.Tests.dll", null, true);
#         CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/TestResult.xml", "./output/tests/windows/x86");

#         EnsureDirectoryExists ("./output/tests/windows/x64");
#         RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", "x64");
#         RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/SkiaSharp.Tests.dll", null, false);
#         CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/TestResult.xml", "./output/tests/windows/x64");
#     }

#     // Linux (x64)
#     if (IsRunningOnLinux ()) {
#         EnsureDirectoryExists ("./output/tests/linux/x64");
#         RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", "x64");
#         RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/SkiaSharp.Tests.dll", null, false);
#         CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/TestResult.xml", "./output/tests/linux/x64");
#     }

#     // .NET Core
#     EnsureDirectoryExists ("./output/tests/netcore");
#     RunNuGetRestore ("./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.sln");
#     RunNetCoreTests ("./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.csproj", null);
#     CopyFileToDirectory ("./tests/SkiaSharp.NetCore.Tests/TestResult.xml", "./output/tests/netcore");
# });
