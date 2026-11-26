DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"

Task("SkiaSharp.Views.UWP.Native")
    .IsDependentOn("validate-windows-toolchain")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("x64", "x64");
    Build("x86", "Win32");
    Build("arm64", "arm64");

    void Build(string arch, string nativeArch)
    {
        if (Skip(arch)) return;

        RunMSBuild("SkiaSharp.Views.UWP.Native.slnx",
            arch,
            restore: false,
            targets: new[] { "Restore" },
            properties: new Dictionary<string, string> {
                { "RestorePackagesConfig", "true" },
                { "VCToolsVersion", TOOLSET_VERSION.Value }
            });
        RunMSBuild("SkiaSharp.Views.UWP.Native.slnx", arch,
            properties: new Dictionary<string, string> {
                { "VCToolsVersion", TOOLSET_VERSION.Value }
            });

        var name = "SkiaSharp.Views.UWP.Native";

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.dll", outDir);
        CopyFileToDirectory($"{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.pdb", outDir);
        
        var anyOutDir = OUTPUT_PATH.Combine("any");
        EnsureDirectoryExists(anyOutDir);

        CopyFileToDirectory($"{name}.Projection/bin/{CONFIGURATION}/net10.0-windows10.0.26100.0/{name}.Projection.dll", anyOutDir);
        CopyFileToDirectory($"{name}.Projection/bin/{CONFIGURATION}/net10.0-windows10.0.26100.0/{name}.Projection.pdb", anyOutDir);
    }
});

Task("Default")
    .IsDependentOn("SkiaSharp.Views.UWP.Native");

RunTarget(TARGET);
