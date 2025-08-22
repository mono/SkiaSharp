DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));

var VERIFY_EXCLUDED = new[] { "VCRUNTIME", "MSVCP" };

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/msbuild.cake"

void CheckDeps(FilePath dll)
{
    Information($"Making sure that there are no dependencies on: {string.Join(", ", VERIFY_EXCLUDED)}");

    var dumpbins = GetFiles($"{VS_INSTALL}/VC/Tools/MSVC/*/bin/Host*/*/dumpbin.exe");
    if (dumpbins.Count == 0) {
        throw new Exception("Could not find dumpbin.exe, please ensure that --vsinstall is used or the envvar VS_INSTALL is set.");
    }

    RunProcess(dumpbins.First(), $"/dependents {dll}", out var stdoutEnum);
    var stdout = stdoutEnum.ToArray();

    var needed = MatchRegex(@"\s\s+(\S+\.dll)", stdout).ToList();

    Information("Dependencies:");
    foreach (var need in needed) {
        Information($"    {need}");
    }

    foreach (var exclude in VERIFY_EXCLUDED) {
        if (needed.Any(o => o.Contains(exclude.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new Exception($"{dll} contained a dependency on {exclude}.");
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
        CheckDeps($"{anyOutDir}/{name}.Projection.dll");
    }
});

Task("Default")
    .IsDependentOn("SkiaSharp.Views.WinUI.Native");

RunTarget(TARGET);
