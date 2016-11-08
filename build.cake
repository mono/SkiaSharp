#addin "Cake.Xamarin"
#addin "Cake.XCode"
#addin "Cake.FileHelpers"

#load "cake/Utils.cake"

using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));

var NuGetSources = new [] { MakeAbsolute (Directory ("./output")).FullPath, "https://api.nuget.org/v3/index.json" };
var NugetToolPath = GetToolPath ("nuget.exe");
var XamarinComponentToolPath = GetToolPath ("xamarin-component.exe");
var CakeToolPath = GetToolPath ("Cake/Cake.exe");
var NUnitConsoleToolPath = GetToolPath ("NUnit.Console/tools/nunit3-console.exe");
var GenApiToolPath = GetToolPath ("Microsoft.DotNet.BuildTools.GenAPI/tools/GenAPI.exe");
var MDocPath = GetToolPath ("mdoc/mdoc.exe");

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath ANGLE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/angle"));
DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/en"));

#load "cake/UtilsManaged.cake"
#load "cake/UtilsMSBuild.cake"
#load "cake/UtilsNative.cake"
#load "cake/TransformToTvOS.cake"
#load "cake/TransformToUWP.cake"
#load "cake/BuildExternals.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .IsDependentOn ("externals-genapi")
    .IsDependentOn ("externals-native")
    .Does (() => 
{
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - the managed C# libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .IsDependentOn ("externals")
    .IsDependentOn ("set-versions")
    .Does (() => 
{
    // set the SHA on the assembly info 
    var sha = EnvironmentVariable ("GIT_COMMIT") ?? string.Empty;
    if (!string.IsNullOrEmpty (sha) && sha.Length >= 6) {
        sha = sha.Substring (0, 6);
        Information ("Setting Git SHA to {0}.", sha);
        ReplaceTextInFiles ("./binding/Binding/Properties/SkiaSharpAssemblyInfo.cs", "{GIT_SHA}", sha);
        ReplaceTextInFiles ("./source/SkiaSharp.Views/SkiaSharp.Views.Shared/Properties/SkiaSharpViewsAssemblyInfo.cs", "{GIT_SHA}", sha);
        ReplaceTextInFiles ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Shared/Properties/SkiaSharpViewsFormsAssemblyInfo.cs", "{GIT_SHA}", sha);
        ReplaceTextInFiles ("./source/SkiaSharp.Svg/SkiaSharp.Svg/Properties/SkiaSharpSvgAssemblyInfo.cs", "{GIT_SHA}", sha);
    }

    // create all the directories
    if (!DirectoryExists ("./output/windows/")) CreateDirectory ("./output/windows/");
    if (!DirectoryExists ("./output/uwp/")) CreateDirectory ("./output/uwp/");
    if (!DirectoryExists ("./output/android/")) CreateDirectory ("./output/android/");
    if (!DirectoryExists ("./output/ios/")) CreateDirectory ("./output/ios/");
    if (!DirectoryExists ("./output/tvos/")) CreateDirectory ("./output/tvos/");
    if (!DirectoryExists ("./output/osx/")) CreateDirectory ("./output/osx/");
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/mac/")) CreateDirectory ("./output/mac/");

    if (IsRunningOnWindows ()) {
        // build bindings
        RunNuGetRestore ("binding/SkiaSharp.Windows.sln");
        DotNetBuild ("binding/SkiaSharp.Windows.sln", c => { 
            c.Configuration = "Release"; 
        });

        // copy build output
        CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.pdb", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll.config", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.Desktop.targets", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.dll", "./output/uwp/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.pdb", "./output/uwp/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.pri", "./output/uwp/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.UWP.targets", "./output/uwp/");

        // build other source
        RunNuGetRestore ("./source/SkiaSharpSource.Windows.sln");
        DotNetBuild ("./source/SkiaSharpSource.Windows.sln", c => { 
            c.Configuration = "Release"; 
        });

        // copy the managed views
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.UWP/bin/Release/SkiaSharp.Views.UWP.dll", "./output/uwp/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.UWP/bin/Release/SkiaSharp.Views.UWP.targets", "./output/uwp/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Desktop/bin/Release/SkiaSharp.Views.Desktop.dll", "./output/windows/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.WPF/bin/Release/SkiaSharp.Views.WPF.dll", "./output/windows/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms/bin/Release/SkiaSharp.Views.Forms.dll", "./output/portable/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.UWP/bin/Release/SkiaSharp.Views.Forms.dll", "./output/uwp/");

        // copy SVG
        CopyFileToDirectory ("./source/SkiaSharp.Svg/SkiaSharp.Svg/bin/Release/SkiaSharp.Svg.dll", "./output/portable/");
    }

    if (IsRunningOnUnix ()) {
        // build
        RunNuGetRestore ("binding/SkiaSharp.Mac.sln");
        DotNetBuild ("binding/SkiaSharp.Mac.sln", c => { 
            c.Configuration = "Release"; 
        });

        // copy build output
        CopyFileToDirectory ("./binding/SkiaSharp.Android/bin/Release/SkiaSharp.dll", "./output/android/");
        CopyFileToDirectory ("./binding/SkiaSharp.iOS/bin/Release/SkiaSharp.dll", "./output/ios/");
        CopyFileToDirectory ("./binding/SkiaSharp.tvOS/bin/Release/SkiaSharp.dll", "./output/tvos/");
        CopyFileToDirectory ("./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.dll", "./output/osx/");
        CopyFileToDirectory ("./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.OSX.targets", "./output/osx/");
        CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/mac/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.Desktop.targets", "./output/mac/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll.config", "./output/mac/");

        // build other source
        RunNuGetRestore ("./source/SkiaSharpSource.Mac.sln");
        DotNetBuild ("./source/SkiaSharpSource.Mac.sln", c => { 
            c.Configuration = "Release"; 
        });

        // copy other outputs
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Android/bin/Release/SkiaSharp.Views.Android.dll", "./output/android/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.iOS/bin/Release/SkiaSharp.Views.iOS.dll", "./output/ios/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.tvOS/bin/Release/SkiaSharp.Views.tvOS.dll", "./output/tvos/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Mac/bin/Release/SkiaSharp.Views.Mac.dll", "./output/osx/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms/bin/Release/SkiaSharp.Views.Forms.dll", "./output/portable/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Android/bin/Release/SkiaSharp.Views.Forms.dll", "./output/android/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.iOS/bin/Release/SkiaSharp.Views.Forms.dll", "./output/ios/");

        // copy SVG
        CopyFileToDirectory ("./source/SkiaSharp.Svg/SkiaSharp.Svg/bin/Release/SkiaSharp.Svg.dll", "./output/portable/");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - some test cases to make sure it works
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .IsDependentOn ("libs")
    .Does (() => 
{
    RunNuGetRestore ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
    
    // Windows (x86 and x64)
    if (IsRunningOnWindows ()) {
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x86" };
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/SkiaSharp.Desktop.Tests.dll");
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x64" };
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/SkiaSharp.Desktop.Tests.dll");
    }
    // Mac OSX (Any CPU)
    if (IsRunningOnUnix ()) {
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/SkiaSharp.Desktop.Tests.dll");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - the demo apps showing off the work
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .Does (() => 
{
    ClearSkiaSharpNuGetCache ();

    // zip the samples for the GitHub release notes
    if (TARGET == "CI") {
        Zip ("./samples", "./output/samples.zip");
    }

    if (IsRunningOnUnix ()) {
        RunNuGetRestore ("./samples/MacSample/MacSample.sln");
        DotNetBuild ("./samples/MacSample/MacSample.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunNuGetRestore ("./samples/FormsSample/FormsSample.Mac.sln");
        DotNetBuild ("./samples/FormsSample/FormsSample.Mac.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "iPhone" };
        });
        RunNuGetRestore ("./samples/TvSample/TvSample.sln");
        DotNetBuild ("./samples/TvSample/TvSample.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "iPhoneSimulator" };
        });
    }
    
    if (IsRunningOnWindows ()) {
        RunNuGetRestore ("./samples/WPFSample/WPFSample.sln");
        DotNetBuild ("./samples/WPFSample/WPFSample.sln", c => { 
            c.Configuration = "Release";
            c.Properties ["Platform"] = new [] { "x86" };
        });
        RunNuGetRestore ("./samples/UWPSample/UWPSample.sln");
        DotNetBuild ("./samples/UWPSample/UWPSample.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunNuGetRestore ("./samples/FormsSample/FormsSample.Windows.sln");
        DotNetBuild ("./samples/FormsSample/FormsSample.Windows.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunNuGetRestore ("./samples/WindowsSample/WindowsSample.sln");
        DotNetBuild ("./samples/WindowsSample/WindowsSample.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x86" };
        });
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DOCS - building the API documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("docs")
    .Does (() => 
{
    // log TODOs
    var docFiles = GetFiles ("./docs/**/*.xml");
    float typeCount = 0;
    float memberCount = 0;
    float totalTypes = 0;
    float totalMembers = 0;
    foreach (var file in docFiles) {
        var xdoc = XDocument.Load (file.ToString ());

        var typesWithDocs = xdoc.Root
            .Elements ("Docs");

        totalTypes += typesWithDocs.Count ();
        var currentTypeCount = typesWithDocs.Where (m => m.Value != null && m.Value.IndexOf ("To be added.") >= 0).Count (); 
        typeCount += currentTypeCount;

        var membersWithDocs = xdoc.Root
            .Elements ("Members")
            .Elements ("Member")
            .Where (m => m.Attribute ("MemberName") != null && m.Attribute ("MemberName").Value != "Dispose"  && m.Attribute ("MemberName").Value != "Finalize")
            .Elements ("Docs");

        totalMembers += membersWithDocs.Count ();
        var currentMemberCount = membersWithDocs.Where (m => m.Value != null && m.Value.IndexOf ("To be added.") >= 0).Count ();
        memberCount += currentMemberCount;

        currentMemberCount += currentTypeCount;
        if (currentMemberCount > 0) {
            var fullName = xdoc.Root.Attribute ("FullName");
            if (fullName != null)
                Information ("Docs missing on {0} = {1}", fullName.Value, currentMemberCount);
        }
    }
    Information (
        "Documentation missing in {0}/{1} ({2:0.0%}) types and {3}/{4} ({5:0.0%}) members.", 
        typeCount, totalTypes, typeCount / totalTypes, 
        memberCount, totalMembers, memberCount / totalMembers);

    if (!DirectoryExists ("./output/docs/msxml/")) CreateDirectory ("./output/docs/msxml/");
    RunMdocMSXml (DOCS_PATH, "./output/docs/msxml/");
    
    if (!DirectoryExists ("./output/docs/mdoc/")) CreateDirectory ("./output/docs/mdoc/");
    RunMdocAssemble (DOCS_PATH, "./output/docs/mdoc/SkiaSharp");

    CopyFileToDirectory ("./docs/SkiaSharp.source", "./output/docs/mdoc/");
});

// we can only update the docs on the platform machines
// becuase each requires platform features for the views 
Task ("update-docs")
    .IsDependentOn ("libs")
    .Does (() => 
{
    // the reference folders to locate assemblies
    IEnumerable<DirectoryPath> refs = new DirectoryPath [] {
            // you never know
        }
        .Union (GetDirectories ("./source/packages/Xamarin.Forms.*/lib/portable*"))
        .Union (GetDirectories ("./source/packages/OpenTK.*/lib/net40*"));
    // add windows-specific references
    if (IsRunningOnWindows ()) {
        // Windows.Foundation.UniversalApiContract is a winmd, so fake the dll
        // types aren't needed here
        DotNetBuild ("./externals/Windows.Foundation.UniversalApiContract/Windows.Foundation.UniversalApiContract.csproj", c => {
            c.Verbosity = Verbosity.Quiet;
        });
        refs = refs.Union (new DirectoryPath [] {
            "./externals/Windows.Foundation.UniversalApiContract/bin/Release",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/MonoAndroid/v1.0",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/MonoAndroid/v2.3",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Xamarin.iOS/v1.0",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Xamarin.TVOS/v1.0",
            "./externals",
        });
    }
    // add mac-specific references
    if (IsRunningOnUnix ()) {
        refs = refs.Union (new DirectoryPath [] {
            "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild-frameworks/.NETPortable/v4.5",
            "/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xbuild-frameworks/MonoAndroid/v1.0",
            "/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xbuild-frameworks/MonoAndroid/v4.5",
            "/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/Xamarin.TVOS",
            "/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/Xamarin.iOS",
            "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/Xamarin.Mac",
        });
    }

    // the assemblies to generate docs for
    var assemblies = new FilePath [] {
        "./output/portable/SkiaSharp.dll",
        "./output/portable/SkiaSharp.Views.Forms.dll",
    };
    // add windows-specific assemblies
    if (IsRunningOnWindows ()) {
        assemblies = assemblies.Union (new FilePath [] {
            "./output/windows/SkiaSharp.Views.Desktop.dll",
            "./output/windows/SkiaSharp.Views.WPF.dll",
            "./output/uwp/SkiaSharp.Views.UWP.dll",
            "./output/android/SkiaSharp.Views.Android.dll",
            "./output/ios/SkiaSharp.Views.iOS.dll",
            "./output/osx/SkiaSharp.Views.Mac.dll",
            "./output/tvos/SkiaSharp.Views.tvOS.dll",
        }).ToArray ();
    }
    // add mac-specific assemblies
    if (IsRunningOnUnix ()) {
        assemblies = assemblies.Union (new FilePath [] {
            "./output/android/SkiaSharp.Views.Android.dll",
            "./output/ios/SkiaSharp.Views.iOS.dll",
            "./output/osx/SkiaSharp.Views.Mac.dll",
            "./output/tvos/SkiaSharp.Views.tvOS.dll",
        }).ToArray ();
    }

    // print out the assemblies
    foreach (var r in refs) {
        Information ("Reference Directory: {0}", r);
    }
    foreach (var a in assemblies) {
        Information ("Processing {0}...", a);
    }

    // generate doc files
    RunMdocUpdate (assemblies, DOCS_PATH, refs.ToArray ());

    // apply some formatting
    var docFiles = GetFiles ("./docs/**/*.xml");
    foreach (var file in docFiles) {

        var xdoc = XDocument.Load (file.ToString ());

        // remove IComponent docs as this is just designer
        var icomponents = xdoc.Root
            .Elements ("Members")
            .Elements ("Member")
            .Where (e => e.Attribute ("MemberName") != null && e.Attribute ("MemberName").Value.StartsWith ("System.ComponentModel.IComponent."))
            .ToArray ();
        foreach (var ic in icomponents) {
            Information ("Removing IComponent member '{0}' from '{1}'...", ic.Attribute ("MemberName").Value, file);
            icomponents.Remove ();
        }

        // get the whitespaces right
        var settings = new XmlWriterSettings {
            Encoding = new UTF8Encoding (),
            Indent = true,
            NewLineChars = "\n",
            OmitXmlDeclaration = true,
        };
        using (var writer = XmlWriter.Create (file.ToString (), settings)) {
            xdoc.Save (writer);
            writer.Flush ();
        }

        // empty line at the end
        System.IO.File.AppendAllText (file.ToString (), "\n");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - building the package for NuGet.org
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .Does (() => 
{
    // we can only build the combined package on CI
    if (TARGET == "CI") {
        PackageNuGet ("./nuget/SkiaSharp.nuspec", "./output/");
        PackageNuGet ("./nuget/SkiaSharp.Views.nuspec", "./output/");
        PackageNuGet ("./nuget/SkiaSharp.Views.Forms.nuspec", "./output/");
    } else {
        if (IsRunningOnWindows ()) {
            PackageNuGet ("./nuget/SkiaSharp.Windows.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Windows.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Forms.Windows.nuspec", "./output/");
        }
        if (IsRunningOnUnix ()) {
            PackageNuGet ("./nuget/SkiaSharp.Mac.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Mac.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Forms.Mac.nuspec", "./output/");
        }
    }
    // SVG is a PCL
    PackageNuGet ("./nuget/SkiaSharp.Svg.nuspec", "./output/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// COMPONENT - building the package for components.xamarin.com
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("component")
    .IsDependentOn ("nuget")
    .Does (() => 
{
    // TODO: Not yet ready
    
    // if (!DirectoryExists ("./output/")) {
    //     CreateDirectory ("./output/");
    // }
    
    // FilePath yaml = "./component/component.yaml";
    // var yamlDir = yaml.GetDirectory ();
    // PackageComponent (yamlDir, new XamarinComponentSettings { 
    //     ToolPath = XamarinComponentToolPath
    // });

    // MoveFiles (yamlDir.FullPath.TrimEnd ('/') + "/*.xam", "./output/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// VERSIONS - update all packages and references to the new version
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("set-versions")
    .Does (() => 
{
    // set the SHA on the assembly info 
    var sha = EnvironmentVariable ("GIT_COMMIT") ?? string.Empty;
    if (!string.IsNullOrEmpty (sha) && sha.Length >= 6) {
        sha = sha.Substring (0, 6);
    } else {
        sha = "{GIT_SHA}";
    }

    // the versions
    var version = "1.55.0.0";
    var fileVersion = "1.55.0.0";
    var versions = new Dictionary<string, string> {
        { "SkiaSharp", "1.55.0" },
        { "SkiaSharp.Views", "1.55.0" },
        { "SkiaSharp.Views.Forms", "1.55.0" },
        { "SkiaSharp.Svg", "1.55.0-beta1" },
    };

    var files = new List<string> ();
    var add = new Action<string> (glob => {
        files.AddRange (GetFiles (glob).Select (p => MakeAbsolute (p).ToString ()));
    });
    // nuspecs
    add ("./nuget/*.nuspec");
    // packages files
    add ("./source/*/*/packages.config");
    add ("./source/*/*/project.json");
    // project files
    add ("./source/*/*/*.nuget.targets");
    add ("./source/*/*/*.csproj");
    // sample packages files
    add ("./samples/*/*/packages.config");
    add ("./samples/*/*/project.json");
    // sample project files
    add ("./samples/*/*/*.nuget.targets");
    add ("./samples/*/*/*.csproj");
    // update
    foreach (var file in files) {
        UpdateSkiaSharpVersion (file, versions);
    }

    // assembly infos
    UpdateAssemblyInfo (
        "./binding/Binding/Properties/SkiaSharpAssemblyInfo.cs",
        version, fileVersion, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Views/SkiaSharp.Views.Shared/Properties/SkiaSharpViewsAssemblyInfo.cs",
        version, fileVersion, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Shared/Properties/SkiaSharpViewsFormsAssemblyInfo.cs",
        version, fileVersion, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Svg/SkiaSharp.Svg/Properties/SkiaSharpSvgAssemblyInfo.cs",
        version, fileVersion, sha);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .IsDependentOn ("clean-externals")
    .IsDependentOn ("clean-managed")
    .Does (() => 
{
});
Task ("clean-managed").Does (() => 
{
    CleanDirectories ("./binding/*/bin");
    CleanDirectories ("./binding/*/obj");

    CleanDirectories ("./samples/*/bin");
    CleanDirectories ("./samples/*/obj");
    CleanDirectories ("./samples/*/AppPackages");
    CleanDirectories ("./samples/*/*/bin");
    CleanDirectories ("./samples/*/*/obj");
    CleanDirectories ("./samples/*/*/AppPackages");
    CleanDirectories ("./samples/*/packages");

    CleanDirectories ("./tests/**/bin");
    CleanDirectories ("./tests/**/obj");

    CleanDirectories ("./source/*/*/bin");
    CleanDirectories ("./source/*/*/obj");
    CleanDirectories ("./source/*/*/Generated Files");
    CleanDirectories ("./source/packages");

    CleanDirectories ("./externals/Windows.Foundation.UniversalApiContract/bin");
    CleanDirectories ("./externals/Windows.Foundation.UniversalApiContract/obj");

    if (DirectoryExists ("./output"))
        DeleteDirectory ("./output", true);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs");

Task ("Everything")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("component")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CI - the master target to build everything
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("CI")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("component")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

Task ("Windows-CI")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("component")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW 
////////////////////////////////////////////////////////////////////////////////////////////////////

Information ("Cake.exe ToolPath: {0}", CakeToolPath);
Information ("Cake.exe NUnitConsoleToolPath: {0}", NUnitConsoleToolPath);
Information ("NuGet.exe ToolPath: {0}", NugetToolPath);
Information ("Xamarin-Component.exe ToolPath: {0}", XamarinComponentToolPath);
Information ("genapi.exe ToolPath: {0}", GenApiToolPath);

ListEnvironmentVariables ();

RunTarget (TARGET);
