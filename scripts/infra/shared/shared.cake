using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

var TARGET = Argument("t", Argument("target", "Default"));
var VERBOSITY = Context.Log.Verbosity;
var CONFIGURATION = Argument("c", Argument("configuration", "Release"));

var VS_INSTALL = Argument("vsinstall", EnvironmentVariable("VS_INSTALL"));
var MSBUILD_EXE = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE"));

var BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT"));
var ADDITIONAL_GN_ARGS = Argument("gnArgs", Argument("gnargs", EnvironmentVariable("ADDITIONAL_GN_ARGS")));

DirectoryPath PROFILE_PATH = EnvironmentVariable("USERPROFILE") ?? EnvironmentVariable("HOME");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD IDENTITY
////////////////////////////////////////////////////////////////////////////////////////////////////

var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview");
var FEATURE_NAME = EnvironmentVariable ("FEATURE_NAME") ?? "";
var BUILD_NUMBER = Argument ("buildNumber", EnvironmentVariable ("BUILD_NUMBER") ?? "0");
var BUILD_COUNTER = Argument ("buildCounter", EnvironmentVariable ("BUILD_COUNTER") ?? BUILD_NUMBER);
var GIT_SHA = Argument ("gitSha", EnvironmentVariable ("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument ("gitBranch", EnvironmentVariable ("GIT_BRANCH_NAME") ?? ""). Replace ("refs/heads/", "");
var GIT_URL = Argument ("gitUrl", EnvironmentVariable ("GIT_URL") ?? "");

var PREVIEW_NUGET_SUFFIX = "";
if (!string.IsNullOrEmpty (FEATURE_NAME)) {
    PREVIEW_NUGET_SUFFIX = $"featurepreview-{FEATURE_NAME}";
} else {
    PREVIEW_NUGET_SUFFIX = $"{PREVIEW_LABEL}";
}
if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
    PREVIEW_NUGET_SUFFIX += $".{BUILD_NUMBER}";
}

var CURRENT_PLATFORM = "";
if (IsRunningOnWindows ()) {
    CURRENT_PLATFORM = "Windows";
} else if (IsRunningOnMacOs ()) {
    CURRENT_PLATFORM = "Mac";
} else if (IsRunningOnLinux ()) {
    CURRENT_PLATFORM = "Linux";
} else {
    throw new Exception ("This script is not running on a known platform.");
}

var MSBUILD_VERSION_PROPERTIES = new Dictionary<string, string> {
    { "GIT_SHA", GIT_SHA },
    { "GIT_BRANCH_NAME", GIT_BRANCH_NAME },
    { "GIT_URL", GIT_URL },
    { "BUILD_COUNTER", BUILD_COUNTER },
    { "BUILD_NUMBER", BUILD_NUMBER },
    { "FEATURE_NAME", FEATURE_NAME },
    { "PREVIEW_LABEL", PREVIEW_LABEL },
};

var DATE_TIME_NOW = DateTime.Now;
var DATE_TIME_STR = DATE_TIME_NOW.ToString ("yyyyMMdd_hhmmss");

////////////////////////////////////////////////////////////////////////////////////////////////////
// WELL-KNOWN PATHS
////////////////////////////////////////////////////////////////////////////////////////////////////

FilePath NUGET_CONFIG_PATH = MakeAbsolute(ROOT_PATH.CombineWithFilePath("nuget.config"));
DirectoryPath PACKAGE_CACHE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));
DirectoryPath OUTPUT_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets"));
DirectoryPath OUTPUT_SPECIAL_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets-special"));
DirectoryPath OUTPUT_SYMBOLS_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets-symbols"));
DirectoryPath DOCS_ROOT_PATH = ROOT_PATH.Combine("docs");
DirectoryPath DOCS_PATH = DOCS_ROOT_PATH.Combine("SkiaSharpAPI");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD OPTIONS
////////////////////////////////////////////////////////////////////////////////////////////////////

var SKIP_EXTERNALS = Argument ("skipexternals", "")
    .ToLower ().Split (new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
var SKIP_BUILD = Argument ("skipbuild", false);
var THROW_ON_FIRST_TEST_FAILURE = Argument ("throwOnFirstTestFailure", false);
var NUGET_DIFF_PRERELEASE = Argument ("nugetDiffPrerelease", false);
var COVERAGE = Argument ("coverage", false);
var CHROMEWEBDRIVER = Argument ("chromedriver", EnvironmentVariable ("CHROMEWEBDRIVER"));

var CI_ARTIFACTS_FEED_URL = Argument ("previewFeed", "https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp-CI/nuget/v3/index.json");

var PREVIEW_ONLY_NUGETS = new List<string> {};

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET PACKAGES
////////////////////////////////////////////////////////////////////////////////////////////////////

var SUPPORTED_NUGETS = new Dictionary<string, Version> {
    // SkiaSharp core
    { "SkiaSharp",                                     new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.Linux",                  new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.Linux.NoDependencies",   new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.NanoServer",             new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.WebAssembly",            new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.Android",                new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.iOS",                    new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.MacCatalyst",            new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.macOS",                  new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.Tizen",                  new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.tvOS",                   new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.Win32",                  new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.WinUI",                  new Version (2, 80, 0) },
    { "SkiaSharp.Views",                               new Version (2, 80, 0) },
    { "SkiaSharp.Views.Desktop.Common",                new Version (2, 80, 0) },
    { "SkiaSharp.Views.Gtk3",                          new Version (2, 80, 0) },
    { "SkiaSharp.Views.Gtk4",                          new Version (3, 119, 0) },
    { "SkiaSharp.Views.WindowsForms",                  new Version (2, 80, 0) },
    { "SkiaSharp.Views.WPF",                           new Version (2, 80, 0) },
    { "SkiaSharp.Views.Uno.WinUI",                     new Version (2, 80, 0) },
    { "SkiaSharp.Views.WinUI",                         new Version (2, 80, 0) },
    { "SkiaSharp.Views.Maui.Core",                     new Version (2, 88, 0) },
    { "SkiaSharp.Views.Maui.Controls",                 new Version (2, 88, 0) },
    { "SkiaSharp.Views.Blazor",                        new Version (2, 80, 0) },
    // HarfBuzzSharp core
    { "HarfBuzzSharp",                                 new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.Android",            new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.iOS",                new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.Linux",              new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.MacCatalyst",        new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.macOS",              new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.Tizen",              new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.tvOS",               new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.WebAssembly",        new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.Win32",              new Version (2, 6, 1) },
    // Extras
    { "SkiaSharp.HarfBuzz",                            new Version (2, 80, 0) },
    { "SkiaSharp.Skottie",                             new Version (2, 88, 0) },
    { "SkiaSharp.SceneGraph",                          new Version (2, 88, 0) },
    { "SkiaSharp.Resources",                           new Version (2, 88, 0) },
    { "SkiaSharp.Vulkan.SharpVk",                      new Version (2, 80, 0) },
    { "SkiaSharp.Direct3D.Vortice",                    new Version (2, 88, 0) },
};

var OBSOLETED_NUGETS = new Dictionary<string, Version> {
    // Obsolete packages no longer built but still tracked for documentation
    { "SkiaSharp.NativeAssets.UWP",                    new Version (2, 80, 0) },
    { "SkiaSharp.NativeAssets.watchOS",                new Version (2, 80, 0) },
    { "SkiaSharp.Views.Gtk2",                          new Version (2, 80, 0) },
    { "SkiaSharp.Views.Maui.Controls.Compatibility",   new Version (2, 88, 0) },
    { "SkiaSharp.Views.Forms",                         new Version (2, 80, 0) },
    { "SkiaSharp.Views.Forms.WPF",                     new Version (2, 80, 0) },
    { "SkiaSharp.Views.Forms.GTK",                     new Version (2, 80, 0) },
    { "SkiaSharp.Views.Uno",                           new Version (2, 80, 0) },
    { "SkiaSharp.Views.NativeAssets.UWP",              new Version (2, 80, 0) },
    { "HarfBuzzSharp.NativeAssets.UWP",                new Version (2, 6, 1) },
    { "HarfBuzzSharp.NativeAssets.watchOS",            new Version (2, 6, 1) },
};

var TRACKED_NUGETS = SUPPORTED_NUGETS
    .Concat(OBSOLETED_NUGETS)
    .ToDictionary(x => x.Key, x => x.Value);

////////////////////////////////////////////////////////////////////////////////////////////////////
// LOGGING
////////////////////////////////////////////////////////////////////////////////////////////////////

Information("Arguments:");
foreach (var arg in Arguments()) {
    foreach (var val in arg.Value) {
        Information($"    {arg.Key.PadRight(30)} {{0}}", val);
    }
}

Information("Source Control:");
Information($"    {"PREVIEW_LABEL".PadRight(30)} {{0}}", PREVIEW_LABEL);
Information($"    {"FEATURE_NAME".PadRight(30)} {{0}}", FEATURE_NAME);
Information($"    {"BUILD_NUMBER".PadRight(30)} {{0}}", BUILD_NUMBER);
Information($"    {"GIT_SHA".PadRight(30)} {{0}}", GIT_SHA);
Information($"    {"GIT_BRANCH_NAME".PadRight(30)} {{0}}", GIT_BRANCH_NAME);
Information($"    {"GIT_URL".PadRight(30)} {{0}}", GIT_URL);

void RunCake(FilePath cake, string target = null, Dictionary<string, string> arguments = null)
{
    var args = Arguments().ToDictionary(a => a.Key, a => a.Value.LastOrDefault());

    args["target"] = target;
    args["verbosity"] = VERBOSITY.ToString();

    if (arguments != null) {
        foreach (var arg in arguments) {
            args[arg.Key] = arg.Value;
        }
    }

    cake = MakeAbsolute(cake);
    var cmd = $"cake {cake} --working=\"{ROOT_PATH}\"";

    foreach (var arg in args) {
        cmd += $@" --{arg.Key}=""{arg.Value}""";
    }

    DotNetTool(cmd);
}

void RunProcess(FilePath process, string args = "")
{
    var result = StartProcess(process, args);
    if (result != 0) {
        throw new Exception($"Process '{process}' failed with error: {result}");
    }
}

void RunProcess(FilePath process, string args, out IEnumerable<string> stdout)
{
    var settings = new ProcessSettings {
        RedirectStandardOutput = true,
        Arguments = args,
    };
    var result = StartProcess(process, settings, out var stdoutActual);
    stdout = stdoutActual.ToArray();
    if (result != 0) {
        throw new Exception($"Process '{process}' failed with error: {result}");
    }
}

void RunProcess(FilePath process, ProcessSettings settings)
{
    var result = StartProcess(process, settings);
    if (result != 0) {
        throw new Exception($"Process '{process}' failed with error: {result}");
    }
}

IProcess RunAndReturnProcess(FilePath process, ProcessSettings settings)
{
    var proc = StartAndReturnProcess(process, settings);
    return proc;
}

IProcess RunAndReturnProcess(FilePath process, string arguments)
{
    var proc = RunAndReturnProcess(process, new ProcessSettings {
        Arguments = arguments,
    });
    return proc;
}

string GetVersion(string lib, string type = "nuget")
{
    return GetRegexValue($@"^{lib}\s*{type}\s*(.*)$", ROOT_PATH.CombineWithFilePath("scripts/VERSIONS.txt"));
}

string GetRegexValue(string regex, FilePath file)
{
    try {
        var contents = System.IO.File.ReadAllText(file.FullPath);
        var match = Regex.Match(contents, regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return match.Groups[1].Value.Trim();
    } catch {
        return "";
    }
}

List<string> MatchRegex(string regex, params string[] lines)
{
    var matches = new List<string>();
    foreach (var line in lines) {
        var match = Regex.Match(line, regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        if (match.Success) {
            matches.Add(match.Groups[1].Value.Trim());
        }
    }
    return matches;
}

void DeleteDir(DirectoryPath dir)
{
    if (DirectoryExists(dir))
        DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true, Force = true });
}

void CleanDir(DirectoryPath dir)
{
    if (DirectoryExists(dir)) {
        foreach (var d in GetSubDirectories(dir)) {
            DeleteDir(d);
        }
        CleanDirectory(dir);
    }

    EnsureDirectoryExists(dir);
}

void TakeSnapshot(DirectoryPath output, string name)
{
    if (!IsRunningOnMacOs())
        return;

    var fname = $"screenshot-{DateTime.Now:yyyyMMdd_hhmmss}-{name}.jpg";
    var dest = output.CombineWithFilePath(fname);

    RunProcess("screencapture", dest.FullPath);
}
