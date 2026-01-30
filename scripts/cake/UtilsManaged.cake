var skipTestOnPlatform = "";
if (IsRunningOnWindows ()) {
    skipTestOnPlatform = "Windows";
} else if (IsRunningOnMacOs ()) {
    skipTestOnPlatform = "macOS";
} else if (IsRunningOnLinux ()) {
    skipTestOnPlatform = "Linux";
} else {
    throw new Exception ("This script is not running on a known platform.");
}

var UNSUPPORTED_TESTS = new Dictionary<string, string>
{
    { "FailingOn", skipTestOnPlatform },
    { "SkipOn", skipTestOnPlatform },
};

void RunTests(FilePath testAssembly, DirectoryPath output, bool is32)
{
    var dir = testAssembly.GetDirectory();
    var settings = new XUnit2Settings {
        ReportName = "TestResults",
        XmlReport = true,
        UseX86 = is32,
        NoAppDomain = true,
        Parallelism = ParallelismOption.All,
        OutputDirectory = MakeAbsolute(output).FullPath,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append("-verbose"),
    };
    foreach (var trait in UNSUPPORTED_TESTS) {
        settings.ExcludeTrait(trait.Key, trait.Value);
    }
    XUnit2(new [] { testAssembly }, settings);
}

void RunDotNetTest(
    FilePath testProject,
    DirectoryPath output,
    string configuration = null,
    Dictionary<string, string> properties = null)
{
    output = MakeAbsolute(output);
    var dir = testProject.GetDirectory();
    var settings = new DotNetTestSettings {
        Configuration = configuration ?? CONFIGURATION,
        NoBuild = true,
        Loggers = new [] { "xunit" },
        WorkingDirectory = dir,
        ResultsDirectory = output,
        Verbosity = DotNetVerbosity.Normal,
        ArgumentCustomization = args => {
            args = args
                .Append("/p:Platform=\"AnyCPU\"");
            if (COVERAGE)
                args = args
                    .Append("/p:CollectCoverage=true")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    .Append($"/p:CoverletOutput={output.Combine("Coverage").FullPath}/");
            if (properties != null) {
                foreach (var prop in properties) {
                    if (!string.IsNullOrEmpty(prop.Value)) {
                        args = args
                            .Append($"/p:{prop.Key}={prop.Value}");
                    }
                }
            }
            return args;
        },
    };
    var filter = string.Join("&", UNSUPPORTED_TESTS.Select(t => $"{t.Key}!={t.Value}"));
    if (!string.IsNullOrEmpty(filter)) {
        settings.Filter = filter;
    }
    DotNetTest(MakeAbsolute(testProject).FullPath, settings);
}

void RunDotNetPublish(
    FilePath testProject,
    DirectoryPath output = null,
    string configuration = null,
    string framework = null,
    string runtime = null)
{
    var dir = testProject.GetDirectory();
    var settings = new DotNetPublishSettings {
        Configuration = configuration ?? CONFIGURATION,
        Framework = framework,
        Runtime = runtime,
        NoBuild = true,
        WorkingDirectory = dir,
        OutputDirectory = output,
    };
    DotNetPublish(testProject.GetFilename().ToString(), settings);
}

void RunCodeCoverage(string testResultsGlob, DirectoryPath output)
{
    try {
        DotNetTool(
            $"reportgenerator" +
            $"  -reports:{testResultsGlob}" +
            $"  -targetdir:{output}" +
            $"  -reporttypes:HtmlInline_AzurePipelines;Cobertura" +
            $"  -assemblyfilters:-*.Tests");
    } catch (Exception ex) {
        Error("Make sure to install the 'dotnet-reportgenerator-globaltool' .NET Core global tool.");
        Error(ex);
        throw;
    }
    var xml = $"{output}/Cobertura.xml";
    var root = FindRegexMatchGroupsInFile(xml, @"<source>(.*)<\/source>", 0)[1].Value;
    ReplaceTextInFiles(xml, root, "");
}

void DecompressArchive(FilePath archive, DirectoryPath outputDir)
{
    using (var stream = System.IO.File.OpenRead(archive.FullPath))
    using (var reader = ReaderFactory.Open(stream)) {
        while(reader.MoveToNextEntry()) {
            if (!reader.Entry.IsDirectory) {
                reader.WriteEntryToDirectory(outputDir.FullPath, new ExtractionOptions {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }
}

IEnumerable<(DirectoryPath path, string platform)> GetPlatformDirectories(DirectoryPath rootDir)
{
    var platformDirs = GetDirectories($"{rootDir}/*");

    // try find any cross-platform frameworks
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName().ToLower();
        if (d.StartsWith("netstandard") || d.StartsWith("portable") || d.Equals("net6.0") || d.Equals("net7.0") || d.Equals("net8.0") || d.Equals("net9.0")) {
            // we just want this single platform
            yield return (dir, null);
            yield break;
        }
    }

    // there were no cross-platform libraries, so process each platform
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName().ToLower();
        if (d.StartsWith("monoandroid") || (d.StartsWith("net") && d.Contains("-android")))
            yield return (dir, "android");
        else if (d.StartsWith("net4"))
            yield return (dir, "net");
        else if (d.StartsWith("uap"))
            yield return (dir, "uwp");
        else if (d.StartsWith("xamarinios") || d.StartsWith("xamarin.ios") || (d.StartsWith("net") && d.Contains("-ios")))
            yield return (dir, "ios");
        else if (d.StartsWith("xamarinmac") || d.StartsWith("xamarin.mac") || (d.StartsWith("net") && d.Contains("-macos")))
            yield return (dir, "macos");
        else if (d.StartsWith("xamarintvos") || d.StartsWith("xamarin.tvos") || (d.StartsWith("net") && d.Contains("-tvos")))
            yield return (dir, "tvos");
        else if (d.StartsWith("xamarinwatchos") || d.StartsWith("xamarin.watchos") || (d.StartsWith("net") && d.Contains("-watchos")))
            yield return (dir, "watchos");
        else if (d.StartsWith("tizen") || (d.StartsWith("net") && d.Contains("-tizen")))
            yield return (dir, "tizen");
        else if (d.StartsWith("net") && d.Contains("-windows"))
            yield return (dir, "windows");
        else if (d.StartsWith("net") && d.Contains("-maccatalyst"))
            yield return (dir, "maccatalyst");
        else if (d.StartsWith("netcoreapp"))
            continue; // skip this one for now
        else
            throw new Exception($"Unknown platform '{d}' found at '{dir}'.");
    }
}

async Task DownloadPackageAsync(string id, DirectoryPath outputDirectory)
{
    var version = "0.0.0-";
    if (!string.IsNullOrEmpty(PREVIEW_LABEL) && PREVIEW_LABEL.StartsWith("pr."))
        version += PREVIEW_LABEL.ToLower();
    else if (!string.IsNullOrEmpty(GIT_SHA))
        version += "commit." + GIT_SHA.ToLower();
    else if (!string.IsNullOrEmpty(GIT_BRANCH_NAME))
        version += "branch." + GIT_BRANCH_NAME.Replace("/", ".").ToLower();
    else
        version += "branch.main";
    version += ".*";

    var filter = new NuGetVersions.Filter {
        IncludePrerelease = true,
        SourceUrl = PREVIEW_FEED_URL,
        VersionRange = VersionRange.Parse(version),
    };

    var latestVersion = await NuGetVersions.GetLatestAsync(id, filter);

    var comparer = new NuGetDiff(PREVIEW_FEED_URL);
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;

    // Track progress dynamically - queue grows as dependencies are discovered
    var queue = new Queue<(string id, NuGetVersion version)>();
    var discovered = new HashSet<string>();
    var completed = new List<string>();
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // Start with the root package
    queue.Enqueue((id, latestVersion));
    discovered.Add(id.ToLower());

    Information($"Starting download of native packages...");
    Information($"");

    while (queue.Count > 0)
    {
        var pkg = queue.Dequeue();
        var currentId = pkg.id.ToLower();
        var currentVersion = pkg.version;
        
        var total = completed.Count + queue.Count + 1; // completed + remaining + current
        var progress = (completed.Count * 100) / total;
        var remaining = queue.Count;
        
        Information($"[{progress,3}%] ({completed.Count + 1}/{total}) Downloading: {currentId}" + 
                   (remaining > 0 ? $"  ({remaining} more in queue)" : ""));

        // Download and extract the package
        var root = await comparer.ExtractCachedPackageAsync(currentId, currentVersion);
        var toolsDir = $"{root}/tools/";
        if (DirectoryExists(toolsDir)) {
            var allFiles = GetFiles(toolsDir + "**/*");
            foreach (var file in allFiles) {
                var relative = MakeAbsolute(Directory(toolsDir)).GetRelativePath(file);
                var dir = $"{outputDirectory}/{relative.GetDirectory()}";
                EnsureDirectoryExists(dir);
                CopyFileToDirectory(file, dir);
            }
        }

        // Discover dependencies and add to queue
        var nuspec = $"{root}/{currentId}.nuspec";
        var xdoc = XDocument.Load(nuspec);
        var xmlns = xdoc.Root.Name.Namespace;
        var dependencies = xdoc.Root.Descendants(xmlns + "dependency").ToArray();

        var newDeps = 0;
        foreach (var dep in dependencies)
        {
            var depId = dep.Attribute("id").Value.ToLower();
            var depVersion = dep.Attribute("version").Value;
            
            if (!discovered.Contains(depId))
            {
                discovered.Add(depId);
                queue.Enqueue((depId, NuGetVersion.Parse(depVersion)));
                newDeps++;
            }
        }
        
        if (newDeps > 0)
            Information($"         └─ Found {newDeps} new dependencies");
        
        completed.Add(currentId);
    }
    
    stopwatch.Stop();
    Information($"");
    Information($"========================================");
    Information($"Download complete!");
    Information($"  Packages: {completed.Count}");
    Information($"  Duration: {stopwatch.Elapsed.TotalSeconds:F1}s");
    Information($"========================================");
}
