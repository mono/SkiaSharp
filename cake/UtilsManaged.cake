void PackageNuGet(FilePath nuspecPath, DirectoryPath outputPath)
{
    EnsureDirectoryExists(outputPath);
    NuGetPack(nuspecPath, new NuGetPackSettings {
        OutputDirectory = MakeAbsolute(outputPath),
        BasePath = nuspecPath.GetDirectory(),
        ToolPath = NuGetToolPath,
    });
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
        Logger = "xunit",
        WorkingDirectory = dir,
        Verbosity = DotNetCoreVerbosity.Normal,
    };
    var traits = CreateTraitsDictionary(UNSUPPORTED_TESTS);
    var filter = string.Join("&", traits.Select(t => $"{t.Name}!={t.Value}"));
    if (!string.IsNullOrEmpty(filter)) {
        settings.Filter = filter;
    }
    DotNetCoreTest(testAssembly.GetFilename().ToString(), settings);
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
        if (d.StartsWith("netstandard") || d.StartsWith("portable")) {
            // we just want this single platform
            yield return (dir, null);
            yield break;
        }
    }

    // there were no cross-platform libraries, so process each platform
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName().ToLower();
        if (d.StartsWith("monoandroid"))
            yield return (dir, "android");
        else if (d.StartsWith("net4"))
            yield return (dir, "net");
        else if (d.StartsWith("uap"))
            yield return (dir, "uwp");
        else if (d.StartsWith("xamarinios") || d.StartsWith("xamarin.ios"))
            yield return (dir, "ios");
        else if (d.StartsWith("xamarinmac") || d.StartsWith("xamarin.mac"))
            yield return (dir, "macos");
        else if (d.StartsWith("xamarintvos") || d.StartsWith("xamarin.tvos"))
            yield return (dir, "tvos");
        else if (d.StartsWith("xamarinwatchos") || d.StartsWith("xamarin.watchos"))
            yield return (dir, "watchos");
        else if (d.StartsWith("tizen"))
            yield return (dir, "tizen");
        else if (d.StartsWith("netcoreapp"))
            ; // skip this one for now
        else
            throw new Exception($"Unknown platform '{d}' found at '{dir}'.");
    }
}

string[] GetReferenceSearchPaths()
{
    var refs = new List<string>();

    if (IsRunningOnWindows()) {
        var vs = VS_INSTALL ?? VSWhereLatest(new VSWhereLatestSettings { Requires = "Component.Xamarin" });
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
        refs.Add($"{pf}/GtkSharp/2.12/lib");
        refs.Add($"{vs}/Common7/IDE/PublicAssemblies");
    } else {
        // TODO
    }

    return refs.ToArray();
}

async Task<NuGetDiff> CreateNuGetDiffAsync()
{
    var comparer = new NuGetDiff();
    comparer.SearchPaths.AddRange(GetReferenceSearchPaths());
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;

    await AddDep("OpenTK.GLControl", "NET40", "reference");
    await AddDep("OpenTK.GLControl", "NET40");
    await AddDep("Tizen.NET", "netstandard2.0");
    await AddDep("Xamarin.Forms", "netstandard2.0");
    await AddDep("Xamarin.Forms", "MonoAndroid90");
    await AddDep("Xamarin.Forms", "Xamarin.iOS10");
    await AddDep("Xamarin.Forms", "Xamarin.Mac");
    await AddDep("Xamarin.Forms", "tizen40");
    await AddDep("Xamarin.Forms", "uap10.0");
    await AddDep("Xamarin.Forms.Platform.WPF", "net45");
    await AddDep("Xamarin.Forms.Platform.GTK", "net45");
    await AddDep("GtkSharp", "netstandard2.0");
    await AddDep("GdkSharp", "netstandard2.0");
    await AddDep("GLibSharp", "netstandard2.0");
    await AddDep("AtkSharp", "netstandard2.0");
    await AddDep("System.Memory", "netstandard2.0");

    return comparer;

    async Task AddDep(string id, string platform, string type = "release")
    {
        var version = GetVersion(id, type);
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        comparer.SearchPaths.Add(System.IO.Path.Combine(root, "lib", platform));
    }
}
