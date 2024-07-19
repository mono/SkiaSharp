DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

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

        RunMSBuild("SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.sln",
            restore: false,
            targets: new[] { "Restore" },
            properties: new Dictionary<string, string> {
                { "RestorePackagesConfig", "true" }
            });
        RunMSBuild("SkiaSharp.Views.WinUI.Native/SkiaSharp.Views.WinUI.Native.sln", arch);

        var name = "SkiaSharp.Views.WinUI.Native";

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.dll", outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.pdb", outDir);
        CopyFileToDirectory($"{name}/{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.winmd", outDir);

        var anyOutDir = OUTPUT_PATH.Combine("any");
        EnsureDirectoryExists(anyOutDir);
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net7.0-windows10.0.19041.0/{name}.Projection.dll", anyOutDir);
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net7.0-windows10.0.19041.0/{name}.Projection.pdb", anyOutDir);
    }
});

Task("Default")
    .IsDependentOn("SkiaSharp.Views.WinUI.Native");

RunTarget(TARGET);
