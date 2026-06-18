// This script holds the mdoc-based docs/ XML generators (docs-update-frameworks,
// docs-format-docs). The API-changelog engine that used to live here
// (docs-api-diff / docs-api-diff-past) now lives in the release-notes skill at
// .agents/skills/release-notes/scripts/api-diff.cake; its behavior spec is
// documentation/dev/release-notes-and-changelogs.md. The two only share the
// NuGet-diff comparer + layout helpers, which live in
// scripts/infra/shared/api-diff-tools.cake and are #loaded by both.

#tool nuget:?package=mdoc&version=5.8.9

using System.Xml;
using System.Xml.Linq;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/download.cake"
#load "../shared/api-diff-tools.cake"


Task ("docs-download-output")
    .Does (async () =>
{
    CleanDir ("./output");

    await DownloadPackageAsync ("_nugets", OUTPUT_NUGETS_PATH);
    await DownloadPackageAsync ("_nugetspreview", OUTPUT_NUGETS_PATH);
});


Task ("docs-update-frameworks")
    .Does (async () =>
{
    // clear the temp dir
    var docsTempPath = $"{ROOT_PATH}/output/docs/temp";
    var docsTempPathFrameowrks = $"{ROOT_PATH}/output/docs/temp/frameworks";
    var docsTempPathNuGets = $"{ROOT_PATH}/output/docs/temp/nugets";
    EnsureDirectoryExists (docsTempPath);
    CleanDirectories (docsTempPath);
    EnsureDirectoryExists (docsTempPathNuGets);
    EnsureDirectoryExists (docsTempPathFrameowrks);

    // extract nugets that were built/downloaded (only supported, not obsolete)
    foreach (var id in SUPPORTED_NUGETS.Keys) {
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
        if (id.StartsWith ("SkiaSharp.Views.Uno"))
            continue;
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        // get the versions
        Information ($"Comparing the assemblies in '{id}'...");
        var allVersions = await NuGetVersions.GetAllAsync (id, new NuGetVersions.Filter {
            MinimumVersion = new NuGetVersion (TRACKED_NUGETS [id])
        });

        // add the current dev version to the mix (only for supported packages)
        var isSupported = SUPPORTED_NUGETS.ContainsKey(id);
        var dev = isSupported ? new NuGetVersion (GetVersion (id)) : null;
        if (dev != null)
            allVersions = allVersions.Union (new [] { dev }).ToArray ();

        // "merge" the patches so we only care about major.minor
        var merged = new Dictionary<string, NuGetVersion> ();
        foreach (var version in allVersions) {
            merged [$"{version.Major}.{version.Minor}"] = version;
        }

        foreach (var version in merged) {
            Information ($"Downloading '{id}' version '{version}'...");
            // get the path to the nuget contents
            var packagePath = (isSupported && version.Value == dev)
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
                else if (id.StartsWith ("SkiaSharp.Direct3D"))
                    moniker = $"skiasharp-direct3d-{version.Key}";
                else if (id.StartsWith ("SkiaSharp.Vulkan"))
                    moniker = $"skiasharp-vulkan-{version.Key}";
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
    RunProcess (Context.Tools.Resolve ("mdoc.exe"), new ProcessSettings {
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
    var docFiles = GetFiles ($"{ROOT_PATH}/docs/**/*.xml");
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

    // sync extension method docs from type files to index.xml
    var indexPath = $"{DOCS_PATH}/index.xml";
    if (FileExists (indexPath)) {
        Information ("Syncing extension method documentation to index.xml...");
        var indexDoc = XDocument.Load (indexPath);
        var synced = 0;
        
        foreach (var extMethod in indexDoc.Descendants ("ExtensionMethod").ToArray ()) {
            var link = extMethod.Element ("Member")?.Element ("Link");
            if (link == null) continue;
            
            var typeName = link.Attribute ("Type")?.Value;
            var memberDocId = link.Attribute ("Member")?.Value;
            if (string.IsNullOrEmpty (typeName) || string.IsNullOrEmpty (memberDocId)) continue;
            
            // Find the source type file
            var typeFileName = typeName.Split ('.').Last () + ".xml";
            var typeFiles = GetFiles ($"{DOCS_PATH}/**/{typeFileName}");
            var typeFile = typeFiles.FirstOrDefault (f => {
                var doc = XDocument.Load (f.FullPath);
                return doc.Root?.Attribute ("FullName")?.Value == typeName;
            });
            
            if (typeFile == null) continue;
            
            var typeDoc = XDocument.Load (typeFile.FullPath);
            var sourceMember = typeDoc.Descendants ("Member")
                .FirstOrDefault (m => m.Elements ("MemberSignature")
                    .Any (s => s.Attribute ("Language")?.Value == "DocId" 
                            && s.Attribute ("Value")?.Value == memberDocId));
            
            if (sourceMember == null) continue;
            
            var sourceDocs = sourceMember.Element ("Docs");
            var targetDocs = extMethod.Element ("Member")?.Element ("Docs");
            
            if (sourceDocs != null && targetDocs != null) {
                // Check if target has "To be added." placeholders
                var hasPlaceholder = targetDocs.Descendants ()
                    .Any (e => e.Value?.Contains ("To be added.") == true);
                
                if (hasPlaceholder) {
                    targetDocs.ReplaceNodes (sourceDocs.Nodes ());
                    synced++;
                    Debug ("  Synced docs for {0}", memberDocId);
                }
            }
        }
        
        if (synced > 0) {
            Information ("Synced {0} extension method(s).", synced);
            
            // Save index.xml with proper formatting
            var settings = new XmlWriterSettings {
                Encoding = new UTF8Encoding (),
                Indent = true,
                NewLineChars = "\n",
                OmitXmlDeclaration = true,
            };
            using (var writer = XmlWriter.Create (indexPath, settings)) {
                indexDoc.Save (writer);
                writer.Flush ();
            }
            System.IO.File.AppendAllText (indexPath, "\n");
        }
    }

    // log summary
    Information (
        "Documentation missing in {0}/{1} ({2:0.0%}) types and {3}/{4} ({5:0.0%}) members.",
        typeCount, totalTypes, typeCount / totalTypes,
        memberCount, totalMembers, memberCount / totalMembers);
});

Task ("update-docs")
    .Description ("Regenerate all docs.")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");

Task ("Default")
    .IsDependentOn ("update-docs");


RunTarget(TARGET);
