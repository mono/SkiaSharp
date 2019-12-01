DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));

#load "../../cake/shared.cake"

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libSkiaSharp", new Dictionary<string, string> {
        { "variant", "linuxnodeps" },
    });
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    RunCake("../linux/build.cake", "libHarfBuzzSharp", new Dictionary<string, string> {
        { "variant", "linuxnodeps" },
    });
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
