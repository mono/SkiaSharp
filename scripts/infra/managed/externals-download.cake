#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

using System.Xml.Linq;
using NuGet.Packaging;
using NuGet.Versioning;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "utils-managed.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD — download pre-built native binaries from CI feed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Does(async () =>
{
    EnsureDirectoryExists ("./output");
    CleanDirectories ("./output");

    await DownloadPackageAsync("_nativeassets", "./output/native");
});

RunTarget(TARGET);
