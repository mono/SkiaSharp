
////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds the native C and C++ externals
Task("externals-native");
Task("externals-native-skip");

foreach (var platform in GetFiles()) {
    Task("externals-windows")
        .IsDependentOn("externals-init")
        .IsDependeeOf(ShouldBuildExternal("windows") ? "externals-native" : "externals-native-skip")
        .WithCriteria(ShouldBuildExternal("windows"))
        .WithCriteria(IsRunningOnWindows())
        .Does(() =>
    {
    });
}

// this builds the native C and C++ externals for Windows UWP
Task("externals-uwp")
    .IsDependentOn("externals-init")
    .IsDependentOn("externals-angle-uwp")
    .IsDependeeOf(ShouldBuildExternal("uwp") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("uwp"))
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
});

// this builds the native C and C++ externals for Mac OS X
Task("externals-macos")
    .IsDependentOn("externals-osx");
Task("externals-osx")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("osx") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("osx"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for iOS
Task("externals-ios")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("ios") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("ios"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for tvOS
Task("externals-tvos")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("tvos") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("tvos"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for watchOS
Task("externals-watchos")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("watchos") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("watchos"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for Android
Task("externals-android")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("android") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("android"))
    .WithCriteria(IsRunningOnMac() || IsRunningOnWindows())
    .Does(() =>
{
});

// this builds the native C and C++ externals for Linux
Task("externals-linux")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("linux") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("linux"))
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
});

Task("externals-tizen")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("tizen") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("tizen"))
    .Does(() =>
{
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("externals-download")
    .IsDependentOn("download-last-successful-build")
    .Does(() =>
{
    var artifactName = "native-default";
    var artifactFilename = $"{artifactName}.zip";
    var url = string.Format(AZURE_BUILD_URL, AZURE_BUILD_ID, artifactName);

    var outputPath = "./output";
    EnsureDirectoryExists(outputPath);
    CleanDirectories(outputPath);

    DownloadFile(url, $"{outputPath}/{artifactFilename}");
    Unzip($"{outputPath}/{artifactFilename}", outputPath);
    MoveDirectory($"{outputPath}/{artifactName}", $"{outputPath}/native");
});

Task("externals-angle-uwp")
    .WithCriteria(!FileExists(ANGLE_PATH.CombineWithFilePath("uwp/ANGLE.WindowsStore.nuspec")))
    .Does(() =>
{
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("clean-externals")
    .Does(() =>
{
    // skia
    CleanDirectories("externals/skia/out");
    CleanDirectories("externals/skia/xcodebuild");

    // angle
    CleanDirectories("externals/angle");

    // all
    CleanDirectories("output/native");
    // ios
    CleanDirectories("native/libSkiaSharp_ios/build");
    CleanDirectories("native/libHarfBuzzSharp_ios/build");
    // tvos
    CleanDirectories("native/libSkiaSharp_tvos/build");
    CleanDirectories("native/libHarfBuzzSharp_tvos/build");
    // watchos
    CleanDirectories("native/libSkiaSharp_watchos/build");
    CleanDirectories("native/libHarfBuzzSharp_watchos/build");
    // osx
    CleanDirectories("native/libSkiaSharp_osx/build");
    CleanDirectories("native/libHarfBuzzSharp_osx/build");
});
