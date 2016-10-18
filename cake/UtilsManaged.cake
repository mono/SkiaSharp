
var RunNuGetRestore = new Action<FilePath> ((solution) =>
{
    NuGetRestore (solution, new NuGetRestoreSettings { 
        ToolPath = NugetToolPath,
        Source = NuGetSources,
        Verbosity = NuGetVerbosity.Detailed
    });
});

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
    if (!DirectoryExists (outputPath)) {
        CreateDirectory (outputPath);
    }

    NuGetPack (nuspecPath, new NuGetPackSettings { 
        Verbosity = NuGetVerbosity.Detailed,
        OutputDirectory = outputPath,        
        BasePath = "./",
        ToolPath = NugetToolPath
    });                
});

var RunTests = new Action<FilePath> ((testAssembly) =>
{
    var dir = testAssembly.GetDirectory ();
    var result = StartProcess (NUnitConsoleToolPath, new ProcessSettings {
        Arguments = string.Format ("\"{0}\" --work=\"{1}\"", testAssembly, dir),
    });
    
    if (result != 0) {
        throw new Exception ("NUnit test failed with error: " + result);
    }
});

var RunMdocUpdate = new Action<FilePath[], DirectoryPath, DirectoryPath[]> ((assemblies, docsRoot, refs) =>
{
    var refArgs = string.Empty;
    if (refs != null) {
        refArgs = string.Join (" ", refs.Select (r => string.Format ("--lib=\"{0}\"", r)));
    }
    var assemblyArgs = string.Join (" ", assemblies.Select (a => string.Format ("\"{0}\"", a)));
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("update --delete --out=\"{0}\" {1} {2}", docsRoot, refArgs, assemblyArgs),
    });
});

var RunMdocMSXml = new Action<DirectoryPath, DirectoryPath> ((docsRoot, outputDir) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("export-msxdoc \"{0}\"", MakeAbsolute (docsRoot)),
        WorkingDirectory = MakeAbsolute (outputDir).ToString ()
    });
});

var RunMdocAssemble = new Action<DirectoryPath, FilePath> ((docsRoot, output) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("assemble --out=\"{0}\" \"{1}\"", output, docsRoot),
    });
});

var ClearSkiaSharpNuGetCache = new Action (() => {
    // first we need to add our new nuget to the cache so we can restore
    // we first need to delete the old stuff
    DirectoryPath home = EnvironmentVariable ("USERPROFILE") ?? EnvironmentVariable ("HOME");
    var installedNuGet = home.Combine (".nuget").Combine ("packages").Combine ("SkiaSharp");
    if (DirectoryExists (installedNuGet)) {
        Warning ("SkiaSharp nugets were installed at '{0}', removing...", installedNuGet);
        CleanDirectory (installedNuGet);
    }
    installedNuGet = home.Combine (".nuget").Combine ("packages").Combine ("SkiaSharp.Views");
    if (DirectoryExists (installedNuGet)) {
        Warning ("SkiaSharp nugets were installed at '{0}', removing...", installedNuGet);
        CleanDirectory (installedNuGet);
    }
    installedNuGet = home.Combine (".nuget").Combine ("packages").Combine ("SkiaSharp.Views.Forms");
    if (DirectoryExists (installedNuGet)) {
        Warning ("SkiaSharp nugets were installed at '{0}', removing...", installedNuGet);
        CleanDirectory (installedNuGet);
    }
});
