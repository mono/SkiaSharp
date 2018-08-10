
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
        var mdFiles = $"{path}/*.diff.md";
        ReplaceTextInFiles (mdFiles, "<h4>", "> ");
        ReplaceTextInFiles (mdFiles, "</h4>", Environment.NewLine);
        ReplaceTextInFiles (mdFiles, "\r\r", "\r");
        foreach (var file in GetFiles (mdFiles)) {
            var dllName = file.GetFilenameWithoutExtension ().GetFilenameWithoutExtension ().GetFilenameWithoutExtension ();
            var changelogPath = (FilePath)$"./changelogs/{id}/{version}/{dllName}.md";
            EnsureDirectoryExists (changelogPath.GetDirectory ());
            CopyFile (file, changelogPath);
        }
    }
}

Task ("docs-api-diff")
    .Does (async () =>
{
    var baseDir = "./output/api-diff";
    CleanDirectories (baseDir);

    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    foreach (var id in TRACKED_NUGETS.Keys) {
        Information ($"Comparing the assemblies in '{id}'...");

        var version = GetVersion (id);
        var latestVersion = (await NuGetVersions.GetLatestAsync (id)).ToNormalizedString ();
        Debug ($"Version '{latestVersion}' is the latest version of '{id}'...");

        // pre-cache so we can have better logs
        Debug ($"Caching version '{latestVersion}' of '{id}'...");
        await comparer.ExtractCachedPackageAsync (id, latestVersion);

        // generate the diff and copy to the changelogs
        Debug ($"Running a diff on '{latestVersion}' vs '{version}' of '{id}'...");
        var diffRoot = $"{baseDir}/{id}";
        using (var reader = new PackageArchiveReader ($"./output/nugets/{id.ToLower ()}.{version}.nupkg")) {
            await comparer.SaveCompleteDiffToDirectoryAsync (id, latestVersion, reader, diffRoot);
        }
        CopyChangelogs (diffRoot, id, version);

        Information ($"Diff complete of '{id}'.");
    }

    // clean up after working
    CleanDirectories (baseDir);
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

            foreach (var (path, platform) in GetPlatformDirectories ($"{packagePath}/lib")) {
                string moniker;
                if (platform == null)
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{version}";
                else
                    moniker = $"{id.ToLower ().Replace (".", "-")}-{platform}-{version}";

                // add the node to the frameworks.xml
                xFrameworks.Add (
                    new XElement ("Framework",
                        new XAttribute ("Name", moniker),
                        new XAttribute ("Source", moniker)));

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
    var refArgs = string.Join (" ", comparer.SearchPaths.Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute ((FilePath) fwxml);
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = $"update --delete --out=\"{DOCS_PATH}\" -lang=DocId --frameworks={fw} {refArgs}",
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
            .Where (m => m.Attribute ("MemberName")?.Value != "Dispose" && m.Attribute ("MemberName")?.Value != "Finalize")
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

Task ("update-docs")
    .IsDependentOn ("docs-api-diff")
    .IsDependentOn ("docs-api-diff-past")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");
    