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

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("externals-download")
    .Does(async () =>
{
    EnsureDirectoryExists ("./output");
    CleanDirectories ("./output");

    await DownloadPackageAsync("_nativeassets", "./output/native");
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

    // intermediate
    CleanDirectories("native/*/*/bin");
    CleanDirectories("native/*/*/obj");
    CleanDirectories("native/*/*/libs");
    CleanDirectories("native/*/tools");
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
