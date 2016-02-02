#addin "Cake.Xamarin"
#addin "Cake.XCode"

#pragma warning disable 0169
var NUGET_RESTORE_SOURCES = new [] { "https://www.nuget.org/api/v2/", "https://www.myget.org/F/xamprojectci/api/v2" };
#pragma warning restore 0169

FilePath GetCakeToolPath ()
{
 	var appRootExe = Context.Environment.GetApplicationRoot ().CombineWithFilePath ("Cake.exe");

 	if (FileExists (appRootExe))
 		return appRootExe;

	var possibleExe = GetFiles ("../**/tools/Cake/Cake.exe").FirstOrDefault ();

	if (possibleExe != null)
		return possibleExe;
		
	var p = System.Diagnostics.Process.GetCurrentProcess ();	
	return new FilePath (p.Modules[0].FileName);
}

FilePath GetNUnitConsoleToolPath ()
{
	var appRoot = Context.Environment.GetApplicationRoot ();

	var consolePath = appRoot.Combine ("../").CombineWithFilePath ("NUnit.Console/tools/nunit3-console.exe");

	if (FileExists (consolePath))
		return consolePath;

	return GetFiles ("../../../**/nunit3-console.exe").FirstOrDefault ();
}

FilePath GetNugetToolPath ()
{
	if (IsRunningOnUnix ())
		return null;

	var appRoot = Context.Environment.GetApplicationRoot ();

	var nugetPath = appRoot.Combine ("../").CombineWithFilePath ("nuget.exe");

	if (FileExists (nugetPath))
		return nugetPath;

	return GetFiles ("../../../**/nuget.exe").FirstOrDefault ();
}

FilePath GetXamarinComponentToolPath ()
{
	var appRoot = Context.Environment.GetApplicationRoot();

	var nugetPath = appRoot.Combine ("../").CombineWithFilePath ("xamarin-component.exe");

	if (FileExists (nugetPath))
		return nugetPath;

	return GetFiles ("../../../**/xamarin-component.exe").FirstOrDefault ();
}

FilePath GetGenApiToolPath ()
{
	var appRoot = Context.Environment.GetApplicationRoot();

	var nugetPath = appRoot.Combine ("../").CombineWithFilePath ("genapi.exe");

	if (FileExists (nugetPath))
		return nugetPath;

	return GetFiles ("../../../**/genapi.exe").FirstOrDefault ();
}

void CopyDirectory (DirectoryPath sourceDir, DirectoryPath destDir, bool overwrite = true, bool copySubDirs = true)
{
    var sourceDirName = MakeAbsolute(sourceDir).FullPath;
    var destDirName = MakeAbsolute(destDir).FullPath;
    var dir = new DirectoryInfo(sourceDirName);
    
    // If the source directory does not exist, throw an exception.
    if (!dir.Exists) {
        throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
    }

    // If the destination directory does not exist, create it.
    if (!DirectoryExists(destDir)) {
        CreateDirectory(destDir);
    }

    // Get the file contents of the directory to copy.
    foreach (FileInfo file in dir.GetFiles()) {
        file.CopyTo(destDir.CombineWithFilePath(file.Name).FullPath, false);
    }

    // If copySubDirs is true, copy the subdirectories.
    if (copySubDirs) {
        foreach (DirectoryInfo subdir in dir.GetDirectories()) {
            CopyDirectory(subdir.FullName, destDir.Combine(subdir.Name).FullPath, copySubDirs);
        }
    }
}

// Get the target to run in subfolders
var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));

CakeStealer.CakeContext = Context;
CakeStealer.NuGetSources = NUGET_RESTORE_SOURCES;
CakeStealer.NugetToolPath = GetNugetToolPath ();
CakeStealer.XamarinComponentToolPath = GetXamarinComponentToolPath ();
CakeStealer.CakeToolPath = GetCakeToolPath ();
CakeStealer.NUnitConsoleToolPath = GetNUnitConsoleToolPath ();
CakeStealer.GenApiToolPath = GetGenApiToolPath ();

Information ("Cake.exe ToolPath: {0}", CakeStealer.CakeToolPath);
Information ("Cake.exe NUnitConsoleToolPath: {0}", CakeStealer.NUnitConsoleToolPath);
Information ("NuGet.exe ToolPath: {0}", CakeStealer.NugetToolPath);
Information ("Xamarin-Component.exe ToolPath: {0}", CakeStealer.XamarinComponentToolPath);
Information ("genapi.exe ToolPath: {0}", CakeStealer.GenApiToolPath);

void ListEnvironmentVariables ()
{
    Information ("Environment Variables:");
    foreach (var envVar in EnvironmentVariables ()) {
        Information ("\tKey: {0}\tValue: \"{1}\"", envVar.Key, envVar.Value);
    }
}
ListEnvironmentVariables ();

public class CakeStealer
{
	static public ICakeContext CakeContext { get; set; }
	static public string[] NuGetSources { get; set; }
	static public FilePath NugetToolPath { get;set; }
	static public FilePath XamarinComponentToolPath { get; set; }
	static public FilePath CakeToolPath { get;set; }
	static public FilePath NUnitConsoleToolPath { get;set; }
	static public FilePath GenApiToolPath { get;set; }
}

public interface ISolutionBuilder
{
	void BuildSolution ();
	void CopyOutput ();
}

public class OutputFileCopy
{
	public FilePath FromFile { get; set; }
	public DirectoryPath ToDirectory { get; set; }
	public FilePath NewFileName { get; set; }
}

public class DefaultSolutionBuilder : CakeStealer, ISolutionBuilder
{
	public DefaultSolutionBuilder ()
	{
		DefaultOutputDirectory = "./output";
		RestoreComponents = false;
		IsWindowsCompatible = false;
		IsMacCompatible = true;
		Platform = DefaultPlatform;
		Configuration = DefaultConfiguration;
		Properties = new Dictionary<string, List<string>> ();
	}

	public string SolutionPath {get; set;}
	public bool IsWindowsCompatible { get; set; }
	public bool IsMacCompatible { get; set; }
	public Dictionary<string, List<string>> Properties { get; set; }
	public IEnumerable<string> Targets { get; set; }

	public virtual string Configuration { get; set; }		
	protected virtual string DefaultConfiguration { get {return "Release"; } }

	public virtual string Platform { get; set; }
	protected virtual string DefaultPlatform { get {return "\"Any CPU\""; } }

	public OutputFileCopy [] OutputFiles { get; set; }
	public virtual string DefaultOutputDirectory { get; set; }
	public virtual bool RestoreComponents { get; set; }

	public Action PreBuildAction { get;set; }
	public Action PostBuildAction { get;set; }

	public virtual bool CanBuildOnPlatform {
		get {
			if (CakeContext.IsRunningOnWindows () && !IsWindowsCompatible)
				return false;
			if (CakeContext.IsRunningOnUnix () && !IsMacCompatible)
				return false;
			return true;
		}
	}

	public virtual void BuildSolution ()
	{		
		if (!CanBuildOnPlatform) {
			CakeContext.Information ("Solution is not configured to build on this platform: {0}", SolutionPath);
			return;
		}

		if (PreBuildAction != null)
			PreBuildAction ();

		if (RestoreComponents) {
			RunComponentRestore (SolutionPath);
		}

		RunNuGetRestore (SolutionPath);
		RunBuild (SolutionPath);

		if (PostBuildAction != null)
			PostBuildAction ();
	}

	public static void RunNuGetRestore (FilePath solution)
	{
		CakeContext.NuGetRestore (solution, new NuGetRestoreSettings { Source = NuGetSources, ToolPath = NugetToolPath }); 
	}

	public static void RunComponentRestore (FilePath solution)
	{		 
		CakeContext.RestoreComponents (solution, new XamarinComponentSettings {  
			ToolPath = XamarinComponentToolPath
		}); 
	}

	public virtual void RunBuild (FilePath solution)
	{
		CakeContext.DotNetBuild (solution, c => { 
			c.Configuration = Configuration; 
			if (!string.IsNullOrEmpty (Platform))
				c.Properties ["Platform"] = new [] { Platform }; 
			if (Targets != null && Targets.Any ()) {
				foreach (var t in Targets)
					c.Targets.Add (t);
			}			
			if (Properties != null && Properties.Any ()) {
				foreach (var kvp in Properties)
					c.Properties.Add (kvp.Key, kvp.Value);
			}
		}); 
	}

	public virtual void CopyOutput ()
	{
		if (OutputFiles == null)
			return;

		if (!CanBuildOnPlatform)
			return;

		foreach (var fileCopy in OutputFiles) {
			FilePath targetFileName;

			var targetDir = fileCopy.ToDirectory == null ? DefaultOutputDirectory :  fileCopy.ToDirectory;
			CakeContext.CreateDirectory (targetDir);

			if (fileCopy.NewFileName != null) {
				targetFileName = targetDir.CombineWithFilePath (fileCopy.NewFileName);
			} else {
				targetFileName = targetDir.CombineWithFilePath (fileCopy.FromFile.GetFilename ());	
			}
			
			var srcAbs = CakeContext.MakeAbsolute (fileCopy.FromFile);
			var destAbs = CakeContext.MakeAbsolute (targetFileName);

			var sourceTime = System.IO.File.GetLastAccessTime (srcAbs.ToString ());
			var destTime = System.IO.File.GetLastAccessTime (destAbs.ToString ());

			CakeContext.Information ("Target Dir: Exists? {0}, {1}", CakeContext.DirectoryExists (targetDir), targetDir);
			
			CakeContext.Information ("Copy From: Exists? {0}, Dir Exists? {1}, Modified: {2}, {3}", 
				CakeContext.FileExists (srcAbs), CakeContext.DirectoryExists (srcAbs.GetDirectory ()), sourceTime, srcAbs);
			CakeContext.Information ("Copy To:   Exists? {0}, Dir Exists? {1}, Modified: {2}, {3}", 
				CakeContext.FileExists (destAbs), CakeContext.DirectoryExists (destAbs.GetDirectory ()), destTime, destAbs);

			if (sourceTime > destTime || !CakeContext.FileExists (destAbs)) {
				CakeContext.Information ("Copying File: {0} to {1}", srcAbs, targetDir);
				CakeContext.CopyFileToDirectory (srcAbs, targetDir);
			}
		}
	}
}

public class IOSSolutionBuilder : DefaultSolutionBuilder
{
	protected override string DefaultPlatform { get { return "iPhone"; } }

	public override void RunBuild (FilePath solution)
	{ 
		if (!CanBuildOnPlatform) {
			CakeContext.Information ("Solution is not configured to build on this platform: {0}", SolutionPath);
			return;
		}

		if (CakeContext.IsRunningOnUnix ()) { 
			CakeContext.MDToolBuild (solution, c => {
                c.Configuration = Configuration;
            }); 
		} else { 
			base.RunBuild (solution); 
		} 
	} 
}

public class WpSolutionBuilder : DefaultSolutionBuilder
{
	public WpSolutionBuilder () : base ()
	{
		IsWindowsCompatible = true;
		IsMacCompatible = false;	
	}

	protected override string DefaultPlatform { get { return ""; } }

	public string WpPlatformTarget { get; set; }


	public override void RunBuild (FilePath solution)
	{
		if (!CanBuildOnPlatform) {
			CakeContext.Information ("Solution is not configured to build on this platform: {0}", SolutionPath);
			return;
		}

        var buildTargets = "";
        if (Targets != null) {
        	foreach (var t in Targets)
        		buildTargets += "/target:" + t + " ";        	
        }

        // We need to invoke MSBuild manually for now since Cake wants to set Platform=x86 if we use the x86 msbuild.exe version
        // and the amd64 msbuild.exe cannot be used to build windows phone projects
        // This should be fixable in cake 0.6.1
        var programFilesPath = CakeContext.Environment.GetSpecialPath(SpecialPath.ProgramFilesX86);
        var binPath = programFilesPath.Combine(string.Concat("MSBuild/", "14.0", "/Bin"));
        var msBuild = binPath.CombineWithFilePath("MSBuild.exe");

        if (!CakeContext.FileExists (msBuild)) {
        	binPath = programFilesPath.Combine(string.Concat("MSBuild/", "12.0", "/Bin"));
        	msBuild = binPath.CombineWithFilePath("MSBuild.exe");
        }

        CakeContext.StartProcess (msBuild, "/m /v:Normal /p:Configuration=Release " + buildTargets.Trim () + " \"" + CakeContext.MakeAbsolute (solution).ToString () + "\"");
	}
}

public class SolutionTestRunner : CakeStealer
{
    public DefaultSolutionBuilder SolutionBuilder { get; set; }
    
    public FilePath TestAssembly { get; set; }
    
    public DirectoryPath TestDiectory { get; set; }
    
    public void BuildSolution ()
    {
        SolutionBuilder.BuildSolution ();
		SolutionBuilder.CopyOutput ();
    }
    
    public void RunTests ()
    {
		if (!SolutionBuilder.CanBuildOnPlatform) {
			CakeContext.Information ("Solution is not configured to run test on this platform: {0}", SolutionBuilder.SolutionPath);
			return;
		}

        var dir = TestDiectory == null ? TestAssembly.GetDirectory() : TestDiectory;
        var result = CakeContext.StartProcess(NUnitConsoleToolPath, new ProcessSettings {
            Arguments = string.Format ("\"{0}\" --work=\"{1}\"", TestAssembly, dir),
        });
        
        if (result != 0) {
            throw new Exception ("NUnit test failed with error: " + result);
        }
    }
}

class CakeSpec
{
	static CakeSpec ()
	{
		Libs = new ISolutionBuilder [] {};
		Tests = new SolutionTestRunner [] {};
		Samples = new ISolutionBuilder [] {};
		NuSpecs = new string [] {};
	}

	static public ISolutionBuilder [] Libs { get; set; }
	static public SolutionTestRunner [] Tests { get; set; }
	static public ISolutionBuilder [] Samples { get; set; }
	static public string [] NuSpecs { get; set; }
}

void RunMake (DirectoryPath directory, string target = "all")
{
	StartProcess ("make", new ProcessSettings {
			Arguments = target,
			WorkingDirectory = directory,
		});
}

void RunInstallNameTool (DirectoryPath directory, string oldName, string newName, FilePath library)
{
	StartProcess ("install_name_tool", new ProcessSettings {
		Arguments = string.Format("-change {0} {1} \"{2}\"", oldName, newName, library),
		WorkingDirectory = directory,
	});
}

void RunLipo (DirectoryPath directory, FilePath output, params FilePath[] inputs)
{
	var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
	StartProcess ("lipo", new ProcessSettings {
		Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
		WorkingDirectory = directory,
	});
}

void RunLibtoolStatic (DirectoryPath directory, FilePath output, params FilePath[] inputs)
{
	var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
	StartProcess ("libtool", new ProcessSettings {
		Arguments = string.Format("-static -o \"{0}\" {1}", output, inputString),
		WorkingDirectory = directory,
	});
}

void BuildXCode (FilePath project, string target, string libraryTitle = null, FilePath fatLibrary = null, DirectoryPath workingDirectory = null)
{
	libraryTitle = libraryTitle ?? target;
    fatLibrary = fatLibrary ?? string.Format("lib{0}.a", libraryTitle);
	workingDirectory = workingDirectory ?? Directory("./externals/");

	var output = string.Format ("lib{0}.a", libraryTitle);
	var i386 = string.Format ("lib{0}-i386.a", libraryTitle);
	var x86_64 = string.Format ("lib{0}-x86_64.a", libraryTitle);
	var armv7 = string.Format ("lib{0}-armv7.a", libraryTitle);
	var armv7s = string.Format ("lib{0}-armv7s.a", libraryTitle);
	var arm64 = string.Format ("lib{0}-arm64.a", libraryTitle);
	
	var buildArch = new Action<string, string, FilePath> ((sdk, arch, dest) => {
		if (!FileExists (dest)) {
			XCodeBuild (new XCodeBuildSettings {
				Project = workingDirectory.CombineWithFilePath (project).ToString (),
				Target = target,
				Sdk = sdk,
				Arch = arch,
				Configuration = "Release",
			});
			var outputPath = workingDirectory.Combine ("build").Combine ("Release-" + sdk).CombineWithFilePath (output);
			CopyFile (outputPath, dest);
		}
	});
	
	buildArch ("iphonesimulator", "i386", workingDirectory.CombineWithFilePath (i386));
	buildArch ("iphonesimulator", "x86_64", workingDirectory.CombineWithFilePath (x86_64));
	
	buildArch ("iphoneos", "armv7", workingDirectory.CombineWithFilePath (armv7));
	buildArch ("iphoneos", "armv7s", workingDirectory.CombineWithFilePath (armv7s));
	buildArch ("iphoneos", "arm64", workingDirectory.CombineWithFilePath (arm64));
	
	RunLipo (workingDirectory,  fatLibrary,  i386, x86_64, armv7, armv7s, arm64);
}

void CleanXCodeBuild (DirectoryPath projectRoot = null, DirectoryPath workingDirectory = null)
{
	workingDirectory = workingDirectory ?? Directory ("./externals/");
	projectRoot = projectRoot ?? workingDirectory;
	
	if (DirectoryExists (workingDirectory.Combine ("build")))
		DeleteDirectory (workingDirectory.Combine ("build"), true);
		
	if (DirectoryExists (workingDirectory.Combine (projectRoot)))
		DeleteDirectory (workingDirectory.Combine (projectRoot), true);
	
	DeleteFiles (System.IO.Path.Combine (workingDirectory.ToString (), "*.a"));
}

void CreatePodfile (DirectoryPath podfilePath, string platform, string platformVersion, IDictionary<string, string> pods)
{
	var builder = new StringBuilder ();
	builder.AppendFormat ("platform :{0}, '{1}'", platform, platformVersion);
	builder.AppendLine ();
	foreach (var pod in pods) {
	    builder.AppendFormat ("pod '{0}', '{1}'", pod.Key, pod.Value);
	    builder.AppendLine ();
	}
	
	if (!DirectoryExists (podfilePath)) {
		CreateDirectory (podfilePath);
	}
	
	System.IO.File.WriteAllText (podfilePath.CombineWithFilePath ("Podfile").ToString (), builder.ToString ());
}

void BuildNuGets (string outputPath)
{
	// NuGet messes up path on mac, so let's add ./ in front twice
	var basePath = IsRunningOnUnix () ? "././" : "./";

	if (!DirectoryExists (outputPath)) {
		CreateDirectory (outputPath);
	}

	foreach (var n in CakeSpec.NuSpecs) {
		NuGetPack (n, new NuGetPackSettings { 
			Verbosity = NuGetVerbosity.Detailed,
			OutputDirectory = outputPath,		
			BasePath = basePath,
			ToolPath = CakeStealer.NugetToolPath
		});				
	}
}

void DefineDefaultTasks ()
{
	Task ("libs-base").Does (() => 
	{
		foreach (var l in CakeSpec.Libs) {
			l.BuildSolution ();
			l.CopyOutput ();
		}
	});

	if (!Tasks.Where (tsk => tsk.Name == "libs").Any ())
	{
		Task ("libs").IsDependentOn ("externals").IsDependentOn ("libs-base");
	}

	Task ("tests-base").Does (() => 
	{
		foreach (var t in CakeSpec.Tests) {
			t.BuildSolution ();
			t.RunTests ();
		}	
	});

	if (!Tasks.Where (tsk => tsk.Name == "tests").Any ())
	{
		Task ("tests").IsDependentOn ("libs").IsDependentOn ("tests-base");
	}

	Task ("samples-base").Does (() => 
	{
		foreach (var s in CakeSpec.Samples) {
			s.BuildSolution ();
			s.CopyOutput ();
		}	
	});

	if (!Tasks.Where (tsk => tsk.Name == "samples").Any ())
	{
		Task ("samples").IsDependentOn ("libs").IsDependentOn ("samples-base");
	}

	Task ("nuget-base").Does (() => 
	{
		BuildNuGets ("./output");
	});

	if (!Tasks.Where (tsk => tsk.Name == "nuget").Any ())
	{
		Task ("nuget").IsDependentOn ("libs").IsDependentOn ("nuget-base");
	}

	Task ("component-base").IsDependentOn ("nuget").Does (() => 
	{
		// Clear out existing .xam files
		if (!DirectoryExists ("./output/"))
			CreateDirectory ("./output/");
		DeleteFiles ("./output/*.xam");

		// Look for all the component.yaml files to build
		var componentYamls = GetFiles ("./**/component.yaml");
		foreach (var yaml in componentYamls) {
			var yamlDir = yaml.GetDirectory ();

			PackageComponent (yamlDir, new XamarinComponentSettings { 
				ToolPath = CakeStealer.XamarinComponentToolPath
			});

			MoveFiles (yamlDir.FullPath.TrimEnd ('/') + "/*.xam", "./output/");
		}
	});

	if (!Tasks.Where (tsk => tsk.Name == "component").Any ())
	{
		Task ("component").IsDependentOn ("nuget").IsDependentOn ("component-base");
	}

	Task ("externals-base");

	if (!Tasks.Where (tsk => tsk.Name == "externals").Any ())
	{
		Task ("externals").IsDependentOn ("externals-base");
	}

	Task ("clean-base").Does (() => 
	{
		CleanDirectories ("./**/bin");
		CleanDirectories ("./**/obj");

		if (DirectoryExists ("./output"))
			DeleteDirectory ("./output", true);
	});

	if (!Tasks.Where (tsk => tsk.Name == "clean").Any ())
	{
		Task ("clean").IsDependentOn ("clean-base");
	}

	if (!Tasks.Where (tsk => tsk.Name == "Default").Any ())
	{
		Task ("Default").IsDependentOn ("libs");
	}
}


