DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// CONTAINER TESTS — run ONLY the console test suite (SkiaSharp.Tests.Console)
//
// This target is invoked INSIDE a container by the bootstrapper's `docker:` feature
// (docker run <image> ... dotnet cake --target=tests-container). It does NOT build any native
// externals — the prebuilt libSkiaSharp is provided in the mounted repo's output/native (populated
// in CI from the `native` artifact; locally by externals-download).
//
// It runs a single assembly (the cross-platform console suite) rather than the full tests-netcore
// set, so the container image stays minimal (no GTK4 / Vulkan / Direct3D dependencies).
//
// --nativePlatform selects which output/native/<platform>/<arch> build to run against. It is passed
// through to the native-asset targets as the SkiaSharp/HarfBuzzSharp NativePlatform MSBuild property,
// so the build copies that platform's library. This is how the Windows container runs against the
// nanoserver build (output/native/nanoserver/x64/libSkiaSharp.dll) and Alpine against the musl build,
// which the OS-derived defaults don't map.
//
//   dotnet cake --target=tests-container --nativePlatform=linux
//   dotnet cake --target=tests-container --nativePlatform=alpine
//   dotnet cake --target=tests-container --nativePlatform=nanoserver
////////////////////////////////////////////////////////////////////////////////////////////////////

// Which output/native/<platform>/<arch> build to run against. The <arch> is derived from the host
// (OSArchitecture) by the native-asset targets, so only the platform is specified here.
var NATIVE_PLATFORM = Argument("nativePlatform", "");

Task ("Default")
    .Description ("Run the .NET console test suite (for containerized runs).")
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    var tfm = "net10.0";
    var testAssembly = "SkiaSharp.Tests.Console";
    var csproj = $"{ROOT_PATH}/tests/{testAssembly}/{testAssembly}.csproj";

    var props = new Dictionary<string, string> {
        // TargetFrameworks (plural) collapses the multi-targeted binding projects to net10.0 only,
        // so the Android/iOS/etc. workloads are never restored/built — keeping the image lean.
        { "TargetFramework", tfm },
        { "TargetFrameworks", tfm },
        // The Windows-only sn.exe re-sign step isn't present in the Nano Server SDK image (and isn't
        // needed to run tests — PublicSign already gives the correct identity for InternalsVisibleTo).
        { "DisableStrongNameSigning", "true" },
        // mdoc.exe doesn't run on Nano Server (missing Windows DLL deps) and doc generation is
        // irrelevant to running the test suite, so skip it in every container build.
        { "SkipMDocGenerateDocs", "true" },
    };

    // Select the native build: the native-asset targets copy this platform's library.
    if (!string.IsNullOrEmpty(NATIVE_PLATFORM)) {
        props["SkiaSharpNativePlatform"] = NATIVE_PLATFORM;
        props["HarfBuzzSharpNativePlatform"] = NATIVE_PLATFORM;
    }

    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj, properties: props);
    }

    var results = $"{ROOT_PATH}/output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}/{tfm}";
    RunDotNetTest (csproj, results, properties: props);
});

RunTarget(TARGET);
