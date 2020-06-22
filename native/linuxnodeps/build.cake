DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/shared.cake"

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT") ?? "linuxnodeps");
OUTPUT_PATH = OUTPUT_PATH.Combine(BUILD_VARIANT);

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", BUILD_VARIANT },
        { "gnArgs", "skia_use_fontconfig=false" },
    });

    var suffix = GetVersion("libSkiaSharp", "suffix");
    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libSkiaSharp{suffix}.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("libSkiaSharp.so contained a dependency on fontconfig.");
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "variant", BUILD_VARIANT },
    });

    RunProcess("ldd", OUTPUT_PATH.CombineWithFilePath($"x64/libHarfBuzzSharp.so").FullPath, out var stdout);

    if (stdout.Any(o => o.Contains("fontconfig")))
        throw new Exception("libHarfBuzzSharp.so contained a dependency on fontconfig.");
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
