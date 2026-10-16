DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"

void CheckUwpDependencies(FilePath dll)
{
    // Any CRT dependency must be the _APP (Store-approved) variant.
    var crtPrefixes = new[] { "VCRUNTIME", "MSVCP", "MSVCR", "CONCRT", "VCCORLIB" };

    Information($"Making sure that all CRT dependencies are _APP variants in {dll.GetFilename()}");

    var dumpbins = GetFiles($"{VS_INSTALL}/VC/Tools/MSVC/*/bin/Host*/*/dumpbin.exe");
    if (dumpbins.Count == 0)
        throw new Exception("Could not find dumpbin.exe, please ensure that --vsinstall is used or the envvar VS_INSTALL is set.");

    RunProcess(dumpbins.First(), $"/dependents {dll}", out var stdoutEnum);

    var needed = new List<string>();
    var inList = false;
    foreach (var line in stdoutEnum.ToArray()) {
        if (line.Contains("has the following dependencies:")) {
            inList = true;
        } else if (line.Contains("Summary")) {
            inList = false;
        } else if (inList) {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"\s\s+(\S+\.dll)");
            if (match.Success)
                needed.Add(match.Groups[1].Value);
        }
    }

    Information("Dependencies:");
    foreach (var need in needed)
        Information($"    {need}");

    foreach (var need in needed) {
        var isCrt = crtPrefixes.Any(p => need.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (!isCrt)
            continue;

        var name = System.IO.Path.GetFileNameWithoutExtension(need);
        if (!name.EndsWith("_APP", StringComparison.OrdinalIgnoreCase))
            throw new Exception($"{dll} contained a desktop CRT dependency on {need}, expected the _APP variant.");
    }
}
Task("SkiaSharp.Views.UWP.Native")
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
                { "RestorePackagesConfig", "true" }
            });
        RunMSBuild("SkiaSharp.Views.UWP.Native.slnx", arch);

        var name = "SkiaSharp.Views.UWP.Native";

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.dll", outDir);
        CopyFileToDirectory($"{name}/bin/{nativeArch}/{CONFIGURATION}/{name}.pdb", outDir);
        
        var anyOutDir = OUTPUT_PATH.Combine("any");
        EnsureDirectoryExists(anyOutDir);

        CopyFileToDirectory($"{name}.Projection/bin/{CONFIGURATION}/net10.0-windows10.0.26100.0/{name}.Projection.dll", anyOutDir);
        CopyFileToDirectory($"{name}.Projection/bin/{CONFIGURATION}/net10.0-windows10.0.26100.0/{name}.Projection.pdb", anyOutDir);

        CheckUwpDependencies($"{outDir}/{name}.dll");
        CheckUwpDependencies($"{anyOutDir}/{name}.Projection.dll");
    }
});

Task("Default")
    .IsDependentOn("SkiaSharp.Views.UWP.Native");

RunTarget(TARGET);
