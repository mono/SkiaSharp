using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var TARGET = Argument("t", Argument("target", "Default"));
var VERBOSITY = Argument("v", Argument("verbosity", Verbosity.Normal));
var CONFIGURATION = Argument("c", Argument("configuration", "Release"));

var VS_INSTALL = Argument("vsinstall", EnvironmentVariable("VS_INSTALL"));
var MSBUILD_EXE = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE"));

var CAKE_ARGUMENTS = (IReadOnlyDictionary<string, string>)Context.Arguments
    .GetType()
    .GetProperty("Arguments")
    .GetValue(Context.Arguments);

var BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT"));
var ADDITIONAL_GN_ARGS = Argument("gnArgs", EnvironmentVariable("ADDITIONAL_GN_ARGS"));

DirectoryPath PROFILE_PATH = EnvironmentVariable("USERPROFILE") ?? EnvironmentVariable("HOME");

void RunCake(FilePath cake, string target = null, Dictionary<string, string> arguments = null)
{
    var args = new Dictionary<string, string>();

    foreach (var arg in CAKE_ARGUMENTS) {
        args[arg.Key] = arg.Value;
    }

    args.Remove("t");
    args["target"] = target;

    if (arguments != null) {
        foreach (var arg in arguments) {
            args[arg.Key] = arg.Value;
        }
    }

    CakeExecuteScript(cake, new CakeSettings {
        WorkingDirectory = cake.GetDirectory(),
        Arguments = args,
    });
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
    var result = StartProcess(process, settings, out stdout);
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

bool IsRunningOnMac()
{
    return System.Environment.OSVersion.Platform == PlatformID.MacOSX || MacPlatformDetector.IsMac.Value;
}

bool IsRunningOnLinux()
{
    return IsRunningOnUnix() && !IsRunningOnMac();
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

internal static class MacPlatformDetector
{
    internal static readonly Lazy<bool> IsMac = new Lazy<bool>(IsRunningOnMac);

    [DllImport("libc")]
    static extern int uname(IntPtr buf);

    static bool IsRunningOnMac()
    {
        IntPtr buf = IntPtr.Zero;
        try {
            buf = Marshal.AllocHGlobal(8192);
            // This is a hacktastic way of getting sysname from uname()
            if (uname(buf) == 0) {
                string os = Marshal.PtrToStringAnsi(buf);
                if (os == "Darwin")
                    return true;
            }
        } catch {
        } finally {
            if (buf != IntPtr.Zero)
                Marshal.FreeHGlobal(buf);
        }
        return false;
    }
}
