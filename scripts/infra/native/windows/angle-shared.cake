using System.Collections.Generic;

// Shared ANGLE checkout + build logic.
//
// Consumed by:
//   native/winui-angle/build.cake  (WinUI 3 / Windows App SDK)
//   native/uwp-angle/build.cake    (UWP, in-box Windows.UI.Xaml)
//
// NOTE: this file relies on helpers from native-shared.cake and
// windows-shared.cake (RunProcess, RunPython, RunGn, RunNinja,
// GetSpectreLibPath, CheckWindowsDependencies, VERIFY_EXCLUDED), so it must be
// #load-ed by a script that also loads those. It deliberately does not #load
// them itself so that the existing load order is left untouched.

// ---------------------------------------------------------------------------
// sync
// ---------------------------------------------------------------------------

// Everything needed to get an ANGLE checkout that can be `gn gen`-ed.
// Every step is idempotent, so this is safe to run on an incremental agent.
void SyncAngle(DirectoryPath anglePath, string angleVersion)
{
    CloneAngle(anglePath, angleVersion);
    SyncAngleSubmodules(anglePath);
    PatchAngleToolchainOutputNames(anglePath);
    WriteAngleGclientArgs(anglePath);
    WriteAngleLastChange(anglePath);
    DownloadAngleRc(anglePath);
    DownloadAngleClang(anglePath);
}

void CloneAngle(DirectoryPath anglePath, string angleVersion)
{
    if (DirectoryExists(anglePath))
        return;

    RunProcess("git", $"clone https://github.com/google/angle.git --branch {angleVersion} --depth 1 --single-branch --shallow-submodules {anglePath}");
}

void SyncAngleSubmodules(DirectoryPath anglePath)
{
    var submodules = new [] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/vulkan-deps",
        "third_party/astc-encoder/src",
        "tools/clang",
    };

    foreach (var submodule in submodules) {
        var sub = anglePath.Combine(submodule);
        if (FileExists(sub.CombineWithFilePath("BUILD.gn")) || FileExists(sub.CombineWithFilePath(".gitignore")))
            continue;

        RunProcess("git", new ProcessSettings {
            Arguments = $"submodule update --init --recursive --depth 1 --single-branch {submodule}",
            WorkingDirectory = anglePath.FullPath,
        });
    }
}

// Emit the import lib and pdb next to the dll instead of into the root of the
// output directory, so the per-arch copies below do not collide.
void PatchAngleToolchainOutputNames(DirectoryPath anglePath)
{
    var toolchain = anglePath.CombineWithFilePath("build/toolchain/win/toolchain.gni");
    var contents = System.IO.File.ReadAllText(toolchain.FullPath);
    var newContents = contents
        .Replace("\"${dllname}.lib\"", "\"{{output_dir}}/{{target_output_name}}.lib\"")
        .Replace("\"${dllname}.pdb\"", "\"{{output_dir}}/{{target_output_name}}.pdb\"");

    if (contents != newContents)
        System.IO.File.WriteAllText(toolchain.FullPath, newContents);
}

// Normally written by gclient, which we do not run.
void WriteAngleGclientArgs(DirectoryPath anglePath)
{
    var gclientArgs = anglePath.CombineWithFilePath("build/config/gclient_args.gni");
    if (FileExists(gclientArgs))
        return;

    var lines = new [] {
        "checkout_angle_internal = false",
        "checkout_angle_mesa = false",
        "checkout_angle_restricted_traces = false",
        "generate_location_tags = false"
    };
    System.IO.File.WriteAllLines(gclientArgs.FullPath, lines);
}

void WriteAngleLastChange(DirectoryPath anglePath)
{
    var lastchange = anglePath.CombineWithFilePath("build/util/LASTCHANGE");
    if (FileExists(lastchange))
        return;

    RunPython(anglePath, anglePath.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
}

void DownloadAngleRc(DirectoryPath anglePath)
{
    const string rcExe = "build/toolchain/win/rc/win/rc.exe";

    var rcPath = anglePath.CombineWithFilePath(rcExe);
    if (FileExists(rcPath))
        return;

    var shaPath = anglePath.CombineWithFilePath($"{rcExe}.sha1");
    var sha = System.IO.File.ReadAllText(shaPath.FullPath);
    var url = $"https://storage.googleapis.com/download/storage/v1/b/chromium-browser-clang/o/rc%2F{sha}?alt=media";
    DownloadFile(url, rcPath);
}

void DownloadAngleClang(DirectoryPath anglePath)
{
    if (FileExists(anglePath.CombineWithFilePath("third_party/llvm-build/Release+Asserts/cr_build_revision")))
        return;

    RunPython(anglePath, anglePath.CombineWithFilePath("tools/clang/scripts/update.py"));
}

// ---------------------------------------------------------------------------
// build
// ---------------------------------------------------------------------------

// GN args common to every ANGLE flavour we ship. `flavorArgs` carries the bits
// that differ (target_os, angle_is_winappsdk, winappsdk_dir, ...) and
// `extraCFlags` is appended to the shared hardening flags.
string AngleGnArgs(string arch, string[] flavorArgs = null, string[] extraCFlags = null)
{
    var cflags = new List<string> { "'/guard:cf'", "'/GS'" };
    if (extraCFlags != null) {
        foreach (var flag in extraCFlags)
            cflags.Add($"'{flag}'");
    }

    var args = new List<string> { $"target_cpu='{arch}'" };

    if (flavorArgs != null)
        args.AddRange(flavorArgs);

    args.Add("is_component_build=false");
    args.Add("is_debug=false");
    args.Add("is_clang=false");
    args.Add("enable_precompiled_headers=false");
    args.Add("angle_enable_null=false");
    args.Add("angle_enable_wgpu=false");
    args.Add("angle_enable_gl_desktop_backend=false");
    args.Add("angle_enable_vulkan=false");
    args.Add($"extra_cflags=[ {string.Join(", ", cflags)} ]");
    args.Add($"extra_ldflags=[ '/guard:cf', '/LIBPATH:{GetSpectreLibPath(arch)}' ]");

    return string.Join(" ", args);
}

// gn gen + ninja + copy the dll/pdb into the output directory.
// `outName` is the folder under ANGLE's out/ directory, e.g. "winuwp".
void BuildAngle(
    DirectoryPath anglePath,
    DirectoryPath outputPath,
    string outName,
    string arch,
    string target,
    string gnArgs,
    bool verifyDependencies)
{
    var outDir = $"out/{outName}/{arch}";

    try
    {
        // We are not Googlers, so use the local Visual Studio toolchain.
        System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

        RunGn(anglePath, outDir, gnArgs);
        RunNinja(anglePath, outDir, target);
    }
    finally
    {
        System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
    }

    var destDir = outputPath.Combine(arch);
    EnsureDirectoryExists(destDir);
    CopyFileToDirectory(anglePath.CombineWithFilePath($"{outDir}/{target}.dll"), destDir);
    CopyFileToDirectory(anglePath.CombineWithFilePath($"{outDir}/{target}.pdb"), destDir);

    if (verifyDependencies)
        CheckWindowsDependencies($"{destDir}/{target}.dll", excluded: VERIFY_EXCLUDED);
}
