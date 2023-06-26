#load "shared.cake"

var PYTHON_EXE = Argument("python", EnvironmentVariable("PYTHON_EXE") ?? "python3");

if (!string.IsNullOrEmpty(PYTHON_EXE) && FileExists(PYTHON_EXE)) {
    var dir = MakeAbsolute((FilePath)PYTHON_EXE).GetDirectory();
    var oldPath = EnvironmentVariable("PATH");
    System.Environment.SetEnvironmentVariable("PATH", dir.FullPath + System.IO.Path.PathSeparator + oldPath);
}

DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia/third_party/externals/harfbuzz"));

var EXE_EXTENSION = IsRunningOnWindows() ? ".exe" : "";
var GN_EXE = Argument("gn", EnvironmentVariable("GN_EXE") ?? SKIA_PATH.CombineWithFilePath($"bin/gn{EXE_EXTENSION}").FullPath);
var NINJA_EXE = Argument("ninja", EnvironmentVariable("NINJA_EXE") ?? DEPOT_PATH.CombineWithFilePath($"ninja{EXE_EXTENSION}").FullPath);

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

    RunProcess(PYTHON_EXE, new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath("tools/git-sync-deps").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

void GnNinja(DirectoryPath outDir, string target, string skiaArgs)
{
    var isCore = Context.Environment.Runtime.IsCoreClr;

    var quote = IsRunningOnWindows() || isCore ? "\"" : "'";
    var innerQuote = IsRunningOnWindows() || isCore ? "\\\"" : "\"";

    // override win_vc with the command line args
    if (!string.IsNullOrEmpty(VS_INSTALL)) {
        DirectoryPath win_vc = VS_INSTALL;
        win_vc = win_vc.Combine("VC");
        skiaArgs += $" win_vc='{win_vc}' ";
    }

    skiaArgs += 
        $" skia_enable_tools=false " +
        $" is_official_build={CONFIGURATION.ToLower() == "release"} ".ToLower();

    // generate native skia build files
    RunProcess(GN_EXE, new ProcessSettings {
        Arguments = $"gen out/{outDir} --script-executable={quote}{PYTHON_EXE}{quote} --args={quote}{skiaArgs.Replace("'", innerQuote)}{quote}",
        WorkingDirectory = SKIA_PATH.FullPath,
    });

    // build native skia
    RunProcess(NINJA_EXE, new ProcessSettings {
        Arguments = $"-C out/{outDir} {target}",
        WorkingDirectory = SKIA_PATH.FullPath,
    });
}

bool Skip(string arch)
{
    arch = arch?.ToLower() ?? "";

    if (BUILD_ARCH.Length == 0 || BUILD_ARCH.Contains("all"))
        return false;

    switch (arch) {
        case "win32":
        case "i386":
            arch = "x86";
            break;
        case "x86_64":
            arch = "x64";
            break;
        case "armeabi-v7a":
        case "armel":
        case "armv7":
        case "armv7k":
            arch = "arm";
            break;
        case "arm64_32":
        case "arm64-v8a":
            arch = "arm64";
            break;
    }

    if (BUILD_ARCH.Contains(arch))
        return false;

    Warning($"Skipping architecture: {arch}");

    return true;
}
