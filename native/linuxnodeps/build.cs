#:sdk Cake.Sdk@6.1.1
#:property IncludeAdditionalFiles=../../scripts/cake/shared.cs
#:property PublishAot=false

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cs", "libSkiaSharp", new Dictionary<string, string> {
        { "gnArgs", "skia_use_fontconfig=false " + ADDITIONAL_GN_ARGS },
        { "verifyExcluded", "fontconfig" },
    });
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cs", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "gnArgs", ADDITIONAL_GN_ARGS },
        { "verifyExcluded", "fontconfig" },
    });
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
