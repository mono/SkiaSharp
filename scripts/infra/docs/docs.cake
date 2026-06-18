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

    // Extract every supported package from output/nugets (the local build output).
    // Obsolete packages are not built, so they are absent here, have no version to
    // document, and simply drop out of the docs.
    foreach (var id in SUPPORTED_NUGETS.Keys) {
        var version = GetVersion (id);
        var localNugetVersion = PREVIEW_ONLY_NUGETS.Contains(id)
            ? $"{version}-{PREVIEW_NUGET_SUFFIX}"
            : version;
        var name = $"{id}.{localNugetVersion}.nupkg";
        var nupkg = $"{OUTPUT_NUGETS_PATH}/{name}";
        if (!FileExists (nupkg))
            throw new Exception ($"Could not find '{nupkg}'. Run the 'docs-download-output' target (or build the packages) to populate output/nugets first.");
        CleanDir ($"{docsTempPathNuGets}/{id}");
        Unzip (nupkg, $"{docsTempPathNuGets}/{id}");
    }

    // Build the temp frameworks.xml that tells mdoc which assemblies make up each
    // moniker. Everything is documented from the packages extracted above; the
    // packages being documented are never queried or downloaded from NuGet, so run
    // 'docs-download-output' first (or a local build) to populate output/nugets.
    var xFrameworks = new XElement ("Frameworks");
    var monikers = new List<string> ();
    foreach (var id in SUPPORTED_NUGETS.Keys) {
        // skip doc generation for Uno, this is the same as WinUI and it is not needed
        if (id.StartsWith ("SkiaSharp.Views.Uno"))
            continue;
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        // Latest-only: every package is documented from its single locally-extracted
        // version with a plain, unversioned moniker. The docs always describe just
        // the current build, so there are no per-version monikers.
        Information ($"Adding the assemblies in '{id}'...");
        var packagePath = $"{docsTempPathNuGets}/{id}";

        // Each platform/TFM directory in the package contributes its assemblies to a
        // moniker. The default moniker is the package id (skiasharp, harfbuzzsharp,
        // skiasharp-skottie, ...), but related packages are merged into one family
        // moniker (all SkiaSharp.Views.Maui.* -> skiasharp-views-maui, the other
        // SkiaSharp.Views.* -> skiasharp-views, ...) so the docs site groups them
        // instead of showing one entry per NuGet package.
        var dirs =
            GetPlatformDirectories ($"{packagePath}/lib").Union(
            GetPlatformDirectories ($"{packagePath}/ref"));
        foreach (var (path, platform) in dirs) {
            string moniker;
            if (id.StartsWith ("SkiaSharp.Views.Maui"))
                moniker = "skiasharp-views-maui";
            else if (id.StartsWith ("SkiaSharp.Views"))
                moniker = "skiasharp-views";
            else if (id.StartsWith ("SkiaSharp.Direct3D"))
                moniker = "skiasharp-direct3d";
            else if (id.StartsWith ("SkiaSharp.Vulkan"))
                moniker = "skiasharp-vulkan";
            else if (platform == null)
                moniker = $"{id.ToLower ().Replace (".", "-")}";
            else
                moniker = $"{id.ToLower ().Replace (".", "-")}-{platform}";

            // record the moniker in frameworks.xml (once per moniker)
            if (!monikers.Contains (moniker)) {
                monikers.Add (moniker);
                xFrameworks.Add (
                    new XElement ("Framework",
                        new XAttribute ("Name", moniker),
                        new XAttribute ("Source", moniker)));
            }

            // stage this moniker's assemblies for mdoc to read
            var o = $"{docsTempPathFrameowrks}/{moniker}";
            EnsureDirectoryExists (o);
            CopyFiles ($"{path}/*.dll", o);
        }
    }
    monikers.Sort ();

    // save the frameworks.xml
    var fwxml = $"{docsTempPathFrameowrks}/frameworks.xml";
    var xdoc = new XDocument (xFrameworks);
    xdoc.Save (fwxml);

    // write the generated moniker list into the docs publishing config so the docs
    // site advertises exactly the monikers produced above
    var docsJsonPath = DOCS_ROOT_PATH.CombineWithFilePath (".openpublishing.publish.config.json");
    var docsJson = ParseJsonFromFile (docsJsonPath);
    docsJson ["docsets_to_publish"][0]["monikers"] = new JArray (monikers.ToArray ());
    SerializeJsonToPrettyFile (docsJsonPath, docsJson);

    // generate doc files. The packages being documented all come from output/nugets
    // above. The comparer is created here only to supply mdoc's --lib search paths:
    // the third-party reference assemblies (Microsoft.iOS/macOS/MacCatalyst/tvOS
    // refs, Maui, GTK, WindowsAppSDK, ...) that SkiaSharp.Views.* assemblies depend
    // on. mdoc fails hard ("Failed to resolve assembly") without them, and they are
    // restored into the package cache rather than shipped in output/nugets.
    var comparer = await CreateNuGetDiffAsync ();
    var refArgs = string.Join (" ", comparer.SearchPaths.Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute ((FilePath) fwxml);
    RunProcess (Context.Tools.Resolve ("mdoc.exe"), new ProcessSettings {
        Arguments = $"update --debug --delete --out=\"{DOCS_PATH}\" --lang=DocId --frameworks={fw} {refArgs}",
        WorkingDirectory = docsTempPathFrameowrks
    });

    // mdoc only ever adds FrameworksIndex/*.xml files; it never deletes the ones for
    // monikers that are no longer generated (e.g. when a package stops being built or
    // a moniker is renamed). Prune those orphans so the FrameworksIndex stays in
    // lockstep with the monikers we just produced.
    var frameworksIndexDir = $"{DOCS_PATH}/FrameworksIndex";
    if (DirectoryExists (frameworksIndexDir)) {
        foreach (var indexFile in GetFiles ($"{frameworksIndexDir}/*.xml")) {
            var indexMoniker = indexFile.GetFilenameWithoutExtension ().ToString ();
            if (!monikers.Contains (indexMoniker)) {
                Information ("Removing orphaned framework index: {0}", indexMoniker);
                DeleteFile (indexFile);
            }
        }
    }

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

    // Load the authoritative set of monikers that docs-update-frameworks just wrote
    // to .openpublishing.publish.config.json. mdoc only ever ADDS framework tokens to
    // a member's FrameworkAlternate/FrameworkOnly attribute; it never removes ones
    // for monikers that are no longer generated. Whenever a moniker disappears (a
    // dropped or renamed package), those stale tokens are left behind and would point
    // at monikers that no longer exist. The loop below uses this set to strip them.
    var validMonikers = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
    var monikersConfigPath = DOCS_ROOT_PATH.CombineWithFilePath (".openpublishing.publish.config.json");
    if (FileExists (monikersConfigPath)) {
        var monikersJson = ParseJsonFromFile (monikersConfigPath);
        foreach (var m in (JArray) monikersJson ["docsets_to_publish"][0]["monikers"])
            validMonikers.Add ((string) m);
    }

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

        // Collapse AssemblyVersions to latest-only. mdoc accumulates one
        // <AssemblyVersion> per historical release inside each <AssemblyInfo> and
        // never removes the old ones, so the list grows forever (2.80.0.0, 2.88.0.0,
        // ... up to the current build). The authoritative current version already
        // lives in FrameworksIndex per moniker, so keep only the highest version in
        // each AssemblyInfo - the one from the current build - to match the
        // latest-only docs model and stop the perpetual growth.
        if (xdoc.Root.Name == "Type") {
            foreach (var info in xdoc.Root.Descendants ("AssemblyInfo")) {
                var versions = info.Elements ("AssemblyVersion").ToList ();
                if (versions.Count <= 1)
                    continue;
                var latest = versions
                    .OrderBy (e => System.Version.TryParse (e.Value, out var v) ? v : new System.Version (0, 0))
                    .Last ();
                foreach (var v in versions) {
                    if (v != latest)
                        v.Remove ();
                }
            }
        }

        // SKDocument.CreatePdf: keep [Obsolete] only on the one genuinely deprecated
        // overload (SKWStream, SKDocumentPdfMetadata, float) and strip it from the
        // other CreatePdf overloads, which are not actually obsolete.
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

        // SK3dView: strip the [Obsolete] attribute from the type; it is not marked
        // obsolete in the current build.
        if (xdoc.Root.Name == "Type" && xdoc.Root.Attribute ("Name")?.Value == "SK3dView") {
            xdoc.Root
                .Element ("Attributes")?
                .Elements ("Attribute")
                .SelectMany (e => e.Elements ("AttributeName"))
                .Where (e => e.Value.Contains ("System.Obsolete"))
                .Remove ();
        }

        // strip orphaned framework tokens left behind by mdoc: any
        // FrameworkAlternate/FrameworkOnly entry that references a moniker which is
        // no longer generated (a dropped or renamed package). Tokens are kept only
        // when they appear in the freshly generated moniker set; an attribute that
        // ends up empty is dropped by the block that follows.
        if (validMonikers.Count > 0) {
            foreach (var attrName in new [] { "FrameworkAlternate", "FrameworkOnly" }) {
                foreach (var el in xdoc.Root.DescendantsAndSelf ().ToArray ()) {
                    var attr = el.Attribute (attrName);
                    if (attr == null || string.IsNullOrEmpty (attr.Value))
                        continue;
                    var kept = attr.Value
                        .Split (new [] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where (t => validMonikers.Contains (t))
                        .ToArray ();
                    if (kept.Length == 0)
                        attr.Remove ();
                    else if (kept.Length != attr.Value.Split (';').Length)
                        attr.Value = string.Join (";", kept);
                }
            }
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
