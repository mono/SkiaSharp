DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));

#load "./scripts/infra/shared/shared.cake"

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
    .IsDependentOn ("nuget-normal")
    .IsDependentOn ("nuget-special");

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
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "update-docs"));

Task ("docs-download-output")
    .Description ("Download CI build output for docs.")
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "docs-download-output"));

Task ("docs-api-diff")
    .Description ("Generate API diffs.")
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "docs-api-diff"));

Task ("docs-api-diff-past")
    .Description ("Generate historical API diffs.")
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "docs-api-diff-past"));

Task ("docs-update-frameworks")
    .Description ("Update doc frameworks.")
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "docs-update-frameworks"));

Task ("docs-format-docs")
    .Description ("Format doc XML files.")
    .Does (() => RunCake ("./scripts/infra/docs/docs.cake", "docs-format-docs"));

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
    .Description ("Generate, prepare, and run all sample projects.")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "Default"));

Task ("samples-generate")
    .Description ("Generate sample project files.")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "samples-generate"));

Task ("samples-prepare")
    .Description ("Prepare samples for building (copy NuGet packages, etc.).")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "samples-prepare"));

Task ("samples-run")
    .Description ("Build and run the generated samples.")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "samples-run"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

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
