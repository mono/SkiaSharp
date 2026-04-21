#:sdk Cake.Sdk
#:property IncludeAdditionalFiles=../../scripts/cake/shared.cs
#:property PublishAot=false

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

Task("libSkiaSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    RunCake(ROOT_PATH.CombineWithFilePath("native/ios/build.cs"), "libSkiaSharp", new Dictionary<string, string> {
        { "variant", "maccatalyst" },
    });
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    RunCake(ROOT_PATH.CombineWithFilePath("native/ios/build.cs"), "libHarfBuzzSharp", new Dictionary<string, string> {
        { "variant", "maccatalyst" },
    });
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
