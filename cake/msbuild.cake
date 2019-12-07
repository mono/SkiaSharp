DirectoryPath PACKAGE_CACHE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));

void RunMSBuild(
    FilePath solution,
    string platform = "Any CPU",
    string platformTarget = null,
    bool restore = true,
    bool restoreOnly = false)
{
    var nugets = MakeAbsolute(ROOT_PATH.Combine("output/nugets"));
    var nugetSources = new [] { nugets.FullPath, "https://api.nuget.org/v3/index.json" };

    EnsureDirectoryExists(nugets);

    MSBuild(solution, c => {
        c.Configuration = CONFIGURATION;
        c.Verbosity = VERBOSITY;
        c.ToolVersion = MSBuildToolVersion.VS2017;

        c.NoLogo = VERBOSITY == Verbosity.Minimal;

        if (restoreOnly) {
            c.Targets.Clear();
            c.Targets.Add("Restore");
        } else {
            c.Restore = restore;
        }

        if (!string.IsNullOrEmpty(platformTarget)) {
            platform = null;
            c.PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), platformTarget);
        } else {
            c.PlatformTarget = PlatformTarget.MSIL;
            c.MSBuildPlatform = MSBuildPlatform.x86;
        }

        if (!string.IsNullOrEmpty(platform)) {
            c.Properties ["Platform"] = new [] { $"\"{platform}\"" };
        }

        c.Properties ["RestoreNoCache"] = new [] { "true" };
        c.Properties ["RestorePackagesPath"] = new [] { PACKAGE_CACHE_PATH.FullPath };
        // c.Properties ["RestoreSources"] = nugetSources;
        var sep = IsRunningOnWindows () ? ";" : "%3B";
        c.ArgumentCustomization = args => args.Append ($"/p:RestoreSources=\"{string.Join (sep, nugetSources)}\"");
    });
}
