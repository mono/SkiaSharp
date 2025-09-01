
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
    // Check if this is the past version mode based on target name
    var isPastMode = TARGET.EndsWith("-past");
    var generatePrettyVersion = !isPastMode;
    var cleanupAfter = isPastMode;
    
    Information ($"Running API diff in {(isPastMode ? "past versions" : "current vs latest")} mode...");

    // clean up diff folders
    var pastDiffDir = $"{OUTPUT_NUGETS_PATH}/api-diffs-past";
    EnsureDirectoryExists (pastDiffDir);
    CleanDirectories (pastDiffDir);
    var diffDir = $"{OUTPUT_NUGETS_PATH}/api-diff";
    EnsureDirectoryExists (diffDir);
    CleanDirectories (diffDir);

    var baseDir = isPastMode ? pastDiffDir : diffDir;

    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    // some parts of SkiaSharp depend on other parts (needed for past mode)
    if (isPastMode) {
        foreach (var dir in GetDirectories($"{PACKAGE_CACHE_PATH}/skiasharp/*/lib/netstandard2.0"))
            comparer.SearchPaths.Add(dir.FullPath);
    }

    // Filter for current mode only
    var filter = isPastMode ? null : new NuGetVersions.Filter {
        IncludePrerelease = NUGET_DIFF_PRERELEASE
    };

    foreach (var id in TRACKED_NUGETS.Keys) {
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        Information ($"Comparing the assemblies in '{id}'...");

        // Build version comparison pairs based on mode
        var versionPairs = new List<(string from, string to, string outputVersion, PackageArchiveReader reader)>();
        
        if (isPastMode) {
            // Past mode: compare all historical versions (each version vs previous)
            var allVersions = await NuGetVersions.GetAllAsync (id);
            for (var idx = 0; idx < allVersions.Length; idx++) {
                var version = allVersions [idx].ToNormalizedString ();
                var previous = idx == 0 ? null : allVersions [idx - 1].ToNormalizedString ();
                versionPairs.Add((previous, version, version, null));
            }
        } else {
            // Current mode: compare current dev version vs latest released
            var version = GetVersion (id);
            var localNugetVersion = PREVIEW_ONLY_NUGETS.Contains(id)
                ? $"{version}-{PREVIEW_NUGET_SUFFIX}"
                : version;
            var latestVersion = (await NuGetVersions.GetLatestAsync (id, filter))?.ToNormalizedString ();
            var reader = new PackageArchiveReader ($"{OUTPUT_NUGETS_PATH}/{id.ToLower ()}.{localNugetVersion}.nupkg");
            versionPairs.Add((latestVersion, localNugetVersion, version, reader));
        }

        // Process each version comparison
        foreach (var (fromVersion, toVersion, outputVersion, reader) in versionPairs) {
            Information ($"Comparing version '{fromVersion}' vs '{toVersion}' of '{id}'...");

            // pre-cache so we can have better logs
            Debug ($"Caching version '{toVersion}' of '{id}'...");
            if (reader == null) {
                await comparer.ExtractCachedPackageAsync (id, toVersion);
            }
            if (!string.IsNullOrEmpty (fromVersion)) {
                Debug ($"Caching version '{fromVersion}' of '{id}'...");
                await comparer.ExtractCachedPackageAsync (id, fromVersion);
            }

            // generate the diff and copy to the changelogs
            Debug ($"Running a diff on '{fromVersion}' vs '{toVersion}' of '{id}'...");
            var diffRoot = isPastMode ? $"{baseDir}/{id}/{outputVersion}" : $"{baseDir}/{id}";
            
            // run the diff with just the breaking changes
            comparer.MarkdownDiffFileExtension = ".breaking.md";
            comparer.IgnoreNonBreakingChanges = true;
            if (reader != null) {
                await comparer.SaveCompleteDiffToDirectoryAsync (id, fromVersion, reader, diffRoot);
            } else {
                await comparer.SaveCompleteDiffToDirectoryAsync (id, fromVersion, toVersion, diffRoot);
            }
            
            // run the diff on everything
            comparer.MarkdownDiffFileExtension = null;
            comparer.IgnoreNonBreakingChanges = false;
            if (reader != null) {
                await comparer.SaveCompleteDiffToDirectoryAsync (id, fromVersion, reader, diffRoot);
            } else {
                await comparer.SaveCompleteDiffToDirectoryAsync (id, fromVersion, toVersion, diffRoot);
            }
            
            CopyChangelogs (diffRoot, id, outputVersion);

            // copy pretty version
            if (generatePrettyVersion) {
                foreach (var md in GetFiles ($"{diffRoot}/*/*.md")) {
                    var tfm = md.GetDirectory ().GetDirectoryName();
                    var prettyPath = ((DirectoryPath)diffDir).CombineWithFilePath ($"{id}/{tfm}/{md.GetFilename ()}");
                    if (!FindTextInFiles (md.FullPath, "No changes").Any ()) {
                        EnsureDirectoryExists (prettyPath.GetDirectory ());
                        CopyFile (md, prettyPath);
                    }
                }
            }

            Debug ($"Diff complete of version '{toVersion}' of '{id}'.");
            
            // Dispose reader for current mode
            reader?.Dispose();
        }

        Information ($"Diff complete of '{id}'.");
    }
});

Task("docs-api-diff-past")
    .Description("Generate API diffs for all past versions (alias for docs-api-diff with past mode detection)")
    .IsDependentOn("docs-api-diff");

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
