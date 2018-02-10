using System.Runtime.InteropServices;
using SharpCompress.Readers;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

var VERBOSITY_NUGET = NuGetVerbosity.Detailed;
switch (VERBOSITY) {
    case Verbosity.Quiet:
    case Verbosity.Minimal:
        VERBOSITY_NUGET = NuGetVerbosity.Quiet;
        break;
    case Verbosity.Normal:
        VERBOSITY_NUGET = NuGetVerbosity.Normal;
        break;
    case Verbosity.Verbose:
    case Verbosity.Diagnostic:
        VERBOSITY_NUGET = NuGetVerbosity.Detailed;
        break;
};

var SolutionProjectRegex = new Regex(@",\s*""(.*?\.\w{2}proj)""");

var RunNuGetRestore = new Action<FilePath> ((solution) =>
{
    NuGetRestore (solution, new NuGetRestoreSettings { 
        ToolPath = NugetToolPath,
        Source = NuGetSources,
        Verbosity = VERBOSITY_NUGET
    });
});

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

var RunMSBuild = new Action<FilePath> ((solution) =>
{
    RunMSBuildWithPlatform (solution, "\"Any CPU\"");
});

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
    EnsureDirectoryExists (outputPath);

    NuGetPack (nuspecPath, new NuGetPackSettings { 
        Verbosity = VERBOSITY_NUGET,
        OutputDirectory = outputPath,        
        BasePath = "./",
        ToolPath = NugetToolPath
    });                
});

var RunProcess = new Action<FilePath, ProcessSettings> ((process, settings) =>
{
    var result = StartProcess (process, settings);
    if (result != 0) {
        throw new Exception ("Process '" + process + "' failed with error: " + result);
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
            skipString += " -notrait \"Category=" + skip + "\"";
        }
    }
    DotNetCoreTool(testAssembly, "xunit", "-verbose -parallel none -nunit \"TestResult.xml\"" + skipString, new DotNetCoreToolSettings {
        WorkingDirectory = dir,
    });
});

var RunMdocUpdate = new Action<FilePath[], DirectoryPath, DirectoryPath[]> ((assemblies, docsRoot, refs) =>
{
    var refArgs = string.Empty;
    if (refs != null) {
        refArgs = string.Join (" ", refs.Select (r => string.Format ("--lib=\"{0}\"", r)));
    }
    var assemblyArgs = string.Join (" ", assemblies.Select (a => string.Format ("\"{0}\"", a)));
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("update --preserve --out=\"{0}\" {1} {2}", docsRoot, refArgs, assemblyArgs),
    });
});

var RunMdocMSXml = new Action<DirectoryPath, DirectoryPath> ((docsRoot, outputDir) =>
{
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("export-msxdoc \"{0}\" --debug", MakeAbsolute (docsRoot)),
        WorkingDirectory = MakeAbsolute (outputDir).ToString ()
    });
});

var RunMdocAssemble = new Action<DirectoryPath, FilePath> ((docsRoot, output) =>
{
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("assemble --out=\"{0}\" \"{1}\" --debug", output, docsRoot),
    });
});

var RunSNVerify = new Action<FilePath> ((assembly) =>
{
    RunProcess (SNToolPath, new ProcessSettings {
        Arguments = string.Format ("-vf \"{0}\"", assembly),
    });
});

var RunSNReSign = new Action<FilePath, FilePath> ((assembly, key) =>
{
    RunProcess (SNToolPath, new ProcessSettings {
        Arguments = string.Format ("-R \"{0}\" \"{1}\"", assembly, key),
    });
});

var RunGenApi = new Action<FilePath, FilePath> ((input, output) =>
{
    RunProcess (GenApiToolPath, new ProcessSettings {
        Arguments = string.Format ("\"{0}\" -out \"{1}\"", input, output),
    });
    ReplaceTextInFiles (output.FullPath, 
        "[System.ComponentModel.EditorBrowsableAttribute(1)]",
        "[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)1)]");
});

var ClearSkiaSharpNuGetCache = new Action (() => {
    // first we need to add our new nuget to the cache so we can restore
    // we first need to delete the old stuff
    var packagesDir = EnvironmentVariable ("NUGET_PACKAGES");
    if (string.IsNullOrEmpty (packagesDir)) {
        var home = EnvironmentVariable ("USERPROFILE") ?? EnvironmentVariable ("HOME");
        packagesDir = ((DirectoryPath) home).Combine (".nuget").Combine ("packages").ToString();
    }
    var installedNuGet = packagesDir + "/*";
    var packages = VERSION_PACKAGES.Keys;
    var dirs = GetDirectories (installedNuGet);
    foreach (var pkg in packages) {
        Information ("Looking for an installed version of {0} in {1}...", pkg, installedNuGet);
        foreach (var dir in dirs) {
            var dirName = dir.GetDirectoryName ();
            if (string.Equals (pkg, dirName, StringComparison.OrdinalIgnoreCase)) {
                Warning ("SkiaSharp nugets were installed at '{0}', removing...", dir);
                CleanDirectory (dir);
            }
        }
    }
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

var CreateSamplesZip = new Action<DirectoryPath, DirectoryPath, DirectoryPath, DirectoryPath> ((samplesDirPath, bindingDirPath, sourceDirPath, outputFilePath) => {
    var dpc = System.IO.Path.DirectorySeparatorChar;
    var toZipPath = new Func<string, string> (inPath => inPath.Replace ('\\', '/'));
    var toMSBuildPath = new Func<string, string> (inPath => inPath.Replace ('/', '\\'));
    var toNativePath = new Func<string, string> (inPath => inPath.Replace ('\\', dpc).Replace ('/', dpc));

    var samplesDir = System.IO.Path.GetFullPath (toNativePath (MakeAbsolute (samplesDirPath).FullPath));
    var bindingDir = System.IO.Path.GetFullPath (toNativePath (MakeAbsolute (bindingDirPath).FullPath));
    var sourceDir = System.IO.Path.GetFullPath (toNativePath (MakeAbsolute (sourceDirPath).FullPath));
    var samplesUri = new Uri (samplesDir);

    using (var zip = System.IO.File.OpenWrite (toNativePath (MakeAbsolute (outputFilePath).FullPath)))
    using (var zipWriter = new ZipWriter (zip, new ZipWriterOptions (CompressionType.Deflate))) {
        foreach (var file in GetFiles (samplesDir + "/**/*")) {
            var abs = System.IO.Path.GetFullPath (toNativePath (file.FullPath));
            var absDir = System.IO.Path.GetDirectoryName (abs);
            var ext = System.IO.Path.GetExtension (abs).ToLowerInvariant ();
            var rel = samplesUri.MakeRelativeUri (new Uri (abs)).ToString ();

            if (ext == ".sln") {
                using (var ms = new MemoryStream ())
                using (var writer = new StreamWriter (ms)) {
                    using (var reader = new StreamReader (abs)) {
                        var skippingProject = false;
                        for (var line = reader.ReadLine (); line != null; line = reader.ReadLine ()) {
                            if (skippingProject) {
                                if (line.Trim ().Equals ("EndProject", StringComparison.OrdinalIgnoreCase)) {
                                    skippingProject = false;
                                }
                            } else {
                                var m = SolutionProjectRegex.Match (line);
                                if (m.Success) {
                                    var relProjectPath = toNativePath (m.Groups[1].Value);
                                    var projectPath = System.IO.Path.GetFullPath (System.IO.Path.Combine (absDir, relProjectPath));
                                    if (!projectPath.StartsWith (samplesDir, StringComparison.OrdinalIgnoreCase)) {
                                        skippingProject = true;
                                    } else {
                                        writer.WriteLine (line);
                                    }
                                } else {
                                    writer.WriteLine (line);
                                }
                            }
                        }
                    }

                    writer.Flush ();
                    ms.Position = 0;
                    zipWriter.Write (rel, ms);
                }
            } else if (ext == ".csproj") {
                var xdoc = XDocument.Load (abs);

                // get all <ProjectReference> elements
                var projRefs1 = xdoc
                    .Root
                    .Elements (MSBuildNS + "ItemGroup")
                    .Elements (MSBuildNS + "ProjectReference");
                var projRefs2 = xdoc
                    .Root
                    .Elements ("ItemGroup")
                    .Elements ("ProjectReference");
                var projRefs = projRefs1.Union (projRefs2).ToArray ();
                // swap out the project references for package references
                foreach (var projRef in projRefs) {
                    var include = projRef.Attribute ("Include")?.Value;
                    if (!string.IsNullOrWhiteSpace (include)) {
                        var absInclude = System.IO.Path.GetFullPath (System.IO.Path.Combine (absDir, toNativePath (include)));
                        if (!absInclude.StartsWith (samplesDir, StringComparison.OrdinalIgnoreCase)) {
                            // not inside the samples directory, so needs to be removed

                            string binding = null;
                            if (absInclude.StartsWith (bindingDir, StringComparison.OrdinalIgnoreCase)) {
                                binding = System.IO.Path.GetFileName (absInclude).Split ('.').FirstOrDefault ();
                            } else if (absInclude.StartsWith (sourceDir, StringComparison.OrdinalIgnoreCase)) {
                                binding = System.IO.Path.GetFileName (System.IO.Path.GetDirectoryName (System.IO.Path.GetDirectoryName (absInclude)));
                            }

                            binding = VERSION_PACKAGES.Keys.FirstOrDefault (p => p.Equals (binding, StringComparison.OrdinalIgnoreCase));
                            if (!string.IsNullOrWhiteSpace (binding)) {
                                var name = projRef.Name.Namespace + "PackageReference";
                                projRef.AddAfterSelf (new XElement (name, new object[] {
                                        new XAttribute("Include", binding),
                                        new XAttribute("Version", VERSION_PACKAGES[binding]),
                                    }));
                                projRef.Remove ();
                            }
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
                        }
                    }
                }

                // save the modified document to the zip file
                using (var ms = new MemoryStream ()) {
                    xdoc.Save (ms);
                    ms.Flush ();
                    ms.Position = 0;
                    zipWriter.Write (rel, ms);
                }
            } else {
                zipWriter.Write (rel, abs);
            }
        }
    }
});
