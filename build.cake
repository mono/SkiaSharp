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
#load "./scripts/infra/managed/cake/externals.cake"
#load "./scripts/infra/managed/cake/UpdateDocs.cake"


Task ("__________________________________")
    .Description ("__________________________________________________");

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .Description ("Build all external dependencies.")
    .IsDependentOn ("externals-native");

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
