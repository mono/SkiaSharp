#addin nuget:?package=Cake.Xamarin&version=3.1.0
#addin nuget:?package=Cake.XCode&version=5.0.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.Json&version=6.0.1
#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=SharpCompress&version=0.32.2
#addin nuget:?package=Mono.Cecil&version=0.11.5
#addin nuget:?package=Mono.ApiTools.ApiInfo&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiff&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiffFormatted&version=1.4.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

#tool nuget:?package=mdoc&version=5.8.9
#tool nuget:?package=vswhere&version=2.8.4

using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SharpCompress.Common;
using SharpCompress.Readers;
using Mono.ApiTools;
using NuGet.Packaging;
using NuGet.Versioning;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));

#load "./scripts/infra/shared/shared.cake"
#load "./scripts/infra/native/shared/native-shared.cake"

#load "./scripts/infra/native/windows/msbuild.cake"
#load "./scripts/infra/managed/cake/UtilsManaged.cake"
#load "./scripts/infra/managed/cake/UpdateDocs.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

var externalsTask = Task("externals-native");

foreach (var cake in GetFiles("native/*/build.cake"))
{
    var native = cake.GetDirectory().GetDirectoryName();
    var should = ShouldBuildExternal(native);
    var localCake = cake;

    var task = Task($"externals-{native}")
        .WithCriteria(should)
        .WithCriteria(!SKIP_BUILD)
        .Does(() => RunCake(localCake, "Default"));

    externalsTask.IsDependentOn(task);
}

Task("externals-osx")
    .IsDependentOn("externals-macos");

Task("externals-nano")
    .IsDependentOn("externals-nanoserver");

Task("externals-catalyst")
    .IsDependentOn("externals-maccatalyst");

Task ("externals")
    .Description ("Build all external dependencies.")
    .IsDependentOn ("externals-native");

Task ("externals-download")
    .Description ("Download pre-built native binaries from CI.")
    .Does (() => RunCake ("./scripts/infra/managed/externals-download.cake", "Default"));

Task ("externals-interop")
    .Description ("Re-generate the interop files.")
    .Does (() => RunCake ("./scripts/infra/managed/interop.cake", "Default"));

bool ShouldBuildExternal(string platform)
{
    platform = platform?.ToLower() ?? "";

    if (SKIP_EXTERNALS.Contains("all") || SKIP_EXTERNALS.Contains("true"))
        return false;

    switch (platform) {
        case "mac":
        case "osx":
            platform = "macos";
            break;
        case "catalyst":
            platform = "maccatalyst";
            break;
        case "win":
            platform = "windows";
            break;
        case "nano":
            platform = "nanoserver";
            break;
    }

    if (SKIP_EXTERNALS.Contains(platform))
        return false;

    return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - build managed assemblies (isolated via RunCake)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .Description ("Build all managed assemblies.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/managed/libs.cake", "Default"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - run test suites (isolated via RunCake)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .Description ("Run all tests.")
    .IsDependentOn ("tests-netfx")
    .IsDependentOn ("tests-netcore")
    .IsDependentOn ("tests-android")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

Task ("tests-netfx")
    .Description ("Run all Full .NET Framework tests.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/tests/tests-netfx.cake", "Default"));

Task ("tests-netcore")
    .Description ("Run all .NET Core tests.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/tests/tests-netcore.cake", "Default"));

Task ("tests-android")
    .Description ("Run all Android tests.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/tests/tests-android.cake", "Default"));

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/tests/tests-apple.cake", "tests-ios"));

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .IsDependentOn ("externals")
    .Does (() => RunCake ("./scripts/infra/tests/tests-apple.cake", "tests-maccatalyst"));

Task ("tests-wasm")
    .Description ("Run WASM tests.")
    .IsDependentOn ("externals-wasm")
    .Does (() => RunCake ("./scripts/infra/tests/tests-wasm.cake", "Default"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - pack NuGet packages (isolated via RunCake)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .Description ("Pack all NuGets.")
    .IsDependentOn ("libs")
    .Does (() => RunCake ("./scripts/infra/package/nuget.cake", "nuget"));

Task ("nuget-normal")
    .Description ("Pack all NuGets (build all required dependencies).")
    .IsDependentOn ("libs")
    .Does (() => RunCake ("./scripts/infra/package/nuget.cake", "nuget-normal"));

Task ("nuget-special")
    .Description ("Pack all special NuGets.")
    .IsDependentOn ("libs")
    .Does (() => RunCake ("./scripts/infra/package/nuget.cake", "nuget-special"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// DOCS - creating the xml, markdown and other documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("update-docs")
    .Description ("Regenerate all docs.")
    .IsDependentOn ("docs-api-diff")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .Description ("Clean up.")
    .IsDependentOn ("clean-externals")
    .IsDependentOn ("clean-managed");

Task ("clean-externals")
    .Description ("Clean native build outputs.")
    .Does (() =>
{
    CleanDirectories("externals/skia/out");
    CleanDirectories("externals/skia/xcodebuild");
    CleanDirectories("externals/angle");
    CleanDirectories("output/native");
    CleanDirectories("native/*/*/bin");
    CleanDirectories("native/*/*/obj");
    CleanDirectories("native/*/*/libs");
    CleanDirectories("native/*/tools");
});

Task ("clean-managed")
    .Description ("Clean up (managed only).")
    .Does (() =>
{
    CleanDirectories ("./binding/*/bin");
    CleanDirectories ("./binding/*/obj");
    DeleteFiles ("./binding/*/project.lock.json");

    CleanDirectories ("./samples/*/*/bin");
    CleanDirectories ("./samples/*/*/obj");
    CleanDirectories ("./samples/*/*/AppPackages");
    CleanDirectories ("./samples/*/*/*/bin");
    CleanDirectories ("./samples/*/*/*/obj");
    DeleteFiles ("./samples/*/*/*/project.lock.json");
    CleanDirectories ("./samples/*/*/*/AppPackages");
    CleanDirectories ("./samples/*/*/packages");

    CleanDirectories ("./tests/**/bin");
    CleanDirectories ("./tests/**/obj");
    CleanDirectories ("./tests/**/artifacts");
    DeleteFiles ("./tests/**/project.lock.json");

    CleanDirectories ("./source/*/*/bin");
    CleanDirectories ("./source/*/*/obj");
    DeleteFiles ("./source/*/*/project.lock.json");
    CleanDirectories ("./source/*/*/Generated Files");
    CleanDirectories ("./source/packages");

    DeleteDir ("./output");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - build sample projects (isolated via RunCake)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .Description ("Build all sample projects.")
    .IsDependentOn ("libs")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "Default"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("----------------------------------")
    .Description ("--------------------------------------------------");

Task ("Default")
    .Description ("Build all managed assemblies and external dependencies.")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs");

Task ("Everything")
    .Description ("Build, pack and test everything.")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW
////////////////////////////////////////////////////////////////////////////////////////////////////

RunTarget (TARGET);
