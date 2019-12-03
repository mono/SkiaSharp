////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

var externalsTask = Task("externals-native");

foreach(var cake in GetFiles("native/*/build.cake"))
{
    var native = cake.GetDirectory().GetDirectoryName();
    var should = ShouldBuildExternal(native);
    var localCake = cake;

    var task = Task($"externals-{native}")
        .WithCriteria(should)
        .Does(() => RunCake(localCake, "Default"));

    externalsTask.IsDependentOn(task);
}

Task("externals-osx")
    .IsDependentOn("externals-macos");

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("externals-download")
    .IsDependentOn("determine-last-successful-build")
    .Does(() =>
{
    var artifactName = "native";
    var artifactFilename = $"{artifactName}.zip";
    var url = string.Format(AZURE_BUILD_URL, AZURE_BUILD_ID, artifactName);

    var outputPath = "./output";
    EnsureDirectoryExists(outputPath);
    CleanDirectories(outputPath);

    DownloadFile(url, $"{outputPath}/{artifactFilename}");
    Unzip($"{outputPath}/{artifactFilename}", outputPath);
    MoveDirectory($"{outputPath}/{artifactName}", $"{outputPath}/native");
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
    CleanDirectories("native-builds/libSkiaSharp_ios/build");
    CleanDirectories("native-builds/libHarfBuzzSharp_ios/build");
    // tvos
    CleanDirectories("native-builds/libSkiaSharp_tvos/build");
    CleanDirectories("native-builds/libHarfBuzzSharp_tvos/build");
    // watchos
    CleanDirectories("native-builds/libSkiaSharp_watchos/build");
    CleanDirectories("native-builds/libHarfBuzzSharp_watchos/build");
    // osx
    CleanDirectories("native-builds/libSkiaSharp_osx/build");
    CleanDirectories("native-builds/libHarfBuzzSharp_osx/build");
});

bool ShouldBuildExternal(string platform)
{
    platform = platform?.ToLower() ?? "";

    if (SKIP_EXTERNALS.Contains("all") || SKIP_EXTERNALS.Contains("true"))
        return false;

    switch (platform) {
        case "mac":
        case "macos":
            platform = "osx";
            break;
        case "win":
            platform = "windows";
            break;
    }

    if (SKIP_EXTERNALS.Contains(platform))
        return false;

    return true;
}
