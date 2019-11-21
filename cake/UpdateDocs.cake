
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
                if (!FindTextInFiles (file.FullPath, "###").Any ())
                    continue;

                dllName += ".breaking";
            }
            var changelogPath = (FilePath)$"./changelogs/{id}/{version}/{dllName}.md";
            EnsureDirectoryExists (changelogPath.GetDirectory ());
            CopyFile (file, changelogPath);
        }
    }
}

Task ("docs-download-build-artifact")
    .IsDependentOn ("download-last-successful-build")
    .Does (() =>
{
    var url = string.Format(AZURE_BUILD_URL, AZURE_BUILD_ID, "nuget");

    EnsureDirectoryExists ("./output");
    CleanDirectories ("./output");

    DownloadFile(url, "./output/nuget.zip");
});

Task ("docs-expand-build-artifact")
    .Does (() =>
{
    Unzip ("./output/nuget.zip", "./output");
    MoveDirectory ("./output/nuget", "./output/nugets");

    foreach (var id in TRACKED_NUGETS.Keys) {
        var version = GetVersion (id);
        var name = $"{id}.{version}.nupkg";
        CleanDirectories ($"./output/{id}");
        Unzip ($"./output/nugets/{name}", $"./output/{id}/nuget");
    }
});

Task ("docs-download-output")
    .IsDependentOn ("docs-download-build-artifact")
    .IsDependentOn ("docs-expand-build-artifact");

Task ("docs-api-diff")
    .Does (async () =>
{
    var baseDir = "./output/nugets/api-diff";
    CleanDirectories (baseDir);

    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    foreach (var id in TRACKED_NUGETS.Keys) {
        Information ($"Comparing the assemblies in '{id}'...");

        var version = GetVersion (id);
        var latestVersion = (await NuGetVersions.GetLatestAsync (id))?.ToNormalizedString ();
        Debug ($"Version '{latestVersion}' is the latest version of '{id}'...");

        // pre-cache so we can have better logs
        if (!string.IsNullOrEmpty (latestVersion)) {
            Debug ($"Caching version '{latestVersion}' of '{id}'...");
            await comparer.ExtractCachedPackageAsync (id, latestVersion);
        }

        // generate the diff and copy to the changelogs
        Debug ($"Running a diff on '{latestVersion}' vs '{version}' of '{id}'...");
        var diffRoot = $"{baseDir}/{id}";
        using (var reader = new PackageArchiveReader ($"./output/nugets/{id.ToLower ()}.{version}.nupkg")) {
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

        Information ($"Diff complete of '{id}'.");
    }
});

Task ("docs-api-diff-past")
    .Does (async () =>
{
    var baseDir = "./output/api-diffs-past";
    CleanDirectories (baseDir);

    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    foreach (var id in TRACKED_NUGETS.Keys) {
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
    EnsureDirectoryExists (docsTempPath);
    CleanDirectories (docsTempPath);

    // get a comparer that will download the nugets
    var comparer = await CreateNuGetDiffAsync ();

    // generate the temp frameworks.xml
    var xFrameworks = new XElement ("Frameworks");
    foreach (var id in TRACKED_NUGETS.Keys) {
        // get the versions
        Information ($"Comparing the assemblies in '{id}'...");
        var allVersions = await NuGetVersions.GetAllAsync (id, new NuGetVersions.Filter {
            MinimumVersion = new NuGetVersion (TRACKED_NUGETS [id])
        });

        // add the current dev version to the mix
        var dev = new NuGetVersion (GetVersion (id));
        allVersions = allVersions.Union (new [] { dev }).ToArray ();

        foreach (var version in allVersions) {
            Information ($"Downloading '{id}' version '{version}'...");
            // get the path to the nuget contents
            var packagePath = version == dev
                ? $"./output/{id}/nuget"
                : await comparer.ExtractCachedPackageAsync (id, version);

            var dirs =
                GetPlatformDirectories ($"{packagePath}/lib").Union(
                GetPlatformDirectories ($"{packagePath}/ref"));
            foreach (var (path, platform) in dirs) {
                string moniker;
                if (id.StartsWith ("SkiaSharp.Views.Forms"))
                    if (id != "SkiaSharp.Views.Forms")
                        continue;
                    else
                        moniker = $"skiasharp-views-forms-{version}";
                else if (id.StartsWith ("SkiaSharp.Views"))
                    moniker = $"skiasharp-views-{version}";
                else if (platform == null)
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{version}";
                else
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{platform}-{version}";

                // add the node to the frameworks.xml
                if (xFrameworks.Elements ("Framework")?.Any (e => e.Attribute ("Name").Value == moniker) != true) {
                    xFrameworks.Add (
                        new XElement ("Framework",
                            new XAttribute ("Name", moniker),
                            new XAttribute ("Source", moniker)));
                }

                // copy the assemblies for the tool
                var o = $"{docsTempPath}/{moniker}";
                EnsureDirectoryExists (o);
                CopyFiles ($"{path}/*.dll", o);
            }
        }
    }

    // save the frameworks.xml
    var fwxml = $"{docsTempPath}/frameworks.xml";
    var xdoc = new XDocument (xFrameworks);
    xdoc.Save (fwxml);

    // generate doc files
    comparer = await CreateNuGetDiffAsync ();
    var refArgs = string.Join (" ", comparer.SearchPaths.Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute ((FilePath) fwxml);
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = $"update --debug --delete --out=\"{DOCS_PATH}\" --lang=DocId --frameworks={fw} {refArgs}",
        WorkingDirectory = docsTempPath
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
                .Element ("Attributes")
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
