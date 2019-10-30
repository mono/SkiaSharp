
var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

void RunMSBuild (
    FilePath solution,
    string platform = "Any CPU",
    string platformTarget = null,
    bool restore = true,
    bool restoreOnly = false)
{
    EnsureDirectoryExists ("./output/nugets/");

    MSBuild (solution, c => {
        c.Configuration = CONFIGURATION;
        c.Verbosity = VERBOSITY;
        c.ToolVersion = MSBuildToolVersion.VS2019;

        if (restoreOnly) {
            c.Targets.Clear();
            c.Targets.Add("Restore");
        } else {
            c.Restore = restore;
        }

        if (!string.IsNullOrEmpty (platformTarget)) {
            platform = null;
            c.PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), platformTarget);
        } else {
            c.PlatformTarget = PlatformTarget.MSIL;
            c.MSBuildPlatform = MSBuildPlatform.x86;
        }

        if (!string.IsNullOrEmpty (platform)) {
            c.Properties ["Platform"] = new [] { "\"" + platform + "\"" };
        }

        c.Properties ["RestoreNoCache"] = new [] { "true" };
        c.Properties ["RestorePackagesPath"] = new [] { PACKAGE_CACHE_PATH.FullPath };
        // c.Properties ["RestoreSources"] = NuGetSources;
        var sep = IsRunningOnWindows () ? ";" : "%3B";
        c.ArgumentCustomization = args => args.Append ($"/p:RestoreSources=\"{string.Join (sep, NuGetSources)}\"");

        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
    });
}

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
    EnsureDirectoryExists (outputPath);

    NuGetPack (nuspecPath, new NuGetPackSettings {
        OutputDirectory = outputPath,
        BasePath = nuspecPath.GetDirectory (),
        ToolPath = NuGetToolPath,
    });
});

var RunProcess = new Action<FilePath, ProcessSettings> ((process, settings) =>
{
    var result = StartProcess (process, settings);
    if (result != 0) {
        throw new Exception ($"Process '{process}' failed with error: {result}");
    }
});

void RunTests (FilePath testAssembly, bool is32)
{
    var dir = testAssembly.GetDirectory ();
    var settings = new XUnit2Settings {
        ReportName = "TestResults",
        XmlReport = true,
        UseX86 = is32,
        NoAppDomain = true,
        Parallelism = ParallelismOption.All,
        OutputDirectory = dir,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append ("-verbose"),
    };
    var traits = CreateTraitsDictionary(UNSUPPORTED_TESTS);
    foreach (var trait in traits) {
        settings.ExcludeTrait(trait.Name, trait.Value);
    }
    XUnit2 (new [] { testAssembly }, settings);
}

void RunNetCoreTests (FilePath testAssembly)
{
    var dir = testAssembly.GetDirectory ();
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

IEnumerable<(string Name, string Value)> CreateTraitsDictionary (string args)
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

var DecompressArchive = new Action<FilePath, DirectoryPath> ((archive, outputDir) => {
    using (var stream = System.IO.File.OpenRead (archive.FullPath))
    using (var reader = ReaderFactory.Open (stream)) {
        while (reader.MoveToNextEntry ()) {
            if (!reader.Entry.IsDirectory) {
                reader.WriteEntryToDirectory (outputDir.FullPath, new ExtractionOptions {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }
});

void CreateSamplesDirectory (DirectoryPath samplesDirPath, DirectoryPath outputDirPath, string versionSuffix = "")
{
    samplesDirPath = MakeAbsolute (samplesDirPath);
    outputDirPath = MakeAbsolute (outputDirPath);

    var solutionProjectRegex = new Regex(@",\s*""(.*?\.\w{2}proj)"", ""(\{.*?\})""");

    EnsureDirectoryExists (outputDirPath);
    CleanDirectory (outputDirPath);

    var ignoreBinObj = new GlobberSettings {
        Predicate = fileSystemInfo => {
            var segments = fileSystemInfo.Path.Segments;
            var keep = segments.All (s =>
                !s.Equals ("bin", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals ("obj", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals ("AppPackages", StringComparison.OrdinalIgnoreCase) &&
                !s.Equals (".vs", StringComparison.OrdinalIgnoreCase));
            return keep;
        }
    };

    var files = GetFiles ($"{samplesDirPath}/**/*", ignoreBinObj);
    foreach (var file in files) {
        var rel = samplesDirPath.GetRelativePath (file);
        var dest = outputDirPath.CombineWithFilePath (rel);
        var ext = file.GetExtension () ?? "";

        if (ext.Equals (".sln", StringComparison.OrdinalIgnoreCase)) {
            var lines = FileReadLines (file.FullPath).ToList ();
            var guids = new List<string> ();

            // remove projects that aren't samples
            for (var i = 0; i < lines.Count; i++) {
                var line = lines [i];
                var m = solutionProjectRegex.Match (line);
                if (!m.Success)
                    continue;

                // get the path of the project relative to the samples directory
                var relProjectPath = (FilePath) m.Groups [1].Value;
                var absProjectPath = GetFullPath (file, relProjectPath);
                var relSamplesPath = samplesDirPath.GetRelativePath (absProjectPath);
                if (!relSamplesPath.FullPath.StartsWith (".."))
                    continue;

                Debug ($"Removing the project '{relProjectPath}' for solution '{rel}'.");

                // skip the next line as it is the "EndProject" line
                guids.Add (m.Groups [2].Value.ToLower ());
                lines.RemoveAt (i--);
                lines.RemoveAt (i--);
            }

            // remove all the other references to this guid
            if (guids.Count > 0) {
                for (var i = 0; i < lines.Count; i++) {
                    var line = lines [i];
                    foreach (var guid in guids) {
                        if (line.ToLower ().Contains (guid)) {
                            lines.RemoveAt (i--);
                        }
                    }
                }
            }

            // save the solution
            EnsureDirectoryExists (dest.GetDirectory ());
            FileWriteLines (dest, lines.ToArray ());
        } else if (ext.Equals (".csproj", StringComparison.OrdinalIgnoreCase)) {
            var xdoc = XDocument.Load (file.FullPath);

            // process all the files and project references
            var projItems = xdoc.Root
                .Elements ().Where (e => e.Name.LocalName == "ItemGroup")
                .Elements ().Where (e => !string.IsNullOrWhiteSpace (e.Attribute ("Include")?.Value))
                .ToArray ();
            foreach (var projItem in projItems) {
                // get files in the include
                var relFilePath = (FilePath) projItem.Attribute ("Include").Value;
                var absFilePath = GetFullPath (file, relFilePath);

                // ignore files in the samples directory
                var relSamplesPath = samplesDirPath.GetRelativePath (absFilePath);
                if (!relSamplesPath.FullPath.StartsWith (".."))
                    continue;

                // substitute <ProjectReference> with <PackageReference>
                if (projItem.Name.LocalName == "ProjectReference" && FileExists (absFilePath)) {
                    var xReference = XDocument.Load (absFilePath.FullPath);
                    var packagingGroup = xReference.Root
                        .Elements ().Where (e => e.Name.LocalName == "PropertyGroup")
                        .Elements ().Where (e => e.Name.LocalName == "PackagingGroup")
                        .FirstOrDefault ()?.Value;
                    var version = GetVersion (packagingGroup);
                    if (!string.IsNullOrWhiteSpace (version)) {
                        Debug ($"Substituting project reference {relFilePath} for project {rel}.");
                        var name = projItem.Name.Namespace + "PackageReference";
                        var suffix = string.IsNullOrEmpty (versionSuffix) ? "" : $"-{versionSuffix}";
                        projItem.AddAfterSelf (new XElement (name, new object[] {
                            new XAttribute("Include", packagingGroup),
                            new XAttribute("Version", version + suffix),
                        }));
                    } else {
                        Warning ($"Unable to find version information for package '{packagingGroup}'.");
                    }
                } else {
                    Debug ($"Removing the file '{relFilePath}' for project '{rel}'.");
                }

                // remove the element as it will be outside the sample directory
                projItem.Remove ();
            }

            // process all the imports
            var imports = xdoc.Root
                .Elements ().Where (e =>
                    e.Name.LocalName == "Import" &&
                    !string.IsNullOrWhiteSpace (e.Attribute ("Project")?.Value))
                .ToArray ();
            foreach (var import in imports) {
                var project = import.Attribute ("Project").Value;

                // skip files inside the samples directory or do not exist
                var absProject = GetFullPath (file, project);
                var relSamplesPath = samplesDirPath.GetRelativePath (absProject);
                if (!relSamplesPath.FullPath.StartsWith (".."))
                    continue;

                Debug ($"Removing import '{project}' for project '{rel}'.");

                // not inside the samples directory, so needs to be removed
                import.Remove ();
            }

            // save the project
            EnsureDirectoryExists (dest.GetDirectory ());
            xdoc.Save (dest.FullPath);
        } else {
            EnsureDirectoryExists (dest.GetDirectory ());
            CopyFile (file, dest);
        }
    }

    DeleteFiles ($"{outputDirPath}/README.md");
    MoveFile ($"{outputDirPath}/README.zip.md", $"{outputDirPath}/README.md");
}

FilePath GetFullPath (FilePath root, FilePath path)
{
    path = path.FullPath.Replace ("*", "_");
    path = root.GetDirectory ().CombineWithFilePath (path);
    return (FilePath) System.IO.Path.GetFullPath (path.FullPath);
}

IEnumerable<(DirectoryPath path, string platform)> GetPlatformDirectories (DirectoryPath rootDir)
{
    var platformDirs = GetDirectories ($"{rootDir}/*");

    // try find any cross-platform frameworks
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName ().ToLower ();
        if (d.StartsWith ("netstandard") || d.StartsWith ("portable")) {
            // we just want this single platform
            yield return (dir, null);
            yield break;
        }
    }

    // there were no cross-platform libraries, so process each platform
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName ().ToLower ();
        if (d.StartsWith ("monoandroid"))
            yield return (dir, "android");
        else if (d.StartsWith ("net4"))
            yield return (dir, "net");
        else if (d.StartsWith ("uap"))
            yield return (dir, "uwp");
        else if (d.StartsWith ("xamarinios") || d.StartsWith ("xamarin.ios"))
            yield return (dir, "ios");
        else if (d.StartsWith ("xamarinmac") || d.StartsWith ("xamarin.mac"))
            yield return (dir, "macos");
        else if (d.StartsWith ("xamarintvos") || d.StartsWith ("xamarin.tvos"))
            yield return (dir, "tvos");
        else if (d.StartsWith ("xamarinwatchos") || d.StartsWith ("xamarin.watchos"))
            yield return (dir, "watchos");
        else if (d.StartsWith ("tizen"))
            yield return (dir, "tizen");
        else
            throw new Exception ($"Unknown platform '{d}' found at '{dir}'.");
    }
}

string[] GetReferenceSearchPaths ()
{
    var refs = new List<string> ();

    if (IsRunningOnWindows ()) {
        var vs = VSWhereLatest (new VSWhereLatestSettings { Requires = "Component.Xamarin" });
        var referenceAssemblies = $"{vs}/Common7/IDE/ReferenceAssemblies/Microsoft/Framework";
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        refs.AddRange (GetDirectories ("./output/docs/temp/*").Select (d => d.FullPath));
        refs.Add ($"{referenceAssemblies}/MonoTouch/v1.0");
        refs.Add ($"{referenceAssemblies}/MonoAndroid/v1.0");
        refs.Add ($"{referenceAssemblies}/MonoAndroid/v9.0");
        refs.Add ($"{referenceAssemblies}/Xamarin.iOS/v1.0");
        refs.Add ($"{referenceAssemblies}/Xamarin.TVOS/v1.0");
        refs.Add ($"{referenceAssemblies}/Xamarin.WatchOS/v1.0");
        refs.Add ($"{referenceAssemblies}/Xamarin.Mac/v2.0");
        refs.Add ($"{pf}/Windows Kits/10/UnionMetadata/Facade");
        refs.Add ($"{pf}/Windows Kits/10/References/Windows.Foundation.UniversalApiContract/1.0.0.0");
        refs.Add ($"{pf}/Windows Kits/10/References/Windows.Foundation.FoundationContract/1.0.0.0");
        refs.Add ($"{pf}/GtkSharp/2.12/lib");
        refs.Add ($"{vs}/Common7/IDE/PublicAssemblies");
    } else {
        // TODO
    }

    return refs.ToArray ();
}

async Task<NuGetDiff> CreateNuGetDiffAsync ()
{
    var comparer = new NuGetDiff ();
    comparer.SearchPaths.AddRange (GetReferenceSearchPaths ());
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;

    await AddDep ("OpenTK.GLControl", "NET40", "reference");
    await AddDep ("OpenTK.GLControl", "NET40");
    await AddDep ("Tizen.NET", "netstandard2.0");
    await AddDep ("Xamarin.Forms", "netstandard2.0");
    await AddDep ("Xamarin.Forms", "MonoAndroid90");
    await AddDep ("Xamarin.Forms", "Xamarin.iOS10");
    await AddDep ("Xamarin.Forms", "Xamarin.Mac");
    await AddDep ("Xamarin.Forms", "tizen40");
    await AddDep ("Xamarin.Forms", "uap10.0");
    await AddDep ("Xamarin.Forms.Platform.WPF", "net45");
    await AddDep ("GtkSharp", "netstandard2.0");
    await AddDep ("GLibSharp", "netstandard2.0");
    await AddDep ("AtkSharp", "netstandard2.0");

    return comparer;

    async Task AddDep(string id, string platform, string type = "release")
    {
        var version = GetVersion (id, type);
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        comparer.SearchPaths.Add(System.IO.Path.Combine(root, "lib", platform));
    }
}
