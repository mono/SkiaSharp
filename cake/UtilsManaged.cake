
var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

void RunMSBuild (FilePath solution, string configuration = "Release", string platform = "Any CPU", string platformTarget = null, bool restore = true)
{
    MSBuild (solution, c => {
        c.Configuration = configuration;
        c.Verbosity = VERBOSITY;
        c.Restore = restore;

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

        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
    });
}

void RunMSBuildRestoreLocal (FilePath solution, DirectoryPath packagesDir, string configuration = "Release")
{
    var dir = solution.GetDirectory ();
    MSBuild (solution, c => {
        c.Configuration = configuration;
        c.Verbosity = VERBOSITY;
        c.Targets.Clear();
        c.Targets.Add("Restore");
        c.Properties ["RestoreNoCache"] = new [] { "true" };
        c.Properties ["RestorePackagesPath"] = new [] { packagesDir.FullPath };
        c.PlatformTarget = PlatformTarget.MSIL;
        c.MSBuildPlatform = MSBuildPlatform.x86;
        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
        // c.Properties ["RestoreSources"] = NuGetSources;
        c.ArgumentCustomization = args => args.Append ($"/p:RestoreSources=\"{string.Join (IsRunningOnWindows () ? ";" : "%3B", NuGetSources)}\"");
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

var RunTests = new Action<FilePath, bool> ((testAssembly, is32) =>
{
    var dir = testAssembly.GetDirectory ();
    var settings = new XUnit2Settings {
        ReportName = "TestResult",
        XmlReport = true,
        UseX86 = is32,
        Parallelism = ParallelismOption.Assemblies,
        OutputDirectory = dir,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append ("-verbose"),
    };
    XUnit2 (new [] { testAssembly }, settings);
});

var RunNetCoreTests = new Action<FilePath> ((testAssembly) =>
{
    var dir = testAssembly.GetDirectory ();
    DotNetCoreTest(testAssembly.GetFilename().ToString(), new DotNetCoreTestSettings {
        Configuration = "Release",
        NoRestore = true,
        TestAdapterPath = ".",
        Logger = "xunit",
        WorkingDirectory = dir,
    });
});

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

var CreateSamplesZip = new Action<DirectoryPath, DirectoryPath> ((samplesDirPath, outputDirPath) => {
    var platformExtensions = new [] {
        ".android",
        ".desktop",
        ".gtk",
        ".ios",
        ".mac",
        ".macos",
        ".netstandard",
        ".osx",
        ".portable",
        ".shared",
        ".tizen",
        ".tvos",
        ".uwp",
        ".watchos",
        ".wpf",
    };
    var workingDir = outputDirPath.Combine ("samples");

    // copy the current samples directory
    EnsureDirectoryExists (workingDir);
    CleanDirectory (workingDir);
    CopyDirectory (samplesDirPath, workingDir);

    // remove any binaries from the samples directory
    var settings = new DeleteDirectorySettings {
        Force = true,
        Recursive = true
    };
    DeleteDirectories (GetDirectories ($"{workingDir}/*/*/*/bin"), settings);
    DeleteDirectories (GetDirectories ($"{workingDir}/*/*/*/obj"), settings);
    DeleteDirectories (GetDirectories ($"{workingDir}/*/*/*/AppPackages"), settings);
    DeleteDirectories (GetDirectories ($"{workingDir}/*/*/.vs"), settings);

    // make sure the paths are in the correct format for comparison
    var dpc = System.IO.Path.DirectorySeparatorChar;
    var toNativePath = new Func<string, string> (inPath => inPath.Replace ('\\', dpc).Replace ('/', dpc));
    var samplesDir = System.IO.Path.GetFullPath (toNativePath (MakeAbsolute (workingDir).FullPath));
    var samplesUri = new Uri (samplesDir);

    // the regex to math the project entries in the solution
    var solutionProjectRegex = new Regex(@",\s*""(.*?\.\w{2}proj)"", ""(\{.*?\})""");

    foreach (var file in GetFiles ($"{workingDir}/**/*")) {
        var abs = System.IO.Path.GetFullPath (toNativePath (file.FullPath));
        var absDir = System.IO.Path.GetDirectoryName (abs);
        var ext = System.IO.Path.GetExtension (abs).ToLower ();
        var rel = samplesUri.MakeRelativeUri (new Uri (abs)).ToString ();

        if (ext == ".sln") {
            var modified = false;
            var lines = FileReadLines (abs).ToList ();
            var guids = new List<string> ();

            // remove projects that aren't samples
            for (var i = 0; i < lines.Count; i++) {
                var line = lines [i];
                var m = solutionProjectRegex.Match (line);
                if (m.Success) {
                    var relProjectPath = toNativePath (m.Groups[1].Value);
                    var projectPath = System.IO.Path.GetFullPath (System.IO.Path.Combine (absDir, relProjectPath));
                    if (!projectPath.StartsWith (samplesDir, StringComparison.OrdinalIgnoreCase)) {
                        // skip the next line as it is the "EndProject" line
                        guids.Add (m.Groups[2].Value. ToLower ());
                        lines.RemoveAt (i);
                        i--;
                        lines.RemoveAt (i);
                        i--;
                        modified = true;
                    }
                }
            }

            // remove all the other references
            if (guids.Count > 0) {
                for (var i = 0; i < lines.Count; i++) {
                    var line = lines [i];
                    foreach (var guid in guids) {
                        if (line.ToLower ().Contains (guid)) {
                            lines.RemoveAt (i);
                            i--;
                        }
                    }
                }
            }

            // save the modified solution
            if (modified) {
                FileWriteLines (abs, lines.ToArray ());
            }
        } else if (ext == ".csproj") {
            var modified = false;
            var xdoc = XDocument.Load (abs);

            // get all <ProjectReference> elements
            var projItems1 = xdoc
                .Root
                .Elements (MSBuildNS + "ItemGroup")
                .Elements ();
            var projItems2 = xdoc
                .Root
                .Elements ("ItemGroup")
                .Elements ();
            var projItems = projItems1.Union (projItems2).ToArray ();
            // swap out the project references for package references
            foreach (var projItem in projItems) {
                var include = projItem.Attribute ("Include")?.Value;
                if (!string.IsNullOrWhiteSpace (include)) {
                    var absInclude = System.IO.Path.GetFullPath (System.IO.Path.Combine (absDir, toNativePath (include)));
                    if (!absInclude.StartsWith (samplesDir, StringComparison.OrdinalIgnoreCase)) {
                        // not inside the samples directory, so needs to be removed
                        if (projItem.Name.LocalName == "ProjectReference") {
                            // get the desired package ID for this project reference
                            // we assume "Desired.Package.Id.<platform>.csproj" or we assume "Desired.Package.Id.csproj"
                            var binding = System.IO.Path.GetFileNameWithoutExtension (absInclude);
                            if (platformExtensions.Contains (System.IO.Path.GetExtension (binding).ToLower ()))
                                binding = System.IO.Path.GetFileNameWithoutExtension (binding);
                            // check to see if we have a specific version
                            var bindingVersion = GetVersion (binding);
                            if (!string.IsNullOrWhiteSpace (bindingVersion)) {
                                // add a <PackageReference>
                                var name = projItem.Name.Namespace + "PackageReference";
                                projItem.AddAfterSelf (new XElement (name, new object[] {
                                        new XAttribute("Include", binding),
                                        new XAttribute("Version", bindingVersion),
                                    }));
                            }
                        }
                        // remove the element
                        projItem.Remove ();
                        modified = true;
                    }
                }
            }

            // get all the <Import> elements
            var imports1 = xdoc.Root.Elements (MSBuildNS + "Import");
            var imports2 = xdoc.Root.Elements ("Import");
            var imports = imports1.Union (imports2).ToArray ();
            // remove them
            foreach (var import in imports) {
                var project = import.Attribute ("Project")?.Value;
                if (!string.IsNullOrWhiteSpace (project)) {
                    var absProject = System.IO.Path.GetFullPath (System.IO.Path.Combine (absDir, toNativePath (project)));
                    if (!absProject.StartsWith (samplesDir, StringComparison.OrdinalIgnoreCase)) {
                        // not inside the samples directory, so needs to be removed
                        import.Remove ();
                        modified = true;
                    }
                }
            }

            // save the modified project
            if (modified) {
                xdoc.Save (abs);
            }
        }
    }

    // finally create the zip
    Zip (workingDir, outputDirPath.CombineWithFilePath ("samples.zip"));
    CleanDirectory (workingDir);
});

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

        // HACK: https://github.com/mono/api-doc-tools/pull/401
        if (!FileExists ("./externals/winmd/Windows.winmd")) {
            EnsureDirectoryExists ("./externals/winmd/");
            CopyFile ($"{pf}/Windows Kits/10/UnionMetadata/Facade/Windows.WinMD", "./externals/winmd/Windows.winmd");
        }
        refs.Add (MakeAbsolute ((FilePath)"./externals/winmd/").FullPath);

        refs.AddRange (GetDirectories ("./output/docs/temp/*").Select (d => d.FullPath));
        refs.Add ($"{referenceAssemblies}/MonoTouch/v1.0");
        refs.Add ($"{referenceAssemblies}/MonoAndroid/v1.0");
        refs.Add ($"{referenceAssemblies}/MonoAndroid/v4.0.3");
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

    var AddDep = new Func<string, string, Task> (async (id, platform) => {
        var version = GetVersion (id, "release");
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        comparer.SearchPaths.Add(System.IO.Path.Combine(root, "lib", platform));
    });

    await AddDep ("OpenTK.GLControl", "NET40");
    await AddDep ("Tizen.NET", "netstandard2.0");
    await AddDep ("Xamarin.Forms", "netstandard2.0");
    await AddDep ("Xamarin.Forms", "MonoAndroid10");
    await AddDep ("Xamarin.Forms", "Xamarin.iOS10");
    await AddDep ("Xamarin.Forms", "Xamarin.Mac");
    await AddDep ("Xamarin.Forms", "tizen40");
    await AddDep ("Xamarin.Forms", "uap10.0");

    return comparer;
}
