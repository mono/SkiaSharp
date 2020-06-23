#load "shared.cake"

var BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT"));
var ADDITIONAL_GN_ARGS = Argument("gnArgs", EnvironmentVariable("ADDITIONAL_GN_ARGS"));

var PYTHON_EXE = Argument("python", EnvironmentVariable("PYTHON_EXE") ?? "python");

if (!string.IsNullOrEmpty(PYTHON_EXE) && FileExists(PYTHON_EXE)) {
    var dir = MakeAbsolute((FilePath)PYTHON_EXE).GetDirectory();
    var oldPath = EnvironmentVariable("PATH");
    System.Environment.SetEnvironmentVariable("PATH", dir.FullPath + System.IO.Path.PathSeparator + oldPath);
}

DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/harfbuzz"));

var EXE_EXTENSION = IsRunningOnWindows() ? ".exe" : "";
var GN_EXE = Argument("gn", EnvironmentVariable("GN_EXE") ?? SKIA_PATH.CombineWithFilePath($"bin/gn{EXE_EXTENSION}").FullPath);
var NINJA_EXE = Argument("ninja", EnvironmentVariable("NINJA_EXE") ?? DEPOT_PATH.CombineWithFilePath($"ninja{EXE_EXTENSION}").FullPath);

////////////////////////////////////////////////////////////////////////////////////////////////////
// TASKS
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("git-sync-deps")
    .Does(() =>
{
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

void StripSign(FilePath target)
{
    if (!IsRunningOnMac())
        throw new InvalidOperationException("lipo is only available on macOS.");

    target = MakeAbsolute(target);
    var archive = target;
    if (target.FullPath.EndsWith(".framework")) {
        archive = $"{target}/{target.GetFilenameWithoutExtension()}";
    }

    // strip anything we can
    RunProcess("strip", new ProcessSettings {
        Arguments = $"-x -S {archive}",
    });

    // re-sign with empty
    RunProcess("codesign", new ProcessSettings {
        Arguments = $"--force --sign - --timestamp=none {target}",
    });
}

void RunLipo(DirectoryPath directory, FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMac())
        throw new InvalidOperationException("lipo is only available on macOS.");

    EnsureDirectoryExists(directory.CombineWithFilePath(output).GetDirectory());

    var inputString = string.Join(" ", inputs.Select(i => string.Format("\"{0}\"", i)));
    RunProcess("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
    });
}

void RunLibtoolStatic(DirectoryPath directory, FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMac())
        throw new InvalidOperationException("libtool is only available on macOS.");

    EnsureDirectoryExists(directory.CombineWithFilePath(output).GetDirectory());

    var inputString = string.Join(" ", inputs.Select(i => string.Format("\"{0}\"", i)));
    RunProcess("libtool", new ProcessSettings {
        Arguments = string.Format("-static -o \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
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
