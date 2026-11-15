#load "../../shared/shared.cake"

var PYTHON_EXE = Argument("python", EnvironmentVariable("PYTHON_EXE") ?? "python3");

if (!string.IsNullOrEmpty(PYTHON_EXE) && FileExists(PYTHON_EXE)) {
    var dir = MakeAbsolute((FilePath)PYTHON_EXE).GetDirectory();
    var oldPath = EnvironmentVariable("PATH");
    System.Environment.SetEnvironmentVariable("PATH", dir.FullPath + System.IO.Path.PathSeparator + oldPath);
}

DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia/third_party/externals/harfbuzz"));

var EXE_EXTENSION = IsRunningOnWindows() ? ".exe" : "";
var GN_EXE = Argument("gn", EnvironmentVariable("GN_EXE") ?? SKIA_PATH.CombineWithFilePath($"bin/gn{EXE_EXTENSION}").FullPath);
var NINJA_EXE = Argument("ninja", EnvironmentVariable("NINJA_EXE") ?? "ninja");

////////////////////////////////////////////////////////////////////////////////////////////////////
// TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("git-sync-deps")
    .Does(() =>
{
    // first run some checks to make sure all the versions are in sync

    var milestoneFile = SKIA_PATH.CombineWithFilePath("include/core/SkMilestone.h");
    var incrementFile = SKIA_PATH.CombineWithFilePath("include/c/sk_types.h");

    var expectedMilestone = GetVersion("libSkiaSharp", "milestone");
    var expectedIncrement = GetVersion("libSkiaSharp", "increment");

    var actualMilestone = GetRegexValue(@"^#define SK_MILESTONE (\d+)\s*$", milestoneFile);
    var actualIncrement = GetRegexValue(@"^#define SK_C_INCREMENT (\d+)\s*$", incrementFile);

    if (actualMilestone != expectedMilestone)
        throw new Exception($"The libskia C++ API version did not match the expected '{expectedMilestone}', instead was '{actualMilestone}'.");
    if (actualIncrement != expectedIncrement)
        throw new Exception($"The libSkiaSharp C API version did not match the expected '{expectedIncrement}', instead was '{actualIncrement}'.");

    RunPython(SKIA_PATH, SKIA_PATH.CombineWithFilePath("tools/git-sync-deps"));
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

void RunPython(DirectoryPath working, FilePath script, string args = "", IDictionary<string, string> envVars = null)
{
    RunProcess(PYTHON_EXE, new ProcessSettings {
        Arguments = $"{script.FullPath} {args}",
        WorkingDirectory = working.FullPath,
        EnvironmentVariables = envVars,
    });
}

void RunGn(DirectoryPath working, DirectoryPath outDir, string args = "", IDictionary<string, string> envVars = null)
{
    var isCore = Context.Environment.Runtime.IsCoreClr;

    var quote = IsRunningOnWindows() || isCore ? "\"" : "'";
    var innerQuote = IsRunningOnWindows() || isCore ? "\\\"" : "\"";

    RunProcess(GN_EXE, new ProcessSettings {
        Arguments = $"gen {outDir} --script-executable={quote}{PYTHON_EXE}{quote} --args={quote}{args.Replace("'", innerQuote)}{quote}",
        WorkingDirectory = working.FullPath,
        EnvironmentVariables = envVars,
    });
}

void RunNinja(DirectoryPath working, DirectoryPath outDir, string target = "")
{
    RunProcess(NINJA_EXE, new ProcessSettings {
        Arguments = $"-C \"{outDir.FullPath}\" {target}",
        WorkingDirectory = working.FullPath,
    });
}

void GenerateGnBuild(DirectoryPath outDir, string skiaArgs)
{
    // override win_vc with the command line args
    if (!string.IsNullOrEmpty(VS_INSTALL)) {
        DirectoryPath win_vc = VS_INSTALL;
        win_vc = win_vc.Combine("VC");
        skiaArgs += $" win_vc='{win_vc}' ";
    }

    // RAW/DNG decoding is unsupported in all SkiaSharp native builds.
    skiaArgs +=
        $" skia_use_dng_sdk=false skia_use_piex=false " +
        $" skia_enable_tools=false " +
        $" is_official_build={CONFIGURATION.ToLower() == "release"} ".ToLower();

    // generate native skia build files
    RunGn(SKIA_PATH, $"out/{outDir}", skiaArgs);
}

void GnNinja(DirectoryPath outDir, string target, string skiaArgs)
{
    GenerateGnBuild(outDir, skiaArgs);

    // build native skia
    RunNinja(SKIA_PATH, $"out/{outDir}", target);
}

string ReduceArch(string arch)
{
    arch = arch?.ToLower() ?? "";

    switch (arch) {
        case "win32":
        case "i386":
        case "i586":
        case "x86":
            return "x86";
        case "x86_64":
        case "x64":
            return "x64";
        case "armeabi-v7a":
        case "armel":
        case "armv7":
        case "armv7k":
        case "arm":
            return "arm";
        case "arm64_32":
        case "arm64-v8a":
        case "aarch64":
        case "arm64":
            return "arm64";
        case "riscv64":
            return "riscv64";
        case "loong64":
        case "loongarch64":
            return "loongarch64";
    }

    throw new Exception($"Unknown architecture: {arch}");
}

bool Skip(string arch)
{
    if (BUILD_ARCH.Length == 0 || BUILD_ARCH.Contains("all"))
        return false;

    arch = ReduceArch(arch);

    if (BUILD_ARCH.Select(ReduceArch).Contains(arch))
        return false;

    Warning($"Skipping architecture: {arch}");

    return true;
}
