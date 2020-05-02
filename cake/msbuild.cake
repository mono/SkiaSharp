DirectoryPath PACKAGE_CACHE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));
DirectoryPath OUTPUT_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets"));

void RunMSBuild(
    FilePath solution,
    string platform = "Any CPU",
    string platformTarget = null,
    bool restore = true,
    bool restoreOnly = false,
    string bl = null)
{
    var nugetSources = new [] { OUTPUT_NUGETS_PATH.FullPath, "https://api.nuget.org/v3/index.json" };

    EnsureDirectoryExists(OUTPUT_NUGETS_PATH);

    MSBuild(solution, c => {
        c.Configuration = CONFIGURATION;
        c.Verbosity = VERBOSITY;
        c.MaxCpuCount = 0;

        if (!string.IsNullOrEmpty(bl)) {
            c.BinaryLogger = new MSBuildBinaryLogSettings {
                Enabled = true,
                FileName = bl,
            };
        }

        if (!string.IsNullOrEmpty(MSBUILD_EXE)) {
            c.ToolPath = MSBUILD_EXE;
        } else if (IsRunningOnWindows() && !string.IsNullOrEmpty(VS_INSTALL)) {
            c.ToolPath = ((DirectoryPath)VS_INSTALL).CombineWithFilePath("MSBuild/Current/Bin/MSBuild.exe");
        }

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
        var sep = IsRunningOnWindows() ? ";" : "%3B";
        c.ArgumentCustomization = args => args.Append($"/p:RestoreSources=\"{string.Join(sep, nugetSources)}\"");
    });
}
