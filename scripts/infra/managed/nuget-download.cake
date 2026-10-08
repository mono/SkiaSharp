#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

using NuGet.Packaging;
using NuGet.Versioning;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/download.cake"

Task("Default")
    .Description("Download one promoted package family without changing native or test outputs.")
    .Does(async () =>
{
    CleanDir(OUTPUT_NUGETS_PATH);
    await DownloadPackageAsync("_nugets", OUTPUT_NUGETS_PATH);
});

RunTarget(TARGET);
