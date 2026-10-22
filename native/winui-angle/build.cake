DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));
string ANGLE_VERSION = GetVersion("ANGLE", "release");
string ANGLE_REVISION = GetVersion("ANGLE", "revision");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"

Task("sync-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // sync ANGLE
    if (!DirectoryExists(ANGLE_PATH)) {
        RunProcess("git", $"clone https://github.com/google/angle.git --branch {ANGLE_VERSION} --depth 1 --single-branch --shallow-submodules {ANGLE_PATH}");
    }
    RunProcess("git", new ProcessSettings {
        Arguments = $"fetch --depth 1 origin {ANGLE_REVISION}",
        WorkingDirectory = ANGLE_PATH.FullPath,
    });
    RunProcess("git", new ProcessSettings {
        Arguments = $"checkout --detach {ANGLE_REVISION}",
        WorkingDirectory = ANGLE_PATH.FullPath,
    });

    // sync submodules
    var submodules = new[] {
        "build",
        "testing",
        "third_party/zlib",
        "third_party/jsoncpp",
        "third_party/vulkan-deps",
        "third_party/astc-encoder/src",
        "third_party/spirv-headers/src",
        "third_party/spirv-tools/src",
        "third_party/vulkan-headers/src",
        "tools/clang",
    };
    RunProcess("git", new ProcessSettings {
        Arguments = $"-c core.longpaths=true submodule update --init --recursive --depth 1 --single-branch -- {string.Join(" ", submodules)}",
        WorkingDirectory = ANGLE_PATH.FullPath,
    });

    // Set the linker PDB path, not just the copied filename: the DLL embeds it.
    {
        var toolchain = ANGLE_PATH.CombineWithFilePath("build/toolchain/win/toolchain.gni");
        var contents = FileReadText(toolchain);
        const string original = "pdbname = \"${dllname}.pdb\"";
        const string replacement = "pdbname = \"{{output_dir}}/{{target_output_name}}.pdb\"";
        var originalCount = System.Text.RegularExpressions.Regex.Matches(contents,
            System.Text.RegularExpressions.Regex.Escape(original)).Count;
        var replacementCount = System.Text.RegularExpressions.Regex.Matches(contents,
            System.Text.RegularExpressions.Regex.Escape(replacement)).Count;
        if (originalCount + replacementCount != 2)
            throw new Exception($"Unexpected ANGLE DLL PDB definitions at {toolchain}.");
        if (originalCount > 0)
            FileWriteText(toolchain, contents.Replace(original, replacement));
    }

    FileWriteLines(ANGLE_PATH.CombineWithFilePath("build/config/gclient_args.gni"), new[] {
        "checkout_angle_internal = false",
        "checkout_angle_mesa = false",
        "checkout_angle_restricted_traces = false",
        "generate_location_tags = false",
    });

    // Apply a real GN config so Ninja also tracks security-option changes.
    {
        CopyDirectory(ROOT_PATH.Combine("native/winui-angle/gn"), ANGLE_PATH.Combine("build/config/skiasharp"));

        var buildConfig = ANGLE_PATH.CombineWithFilePath("build/config/BUILDCONFIG.gn");
        var contents = FileReadText(buildConfig);
        const string config = "  \"//build/config/skiasharp:skiasharp\",";
        if (!contents.Contains(config)) {
            const string anchor = "default_compiler_configs = [";
            if (!contents.Contains(anchor))
                throw new Exception($"Cannot apply the SkiaSharp config to ANGLE targets at {buildConfig}.");
            FileWriteText(buildConfig, contents.Replace(anchor, $"{anchor}\n{config}"));
        }
    }

    // set version numbers
    {
        var lastchange = ANGLE_PATH.CombineWithFilePath("build/util/LASTCHANGE");
        RunPython(ANGLE_PATH, ANGLE_PATH.CombineWithFilePath("build/util/lastchange.py"), $"-o {lastchange}");
    }

    // download rc.exe
    var rc_exe = "build/toolchain/win/rc/win/rc.exe";
    var rcPath = ANGLE_PATH.CombineWithFilePath(rc_exe);
    {
        var sha = FileReadText(ANGLE_PATH.CombineWithFilePath($"{rc_exe}.sha1")).Trim();
        string actualSha = "";
        if (FileExists(rcPath)) {
            using var stream = System.IO.File.OpenRead(rcPath.FullPath);
            using var algorithm = System.Security.Cryptography.SHA1.Create();
            actualSha = string.Concat(algorithm.ComputeHash(stream).Select(value => value.ToString("x2")));
        }
        var url = $"https://storage.googleapis.com/download/storage/v1/b/chromium-browser-clang/o/rc%2F{sha}?alt=media";
        if (actualSha != sha) {
            DownloadFile(url, rcPath);
            using var stream = System.IO.File.OpenRead(rcPath.FullPath);
            using var algorithm = System.Security.Cryptography.SHA1.Create();
            actualSha = string.Concat(algorithm.ComputeHash(stream).Select(value => value.ToString("x2")));
        }
        if (actualSha != sha)
            throw new Exception($"Chromium resource compiler hash mismatch: expected {sha}, found {actualSha}.");
    }

    var winappsdkVersion = GetVersion("Microsoft.WindowsAppSDK", "release");
    var nugetUrl = $"https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/microsoft.windowsappsdk/{winappsdkVersion}/microsoft.windowsappsdk.{winappsdkVersion}.nupkg";
    var nupkgPath = ROOT_PATH.CombineWithFilePath($"externals/microsoft.windowsappsdk.{winappsdkVersion}.nupkg");
    CleanDirectory(WINAPPSDK_PATH);
    DownloadFile(nugetUrl, nupkgPath);
    Unzip(nupkgPath, WINAPPSDK_PATH);
    DeleteFile(nupkgPath);

    var vcvarsall = ROOT_PATH.CombineWithFilePath("scripts/infra/native/windows/vcvarsall.bat");
    var generateScript = MakeAbsolute(File("generate_winappsdk_headers.ps1"));
    RunProcess(vcvarsall,
        $"\"{VS_INSTALL}\" \"x64 {WINDOWS_SDK_VERSION}\" pwsh -NoProfile -ExecutionPolicy Bypass " +
        $"-File \"{generateScript}\" " +
        $"-Path \"{WINAPPSDK_PATH}\" -WindowsSdkVersion \"{WINDOWS_SDK_VERSION}\"");
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x86", "x64", "arm64" })
    {
        Build(arch, "libEGL", wasdk: false);
        Build(arch, "libGLESv2", wasdk: true);
    }

    void Build(string arch, string target, bool wasdk)
    {
        if (Skip(arch)) return;

        var suffix = wasdk ? "_wasdk" : "";
        var spectreLibPath = GetSpectreLibPath(arch);
        var previousDepotToolsWinToolchain = EnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN");

        try
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

            RunGn(ANGLE_PATH, $"out/winui{suffix}/{arch}",
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"use_custom_libcxx=false " +
                $"use_custom_libcxx_for_host=false " +
                $"enable_rust=false " +
                $"enable_rust_cxx=false " +
                $"use_siso=false " +
                $"angle_build_tests=false " +
                $"angle_is_winappsdk={wasdk} ".ToLower() +
                $"winappsdk_dir='{WINAPPSDK_PATH}' " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false " +
                $"spectre_lib_root='{System.IO.Path.GetDirectoryName(spectreLibPath)}'",
                new Dictionary<string, string> {
                    { "GYP_MSVS_OVERRIDE_PATH", VS_INSTALL },
                    { "vs2026_install", VS_INSTALL },
                    { "VCToolsVersion", TOOLSET_VERSION.Value },
                });

            RunNinja(ANGLE_PATH, $"out/winui{suffix}/{arch}", target);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", previousDepotToolsWinToolchain);
        }

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.pdb"), outDir);
        var dumpbin = ((DirectoryPath)VS_INSTALL).CombineWithFilePath(
            $"VC/Tools/MSVC/{TOOLSET_VERSION.Value}/bin/Hostx64/x64/dumpbin.exe");
        RunProcess(dumpbin, $"/headers \"{outDir}/{target}.dll\"", out var headers);
        var pdbReference = System.Text.RegularExpressions.Regex.Match(string.Join("\n", headers),
            @"Format:\s*RSDS,.*,\s*([^\r\n]+\.pdb)");
        if (!pdbReference.Success || System.IO.Path.GetFileName(pdbReference.Groups[1].Value.Trim()) != $"{target}.pdb")
            throw new Exception($"ANGLE {target}.dll does not reference its packaged {target}.pdb.");
        Information($"Verified embedded PDB reference: {pdbReference.Groups[1].Value.Trim()}");
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
