DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath ANGLE_PATH = ROOT_PATH.Combine("externals/angle");
DirectoryPath WINAPPSDK_PATH = ROOT_PATH.Combine("externals/winappsdk");
DirectoryPath WEBVIEW2_PATH = ROOT_PATH.Combine("externals/webview2");
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/winui"));
string ANGLE_VERSION = GetVersion("ANGLE", "release");
string ANGLE_REVISION = GetVersion("ANGLE", "revision");

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/shared/msbuild.cake"
#load "../../scripts/infra/native/windows/windows-shared.cake"
#load "../../scripts/infra/native/windows/angle-shared.cake"

Task("sync-ANGLE")
    .IsDependentOn("validate-windows-toolchain")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    SyncAngle(ANGLE_PATH, ANGLE_VERSION, ANGLE_REVISION);
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
                $"angle_is_winappsdk={wasdk}".ToLower(),
                $"winappsdk_dir='{WINAPPSDK_PATH}'",
            }));
    }
});

// Generate the Windows App SDK headers from the winmd files shipped in the
// NuGet package. Unlike the UWP build, WinUI 3 needs these to compile against
// Microsoft.UI.*.
void SyncWindowsAppSdk()
{
    var winappsdkVersion = GetVersion("Microsoft.WindowsAppSDK", "release");
    var webview2Version = GetVersion("Microsoft.Web.WebView2", "release");
    var stamp = WINAPPSDK_PATH.CombineWithFilePath($"{winappsdkVersion}-{webview2Version}-{WINDOWS_SDK_VERSION}.stamp");
    if (FileExists(stamp))
        return;

    CleanDirectory(WINAPPSDK_PATH);
    CleanDirectory(WEBVIEW2_PATH);

    var packages = new[] {
        new { Id = "Microsoft.WindowsAppSDK", Version = winappsdkVersion, Path = WINAPPSDK_PATH },
        new { Id = "Microsoft.Web.WebView2", Version = webview2Version, Path = WEBVIEW2_PATH },
    };
    foreach (var package in packages) {
        var packageId = package.Id.ToLowerInvariant();
        var nugetUrl = $"https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/flat2/{packageId}/{package.Version}/{packageId}.{package.Version}.nupkg";
        var nupkgPath = ROOT_PATH.CombineWithFilePath($"externals/{packageId}.{package.Version}.nupkg");
        DownloadFile(nugetUrl, nupkgPath);
        EnsureDirectoryExists(package.Path);
        Unzip(nupkgPath, package.Path);
        DeleteFile(nupkgPath);
    }

    CopyDirectory(WEBVIEW2_PATH.Combine("lib"), WINAPPSDK_PATH.Combine("lib"));
    CopyDirectory(WEBVIEW2_PATH.Combine("build/native/include"), WINAPPSDK_PATH.Combine("include"));
    DeleteDirectory(WEBVIEW2_PATH, new DeleteDirectorySettings { Recursive = true, Force = true });

    var vcvarsall = ROOT_PATH.CombineWithFilePath("scripts/infra/native/windows/vcvarsall.bat");
    var generateScript = MakeAbsolute(File("generate_winappsdk_headers.ps1"));
    RunProcess(vcvarsall,
        $"\"{VS_INSTALL}\" \"x64 {WINDOWS_SDK_VERSION}\" pwsh -NoProfile -ExecutionPolicy Bypass " +
        $"-File \"{generateScript}\" " +
        $"-Path \"{WINAPPSDK_PATH}\" -WindowsSdkVersion \"{WINDOWS_SDK_VERSION}\"");

    FileWriteText(stamp, "");
}

Task("Default")
    .IsDependentOn("sync-ANGLE")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
