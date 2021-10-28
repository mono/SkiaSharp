DirectoryPath PACKAGE_CACHE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));
DirectoryPath OUTPUT_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets"));
DirectoryPath OUTPUT_SPECIAL_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets-special"));
DirectoryPath OUTPUT_SYMBOLS_NUGETS_PATH = MakeAbsolute(ROOT_PATH.Combine("output/nugets-symbols"));

var NUGETS_SOURCES = new [] {
    OUTPUT_NUGETS_PATH.FullPath,
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/azure-public/vside/_packaging/xamarin-impl/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-aspnetcore-7c57ecbd-3/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-runtime-4822e3c3-5/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-windowsdesktop-59fea7da-4/nuget/v3/index.json",
    "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-emsdk-1ec2e17f-4/nuget/v3/index.json"
};

void RunNuGetRestorePackagesConfig(FilePath sln)
{
    var dir = sln.GetDirectory();

    EnsureDirectoryExists(OUTPUT_NUGETS_PATH);

    var settings = new NuGetRestoreSettings {
        Source = NUGETS_SOURCES,
        NoCache = true,
        PackagesDirectory = dir.Combine("packages"),
    };

    foreach (var config in GetFiles(dir + "/**/packages.config"))
        NuGetRestore(config, settings);
}

void RunMSBuild(
    FilePath solution,
    string platform = "Any CPU",
    string platformTarget = null,
    bool restore = true,
    bool bl = true,
    string[] targets = null,
    string configuration = null,
    Dictionary<string, string> properties = null)
{
    EnsureDirectoryExists(OUTPUT_NUGETS_PATH);

    MSBuild(solution, c => {
        c.Configuration = configuration ?? CONFIGURATION;
        c.Verbosity = VERBOSITY;
        c.MaxCpuCount = 0;

        var relativeSolution = MakeAbsolute(ROOT_PATH).GetRelativePath(MakeAbsolute(solution));
        var blPath = ROOT_PATH.Combine("output/logs/binlogs").CombineWithFilePath(relativeSolution + ".binlog");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = blPath.FullPath,
        };

        if (!string.IsNullOrEmpty(MSBUILD_EXE)) {
            c.ToolPath = MSBUILD_EXE;
        } else if (IsRunningOnWindows() && !string.IsNullOrEmpty(VS_INSTALL)) {
            c.ToolPath = ((DirectoryPath)VS_INSTALL).CombineWithFilePath("MSBuild/Current/Bin/MSBuild.exe");
        }

        c.NoLogo = VERBOSITY == Verbosity.Minimal;
        c.Restore = restore;

        if (targets?.Length > 0) {
            c.Targets.Clear();
            foreach (var target in targets) {
                c.Targets.Add(target);
            }
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

        if (properties != null) {
            foreach (var prop in properties) {
                c.Properties [prop.Key] = new [] { prop.Value };
            }
        }
        // c.Properties ["RestoreSources"] = NUGETS_SOURCES;
        var sep = IsRunningOnWindows() ? ";" : "%3B";
        c.ArgumentCustomization = args => args.Append($"/p:RestoreSources=\"{string.Join(sep, NUGETS_SOURCES)}\"");
    });
}
