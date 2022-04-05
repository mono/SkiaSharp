using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var TARGET = Argument("t", Argument("target", "Default"));
var VERBOSITY = Argument("v", Argument("verbosity", Verbosity.Normal));
var CONFIGURATION = Argument("c", Argument("configuration", "Release"));

var VS_INSTALL = Argument("vsinstall", EnvironmentVariable("VS_INSTALL"));
var MSBUILD_EXE = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE"));

var BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT"));
var ADDITIONAL_GN_ARGS = Argument("gnArgs", Argument("gnargs", EnvironmentVariable("ADDITIONAL_GN_ARGS")));

DirectoryPath PROFILE_PATH = EnvironmentVariable("USERPROFILE") ?? EnvironmentVariable("HOME");

Information("Arguments:");
foreach (var arg in Arguments()) {
    foreach (var val in arg.Value) {
        Information($"    {arg.Key.PadRight(30)} {{0}}", val);
    }
}

void RunCake(FilePath cake, string target = null, Dictionary<string, string> arguments = null)
{
    var args = Arguments().ToDictionary(a => a.Key, a => a.Value.LastOrDefault());

    args["target"] = target;

    if (arguments != null) {
        foreach (var arg in arguments) {
            args[arg.Key] = arg.Value;
        }
    }

    cake = MakeAbsolute(cake);
    var cmd = $"cake {cake}";

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

string GetVersion(string lib, string type = "nuget")
{
    return GetRegexValue($@"^{lib}\s*{type}\s*(.*)$", ROOT_PATH.CombineWithFilePath("VERSIONS.txt"));
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
