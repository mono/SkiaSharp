DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/shared.cake"

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    RunCake("../win32/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", "nanoserver" },
        { "gn", "extra_cflags+=[ '-DSK_BUILD_FOR_NANOSERVER' ]" },
        { "arch", "x64" },
    });
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    RunCake("../win32/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "arch", "x64" },
    });

    var outDir = OUTPUT_PATH.Combine($"nanoserver/x64");
    EnsureDirectoryExists(outDir);
    var srcDir = OUTPUT_PATH.Combine($"windows/x64");
    CopyFileToDirectory(srcDir.CombineWithFilePath("libHarfBuzzSharp.dll"), outDir);
    CopyFileToDirectory(srcDir.CombineWithFilePath("libHarfBuzzSharp.pdb"), outDir);
});

Task("Build")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp")
    .Does(() =>
{
    var outDir = OUTPUT_PATH.Combine($"nanoserver/x64");
    foreach (var dll in GetFiles($"{outDir}/*.dll")) {
        RunProcess("nano-api-scan", dll.FullPath);
    }
});

RunTarget(TARGET);
