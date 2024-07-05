DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

#load "ANGLE.cake"

Task("ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    var ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
    var WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");

    var branch = GetVersion("ANGLE", "release");

    InitializeAngle(branch, ANGLE_PATH, WINAPPSDK_PATH);

    Build("x86");
    Build("x64");
    Build("arm64");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        try {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "0");

            RunGn(ANGLE_PATH, $"out/winui/{arch}", 
                $"target_cpu='{arch}' " +
                $"is_component_build=false " +
                $"is_debug=false " +
                $"is_clang=false " +
                $"angle_is_winappsdk=true " +
                $"winappsdk_dir='{WINAPPSDK_PATH}' " +
                $"enable_precompiled_headers=false " +
                $"angle_enable_null=false " +
                $"angle_enable_wgpu=false " +
                $"angle_enable_gl_desktop_backend=false " +
                $"angle_enable_vulkan=false");

            RunNinja(ANGLE_PATH, $"out/winui/{arch}", "libEGL libGLESv2");
        } finally {
            System.Environment.SetEnvironmentVariable("DEPOT_TOOLS_WIN_TOOLCHAIN", "");
        }

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libEGL.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libEGL.pdb"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libGLESv2.dll"), outDir);
        CopyFileToDirectory(ANGLE_PATH.CombineWithFilePath($"out/winui/{arch}/libGLESv2.pdb"), outDir);
    }
});

Task("SkiaSharp.Views.WinUI.Native")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("x86", "Win32");
    Build("x64", "x64");
    Build("arm64", "arm64");

    void Build(string arch, string nativeArch)
    {
        if (Skip(arch)) return;

        RunProcess("nuget", "restore SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.sln");
        RunMSBuild("SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.sln", arch);

        var name = "SkiaSharp.Views.WinUI.Native";

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.dll", outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.pdb", outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.winmd", outDir);

        var anyOutDir = OUTPUT_PATH.Combine("any");
        EnsureDirectoryExists(anyOutDir);
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net8.0-windows10.0.19041.0/{name}.Projection.dll", anyOutDir);
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net8.0-windows10.0.19041.0/{name}.Projection.pdb", anyOutDir);
    }
});

Task("Default")
    .IsDependentOn("ANGLE")
    .IsDependentOn("SkiaSharp.Views.WinUI.Native");

RunTarget(TARGET);
