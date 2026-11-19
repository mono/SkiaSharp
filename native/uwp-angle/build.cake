DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));

string ANGLE_VERSION = GetVersion("ANGLE", "release");
string ANGLE_REVISION = GetVersion("ANGLE", "revision");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

// ---------------------------------------------------------------------------
// tasks
// ---------------------------------------------------------------------------

Task("sync-ANGLE")
    .IsDependentOn("validate-windows-toolchain")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // NOTE: unlike the WinUI (winui-angle) build, the UWP build does NOT use
    // the Windows App SDK. Modern .NET UWP apps use the in-box
    // Windows.UI.Xaml stack, so no WinAppSDK download or header generation is
    // needed here (angle_is_winappsdk stays false).
    SyncAngle(ANGLE_PATH, ANGLE_VERSION, ANGLE_REVISION);
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x64", "x86", "arm64" })
    {
        Build(arch, "libEGL");
        Build(arch, "libGLESv2");
    }

    void Build(string arch, string target)
    {
        if (Skip(arch)) return;

        BuildAngle(
            anglePath: ANGLE_PATH,
            outputPath: OUTPUT_PATH,
            outName: "winuwp",
            arch: arch,
            target: target,
            gnArgs: AngleGnArgs(arch,
                new [] {
                    "target_os='winuwp'",
                    "angle_is_winappsdk=false",
                }));
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);