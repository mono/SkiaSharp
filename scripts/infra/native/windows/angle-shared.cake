using System.Collections.Generic;

// ANGLE is declared in Skia's Windows-specific dependencies.
GIT_SYNC_DEPS_OS = "win";

void PrepareAngle(DirectoryPath anglePath)
{
    var submodules = new[] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/vulkan-deps",
        "third_party/astc-encoder/src",
        "tools/clang",
    };

    RunProcess("git", new ProcessSettings {
        Arguments = $"-c core.longpaths=true submodule update --init --recursive --depth 1 --single-branch -- {string.Join(" ", submodules)}",
        WorkingDirectory = anglePath.FullPath,
    });

    PatchAngleToolchainOutputNames(anglePath);
    WriteAngleGclientArgs(anglePath);
    WriteAngleLastChange(anglePath);
    DownloadAngleRc(anglePath);
    DownloadAngleClang(anglePath);
}

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

void WriteAngleGclientArgs(DirectoryPath anglePath)
{
    var gclientArgs = anglePath.CombineWithFilePath("build/config/gclient_args.gni");
    if (FileExists(gclientArgs))
        return;

    var lines = new[] {
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
