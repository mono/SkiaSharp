using System.Runtime.InteropServices;
using SharpCompress.Readers;

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

var CreateSamplesZip = new Action<DirectoryPath> ((samplesDir) => {

});
