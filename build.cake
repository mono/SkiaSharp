#addin "Cake.Xamarin"
#addin "Cake.XCode"

#load "common.cake"

using System.Xml;
using System.Xml.Linq;

var ROOT_PATH = MakeAbsolute(Directory("."));
var DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("depot_tools"));
var SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("skia"));

void RunGyp ()
{
    StartProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.Combine("bin").CombineWithFilePath("sync-and-gyp").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
}


Task ("externals")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
    .IsDependentOn ("externals-ios")
    .IsDependentOn ("externals-android")
    .Does (() => 
{
});

Task ("externals-windows").Does (() => 
{
    var fixup = new Action (() => {
        var props = SKIA_PATH.Combine ("out/gyp/libjpeg-turbo.props").FullPath;
        var xdoc = XDocument.Load (props);
        var ns = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";
        var temp = xdoc.Root
            .Elements (ns + "ItemDefinitionGroup")
            .Elements (ns + "assemble")
            .Elements (ns + "CommandLineTemplate")
            .Single ();
        var newInclude = SKIA_PATH.Combine ("third_party/externals/libjpeg-turbo/win/").FullPath;
        if (!temp.Value.Contains (newInclude)) {
            temp.Value += " \"-I" + newInclude + "\"";
            xdoc.Save (props);
        }
    });

    // set up the gyp environment variables
    var oldPathVar = Environment.GetEnvironmentVariable("PATH");
    try {
        Environment.SetEnvironmentVariable ("PATH", oldPathVar + ";" + DEPOT_PATH, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable ("GYP_GENERATORS", "ninja,msvs", EnvironmentVariableTarget.Process);
        
        // build the x86 vesion
        Environment.SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='x86'", EnvironmentVariableTarget.Process);
        RunGyp ();
        fixup ();
        DotNetBuild ("native-builds/libskia_windows/libskia_windows_x86.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "Win32" };
        });
        CreateDirectory ("native-builds/lib/windows/x86");
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.lib", "native-builds/lib/windows/x86");
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.dll", "native-builds/lib/windows/x86");
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.pdb", "native-builds/lib/windows/x86");
        
        // build the x64 vesion
        Environment.SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='x86_64'", EnvironmentVariableTarget.Process);
        RunGyp ();
        fixup ();
        DotNetBuild ("native-builds/libskia_windows/libskia_windows_x64.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x64" };
        });
        if (!DirectoryExists ("native-builds/lib/windows/x64")) {
            CreateDirectory ("native-builds/lib/windows/x64");
        }
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.lib", "native-builds/lib/windows/x64");
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.dll", "native-builds/lib/windows/x64");
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.pdb", "native-builds/lib/windows/x64");
    } finally {    
        // unset all environment variables
        Environment.SetEnvironmentVariable ("PATH", oldPathVar, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable ("GYP_DEFINES", "", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable ("GYP_GENERATORS", "", EnvironmentVariableTarget.Process);
    }
});
Task ("externals-osx").Does (() => 
{
});
Task ("externals-ios").Does (() => 
{
});
Task ("externals-android").Does (() => 
{
});


DefineDefaultTasks ();

RunTarget (TARGET);
