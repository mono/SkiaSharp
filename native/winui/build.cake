DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

void CheckDeps(FilePath so)
{
    Information($"Making sure that there are no dependencies on: {string.Join(", ", VERIFY_EXCLUDED)}");

    RunProcess("dumpbin", $"/dependents {so}", out var stdoutEnum);
    var stdout = stdoutEnum.ToArray();

    var needed = MatchRegex(@"\(NEEDED\).+\[(.+)\]", stdout).ToList();

    Information("Dependencies:");
    foreach (var need in needed) {
        Information($"    {need}");
    }

    foreach (var exclude in VERIFY_EXCLUDED) {
        if (needed.Any(o => o.Contains(exclude.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new Exception($"{so} contained a dependency on {exclude}.");
    }

    var glibcs = MatchRegex(@"GLIBC_([\w\.\d]+)", stdout).Distinct().ToList();
    glibcs.Sort();

    Information("GLIBC:");
    foreach (var glibc in glibcs) {
        Information($"    {glibc}");
    }
    
    if (VERIFY_GLIBC_MAX != null) {
        foreach (var glibc in glibcs) {
            var version = System.Version.Parse(glibc);
            if (version > VERIFY_GLIBC_MAX)
                throw new Exception($"{so} contained a dependency on GLIBC {glibc} which is greater than the expected GLIBC {VERIFY_GLIBC_MAX}.");
        }
    }
}

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
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net6.0-windows10.0.19041.0/{name}.Projection.dll", anyOutDir);
        CopyFileToDirectory($"{name}/{name}.Projection/bin/{CONFIGURATION}/net6.0-windows10.0.19041.0/{name}.Projection.pdb", anyOutDir);

        CheckDeps($"{outDir}/{name}.dll");
    }
});

Task("Default")
    .IsDependentOn("SkiaSharp.Views.WinUI.Native");

RunTarget(TARGET);
