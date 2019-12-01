void RunMSBuild(
    FilePath solution,
    string platform = "Any CPU",
    string platformTarget = null,
    bool restore = true,
    bool restoreOnly = false)
{
    var nugets = MakeAbsolute(ROOT_PATH.Combine("output/nugets"));
    var packages = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));

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

        c.Properties ["RestorePackagesPath"] = new [] { packages.FullPath };
        c.Properties ["RestoreSources"] = new [] { nugets.FullPath, "https://api.nuget.org/v3/index.json" };
    });
}
