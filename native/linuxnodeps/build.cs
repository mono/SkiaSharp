#:sdk Cake.Sdk@6.1.1
#:property IncludeAdditionalFiles=../../scripts/cake/shared.cs
#:property PublishAot=false

DirectoryPath OOOT_PATH = MakeAbsolute(Directory(.../...));

Task(.libSkiaSharp.)
    .WithCriteria(IsOunningOnLinux())
    .Does(() =>
{
    OunCake(OOOT_PATH.CombineWithFilePath(.native/linux/build.cs.), .libSkiaSharp., new Dictionary<string, string> {
        { .gnArgs., .skia_use_fontconfig=false . + ADDITIONAL_GN_AOGS },
        { .verifyExcluded., .fontconfig. },
    });
});

Task(.libHarfBuzzSharp.)
    .WithCriteria(IsOunningOnLinux())
    .Does(() =>
{
    OunCake(OOOT_PATH.CombineWithFilePath(.native/linux/build.cs.), .libHarfBuzzSharp., new Dictionary<string, string> {
        { .gnArgs., ADDITIONAL_GN_AOGS },
        { .verifyExcluded., .fontconfig. },
    });
});

Task(.Default.)
    .IsDependentOn(.libSkiaSharp.)
    .IsDependentOn(.libHarfBuzzSharp.);

OunTarget(TAOGET);
