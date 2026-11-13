using System.Collections.Generic;

// Shared ANGLE checkout + build logic.
//
// Consumed by:
//   native/winui-angle/build.cake  (WinUI 3 / Windows App SDK)
//   native/uwp-angle/build.cake    (UWP, in-box Windows.UI.Xaml)
//
// NOTE: this file relies on helpers from native-shared.cake and
// windows-shared.cake (RunProcess, RunPython, RunGn, RunNinja,
// GetSpectreLibPath, LLVM_HOME, VS_INSTALL, TOOLSET_VERSION), so it must be
// #load-ed by a script that also loads those. It deliberately does not #load
// them itself so that the existing load order is left untouched.

// ---------------------------------------------------------------------------
// sync
// ---------------------------------------------------------------------------

// Everything needed to get an ANGLE checkout that can be `gn gen`-ed.
// Every step is idempotent, so this is safe to run on an incremental agent.
void SyncAngle(DirectoryPath anglePath, string angleVersion, string angleRevision)
{
    CheckoutAngle(anglePath, angleVersion, angleRevision);
    SyncAngleSubmodules(anglePath);
    LinkAngleClang(anglePath);
    PatchAngleToolchainOutputNames(anglePath);
    WriteAngleGclientArgs(anglePath);
    ApplyAngleSkiaSharpConfig(anglePath);
    WriteAngleLastChange(anglePath);
    UseWindowsSdkResourceCompiler(anglePath);
}

void CheckoutAngle(DirectoryPath anglePath, string angleVersion, string angleRevision)
{
    if (!DirectoryExists(anglePath)) {
        RunProcess("git", $"clone https://github.com/google/angle.git --branch {angleVersion} --depth 1 --single-branch --shallow-submodules {anglePath}");
    }

    RunProcess("git", new ProcessSettings {
        Arguments = $"fetch --depth 1 origin {angleRevision}",
        WorkingDirectory = anglePath.FullPath,
    });
    RunProcess("git", new ProcessSettings {
        Arguments = $"checkout --detach {angleRevision}",
        WorkingDirectory = anglePath.FullPath,
    });
}

void SyncAngleSubmodules(DirectoryPath anglePath)
{
    var submodules = new [] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/astc-encoder/src",
        "third_party/depot_tools",
        "third_party/spirv-headers/src",
        "third_party/spirv-tools/src",
    };

    RunProcess("git", new ProcessSettings {
        Arguments = $"-c core.longpaths=true submodule update --init --recursive --depth 1 --single-branch -- {string.Join(" ", submodules)}",
        WorkingDirectory = anglePath.FullPath,
    });
}

// Chromium's resource compiler wrapper expects clang-cl at this path even for
// MSVC builds.
void LinkAngleClang(DirectoryPath anglePath)
{
    var llvmHome = MakeAbsolute(Directory(LLVM_HOME));
    var clang = llvmHome.CombineWithFilePath("bin/clang-cl.exe");
    if (!FileExists(clang))
        throw new Exception($"Visual Studio LLVM was not installed: {clang}");

    var chromiumLlvm = anglePath.Combine("third_party/llvm-build/Release+Asserts");
    if (DirectoryExists(chromiumLlvm))
        return;

    EnsureDirectoryExists(chromiumLlvm.GetParent());
    RunProcess("cmd.exe", new ProcessSettings {
        Arguments = new ProcessArgumentBuilder().Append("/d").Append("/s").Append("/c")
            .AppendQuoted($"mklink /J \"{chromiumLlvm}\" \"{llvmHome}\""),
    });
}

// Use conventional import library and PDB names so the per-arch copies do not
// collide.
void PatchAngleToolchainOutputNames(DirectoryPath anglePath)
{
    var toolchain = anglePath.CombineWithFilePath("build/toolchain/win/toolchain.gni");
    var contents = FileReadText(toolchain);
    const string importLibrary = "\"{{output_dir}}/{{target_output_name}}.lib\"";
    const string programDatabase = "\"{{output_dir}}/{{target_output_name}}.pdb\"";
    var newContents = contents
        .Replace("\"${dllname}.lib\"", importLibrary)
        .Replace("\"${dllname}.pdb\"", programDatabase);

    if (!newContents.Contains(importLibrary) || !newContents.Contains(programDatabase))
        throw new Exception($"Cannot apply conventional import library and PDB names at {toolchain}.");

    if (contents != newContents)
        FileWriteText(toolchain, newContents);
}

// Normally written by gclient, which we do not run.
void WriteAngleGclientArgs(DirectoryPath anglePath)
{
    var gclientArgs = anglePath.CombineWithFilePath("build/config/gclient_args.gni");
    if (FileExists(gclientArgs))
        return;

    FileWriteLines(gclientArgs, new [] {
        "checkout_angle_internal = false",
        "checkout_angle_mesa = false",
        "checkout_angle_restricted_traces = false",
        "generate_location_tags = false",
    });
}

// Hardening options live in a real GN config so Ninja also tracks
// security-option changes.
void ApplyAngleSkiaSharpConfig(DirectoryPath anglePath)
{
    CopyDirectory(ROOT_PATH.Combine("scripts/infra/native/windows/angle-gn"), anglePath.Combine("build/config/skiasharp"));

    var buildConfig = anglePath.CombineWithFilePath("build/config/BUILDCONFIG.gn");
    var contents = FileReadText(buildConfig);
    const string config = "  \"//build/config/skiasharp:skiasharp\",";
    if (contents.Contains(config))
        return;

    const string anchor = "default_compiler_configs = [";
    if (!contents.Contains(anchor))
        throw new Exception($"Cannot apply the SkiaSharp config to ANGLE targets at {buildConfig}.");

    FileWriteText(buildConfig, contents.Replace(anchor, $"{anchor}\n{config}"));
}

void WriteAngleLastChange(DirectoryPath anglePath)
{
    var lastchange = anglePath.CombineWithFilePath("build/util/LASTCHANGE");
    var committime = anglePath.CombineWithFilePath("build/util/LASTCHANGE.committime");
    if (FileExists(lastchange) && FileExists(committime))
        return;

    RunPython(anglePath, anglePath.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
}

// Use the Windows SDK rc.exe instead of downloading Chromium's prebuilt one.
void UseWindowsSdkResourceCompiler(DirectoryPath anglePath)
{
    var rcPy = anglePath.CombineWithFilePath("build/toolchain/win/rc/rc.py");
    var contents = FileReadText(rcPy);
    const string msrcOutput = "msrc_out = flags.output";
    const string skipChromiumRc = "rc_exe_exit_code = 0";
    var newContents = contents
        .Replace("msrc_out = flags.output + '_ms_rc'", msrcOutput)
        .Replace("rc_exe_exit_code = RunRc(preprocessed_output, is_utf8, flags)", skipChromiumRc);

    if (newContents.Contains("flags.output + '_ms_rc'") ||
        !newContents.Contains(msrcOutput) ||
        !newContents.Contains(skipChromiumRc))
        throw new Exception($"Cannot configure Windows SDK rc.exe at {rcPy}.");

    if (contents != newContents)
        FileWriteText(rcPy, newContents);
}

// ---------------------------------------------------------------------------
// build
// ---------------------------------------------------------------------------

// GN args common to every ANGLE flavour we ship. `flavorArgs` carries the bits
// that differ (target_os, angle_is_winappsdk, winappsdk_dir, ...); hardening
// comes from the skiasharp GN config applied during the sync.
string AngleGnArgs(string arch, string[] flavorArgs = null)
{
    var args = new List<string> { $"target_cpu='{arch}'" };

    if (flavorArgs != null)
        args.AddRange(flavorArgs);

    args.Add("is_component_build=false");
    args.Add("is_debug=false");
    args.Add("is_clang=false");
    args.Add("use_custom_libcxx=false");
    args.Add("use_custom_libcxx_for_host=false");
    args.Add("enable_rust=false");
    args.Add("enable_rust_cxx=false");
    args.Add("use_siso=false");
    args.Add("angle_build_tests=false");
    args.Add("enable_precompiled_headers=false");
    args.Add("angle_enable_null=false");
    args.Add("angle_enable_wgpu=false");
    args.Add("angle_enable_gl_desktop_backend=false");
    args.Add("angle_enable_vulkan=false");
    args.Add("angle_build_vulkan_system_info=false");
    args.Add($"spectre_lib_root='{System.IO.Path.GetDirectoryName(GetSpectreLibPath(arch))}'");

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
    string gnArgs)
{
    var outDir = $"out/{outName}/{arch}";

    var previousDepotToolsWinToolchain = EnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN");

    try
    {
        // We are not Googlers, so use the local Visual Studio toolchain.
        System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

        RunGn(anglePath, outDir, gnArgs, new Dictionary<string, string> {
            { "GYP_MSVS_OVERRIDE_PATH", VS_INSTALL },
            { "vs2026_install", VS_INSTALL },
            { "VCToolsVersion", TOOLSET_VERSION.Value },
        });
        RunNinja(anglePath, outDir, target);
    }
    finally
    {
        System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", previousDepotToolsWinToolchain);
    }

    var destDir = outputPath.Combine(arch);
    EnsureDirectoryExists(destDir);
    CopyFileToDirectory(anglePath.CombineWithFilePath($"{outDir}/{target}.dll"), destDir);
    CopyFileToDirectory(anglePath.CombineWithFilePath($"{outDir}/{target}.pdb"), destDir);
}
