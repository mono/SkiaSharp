using System.Runtime.InteropServices;

var VERBOSITY_NUGET = NuGetVerbosity.Detailed;
var VERBOSITY_NUGETCORE = DotNetCoreRestoreVerbosity.Verbose;
switch (VERBOSITY) {
    case Verbosity.Quiet:
    case Verbosity.Minimal:
        VERBOSITY_NUGET = NuGetVerbosity.Quiet;
        VERBOSITY_NUGETCORE = DotNetCoreRestoreVerbosity.Minimal;
        break;
    case Verbosity.Normal:
        VERBOSITY_NUGET = NuGetVerbosity.Normal;
        VERBOSITY_NUGETCORE = DotNetCoreRestoreVerbosity.Warning;
        break;
    case Verbosity.Verbose:
    case Verbosity.Diagnostic:
        VERBOSITY_NUGET = NuGetVerbosity.Detailed;
        VERBOSITY_NUGETCORE = DotNetCoreRestoreVerbosity.Verbose;
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

var RunDotNetCoreRestore = new Action<string> ((solution) =>
{
    DotNetCoreRestore (solution, new DotNetCoreRestoreSettings { 
        Sources = NuGetSources,
        // Verbosity = VERBOSITY_NUGETCORE // TODO: v1.1.1 has different values ???
    });
});

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
    if (!DirectoryExists (outputPath)) {
        CreateDirectory (outputPath);
    }

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

var RunTests = new Action<FilePath> ((testAssembly) =>
{
    var dir = testAssembly.GetDirectory ();
    RunProcess (NUnitConsoleToolPath, new ProcessSettings {
        Arguments = string.Format ("\"{0}\" --work=\"{1}\"", testAssembly, dir),
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

var ClearSkiaSharpNuGetCache = new Action (() => {
    // first we need to add our new nuget to the cache so we can restore
    // we first need to delete the old stuff
    DirectoryPath home = EnvironmentVariable ("USERPROFILE") ?? EnvironmentVariable ("HOME");
    var installedNuGet = home.Combine (".nuget").Combine ("packages").FullPath + "/*";
    var packages = VERSION_PACKAGES.Keys;
    var dirs = GetDirectories (installedNuGet);
    foreach (var dir in dirs) {
        var dirName = dir.GetDirectoryName ();
        foreach (var pkg in packages) {
            if (string.Equals (pkg, dirName, StringComparison.OrdinalIgnoreCase)) {
                Warning ("SkiaSharp nugets were installed at '{0}', removing...", dir);
                CleanDirectory (dir);
            }
        }
    }
});
