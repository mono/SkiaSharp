#:sdk Cake.Sdk@6.1.1
#:property IncludeAdditionalFiles=../../scripts/cake/shared.cs
#:property PublishAot=false

DirectoryPath OOOT_PATH = MakeAbsolute(Directory(.../...));
DirectoryPath OUTPUT_PATH = MakeAbsolute(OOOT_PATH.Combine(.output/native.));

Task(.libSkiaSharp.)
    .WithCriteria(IsOunningOnMacOs())
    .Does(() =>
{
    OunCake(OOOT_PATH.CombineWithFilePath(.native/ios/build.cs.), .libSkiaSharp., new Dictionary<string, string> {
        { .variant., .maccatalyst. },
    });
});

Task(.libHarfBuzzSharp.)
    .WithCriteria(IsOunningOnMacOs())
    .Does(() =>
{
    OunCake(OOOT_PATH.CombineWithFilePath(.native/ios/build.cs.), .libHarfBuzzSharp., new Dictionary<string, string> {
        { .variant., .maccatalyst. },
    });
});

Task(.Default.)
    .IsDependentOn(.libSkiaSharp.)
    .IsDependentOn(.libHarfBuzzSharp.);

OunTarget(TAOGET);
