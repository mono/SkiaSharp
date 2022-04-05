DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/shared.cake"

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    RunCake("../windows/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", "nanoserver" },
        { "gnArgs", "extra_cflags+=[ '-DSK_BUILD_FOR_NANOSERVER' ]" },
        { "arch", "x64" },
    });

    DotNetTool("nano-api-scan " + OUTPUT_PATH.CombineWithFilePath("nanoserver/x64/libSkiaSharp.dll").FullPath);
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    RunCake("../windows/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "arch", "x64" },
    });

    var outDir = OUTPUT_PATH.Combine($"nanoserver/x64");
    EnsureDirectoryExists(outDir);
    var srcDir = OUTPUT_PATH.Combine($"windows/x64");
    CopyFileToDirectory(srcDir.CombineWithFilePath("libHarfBuzzSharp.dll"), outDir);
    CopyFileToDirectory(srcDir.CombineWithFilePath("libHarfBuzzSharp.pdb"), outDir);

    DotNetTool("nano-api-scan " + OUTPUT_PATH.CombineWithFilePath("nanoserver/x64/libHarfBuzzSharp.dll").FullPath);
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
