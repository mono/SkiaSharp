using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var TARGET = Argument("t", Argument("target", "Default"));
var VERBOSITY = Argument("v", Argument("verbosity", Verbosity.Normal));
var CONFIGURATION = Argument("c", Argument("configuration", "Release"));

var CAKE_ARGUMENTS = (IReadOnlyDictionary<string, string>)Context.Arguments
    .GetType()
    .GetProperty("Arguments")
    .GetValue(Context.Arguments);

DirectoryPath PROFILE_PATH = EnvironmentVariable("USERPROFILE") ?? EnvironmentVariable("HOME");

void RunCake(FilePath cake, string target = null, Dictionary<string, string> arguments = null)
{
    var args = new Dictionary<string, string>();

    foreach (var arg in CAKE_ARGUMENTS) {
        args[args.Key] = arg.Value;
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

void RunProcess(FilePath process, string args)
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
    try {
        var file = ROOT_PATH.CombineWithFilePath("VERSIONS.txt");
        var contents = System.IO.File.ReadAllText(file.FullPath);
        var match = Regex.Match(contents, $@"^{lib}\s*{type}\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
