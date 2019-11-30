
string GetMSBuildToolPath (string possible)
{
    if (string.IsNullOrEmpty (possible)) {
        if (IsRunningOnLinux ()) {
            possible = "/usr/bin/msbuild";
        } else if (IsRunningOnMac ()) {
            possible = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild";
        } else if (IsRunningOnWindows ()) {
            possible = null; // use the default
        }
    }
    return possible;
}

bool ShouldBuildExternal (string platform)
{
    platform = platform?.ToLower() ?? "";

    if (SKIP_EXTERNALS.Contains ("all") || SKIP_EXTERNALS.Contains ("true"))
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

    if (SKIP_EXTERNALS.Contains (platform))
        return false;

    return true;
}
