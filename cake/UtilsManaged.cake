
var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

var RunMSBuildWithPlatform = new Action<FilePath, string> ((solution, platform) =>
{
    MSBuild (solution, c => { 
        c.Configuration = "Release"; 
        c.Verbosity = VERBOSITY;
        c.Properties ["Platform"] = new [] { platform };
        c.MSBuildPlatform = MSBuildPlatform.x86;
        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
    });
});

var RunMSBuildWithPlatformTarget = new Action<FilePath, string> ((solution, platformTarget) =>
{
    MSBuild (solution, c => { 
        c.Configuration = "Release"; 
        c.Verbosity = VERBOSITY;
        c.PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), platformTarget);
        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
    });
});

var RunMSBuildRestore = new Action<FilePath> ((solution) =>
{
    MSBuild (solution, c => { 
        c.Configuration = "Release"; 
        c.Targets.Clear();
        c.Targets.Add("Restore");
        c.Verbosity = VERBOSITY;
        c.PlatformTarget = PlatformTarget.MSIL;
        c.MSBuildPlatform = MSBuildPlatform.x86;
        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
    });
});

var RunMSBuildRestoreLocal = new Action<FilePath> ((solution) =>
{
    var dir = solution.GetDirectory ();
    MSBuild (solution, c => { 
        c.Configuration = "Release"; 
        c.Targets.Clear();
        c.Targets.Add("Restore");
        c.Verbosity = VERBOSITY;
        c.Properties ["RestoreNoCache"] = new [] { "true" };
        c.Properties ["RestorePackagesPath"] = new [] { "./externals/packages" };
        c.PlatformTarget = PlatformTarget.MSIL;
        c.MSBuildPlatform = MSBuildPlatform.x86;
        if (!string.IsNullOrEmpty (MSBuildToolPath)) {
            c.ToolPath = MSBuildToolPath;
        }
        // c.Properties ["RestoreSources"] = NuGetSources;
        c.ArgumentCustomization = args => args.Append ($"/p:RestoreSources=\"{string.Join (IsRunningOnWindows () ? ";" : "%3B", NuGetSources)}\"");
    });
});

var RunMSBuild = new Action<FilePath> ((solution) =>
{
    RunMSBuildWithPlatform (solution, "\"Any CPU\"");
});

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
    EnsureDirectoryExists (outputPath);

    NuGetPack (nuspecPath, new NuGetPackSettings {
        OutputDirectory = outputPath,
        BasePath = nuspecPath.GetDirectory (),
        ToolPath = NugetToolPath
    });
});

var RunProcess = new Action<FilePath, ProcessSettings> ((process, settings) =>
{
    var result = StartProcess (process, settings);
    if (result != 0) {
        throw new Exception ($"Process '{process}' failed with error: {result}");
    }
});

var RunTests = new Action<FilePath, string[], bool> ((testAssembly, skip, is32) =>
{
    var dir = testAssembly.GetDirectory ();
    var settings = new XUnit2Settings {
        NUnitReport = true,
        ReportName = "TestResult",
        UseX86 = is32,
        Parallelism = ParallelismOption.Assemblies,
        OutputDirectory = dir,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append ("-verbose"),
    };
    if (skip != null) {
        settings.TraitsToExclude.Add ("Category", skip);
    }
    XUnit2 (new [] { testAssembly }, settings);
});

var RunNetCoreTests = new Action<FilePath, string[]> ((testAssembly, skip) =>
{
    var dir = testAssembly.GetDirectory ();
    string skipString = string.Empty;
    if (skip != null) {
        foreach (var s in skip) {
            skipString += $" -notrait \"Category={skip}\"";
        }
    }
    DotNetCoreTool(testAssembly, "xunit", $"-verbose -parallel none -nunit \"TestResult.xml\" {skipString}", new DotNetCoreToolSettings {
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
        var ext = System.IO.Path.GetExtension (abs).ToLowerInvariant ();
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
                        guids.Add (m.Groups[2].Value. ToLowerInvariant ());
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
                        if (line.ToLowerInvariant ().Contains (guid)) {
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
