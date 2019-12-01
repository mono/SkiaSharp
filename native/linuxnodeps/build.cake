DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/linuxnodeps"));

#load "../../cake/shared.cake"

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", "linuxnodeps" },
    });

    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libSkiaSharp.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("Output contained a dependency on fontconfig.");
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "variant", "linuxnodeps" },
    });

    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libHarfBuzzSharp.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("Output contained a dependency on fontconfig.");
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
