DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/shared.cake"

var BUILD_VARIANT = Argument("variant", EnvironmentVariable("BUILD_VARIANT") ?? "linuxnodeps");
var BUILD_ARCH = Argument("arch", Argument("buildarch", EnvironmentVariable("BUILD_ARCH") ?? ""))
    .ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
var ADDITIONAL_GN_ARGS = Argument("gnArgs", EnvironmentVariable("ADDITIONAL_GN_ARGS"));

OUTPUT_PATH = OUTPUT_PATH.Combine(BUILD_VARIANT);

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var arch in BUILD_ARCH) {
        RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
            { "variant", BUILD_VARIANT },
            { "arch", arch },
            { "gnArgs", "skia_use_fontconfig=false " + ADDITIONAL_GN_ARGS },
        });

        RunProcess("readelf", $"-d {OUTPUT_PATH.CombineWithFilePath($"{arch}/libSkiaSharp.so")}", out var stdout);

        if (stdout.Any(o => o.Contains("fontconfig")))
            throw new Exception("libSkiaSharp.so contained a dependency on fontconfig.");
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var arch in BUILD_ARCH) {
        RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
            { "variant", BUILD_VARIANT },
            { "arch", arch },
        });

        RunProcess("readelf", $"-d {OUTPUT_PATH.CombineWithFilePath($"{arch}/libHarfBuzzSharp.so")}", out var stdout);

        if (stdout.Any(o => o.Contains("fontconfig")))
            throw new Exception("libHarfBuzzSharp.so contained a dependency on fontconfig.");
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
