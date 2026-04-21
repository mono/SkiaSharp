using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public static partial class Program
{
    // These use BCL-only APIs so they're safe as field initializers (no Cake context needed).
    // All Main_* methods can rely on these being initialized.
    internal static DirectoryPath ROOT_PATH = (DirectoryPath)System.IO.Directory.GetCurrentDirectory();
    internal static DirectoryPath PROFILE_PATH =
        (DirectoryPath)(Environment.GetEnvironmentVariable("USERPROFILE")
                      ?? Environment.GetEnvironmentVariable("HOME")
                      ?? "");

    internal static string TARGET;
    internal static Verbosity VERBOSITY;
    internal static string CONFIGURATION;

    internal static string VS_INSTALL;
    internal static string MSBUILD_EXE;

    internal static string[] BUILD_ARCH;

    internal static string BUILD_VARIANT;
    internal static string ADDITIONAL_GN_ARGS;

    private static void Main_Shared()
    {
        TARGET = Argument("t", Argument("target", "Default"));
        VERBOSITY = Context.Log.Verbosity;
        CONFIGURATION = Argument("c", Argument("configuration", "Release"));

        VS_INSTALL = Argument("vsinstall", EnvironmentVariable("VS_INSTALL"));
        MSBUILD_EXE = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE"));

        BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
            .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT"));
        ADDITIONAL_GN_ARGS = Argument("gnArgs", Argument("gnargs", EnvironmentVariable("ADDITIONAL_GN_ARGS")));

        Information("Arguments:");
        foreach (var arg in Arguments()) {
            foreach (var val in arg.Value) {
                Information($"    {arg.Key.PadRight(30)} {{0}}", val);
            }
        }
    }

    internal static void RunCake(FilePath cake, string target = null, Dictionary<string, string> arguments = null)
    {
        var args = Arguments().ToDictionary(a => a.Key, a => a.Value.LastOrDefault());

        args["target"] = target ?? "Default";
        args["verbosity"] = VERBOSITY.ToString();

        if (arguments is not null) {
            foreach (var (k, v) in arguments) {
                args[k] = v;
            }
        }

        cake = MakeAbsolute(cake);
        var cmd = string.Join(" ",
            args.Where(a => !string.IsNullOrEmpty(a.Value))
                .Select(a => $@"--{a.Key}=""{a.Value}"""));

        // Set working directory to the script's directory so relative paths
        // in the child build (e.g., "libHarfBuzzSharp/libHarfBuzzSharp.sln")
        // resolve correctly — matching the old dotnet cake behavior.
        RunProcess("dotnet", new ProcessSettings {
            Arguments = $@"run --file ""{cake}"" -- {cmd}",
            WorkingDirectory = cake.GetDirectory().FullPath,
        });
    }

    internal static void RunProcess(FilePath process, string args = "")
    {
        var result = StartProcess(process, args);
        if (result != 0) {
            throw new Exception($"Process '{process}' failed with error: {result}");
        }
    }

    internal static void RunProcess(FilePath process, string args, out IEnumerable<string> stdout)
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

    internal static void RunProcess(FilePath process, ProcessSettings settings)
    {
        var result = StartProcess(process, settings);
        if (result != 0) {
            throw new Exception($"Process '{process}' failed with error: {result}");
        }
    }

    internal static IProcess RunAndReturnProcess(FilePath process, ProcessSettings settings)
    {
        var proc = StartAndReturnProcess(process, settings);
        return proc;
    }

    internal static IProcess RunAndReturnProcess(FilePath process, string arguments)
    {
        var proc = RunAndReturnProcess(process, new ProcessSettings {
            Arguments = arguments,
        });
        return proc;
    }

    internal static string GetVersion(string lib, string type = "nuget")
    {
        return GetRegexValue($@"^{lib}\s*{type}\s*(.*)$", ROOT_PATH.CombineWithFilePath("scripts/VERSIONS.txt"));
    }

    internal static string GetRegexValue(string regex, FilePath file)
    {
        try {
            var contents = System.IO.File.ReadAllText(file.FullPath);
            var match = Regex.Match(contents, regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return match.Groups[1].Value.Trim();
        } catch {
            return "";
        }
    }

    internal static List<string> MatchRegex(string regex, params string[] lines)
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

    internal static void DeleteDir(DirectoryPath dir)
    {
        if (DirectoryExists(dir))
            DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true, Force = true });
    }

    internal static void CleanDir(DirectoryPath dir)
    {
        if (DirectoryExists(dir)) {
            foreach (var d in GetSubDirectories(dir)) {
                DeleteDir(d);
            }
            CleanDirectory(dir);
        }

        EnsureDirectoryExists(dir);
    }

    internal static void TakeSnapshot(DirectoryPath output, string name)
    {
        if (!IsRunningOnMacOs())
            return;

        var fname = $"screenshot-{DateTime.Now:yyyyMMdd_hhmmss}-{name}.jpg";
        var dest = output.CombineWithFilePath(fname);

        RunProcess("screencapture", dest.FullPath);
    }
}
