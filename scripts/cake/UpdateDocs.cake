// # dependencies
// mdoc                                            release     5.8.9
// harfbuzz                                        release     8.3.1
// skia                                            release     m119
// xunit                                           release     2.4.2
// xunit.runner.console                            release     2.4.2
// OpenTK                                          release     3.1.0
// OpenTK.GLControl                                release     3.1.0
// GtkSharp                                        release     3.24.24.95
// GdkSharp                                        release     3.24.24.95
// GLibSharp                                       release     3.24.24.95
// AtkSharp                                        release     3.24.24.95
// System.Memory                                   release     4.5.5
// SharpVk                                         release     0.4.2
// Uno.WinUI                                       release     5.2.175
// Microsoft.WindowsAppSDK                         release     1.4.230913002
// Microsoft.Maui.Graphics                         release     7.0.92
// Microsoft.Windows.SDK.NET.Ref                   release     10.0.19041.27
// Microsoft.AspNetCore.Components.Web             release     6.0.0
// ANGLE                                           release     chromium/6275
// Microsoft.iOS.Ref.net8.0_17.0                   release     17.0.8523
// Microsoft.MacCatalyst.Ref.net8.0_17.0           release     17.0.8523
// Microsoft.tvOS.Ref.net8.0_17.0                  release     17.0.8523
// Microsoft.macOS.Ref.net8.0_14.0                 release     14.0.8523
// Samsung.Tizen.Ref                               release     10.0.109
// Xamarin.VisualStudio.Apple.Sdk                  url         https://download.visualstudio.microsoft.com/download/pr/42786999-d45b-4428-b946-248bb9676505/d37d5503fe30287fb73facbc34321865fdced518ba9205ab5581c7487e4ed6d6/Xamarin.VisualStudio.Apple.Sdk.17.12.0.94.vsix
// Xamarin.Android.Sdk                             url         https://download.visualstudio.microsoft.com/download/pr/a3846965-8f4c-42fa-b728-b6ea5f0a2a16/90d568134f0f5b472e5c085fc6573c76bd40231892a89eb7551940dcc055eb8a/Xamarin.Android.Sdk-13.2.2.0.vsix

async Task<NuGetDiff> CreateNuGetDiffAsync()
{
    var comparer = new NuGetDiff();
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;
    comparer.IgnoreResolutionErrors = true;
    
    Verbose ($"Adding dependencies...");

    await AddDep("OpenTK.GLControl", "NET20");
    await AddDep("GtkSharp", "netstandard2.0");
    await AddDep("GdkSharp", "netstandard2.0");
    await AddDep("GLibSharp", "netstandard2.0");
    await AddDep("AtkSharp", "netstandard2.0");
    await AddDep("System.Memory", "netstandard2.0");
    await AddDep("Microsoft.WindowsAppSDK", "net5.0-windows10.0.18362.0");
    await AddDep("Microsoft.Maui.Graphics", "netstandard2.0");
    await AddDep("Microsoft.Windows.SDK.NET.Ref", "");
    await AddDep("Microsoft.iOS.Ref.net8.0_17.0", "net8.0");
    await AddDep("Microsoft.MacCatalyst.Ref.net8.0_17.0", "net8.0");
    await AddDep("Microsoft.tvOS.Ref.net8.0_17.0", "net8.0");
    await AddDep("Microsoft.macOS.Ref.net8.0_14.0", "net8.0");
    await AddDep("Samsung.Tizen.Ref", "net8.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.iOS/v1.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.TVOS/v1.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.Mac/v2.0");
    await AddVsixDep("Xamarin.Android.Sdk", "$ReferenceAssemblies/Microsoft/Framework/MonoAndroid/v1.0");
    await AddVsixDep("Xamarin.Android.Sdk", "$ReferenceAssemblies/Microsoft/Framework/MonoAndroid/v13.0");

    // some parts of SkiaSharp depend on other parts
    foreach (var dir in GetDirectories($"{PACKAGE_CACHE_PATH}/skiasharp/*/lib/netstandard2.0"))
        comparer.SearchPaths.Add(dir.FullPath);

    Verbose("Added search paths:");
    foreach (var path in comparer.SearchPaths) {
        var found = GetFiles($"{path}/*.dll").Any() || GetFiles($"{path}/*.winmd").Any();
        Verbose($"    {(found ? " " : "!")} {path}");
    }

    return comparer;

    async Task AddVsixDep(string id, string localPath, string type = "url")
    {
        var url = GetVersion(id, type);
        var fileName = System.IO.Path.GetFileName(new Uri(url).LocalPath);
        Verbose ($"    Adding VSIX dependency {id} ({fileName})...");
        var dest = System.IO.Path.Combine(PACKAGE_CACHE_PATH.FullPath, id.ToLower(), fileName);
        if (!FileExists(dest)) {
            EnsureDirectoryExists(System.IO.Path.GetDirectoryName(dest));
            Verbose($"      Downloading {url} to {dest}");
            DownloadFile(url, dest);
        }
        var extractDir = System.IO.Path.Combine(PACKAGE_CACHE_PATH.FullPath, id.ToLower(), System.IO.Path.GetFileNameWithoutExtension(fileName));
        if (!DirectoryExists(extractDir)) {
            Verbose($"      Extracting {dest} to {extractDir}");
            EnsureDirectoryExists(extractDir);
            DecompressArchive(dest, extractDir);
        }
        var searchPath = System.IO.Path.Combine(extractDir, localPath);
        if (DirectoryExists(searchPath)) {
            Verbose($"      Adding VSIX search path: {searchPath}");
            comparer.SearchPaths.Add(searchPath);
        } else {
            Verbose($"      No VSIX search path found at: {searchPath}");
        }
    }
        
    async Task AddDep(string id, string platform, string type = "release")
    {
        var version = GetVersion(id, type);
        Verbose ($"    Adding dependency {id} version {version}...");
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        var libPath = System.IO.Path.Combine(root, "lib", platform);
        var refPath = System.IO.Path.Combine(root, "ref", platform);
        if (DirectoryExists(libPath)) {
            Verbose ($"      lib path {libPath}");
            comparer.SearchPaths.Add(libPath);
        } else if (DirectoryExists(refPath)) {
            Verbose ($"      ref path {libPath}");
            comparer.SearchPaths.Add(refPath);
        } else {
            Verbose ($"      no lib or ref path");
        }
    }
}

void CopyChangelogs (DirectoryPath diffRoot, string id, string version)
{
    foreach (var (path, platform) in GetPlatformDirectories (diffRoot)) {
        // first, make sure to create markdown files for unchanged assemblies
        var xmlFiles = $"{path}/*.new.info.xml";
        foreach (var file in GetFiles (xmlFiles)) {
            var dll = file.GetFilenameWithoutExtension ().GetFilenameWithoutExtension ().GetFilenameWithoutExtension ();
            var md = $"{path}/{dll}.diff.md";
            if (!FileExists (md)) {
                var n = Environment.NewLine;
                var noChangesText = $"# API diff: {dll}{n}{n}## {dll}{n}{n}> No changes.{n}";
                FileWriteText (md, noChangesText);
            }
        }

        // now copy the markdown files to the changelogs
        var mdFiles = $"{path}/*.*.md";
        ReplaceTextInFiles (mdFiles, "<h4>", "> ");
        ReplaceTextInFiles (mdFiles, "</h4>", Environment.NewLine);
        ReplaceTextInFiles (mdFiles, "\r\r", "\r");
        foreach (var file in GetFiles (mdFiles)) {
            var dllName = file.GetFilenameWithoutExtension ().GetFilenameWithoutExtension ().GetFilenameWithoutExtension ();
            if (file.GetFilenameWithoutExtension ().GetExtension () == ".breaking") {
                // skip over breaking changes without any breaking changes
                if (!FindTextInFiles (file.FullPath, "###").Any ()) {
                    DeleteFile (file);
                    continue;
                }

                dllName += ".breaking";
            }
            var changelogPath = (FilePath)$"./changelogs/{id}/{version}/{dllName}.md";
            EnsureDirectoryExists (changelogPath.GetDirectory ());
            CopyFile (file, changelogPath);
            var changelogOutputPath = (FilePath)$"./output/logs/changelogs/{id}/{version}/{dllName}.md";
            EnsureDirectoryExists (changelogOutputPath.GetDirectory ());
            CopyFile (file, changelogOutputPath);
        }
    }
}

Task ("docs-download-output")
    .Does (async () =>
{
    CleanDir ("./output");

    await DownloadPackageAsync ("_nugets", OUTPUT_NUGETS_PATH);
    await DownloadPackageAsync ("_nugetspreview", OUTPUT_NUGETS_PATH);
});

Task ("docs-api-diff")
    .Does (async () =>
{
    // working version
    var baseDir = $"{OUTPUT_NUGETS_PATH}/api-diff";
    CleanDirectories (baseDir);

    // pretty version
    var diffDir = "./output/api-diff";
    EnsureDirectoryExists (diffDir);
    CleanDirectories (diffDir);

    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    var filter = new NuGetVersions.Filter {
        IncludePrerelease = NUGET_DIFF_PRERELEASE
    };

    foreach (var id in TRACKED_NUGETS.Keys) {
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        Information ($"Comparing the assemblies in '{id}'...");

        var version = GetVersion (id);
        var localNugetVersion = PREVIEW_ONLY_NUGETS.Contains(id)
            ? $"{version}-{PREVIEW_NUGET_SUFFIX}"
            : version;

        var latestVersion = (await NuGetVersions.GetLatestAsync (id, filter))?.ToNormalizedString ();
        Debug ($"Version '{latestVersion}' is the latest version of '{id}'...");

        // pre-cache so we can have better logs
        if (!string.IsNullOrEmpty (latestVersion)) {
            Debug ($"Caching version '{latestVersion}' of '{id}'...");
            await comparer.ExtractCachedPackageAsync (id, latestVersion);
        }

        // generate the diff and copy to the changelogs
        Debug ($"Running a diff on '{latestVersion}' vs '{localNugetVersion}' of '{id}'...");
        var diffRoot = $"{baseDir}/{id}";
        using (var reader = new PackageArchiveReader ($"{OUTPUT_NUGETS_PATH}/{id.ToLower ()}.{localNugetVersion}.nupkg")) {
            // run the diff with just the breaking changes
            comparer.MarkdownDiffFileExtension = ".breaking.md";
            comparer.IgnoreNonBreakingChanges = true;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, latestVersion, reader, diffRoot);
            // run the diff on everything
            comparer.MarkdownDiffFileExtension = null;
            comparer.IgnoreNonBreakingChanges = false;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, latestVersion, reader, diffRoot);
        }
        CopyChangelogs (diffRoot, id, version);

        // copy pretty version
        foreach (var md in GetFiles ($"{diffRoot}/*/*.md")) {
            var tfm = md.GetDirectory ().GetDirectoryName();
            var prettyPath = ((DirectoryPath)diffDir).CombineWithFilePath ($"{id}/{tfm}/{md.GetFilename ()}");
            if (!FindTextInFiles (md.FullPath, "No changes").Any ()) {
                EnsureDirectoryExists (prettyPath.GetDirectory ());
                CopyFile (md, prettyPath);
            }
        }

        Information ($"Diff complete of '{id}'.");
    }
});

Task ("docs-api-diff-past")
    .Does (async () =>
{
    var baseDir = "./output/api-diffs-past";
    CleanDirectories (baseDir);

    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    foreach (var id in TRACKED_NUGETS.Keys) {
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        Information ($"Comparing the assemblies in '{id}'...");

        var allVersions = await NuGetVersions.GetAllAsync (id);
        for (var idx = 0; idx < allVersions.Length; idx++) {
            // get the versions for the diff
            var version = allVersions [idx].ToNormalizedString ();
            var previous = idx == 0 ? null : allVersions [idx - 1].ToNormalizedString ();
            Information ($"Comparing version '{previous}' vs '{version}' of '{id}'...");

            // pre-cache so we can have better logs
            Debug ($"Caching version '{version}' of '{id}'...");
            await comparer.ExtractCachedPackageAsync (id, version);
            if (previous != null) {
                Debug ($"Caching version '{previous}' of '{id}'...");
                await comparer.ExtractCachedPackageAsync (id, previous);
            }

            // generate the diff and copy to the changelogs
            Debug ($"Running a diff on '{previous}' vs '{version}' of '{id}'...");
            var diffRoot = $"{baseDir}/{id}/{version}";
            // run the diff with just the breaking changes
            comparer.MarkdownDiffFileExtension = ".breaking.md";
            comparer.IgnoreNonBreakingChanges = true;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, previous, version, diffRoot);
            // run the diff on everything
            comparer.MarkdownDiffFileExtension = null;
            comparer.IgnoreNonBreakingChanges = false;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, previous, version, diffRoot);
            CopyChangelogs (diffRoot, id, version);

            Debug ($"Diff complete of version '{version}' of '{id}'.");
        }
        Information ($"Diff complete of '{id}'.");
    }

    // clean up after working
    CleanDirectories (baseDir);
});

Task ("docs-update-frameworks")
    .Does (async () =>
{
    // clear the temp dir
    var docsTempPath = "./output/docs/temp";
    var docsTempPathFrameowrks = "./output/docs/temp/frameworks";
    var docsTempPathNuGets = "./output/docs/temp/nugets";
    EnsureDirectoryExists (docsTempPath);
    CleanDirectories (docsTempPath);
    EnsureDirectoryExists (docsTempPathNuGets);
    EnsureDirectoryExists (docsTempPathFrameowrks);

    // extract nugets that were built/downloaded
    foreach (var id in TRACKED_NUGETS.Keys) {
        var version = GetVersion (id);
        var localNugetVersion = PREVIEW_ONLY_NUGETS.Contains(id)
            ? $"{version}-{PREVIEW_NUGET_SUFFIX}"
            : version;
        var name = $"{id}.{localNugetVersion}.nupkg";
        CleanDir ($"{docsTempPathNuGets}/{id}");
        Unzip ($"{OUTPUT_NUGETS_PATH}/{name}", $"{docsTempPathNuGets}/{id}");
    }

    // get a comparer that will download the nugets
    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();

    // generate the temp frameworks.xml
    var xFrameworks = new XElement ("Frameworks");
    var monikers = new List<string> ();
    foreach (var id in TRACKED_NUGETS.Keys) {
        // skip doc generation for Uno, this is the same as WinUI and it is not needed
        if (id.StartsWith ("SkiaSharp.Views.Uno.WinUI") )
            continue;
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        // get the versions
        Information ($"Comparing the assemblies in '{id}'...");
        var allVersions = await NuGetVersions.GetAllAsync (id, new NuGetVersions.Filter {
            MinimumVersion = new NuGetVersion (TRACKED_NUGETS [id])
        });

        // add the current dev version to the mix
        var dev = new NuGetVersion (GetVersion (id));
        allVersions = allVersions.Union (new [] { dev }).ToArray ();

        // "merge" the patches so we only care about major.minor
        var merged = new Dictionary<string, NuGetVersion> ();
        foreach (var version in allVersions) {
            merged [$"{version.Major}.{version.Minor}"] = version;
        }

        foreach (var version in merged) {
            Information ($"Downloading '{id}' version '{version}'...");
            // get the path to the nuget contents
            var packagePath = version.Value == dev
                ? $"{docsTempPathNuGets}/{id}"
                : await comparer.ExtractCachedPackageAsync (id, version.Value);

            var dirs =
                GetPlatformDirectories ($"{packagePath}/lib").Union(
                GetPlatformDirectories ($"{packagePath}/ref"));
            foreach (var (path, platform) in dirs) {
                string moniker;
                if (id.StartsWith ("SkiaSharp.Views.Forms"))
                    if (id != "SkiaSharp.Views.Forms")
                        continue;
                    else
                        moniker = $"skiasharp-views-forms-{version.Key}";
                else if (id.StartsWith ("SkiaSharp.Views.Maui"))
                    moniker = $"skiasharp-views-maui-{version.Key}";
                else if (id.StartsWith ("SkiaSharp.Views"))
                    moniker = $"skiasharp-views-{version.Key}";
                else if (platform == null)
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{version.Key}";
                else
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{platform}-{version.Key}";

                // add the node to the frameworks.xml
                if (!monikers.Contains (moniker)) {
                    monikers.Add (moniker);
                    xFrameworks.Add (
                        new XElement ("Framework",
                            new XAttribute ("Name", moniker),
                            new XAttribute ("Source", moniker)));
                }

                // copy the assemblies for the tool
                var o = $"{docsTempPathFrameowrks}/{moniker}";
                EnsureDirectoryExists (o);
                CopyFiles ($"{path}/*.dll", o);
            }
        }
    }
    monikers.Sort ();

    // save the frameworks.xml
    var fwxml = $"{docsTempPathFrameowrks}/frameworks.xml";
    var xdoc = new XDocument (xFrameworks);
    xdoc.Save (fwxml);

    // update the docs json
    var docsJsonPath = DOCS_ROOT_PATH.CombineWithFilePath (".openpublishing.publish.config.json");
    var docsJson = ParseJsonFromFile (docsJsonPath);
    docsJson ["docsets_to_publish"][0]["monikers"] = new JArray (monikers.ToArray ());
    SerializeJsonToPrettyFile (docsJsonPath, docsJson);

    // generate doc files
    comparer = await CreateNuGetDiffAsync ();
    var refArgs = string.Join (" ", comparer.SearchPaths.Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute ((FilePath) fwxml);
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = $"update --debug --delete --out=\"{DOCS_PATH}\" --lang=DocId --frameworks={fw} {refArgs}",
        WorkingDirectory = docsTempPathFrameowrks
    });

    // clean up after working
    CleanDirectories (docsTempPath);
});

Task ("docs-format-docs")
    .Does (() =>
{
    // process the generated docs
    var docFiles = GetFiles ("./docs/**/*.xml");
    float typeCount = 0;
    float memberCount = 0;
    float totalTypes = 0;
    float totalMembers = 0;
    foreach (var file in docFiles) {
        Debug("Processing {0}...", file.FullPath);

        var xdoc = XDocument.Load (file.FullPath);

        // remove IComponent docs as this is just designer
        if (xdoc.Root.Name == "Type") {
            xdoc.Root
                .Elements ("Members")
                .Elements ("Member")
                .Where (e => e.Attribute ("MemberName")?.Value?.StartsWith ("System.ComponentModel.IComponent.") == true)
                .Remove ();
        }

        // remove any duplicate public keys
        if (xdoc.Root.Name == "Overview") {
            var multiKey = xdoc.Root
                .Elements ("Assemblies")
                .Elements ("Assembly")
                .Where (e => e.Elements ("AssemblyPublicKey").Count () > 1);
            foreach (var mass in multiKey) {
                mass.Elements ("AssemblyPublicKey")
                    .Skip (1)
                    .Remove ();
            }
        }

        // remove any assembly attributes for now: https://github.com/mono/api-doc-tools/issues/560
        if (xdoc.Root.Name == "Overview") {
            xdoc.Root
                .Elements ("Assemblies")
                .Elements ("Assembly")
                .Elements ("Attributes")
                .Elements ("Attribute")
                .Remove ();
        }

        // remove any duplicate AssemblyVersions
        if (xdoc.Root.Name == "Type") {
            foreach (var info in xdoc.Root.Descendants ("AssemblyInfo")) {
                var versions = info.Elements ("AssemblyVersion");
                var newVersions = new List<XElement> ();
                foreach (var version in versions) {
                    if (newVersions.All (nv => nv.Value != version.Value)) {
                        newVersions.Add (version);
                    }
                }
                versions.Remove ();
                info.Add (newVersions.OrderBy (e => e.Value));
            }
        }

        // Fix the type rename from SkPath1DPathEffectStyle to SKPath1DPathEffectStyle
        // this breaks linux as it is just a case change and that OS is case sensitive
        if (xdoc.Root.Name == "Overview") {
            xdoc.Root
                .Elements ("Types")
                .Elements ("Namespace")
                .Elements ("Type")
                .Where (e => e.Attribute ("Name")?.Value == "SkPath1DPathEffectStyle")
                .Remove ();
        }

        // remove the duplicate SKDynamicMemoryWStream.CopyTo method with a different return type
        if (xdoc.Root.Name == "Type" && xdoc.Root.Attribute ("Name")?.Value == "SKDynamicMemoryWStream") {
            var copyTos = xdoc.Root
                .Elements ("Members")
                .Elements ("Member")
                .Where (e => e.Attribute ("MemberName")?.Value == "CopyTo")
                .Where (e => e.Elements ("MemberSignature").Any (s => s.Attribute ("Value")?.Value == "M:SkiaSharp.SKDynamicMemoryWStream.CopyTo(SkiaSharp.SKWStream)"));
            var voidReturn = copyTos.FirstOrDefault (e => e.Element ("ReturnValue")?.Element ("ReturnType")?.Value == "System.Void");
            var boolReturn = copyTos.FirstOrDefault (e => e.Element ("ReturnValue")?.Element ("ReturnType")?.Value == "System.Boolean");
            if (voidReturn != null && boolReturn != null) {
                boolReturn
                    .Element ("AssemblyInfo")
                    .Elements ("AssemblyVersion")
                    .FirstOrDefault ()
                    .AddBeforeSelf (voidReturn.Element ("AssemblyInfo").Elements ("AssemblyVersion"));
                voidReturn.Remove ();
            }
        }

        // remove the no-longer-obsolete document members
        if (xdoc.Root.Name == "Type" && xdoc.Root.Attribute ("Name")?.Value == "SKDocument") {
            xdoc.Root
                .Elements ("Members")
                .Elements ("Member")
                .Where (e => e.Attribute ("MemberName")?.Value == "CreatePdf")
                .Where (e => e.Elements ("MemberSignature").All (s => s.Attribute ("Value")?.Value != "M:SkiaSharp.SKDocument.CreatePdf(SkiaSharp.SKWStream,SkiaSharp.SKDocumentPdfMetadata,System.Single)"))
                .SelectMany (e => e.Elements ("Attributes").Elements ("Attribute").Elements ("AttributeName"))
                .Where (e => e.Value.Contains ("System.Obsolete"))
                .Remove ();
        }

        // remove the no-longer-obsolete SK3dView attributes
        if (xdoc.Root.Name == "Type" && xdoc.Root.Attribute ("Name")?.Value == "SK3dView") {
            xdoc.Root
                .Element ("Attributes")?
                .Elements ("Attribute")
                .SelectMany (e => e.Elements ("AttributeName"))
                .Where (e => e.Value.Contains ("System.Obsolete"))
                .Remove ();
        }

        // remove empty FrameworkAlternate elements
        var emptyAlts = xdoc.Root
            .Descendants ()
            .Where (d => d.Attribute ("FrameworkAlternate") != null && string.IsNullOrEmpty (d.Attribute ("FrameworkAlternate").Value))
            .ToArray ();
        foreach (var empty in emptyAlts) {
            if (empty?.Parent != null) {
                empty.Remove ();
            }
        }

        // remove empty Attribute elements
        xdoc.Root
            .Descendants ("Attribute")
            .Where (e => !e.Elements ().Any ())
            .Remove ();

        // special case for Android resources: don't process
        if (xdoc.Root.Name == "Type") {
            var nameAttr = xdoc.Root.Attribute ("FullName")?.Value;
            if (nameAttr == "SkiaSharp.Views.Android.Resource" || nameAttr?.StartsWith ("SkiaSharp.Views.Android.Resource+") == true) {
                DeleteFile (file);
                continue;
            }
        }
        if (xdoc.Root.Name == "Overview") {
            foreach (var type in xdoc.Root.Descendants ("Type").ToArray ()) {
                var nameAttr = type.Attribute ("Name")?.Value;
                if (nameAttr == "Resource" || nameAttr?.StartsWith ("Resource+") == true) {
                    type.Remove ();
                }
            }
        }
        if (xdoc.Root.Name == "Framework") {
            foreach (var type in xdoc.Root.Descendants ("Type").ToArray ()) {
                var nameAttr = type.Attribute ("Name")?.Value;
                if (nameAttr == "SkiaSharp.Views.Android.Resource" || nameAttr?.StartsWith ("SkiaSharp.Views.Android.Resource/") == true) {
                    type.Remove ();
                }
            }
        }

        // count the types without docs
        var typesWithDocs = xdoc.Root
            .Elements ("Docs");
        totalTypes += typesWithDocs.Count ();
        var currentTypeCount = typesWithDocs.Count (m => m.Value?.IndexOf ("To be added.") >= 0);
        typeCount += currentTypeCount;

        // count the members without docs
        var membersWithDocs = xdoc.Root
            .Elements ("Members")
            .Elements ("Member")
            .Elements ("Docs");
        totalMembers += membersWithDocs.Count ();
        var currentMemberCount = membersWithDocs.Count (m => m.Value?.IndexOf ("To be added.") >= 0);
        memberCount += currentMemberCount;

        // log if either type or member has missing docs
        currentMemberCount += currentTypeCount;
        if (currentMemberCount > 0) {
            var fullName = xdoc.Root.Attribute ("FullName");
            if (fullName != null)
                Information ("Docs missing on {0} = {1}", fullName.Value, currentMemberCount);
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

    // log summary
    Information (
        "Documentation missing in {0}/{1} ({2:0.0%}) types and {3}/{4} ({5:0.0%}) members.",
        typeCount, totalTypes, typeCount / totalTypes,
        memberCount, totalMembers, memberCount / totalMembers);
});
