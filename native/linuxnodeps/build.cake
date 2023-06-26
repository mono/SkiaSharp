DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../../scripts/cake/shared.cake"

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "gnArgs", "skia_use_fontconfig=false " + ADDITIONAL_GN_ARGS },
        { "verifyExcluded", "fontconfig" },
    });
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "gnArgs", ADDITIONAL_GN_ARGS },
        { "verifyExcluded", "fontconfig" },
    });
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
