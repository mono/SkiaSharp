DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/skia/third_party/externals/angle2");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

Task("prepare-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    PrepareAngle(ANGLE_PATH);
});

Task("ANGLE")
    .IsDependentOn("prepare-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x86", "x64", "arm64" })
    {
        Build(arch, "libEGL");
        Build(arch, "libGLESv2");
    }

    void Build(string arch, string target)
    {
        if (Skip(arch)) return;

        BuildAngle(
            anglePath: ANGLE_PATH,
            outputPath: null,
            outName: "winuwp-validation",
            arch: arch,
            target: target,
            gnArgs: AngleGnArgs(arch, new[] {
                "target_os='winuwp'",
                "angle_is_winappsdk=false",
            }),
            verifyDependencies: false);
    }
});

Task("Default")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
