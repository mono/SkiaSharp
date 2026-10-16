DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));
string ANGLE_VERSION = GetVersion("ANGLE", "release");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

Task("sync-ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    SyncAngle(ANGLE_PATH, ANGLE_VERSION);
    SyncWindowsAppSdk();
});

Task("ANGLE")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    foreach (var arch in new[] { "x86", "x64", "arm64" })
    {
        Build(arch, "libEGL", wasdk: false);
        Build(arch, "libGLESv2", wasdk: true);
    }

    void Build(string arch, string target, bool wasdk)
    {
        if (Skip(arch)) return;

        BuildAngle(
            anglePath: ANGLE_PATH,
            outputPath: OUTPUT_PATH,
            outName: wasdk ? "winui_wasdk" : "winui",
            arch: arch,
            target: target,
            gnArgs: AngleGnArgs(arch, new [] {
                $"angle_is_winappsdk={(wasdk ? "true" : "false")}",
                $"winappsdk_dir='{WINAPPSDK_PATH}'",
            }),
            verifyDependencies: true);
    }
});

// Generate the Windows App SDK headers from the winmd files shipped in the
// NuGet package. Unlike the UWP build, WinUI 3 needs these to compile against
// Microsoft.UI.*.
void SyncWindowsAppSdk()
{
    if (FileExists(WINAPPSDK_PATH.Combine("include").CombineWithFilePath("Microsoft.UI.Dispatching.h")))
        return;

    var winappsdkVersion = GetVersion("Microsoft.WindowsAppSDK", "release");
    var stamp = WINAPPSDK_PATH.CombineWithFilePath($"{winappsdkVersion}.stamp");

    // Download and extract the NuGet package using .NET HTTP (works on
    // restricted agents where Python's urllib is blocked by firewall policy).
    if (!FileExists(stamp)) {
        var nugetUrl = $"https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/microsoft.windowsappsdk/{winappsdkVersion}/microsoft.windowsappsdk.{winappsdkVersion}.nupkg";
        var nupkgPath = WINAPPSDK_PATH.CombineWithFilePath($"{winappsdkVersion}.nupkg");
        EnsureDirectoryExists(WINAPPSDK_PATH);
        DownloadFile(nugetUrl, nupkgPath);
        Unzip(nupkgPath, WINAPPSDK_PATH);
        DeleteFile(nupkgPath);
        System.IO.File.WriteAllText(stamp.FullPath, "");
    }

    // Run the header generation script under vcvarsall.bat so midlrt can find cl.exe.
    var vcvarsall = ROOT_PATH.CombineWithFilePath("scripts/infra/native/windows/vcvarsall.bat");
    var generateScript = MakeAbsolute(File("generate_winappsdk_headers.ps1"));
    RunProcess(vcvarsall, $"\"{VS_INSTALL}\" \"x64\" pwsh -NoProfile -ExecutionPolicy Bypass -File \"{generateScript}\" -Path \"{WINAPPSDK_PATH}\"");
}

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
