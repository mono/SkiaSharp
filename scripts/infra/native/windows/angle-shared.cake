using System.Collections.Generic;

// ANGLE is declared in Skia's Windows-specific dependencies.
GIT_SYNC_DEPS_OS = "win";

void PrepareAngle(DirectoryPath anglePath)
{
    RestoreAngleToolchainOutputNames(anglePath);

    var submodules = new[] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/vulkan-deps",
        "third_party/astc-encoder/src",
        "third_party/depot_tools",
        "third_party/spirv-headers/src",
        "third_party/spirv-tools/src",
        "third_party/vulkan-headers/src",
        "tools/clang",
    };

    RunProcess("git", new ProcessSettings {
        Arguments = $"-c core.longpaths=true submodule update --init --recursive --depth 1 --single-branch -- {string.Join(" ", submodules)}",
        WorkingDirectory = anglePath.FullPath,
    });

    DownloadAngleGn(anglePath);
    GN_EXE = anglePath.CombineWithFilePath("out/tools/gn/gn.exe").FullPath;

    PatchAngleToolchainOutputNames(anglePath);
    WriteAngleGclientArgs(anglePath);
    WriteAngleLastChange(anglePath);
    DownloadAngleRc(anglePath);
    DownloadAngleClang(anglePath);
}

void DownloadAngleGn(DirectoryPath anglePath)
{
    var deps = anglePath.CombineWithFilePath("DEPS");
    var gnVersion = GetRegexValue(
        @"'buildtools/win':\s*\{[\s\S]*?'package':\s*'gn/gn/windows-amd64'[\s\S]*?'version':\s*'([^']+)'",
        deps);
    if (string.IsNullOrEmpty(gnVersion))
        throw new Exception($"Could not find the Windows GN version in {deps}.");

    var gnPath = anglePath.CombineWithFilePath("out/tools/gn/gn.exe");
    if (FileExists(gnPath)) {
        RunProcess(gnPath, "--version", out var versionOutput);
        var installedRevision = string.Join("", versionOutput)
            .Split('(', ')')
            .FirstOrDefault(value => value.Length >= 7 && value.All(Uri.IsHexDigit));
        var requiredRevision = gnVersion.Replace("git_revision:", "");
        if (!string.IsNullOrEmpty(installedRevision) && requiredRevision.StartsWith(installedRevision))
            return;
    }

    var cipd = anglePath.CombineWithFilePath("third_party/depot_tools/cipd.bat");
    var gnDirectory = gnPath.GetDirectory();
    EnsureDirectoryExists(gnDirectory);

    var command =
        $"call \"{cipd.FullPath}\" install gn/gn/windows-amd64 {gnVersion} -root \"{gnDirectory.FullPath}\"";
    RunProcess("cmd.exe", new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("/d")
            .Append("/s")
            .Append("/c")
            .AppendQuoted(command),
        WorkingDirectory = anglePath.FullPath,
    });
}

void RestoreAngleToolchainOutputNames(DirectoryPath anglePath)
{
    var toolchain = anglePath.CombineWithFilePath("build/toolchain/win/toolchain.gni");
    if (!FileExists(toolchain))
        return;

    var buildPath = anglePath.Combine("build");
    var result = StartProcess("git", new ProcessSettings {
        Arguments = "show HEAD:toolchain/win/toolchain.gni",
        WorkingDirectory = buildPath.FullPath,
        RedirectStandardOutput = true,
    }, out var originalLines);
    if (result != 0)
        throw new Exception($"Failed to inspect {toolchain}.");
    var restoreLib = originalLines.Any(line => line.Trim() == "\"${dllname}.lib\",");
    var restorePdb = originalLines.Any(line => line.Trim() == "\"${dllname}.pdb\",");
    if (!restoreLib && !restorePdb)
        return;

    var contents = System.IO.File.ReadAllText(toolchain.FullPath);
    var newContents = contents;
    if (restoreLib)
        newContents = newContents.Replace("        \"{{output_dir}}/{{target_output_name}}.lib\",", "        \"${dllname}.lib\",");
    if (restorePdb)
        newContents = newContents.Replace("        \"{{output_dir}}/{{target_output_name}}.pdb\",", "        \"${dllname}.pdb\",");

    if (contents != newContents)
        System.IO.File.WriteAllText(toolchain.FullPath, newContents);
}

void PatchAngleToolchainOutputNames(DirectoryPath anglePath)
{
    var toolchain = anglePath.CombineWithFilePath("build/toolchain/win/toolchain.gni");
    var contents = System.IO.File.ReadAllText(toolchain.FullPath);
    var newContents = contents
        .Replace("        \"${dllname}.lib\",", "        \"{{output_dir}}/{{target_output_name}}.lib\",")
        .Replace("        \"${dllname}.pdb\",", "        \"{{output_dir}}/{{target_output_name}}.pdb\",");

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
    RunPython(anglePath, anglePath.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
}

void DownloadAngleRc(DirectoryPath anglePath)
{
    const string rcExe = "build/toolchain/win/rc/win/rc.exe";

    var rcPath = anglePath.CombineWithFilePath(rcExe);
    var shaPath = anglePath.CombineWithFilePath($"{rcExe}.sha1");
    var sha = System.IO.File.ReadAllText(shaPath.FullPath).Trim();
    if (FileExists(rcPath)) {
        using (var stream = System.IO.File.OpenRead(rcPath.FullPath))
        using (var algorithm = System.Security.Cryptography.SHA1.Create()) {
            var actualSha = string.Concat(algorithm.ComputeHash(stream).Select(value => value.ToString("x2")));
            if (string.Equals(actualSha, sha, StringComparison.OrdinalIgnoreCase))
                return;
        }
    }

    var url = $"https://storage.googleapis.com/download/storage/v1/b/chromium-browser-clang/o/rc%2F{sha}?alt=media";
    DownloadFile(url, rcPath);
}

void DownloadAngleClang(DirectoryPath anglePath)
{
    RunPython(anglePath, anglePath.CombineWithFilePath("tools/clang/scripts/update.py"));
}

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
    args.Add("enable_precompiled_headers=false");
    args.Add("angle_enable_null=false");
    args.Add("angle_enable_wgpu=false");
    args.Add("angle_enable_gl_desktop_backend=false");
    args.Add("angle_enable_vulkan=false");

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

    var builtDll = anglePath.CombineWithFilePath($"{outDir}/{target}.dll");
    var verificationDll = builtDll;

    if (outputPath != null) {
        var destDir = outputPath.Combine(arch);
        var builtPdb = anglePath.CombineWithFilePath($"{outDir}/{target}.pdb");
        if (!FileExists(builtPdb))
            builtPdb = anglePath.CombineWithFilePath($"{outDir}/{target}.dll.pdb");

        EnsureDirectoryExists(destDir);
        CopyFileToDirectory(builtDll, destDir);
        CopyFile(builtPdb, destDir.CombineWithFilePath($"{target}.pdb"));
        verificationDll = destDir.CombineWithFilePath($"{target}.dll");
    }

    if (verifyDependencies)
        CheckWindowsDependencies(verificationDll, excluded: VERIFY_EXCLUDED);
}
