using System.Linq;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));
DirectoryPath PATCHES_PATH = MakeAbsolute(Directory("./patches"));

string ANGLE_VERSION = GetVersion("ANGLE", "release");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

// ---------------------------------------------------------------------------
// patches
// ---------------------------------------------------------------------------

// Apply the *.patch files from ./patches to a checkout, in filename order.
// Idempotent: patches already present in the tree are skipped, so this is safe
// on an incremental agent where the ANGLE clone is reused.

void ApplyPatches(DirectoryPath root, DirectoryPath patchesPath)
{
    foreach (var patch in GetFiles($"{patchesPath}/*.patch").OrderBy(p => p.FullPath))
    {
        var args = $"apply --ignore-whitespace \"{patch.FullPath}\"";

        // Reverse-check succeeds only if the tree already contains exactly
        // this patch — cheap, exact "is it applied?" test.
        var applied = StartProcess("git", new ProcessSettings {
            WorkingDirectory = root,
            Arguments = $"{args} --reverse --check",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) == 0;

        if (applied) {
            Information($"{patch.GetFilename()} is already applied, skipping.");
            continue;
        }

        Information($"Applying {patch.GetFilename()}...");

        if (StartProcess("git", new ProcessSettings { WorkingDirectory = root, Arguments = args }) != 0)
            throw new Exception(
                $"Failed to apply {patch.GetFilename()} in {root}. Either ANGLE moved and the " +
                $"patch needs refreshing, or the file was modified locally — " +
                $"'git checkout -- .' in {root} and re-run to rule the latter out.");
    }
}

// ---------------------------------------------------------------------------
// tasks
// ---------------------------------------------------------------------------

Task("sync-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    // NOTE: unlike the WinUI (winui-angle) build, the UWP build does NOT use
    // the Windows App SDK. Modern .NET UWP apps use the in-box
    // Windows.UI.Xaml stack, so no WinAppSDK download or header generation is
    // needed here (angle_is_winappsdk stays false).
    SyncAngle(ANGLE_PATH, ANGLE_VERSION);

    ApplyPatches(ANGLE_PATH, PATCHES_PATH);
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
                }),
            verifyDependencies: false);
    }
});

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);