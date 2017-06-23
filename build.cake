#addin "Cake.Xamarin"
#addin "Cake.XCode"
#addin "Cake.FileHelpers"
#reference "tools/SharpCompress/lib/net45/SharpCompress.dll"

using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

#load "cake/Utils.cake"

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));
var VERBOSITY = (Verbosity) Enum.Parse (typeof(Verbosity), Argument ("v", Argument ("verbosity", Argument ("Verbosity", "Verbose"))), true);

var NuGetSources = new [] { MakeAbsolute (Directory ("./output")).FullPath, "https://api.nuget.org/v3/index.json" };
var NugetToolPath = GetToolPath ("nuget.exe");
var XamarinComponentToolPath = GetToolPath ("XamarinComponent/tools/xamarin-component.exe");
var CakeToolPath = GetToolPath ("Cake/Cake.exe");
var NUnitConsoleToolPath = GetToolPath ("NUnit.ConsoleRunner/tools/nunit3-console.exe");
var GenApiToolPath = GetToolPath ("Microsoft.DotNet.BuildTools.GenAPI/tools/GenAPI.exe");
var MDocPath = MakeAbsolute ((FilePath)"externals/api-doc-tools/bin/Release/mdoc.exe");
var SNToolPath = GetSNToolPath (EnvironmentVariable ("SN_EXE"));
var MSBuildToolPath = GetMSBuildToolPath (EnvironmentVariable ("MSBUILD_EXE"));

var VERSION_ASSEMBLY = "1.58.0.0";
var VERSION_FILE = "1.58.0.0";
var VERSION_SONAME = VERSION_FILE.Substring(VERSION_FILE.IndexOf(".") + 1);

var ANGLE_VERSION_SOURCE = "2.1.13";

var HARFBUZZ_VERSION_SOURCE = "1.4.6";
var HARFBUZZ_VERSION_ASSEMBLY = "1.0.0.0";
var HARFBUZZ_VERSION_FILE = "1.4.6.0";
var HARFBUZZ_VERSION_SONAME = HARFBUZZ_VERSION_FILE.Substring(0, HARFBUZZ_VERSION_FILE.LastIndexOf("."));

var VERSION_PACKAGES = new Dictionary<string, string> {
    { "SkiaSharp", "1.58.0" },
    { "SkiaSharp.Views", "1.58.0" },
    { "SkiaSharp.Views.Forms", "1.58.0" },
    { "SkiaSharp.HarfBuzz", "1.58.0-beta" },

    { "HarfBuzzSharp", "1.4.6" },
};

var CI_TARGETS = new string[] { "CI", "WINDOWS-CI", "LINUX-CI", "MAC-CI" };
var IS_ON_CI = CI_TARGETS.Contains (TARGET.ToUpper ());
var IS_ON_FINAL_CI = TARGET.ToUpper () == "CI";

string ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-sdk-macosx";
string ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? ANDROID_HOME;
string ANDROID_NDK_HOME = EnvironmentVariable ("ANDROID_NDK_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-ndk";

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath ANGLE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/angle"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/harfbuzz"));
DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/en"));

#load "cake/UtilsManaged.cake"
#load "cake/UtilsMSBuild.cake"
#load "cake/UtilsNative.cake"
#load "cake/BuildExternals.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
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
    // create all the directories
    if (!DirectoryExists ("./output/windows/")) CreateDirectory ("./output/windows/");
    if (!DirectoryExists ("./output/uwp/")) CreateDirectory ("./output/uwp/");
    if (!DirectoryExists ("./output/android/")) CreateDirectory ("./output/android/");
    if (!DirectoryExists ("./output/ios/")) CreateDirectory ("./output/ios/");
    if (!DirectoryExists ("./output/tvos/")) CreateDirectory ("./output/tvos/");
    if (!DirectoryExists ("./output/osx/")) CreateDirectory ("./output/osx/");
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/mac/")) CreateDirectory ("./output/mac/");
    if (!DirectoryExists ("./output/netstandard/")) CreateDirectory ("./output/netstandard/");
    if (!DirectoryExists ("./output/linux/")) CreateDirectory ("./output/linux/");
    if (!DirectoryExists ("./output/interactive/")) CreateDirectory ("./output/interactive/");

    // .NET Standard / .NET Core
    var netStandardSln = new [] { 
        "binding/SkiaSharp.NetStandard.sln", 
        "binding/HarfBuzzSharp.NetStandard.sln", 
        "source/SkiaSharpSource.NetStandard.sln" 
    };
    foreach (var sln in netStandardSln) {
        RunNuGetRestore (sln);
        RunMSBuild (sln);
    }
    // TODO: remove this nonsense !!!
    // Assembly signing is not supported on non-Windows ???, so we MUST NOT use the output
    // See: https://github.com/dotnet/roslyn/issues/8210
    if (!IS_ON_CI || IsRunningOnWindows ()) {
        CopyFileToDirectory ("./binding/SkiaSharp.NetStandard/bin/Release/SkiaSharp.dll", "./output/netstandard/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.NetStandard/bin/Release/HarfBuzzSharp.dll", "./output/netstandard/");
        CopyFileToDirectory ("./source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz.NetStandard/bin/Release/SkiaSharp.HarfBuzz.dll", "./output/netstandard/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.NetStandard/bin/Release/SkiaSharp.Views.Forms.dll", "./output/netstandard/");
    }

    // Generate the portable code - we can't do it automatically as there are issues on linux
    RunGenApi ("./binding/SkiaSharp.NetStandard/bin/Release/SkiaSharp.dll");
    RunGenApi ("./binding/HarfBuzzSharp.NetStandard/bin/Release/HarfBuzzSharp.dll");

    // .NET Framework / Xamarin
    if (IsRunningOnWindows ()) {
        RunNuGetRestore ("./source/SkiaSharpSource.Windows.sln");
        RunMSBuild ("./source/SkiaSharpSource.Windows.sln");
        // SkiaSharp
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/nuget/build/net45/SkiaSharp.dll.config", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/nuget/build/net45/SkiaSharp.Desktop.targets", "./output/windows/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.dll", "./output/uwp/");
        CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.pri", "./output/uwp/");
        // HarfBuzzSharp
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/HarfBuzzSharp.dll", "./output/windows/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/nuget/build/net45/HarfBuzzSharp.dll.config", "./output/windows/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/nuget/build/net45/HarfBuzzSharp.Desktop.targets", "./output/windows/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.UWP/bin/Release/HarfBuzzSharp.dll", "./output/uwp/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.UWP/bin/Release/HarfBuzzSharp.pri", "./output/uwp/");
        // SkiaSharp.Views
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.UWP/bin/Release/SkiaSharp.Views.UWP.dll", "./output/uwp/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Desktop/bin/Release/SkiaSharp.Views.Desktop.dll", "./output/windows/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.WPF/bin/Release/SkiaSharp.Views.WPF.dll", "./output/windows/");
        // SkiaSharp.Views.Forms
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms/bin/Release/SkiaSharp.Views.Forms.dll", "./output/portable/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.UWP/bin/Release/SkiaSharp.Views.Forms.dll", "./output/uwp/");
    } else if (IsRunningOnMac ()) {
        RunNuGetRestore ("binding/SkiaSharp.Mac.sln");
        RunMSBuild ("binding/SkiaSharp.Mac.sln");
        // SkiaSharp
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/mac/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/nuget/build/net45/SkiaSharp.dll.config", "./output/mac/");
        CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/nuget/build/net45/SkiaSharp.Desktop.targets", "./output/mac/");
        CopyFileToDirectory ("./binding/SkiaSharp.Android/bin/Release/SkiaSharp.dll", "./output/android/");
        CopyFileToDirectory ("./binding/SkiaSharp.iOS/bin/Release/SkiaSharp.dll", "./output/ios/");
        CopyFileToDirectory ("./binding/SkiaSharp.tvOS/bin/Release/SkiaSharp.dll", "./output/tvos/");
        CopyFileToDirectory ("./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.dll", "./output/osx/");
        // HarfBuzzSharp
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/HarfBuzzSharp.dll", "./output/mac/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/nuget/build/net45/HarfBuzzSharp.dll.config", "./output/mac/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Desktop/bin/Release/nuget/build/net45/HarfBuzzSharp.Desktop.targets", "./output/mac/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.Android/bin/Release/HarfBuzzSharp.dll", "./output/android/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.iOS/bin/Release/HarfBuzzSharp.dll", "./output/ios/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.tvOS/bin/Release/HarfBuzzSharp.dll", "./output/tvos/");
        CopyFileToDirectory ("./binding/HarfBuzzSharp.OSX/bin/Release/HarfBuzzSharp.dll", "./output/osx/");
        // SkiaSharp.Views
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Android/bin/Release/SkiaSharp.Views.Android.dll", "./output/android/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.iOS/bin/Release/SkiaSharp.Views.iOS.dll", "./output/ios/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.Mac/bin/Release/SkiaSharp.Views.Mac.dll", "./output/osx/");
        CopyFileToDirectory ("./source/SkiaSharp.Views/SkiaSharp.Views.tvOS/bin/Release/SkiaSharp.Views.tvOS.dll", "./output/tvos/");
        // SkiaSharp.Views.Forms
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms/bin/Release/SkiaSharp.Views.Forms.dll", "./output/portable/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Android/bin/Release/SkiaSharp.Views.Forms.dll", "./output/android/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.iOS/bin/Release/SkiaSharp.Views.Forms.dll", "./output/ios/");
        CopyFileToDirectory ("./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Mac/bin/Release/SkiaSharp.Views.Forms.dll", "./output/osx/");
    } else if (IsRunningOnLinux ()) {
        RunNuGetRestore ("./source/SkiaSharpSource.Linux.sln");
        RunMSBuild ("./source/SkiaSharpSource.Linux.sln");
    }
    // SkiaSharp
    CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
    // HarfBuzzSharp
    CopyFileToDirectory ("./binding/HarfBuzzSharp.Portable/bin/Release/HarfBuzzSharp.dll", "./output/portable/");
    // SkiaSharp.HarfBuzz
    CopyFileToDirectory ("./source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/bin/Release/SkiaSharp.HarfBuzz.dll", "./output/portable/");
    // SkiaSharp.Workbooks
    CopyFileToDirectory ("./source/SkiaSharp.Workbooks/bin/Release/SkiaSharp.Workbooks.dll", "./output/interactive/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - some test cases to make sure it works
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .Does (() => 
{
    ClearSkiaSharpNuGetCache ();

    RunNuGetRestore ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");

    // Windows (x86 and x64)
    if (IsRunningOnWindows ()) {
        if (!DirectoryExists ("./output/tests/windows/x86")) CreateDirectory ("./output/tests/windows/x86");
        RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", "x86");
        RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/SkiaSharp.Desktop.Tests.dll");
        CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/TestResult.xml", "./output/tests/windows/x86");

        if (!DirectoryExists ("./output/tests/windows/x64")) CreateDirectory ("./output/tests/windows/x64");
        RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", "x64");
        RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/SkiaSharp.Desktop.Tests.dll");
        CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/x64/Release/TestResult.xml", "./output/tests/windows/x64");
    }

    // Mac OSX (Any CPU)
    if (IsRunningOnMac ()) {
        if (!DirectoryExists ("./output/tests/mac/AnyCPU")) CreateDirectory ("./output/tests/mac/AnyCPU");
        RunMSBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
        RunTests ("./tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/SkiaSharp.Desktop.Tests.dll");
        CopyFileToDirectory ("./tests/SkiaSharp.Desktop.Tests/bin/AnyCPU/Release/TestResult.xml", "./output/tests/mac/AnyCPU");
    }

    // .NET Core
    if (!DirectoryExists ("./output/tests/netcore")) CreateDirectory ("./output/tests/netcore");
    RunDotNetCoreRestore ("./tests/SkiaSharp.NetCore.Tests.Runner/SkiaSharp.NetCore.Tests.Runner.sln");
    DotNetCorePublish ("./tests/SkiaSharp.NetCore.Tests.Runner", new DotNetCorePublishSettings {
        Configuration = "Release",
        OutputDirectory = "./tests/SkiaSharp.NetCore.Tests.Runner/artifacts/"
    });
    RunNetCoreTests ("./tests/SkiaSharp.NetCore.Tests.Runner/artifacts/SkiaSharp.NetCore.Tests.Runner.dll");
    CopyFileToDirectory ("./tests/SkiaSharp.NetCore.Tests.Runner/artifacts/TestResult.xml", "./output/tests/netcore");
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
    CleanDirectories ("./samples/*/packages/SkiaSharp.*");

    // zip the samples for the GitHub release notes
    if (IS_ON_CI) {
        Zip ("./samples", "./output/samples.zip");
    }

    if (IsRunningOnLinux ()) {

    }

    if (IsRunningOnMac ()) {
        RunNuGetRestore ("./samples/MacSample/MacSample.sln");
        RunMSBuildWithPlatform ("./samples/MacSample/MacSample.sln", "x86");
        RunNuGetRestore ("./samples/FormsSample/FormsSample.Mac.sln");
        RunMSBuildWithPlatform ("./samples/FormsSample/FormsSample.Mac.sln", "iPhone");
        RunNuGetRestore ("./samples/TvSample/TvSample.sln");
        RunMSBuildWithPlatform ("./samples/TvSample/TvSample.sln", "iPhoneSimulator");
    }
    
    if (IsRunningOnWindows ()) {
        RunNuGetRestore ("./samples/WPFSample/WPFSample.sln");
        RunMSBuild ("./samples/WPFSample/WPFSample.sln");
        RunNuGetRestore ("./samples/UWPSample/UWPSample.sln");
        RunMSBuild ("./samples/UWPSample/UWPSample.sln");
        RunNuGetRestore ("./samples/FormsSample/FormsSample.Windows.sln");
        RunMSBuild ("./samples/FormsSample/FormsSample.Windows.sln");
        RunNuGetRestore ("./samples/WindowsSample/WindowsSample.sln");
        RunMSBuild ("./samples/WindowsSample/WindowsSample.sln");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DOCS - building the API documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("docs")
    .Does (() => 
{
    // first build mdoc
    RunNuGetRestore ("./externals/api-doc-tools/apidoctools.sln");
    RunMSBuild ("./externals/api-doc-tools/apidoctools.sln");

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
    // first build mdoc
    RunNuGetRestore ("./externals/api-doc-tools/apidoctools.sln");
    RunMSBuild ("./externals/api-doc-tools/apidoctools.sln");

    // the reference folders to locate assemblies
    IEnumerable<DirectoryPath> refs = new DirectoryPath [] {
            "./output/portable/",
        }
        .Union (GetDirectories ("./source/packages/Xamarin.Forms.*/lib/portable*"))
        .Union (GetDirectories ("./source/packages/OpenTK.*/lib/net40*"));
    // add windows-specific references
    if (IsRunningOnWindows ()) {
        // Windows.Foundation.UniversalApiContract is a winmd, so fake the dll
        // types aren't needed here
        RunMSBuild ("./externals/Windows.Foundation.UniversalApiContract/Windows.Foundation.UniversalApiContract.csproj");
        refs = refs.Union (new DirectoryPath [] {
            "./externals/Windows.Foundation.UniversalApiContract/bin/Release",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/MonoAndroid/v1.0",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/MonoAndroid/v2.3",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Xamarin.iOS/v1.0",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Xamarin.TVOS/v1.0",
            "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Xamarin.Mac/v2.0",
            "./externals",
        });
    }
    // add mac-specific references
    if (IsRunningOnMac ()) {
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
        "./output/portable/HarfBuzzSharp.dll",
        "./output/portable/SkiaSharp.HarfBuzz.dll",
    };
    // add windows-specific assemblies
    if (IsRunningOnWindows ()) {
        assemblies = assemblies.Union (new FilePath [] {
            "./output/windows/SkiaSharp.Views.Desktop.dll",
            "./output/windows/SkiaSharp.Views.WPF.dll",
            "./output/android/SkiaSharp.Views.Android.dll",
            "./output/ios/SkiaSharp.Views.iOS.dll",
            "./output/osx/SkiaSharp.Views.Mac.dll",
            "./output/tvos/SkiaSharp.Views.tvOS.dll",
            "./output/uwp/SkiaSharp.Views.UWP.dll",
        }).ToArray ();
    }
    // add mac-specific assemblies
    if (IsRunningOnMac ()) {
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
    // Because some platforms don't support signing,
    // we must make sure the final package only contains signed assemblies
    // And, we want to make sure all is well on Windows too
    if (IS_ON_FINAL_CI || IsRunningOnWindows ()) {
        var excludedAssemblies = new string[] {
            "/SkiaSharp.Views.Forms.dll", // Xamarin.Forms is not sigend, so we can't sign
            "/SkiaSharp.Workbooks.dll" // Workbooks integration library is not signed, so we can't sign
        };
        foreach (var f in GetFiles("./output/*/*.dll")) {
            // skip the excluded assemblies
            var excluded = false;
            foreach (var assembly in excludedAssemblies) {
                if (f.FullPath.EndsWith (assembly)) {
                    excluded = true;
                    break;
                }
            }
            // verify
            if (!excluded) {
                Information("Making sure that '{0}' is signed.", f);
                RunSNTool(f);
            }
        }
    }

    // we can only build the combined package on CI
    if (IS_ON_FINAL_CI) {
        PackageNuGet ("./nuget/SkiaSharp.nuspec", "./output/");
        PackageNuGet ("./nuget/HarfBuzzSharp.nuspec", "./output/");
        PackageNuGet ("./nuget/SkiaSharp.Views.nuspec", "./output/");
        PackageNuGet ("./nuget/SkiaSharp.Views.Forms.nuspec", "./output/");
    } else {
        if (IsRunningOnWindows ()) {
            PackageNuGet ("./nuget/SkiaSharp.Windows.nuspec", "./output/");
            PackageNuGet ("./nuget/HarfBuzzSharp.Windows.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Windows.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Forms.Windows.nuspec", "./output/");
        }
        if (IsRunningOnMac ()) {
            PackageNuGet ("./nuget/SkiaSharp.Mac.nuspec", "./output/");
            PackageNuGet ("./nuget/HarfBuzzSharp.Mac.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Mac.nuspec", "./output/");
            PackageNuGet ("./nuget/SkiaSharp.Views.Forms.Mac.nuspec", "./output/");
        }
        if (IsRunningOnLinux ()) {
            PackageNuGet ("./nuget/SkiaSharp.Linux.nuspec", "./output/");
            PackageNuGet ("./nuget/HarfBuzzSharp.Linux.nuspec", "./output/");
        }
    }
    // HarfBuzz is a PCL
    PackageNuGet ("./nuget/SkiaSharp.HarfBuzz.nuspec", "./output/");
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
    // tests packages files
    add ("./tests/*/packages.config");
    add ("./tests/*/project.json");
    // tests project files
    add ("./tests/*/*.nuget.targets");
    add ("./tests/*/*.csproj");
    // update
    foreach (var file in files) {
        UpdateSkiaSharpVersion (file, VERSION_PACKAGES);
    }

    // assembly infos
    UpdateAssemblyInfo (
        "./binding/Binding/Properties/SkiaSharpAssemblyInfo.cs",
        VERSION_ASSEMBLY, VERSION_FILE, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Views/SkiaSharp.Views.Shared/Properties/SkiaSharpViewsAssemblyInfo.cs",
        VERSION_ASSEMBLY, VERSION_FILE, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Views.Forms/SkiaSharp.Views.Forms.Shared/Properties/SkiaSharpViewsFormsAssemblyInfo.cs",
        VERSION_ASSEMBLY, VERSION_FILE, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz.Shared/Properties/SkiaSharpHarfBuzzAssemblyInfo.cs",
        VERSION_ASSEMBLY, VERSION_FILE, sha);

    UpdateAssemblyInfo (
        "./binding/HarfBuzzSharp.Shared/Properties/HarfBuzzSharpAssemblyInfo.cs",
        HARFBUZZ_VERSION_ASSEMBLY, HARFBUZZ_VERSION_FILE, sha);
    UpdateAssemblyInfo (
        "./source/SkiaSharp.Workbooks/Properties/SkiaSharpWorkbooksAssemblyInfo.cs",
        VERSION_ASSEMBLY, VERSION_FILE, sha);
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
    DeleteFiles ("./binding/*/project.lock.json");

    CleanDirectories ("./samples/*/bin");
    CleanDirectories ("./samples/*/obj");
    CleanDirectories ("./samples/*/AppPackages");
    CleanDirectories ("./samples/*/*/bin");
    CleanDirectories ("./samples/*/*/obj");
    DeleteFiles ("./samples/*/*/project.lock.json");
    CleanDirectories ("./samples/*/*/AppPackages");
    CleanDirectories ("./samples/*/packages");

    CleanDirectories ("./tests/**/bin");
    CleanDirectories ("./tests/**/obj");
    CleanDirectories ("./tests/**/artifacts");
    DeleteFiles ("./tests/**/project.lock.json");

    CleanDirectories ("./source/*/*/bin");
    CleanDirectories ("./source/*/*/obj");
    DeleteFiles ("./source/*/*/project.lock.json");
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

Task ("Mac-CI")
    .IsDependentOn ("CI");

Task ("Windows-CI")
    .IsDependentOn ("CI");

Task ("Linux-CI")
    .IsDependentOn ("CI");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW 
////////////////////////////////////////////////////////////////////////////////////////////////////

Information ("Cake.exe ToolPath: {0}", CakeToolPath);
Information ("NUnitConsole ToolPath: {0}", NUnitConsoleToolPath);
Information ("NuGet.exe ToolPath: {0}", NugetToolPath);
Information ("Xamarin-Component.exe ToolPath: {0}", XamarinComponentToolPath);
Information ("genapi.exe ToolPath: {0}", GenApiToolPath);
Information ("sn.exe ToolPath: {0}", SNToolPath);
Information ("msbuild.exe ToolPath: {0}", MSBuildToolPath);

if (IS_ON_CI) {
    Information ("Detected that we are building on CI, {0}.", IS_ON_FINAL_CI ? "and on FINAL CI" : "but NOT on final CI");
} else {
    Information ("Detected that we are {0} on CI.", "NOT");
}

ListEnvironmentVariables ();

RunTarget (TARGET);
