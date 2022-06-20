void PackageNuGet(FilePath nuspecPath, DirectoryPath outputPath, bool allowDefaultExcludes = false, string symbolsFormat = null)
{
    EnsureDirectoryExists(outputPath);
    var settings = new NuGetPackSettings {
        OutputDirectory = MakeAbsolute(outputPath),
        BasePath = nuspecPath.GetDirectory(),
        Properties = new Dictionary<string, string> {
            // NU5048: The 'PackageIconUrl'/'iconUrl' element is deprecated. Consider using the 'PackageIcon'/'icon' element instead.
            // NU5105: The package version 'xxx' uses SemVer 2.0.0 or components of SemVer 1.0.0 that are not supported on legacy clients.
            // NU5125: The 'licenseUrl' element will be deprecated. Consider using the 'license' element instead.
            { "NoWarn", "NU5048,NU5105,NU5125" }
        },
    };
    if (allowDefaultExcludes) {
        settings.ArgumentCustomization = args => args.Append("-NoDefaultExcludes");
    }
    if (!string.IsNullOrEmpty(symbolsFormat)) {
        settings.Symbols = true;
        settings.SymbolPackageFormat = symbolsFormat;
    }
    NuGetPack(nuspecPath, settings);
}

void RunTests(FilePath testAssembly, bool is32)
{
    var dir = testAssembly.GetDirectory();
    var settings = new XUnit2Settings {
        ReportName = "TestResults",
        XmlReport = true,
        UseX86 = is32,
        NoAppDomain = true,
        Parallelism = ParallelismOption.All,
        OutputDirectory = dir,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append("-verbose"),
    };
    var traits = CreateTraitsDictionary(UNSUPPORTED_TESTS);
    foreach (var trait in traits) {
        settings.ExcludeTrait(trait.Name, trait.Value);
    }
    XUnit2(new [] { testAssembly }, settings);
}

void RunNetCoreTests(FilePath testAssembly)
{
    var dir = testAssembly.GetDirectory();
    var buildSettings = new DotNetCoreBuildSettings {
        Configuration = CONFIGURATION,
        WorkingDirectory = dir,
    };
    DotNetCoreBuild(testAssembly.GetFilename().ToString(), buildSettings);
    var settings = new DotNetCoreTestSettings {
        Configuration = CONFIGURATION,
        NoBuild = true,
        TestAdapterPath = ".",
        Loggers = new [] { "xunit" },
        WorkingDirectory = dir,
        Verbosity = DotNetCoreVerbosity.Normal,
        ArgumentCustomization = args => {
            if (COVERAGE)
                args = args
                    .Append("/p:CollectCoverage=true")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    .Append("/p:CoverletOutput=Coverage/");
            return args;
        },
    };
    var traits = CreateTraitsDictionary(UNSUPPORTED_TESTS);
    var filter = string.Join("&", traits.Select(t => $"{t.Name}!={t.Value}"));
    if (!string.IsNullOrEmpty(filter)) {
        settings.Filter = filter;
    }
    DotNetCoreTest(testAssembly.GetFilename().ToString(), settings);
}

void RunNetCorePublish(FilePath testProject, DirectoryPath output)
{
    var dir = testProject.GetDirectory();
    var settings = new DotNetCorePublishSettings {
        Configuration = CONFIGURATION,
        NoBuild = true,
        WorkingDirectory = dir,
        OutputDirectory = output,
    };
    DotNetCorePublish(testProject.GetFilename().ToString(), settings);
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

IEnumerable<(string Name, string Value)> CreateTraitsDictionary(string args)
{
    if (!string.IsNullOrEmpty(args)) {
        var traits = args.Split(';');
        foreach (var trait in traits) {
            var kv = trait.Split('=');
            if (kv.Length != 2)
                continue;
            yield return (kv[0], kv[1]);
        }
    }
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
        if (d.StartsWith("netstandard") || d.StartsWith("portable") || d.Equals("net6.0")) {
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

string[] GetReferenceSearchPaths()
{
    var refs = new List<string>();

    if (IsRunningOnWindows()) {
        var vs =
            VS_INSTALL ??
            VSWhereLatest(new VSWhereLatestSettings { Requires = "Component.Xamarin" }) ??
            VSWhereLatest(new VSWhereLatestSettings { Requires = "Component.Xamarin", IncludePrerelease = true });
        var referenceAssemblies = $"{vs}/Common7/IDE/ReferenceAssemblies/Microsoft/Framework";
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        refs.AddRange(GetDirectories("./output/docs/temp/*").Select(d => d.FullPath));
        refs.Add($"{referenceAssemblies}/MonoTouch/v1.0");
        refs.Add($"{referenceAssemblies}/MonoAndroid/v1.0");
        refs.Add($"{referenceAssemblies}/MonoAndroid/v9.0");
        refs.Add($"{referenceAssemblies}/Xamarin.iOS/v1.0");
        refs.Add($"{referenceAssemblies}/Xamarin.TVOS/v1.0");
        refs.Add($"{referenceAssemblies}/Xamarin.WatchOS/v1.0");
        refs.Add($"{referenceAssemblies}/Xamarin.Mac/v2.0");
        refs.Add($"{pf}/Windows Kits/10/UnionMetadata/Facade");
        refs.Add($"{pf}/Windows Kits/10/References/Windows.Foundation.UniversalApiContract/1.0.0.0");
        refs.Add($"{pf}/Windows Kits/10/References/Windows.Foundation.FoundationContract/1.0.0.0");
        refs.Add($"{pf}/GtkSharp/2.12/lib/gtk-sharp-2.0");
        refs.Add($"{vs}/Common7/IDE/PublicAssemblies");
    } else {
        // TODO
    }

    return refs.ToArray();
}

string[] GetDotNetPacksSearchPaths()
{
    var refs = new List<string>();

    RunProcess("dotnet", "--list-sdks", out var sdks);

    var last = sdks.Last();
    var start = last.IndexOf("[") + 1;
    var latestSdk = (DirectoryPath)(last.Substring(start, last.Length - start - 1));
    var dotnetRoot = latestSdk.Combine("..");

    foreach(var pack in GetDirectories(dotnetRoot.Combine("packs").FullPath + "/*.Ref.*")) {
        var latestPath = GetDirectories(pack.FullPath + "/*").Last();
        refs.AddRange(GetDirectories(latestPath.FullPath + "/ref/net*").Select(d => d.FullPath));
    }

    foreach(var pack in GetDirectories(dotnetRoot.Combine("packs").FullPath + "/*.Ref")) {
        var latestPath = GetDirectories(pack.FullPath + "/*").Last();
        refs.AddRange(GetDirectories(latestPath.FullPath + "/ref/net*").Select(d => d.FullPath));
    }

    return refs.ToArray();
}

async Task<NuGetDiff> CreateNuGetDiffAsync()
{
    var comparer = new NuGetDiff();
    comparer.SearchPaths.AddRange(GetDotNetPacksSearchPaths());
    comparer.SearchPaths.AddRange(GetReferenceSearchPaths());
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;

    await AddDep("OpenTK.GLControl", "NET20");
    await AddDep("Tizen.NET", "netstandard2.0");
    await AddDep("Xamarin.Forms", "netstandard2.0");
    await AddDep("Xamarin.Forms", "MonoAndroid90");
    await AddDep("Xamarin.Forms", "Xamarin.iOS10");
    await AddDep("Xamarin.Forms", "Xamarin.Mac");
    await AddDep("Xamarin.Forms", "tizen40");
    await AddDep("Xamarin.Forms", "uap10.0.16299");
    await AddDep("Xamarin.Forms.Platform.WPF", "net461");
    await AddDep("Xamarin.Forms.Platform.GTK", "net45");
    await AddDep("GtkSharp", "netstandard2.0");
    await AddDep("GdkSharp", "netstandard2.0");
    await AddDep("GLibSharp", "netstandard2.0");
    await AddDep("AtkSharp", "netstandard2.0");
    await AddDep("System.Memory", "netstandard2.0");
    await AddDep("Uno.UI", "netstandard2.0");
    await AddDep("Uno.UI", "MonoAndroid10.0");
    await AddDep("Uno.UI", "xamarinios10");
    await AddDep("Uno.UI", "xamarinmac20");
    await AddDep("Uno.UI", "UAP");
    await AddDep("Microsoft.WindowsAppSDK", "net5.0-windows10.0.18362.0");
    await AddDep("Microsoft.Maui.Graphics", "netstandard2.0");
    await AddDep("Microsoft.Windows.SDK.NET.Ref", "");

    await AddDep("OpenTK.GLControl", "NET40", "reference");
    await AddDep("Xamarin.Forms", "Xamarin.iOS10", "reference");
    await AddDep("Xamarin.Forms", "Xamarin.Mac", "reference");
    await AddDep("Xamarin.Forms", "uap10.0", "reference");

    Verbose("Added search paths:");
    foreach (var path in comparer.SearchPaths) {
        var found = GetFiles($"{path}/*.dll").Any() || GetFiles($"{path}/*.winmd").Any();
        Verbose($"    {(found ? " " : "!")} {path}");
    }

    return comparer;

    async Task AddDep(string id, string platform, string type = "release")
    {
        var version = GetVersion(id, type);
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        comparer.SearchPaths.Add(System.IO.Path.Combine(root, "lib", platform));
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

    await Download(id, latestVersion);

    async Task Download(string currentId, NuGetVersion currentVersion)
    {
        currentId = currentId.ToLower();

        Information($"Downloading '{currentId}' version '{currentVersion}'...");

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

        var nuspec = $"{root}/{currentId}.nuspec";
        var xdoc = XDocument.Load(nuspec);
        var xmlns = xdoc.Root.Name.Namespace;
        var dependencies = xdoc.Root.Descendants(xmlns + "dependency").ToArray();

        foreach (var dep in dependencies) {
            var depId = dep.Attribute("id").Value;
            var depVersion = dep.Attribute("version").Value;
            await Download(depId, NuGetVersion.Parse(depVersion));
        }
    }
}
