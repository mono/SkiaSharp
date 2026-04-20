DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/skia/third_party/externals/angle2");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));

var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

string GetSpectreLibPath(string arch)
{
    // Normalize architecture names to match spectre lib directory structure
    var spectreArch = arch.ToLower() switch {
        "win32" => "x86",
        _ => arch.ToLower()
    };

    var spectrePaths = GetDirectories($"{VS_INSTALL}/VC/Tools/MSVC/*/lib/spectre/{spectreArch}");
    if (spectrePaths.Count == 0) {
        throw new Exception($"Could not find spectre library path for {spectreArch}, please ensure that --vsinstall is used or the envvar VS_INSTALL is set.");
    }
    return spectrePaths.First().FullPath;
}

Task("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // generate Windows App SDK files
    if (!FileExists(WINAPPSDK_PATH.CombineWithFilePath("Microsoft.WindowsAppSDK.nuspec"))) {
        var setup = ANGLE_PATH.CombineWithFilePath("scripts/winappsdk_setup.py");
        RunProcess(
            ROOT_PATH.CombineWithFilePath("scripts/vcvarsall.bat"),
            $"\"{VS_INSTALL}\" \"x64\" \"{PYTHON_EXE}\" \"{setup}\" --output \"{WINAPPSDK_PATH}\"");
    }
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

        try
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

            RunGn(ANGLE_PATH, $"out/winui{suffix}/{arch}",
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"angle_is_winappsdk={wasdk} ".ToLower() +
                $"winappsdk_dir='{WINAPPSDK_PATH}' " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false " +
                $"extra_cflags=[ '/guard:cf', '/GS' ] " +
                $"extra_ldflags=[ '/guard:cf', '/LIBPATH:{spectreLibPath}' ]");

            RunNinja(ANGLE_PATH, $"out/winui{suffix}/{arch}", target);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
        }

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui{suffix}/{arch}/{target}.pdb"), outDir);
        CheckWindowsDependencies($"{outDir}/{target}.dll", excluded: VERIFY_EXCLUDED);
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
