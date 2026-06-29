// This script holds the mdoc-based docs/ XML generators (docs-update-frameworks,
// docs-format-docs). The API-diff engine (docs-api-diff / docs-api-diff-past)
// lives alongside this file at
// scripts/infra/docs/api-diff.cake; its behavior spec is
// documentation/dev/release-notes-and-api-diffs.md. The two only share the
// NuGet-diff comparer + layout helpers, which live in
// api-diff-tools.cake (alongside this file) and are #loaded by both.

#tool nuget:?package=mdoc&version=5.8.9

using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/download.cake"
#load "api-diff-tools.cake"

// Count every type (including nested) in an assembly. Used to keep the richest
// build when several TFM folders contribute an assembly with the same file name
// to a single docs moniker (see the staging loop in docs-update-frameworks).
int CountAssemblyTypes (string path)
{
    try {
        using (var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly (path))
            return asm.Modules.Sum (m => m.GetTypes ().Count ());
    } catch (Exception ex) {
        Warning ("Could not read types from '{0}': {1}", path, ex.Message);
        return 0;
    }
}

// mdoc ships as a .NET Framework executable (mdoc.exe). On Windows it runs natively;
// on Linux/macOS (Linux CI and the local docs Docker image) the very same assembly is
// launched through mono. Centralising the launch here keeps the docs pipeline
// host-independent, so the auto-api-docs-writer workflow runs on Linux + mono.
void RunMdoc (string arguments, DirectoryPath workingDirectory)
{
    var mdoc = Context.Tools.Resolve ("mdoc.exe");
    if (mdoc == null)
        throw new Exception ("Could not resolve 'mdoc.exe' (the #tool nuget:?package=mdoc restore may have failed).");

    if (IsRunningOnWindows ()) {
        RunProcess (mdoc, new ProcessSettings {
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        });
    } else {
        RunProcess ("mono", new ProcessSettings {
            Arguments = $"\"{mdoc.FullPath}\" {arguments}",
            WorkingDirectory = workingDirectory
        });
    }
}

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
        //
        // Document the reference assemblies (ref/) when the package ships them: ref/ is
        // the canonical public API surface. The implementation assemblies under lib/
        // also carry members that are excluded from the public contract — notably
        // [Obsolete(..., error: true)] overloads kept only for binary compatibility,
        // e.g. the by-value SKCanvas.SetMatrix(SKMatrix) sibling of SetMatrix(in SKMatrix).
        // mdoc cannot deterministically order such a pair (its member comparer ignores
        // the by-ref marker, so the two compare equal and an unstable sort swaps them on
        // every run), which made doc generation non-idempotent: a clean pass produced one
        // ordering, the next pass the other, with no fixed point. The C# compiler strips
        // those members from ref/, so documenting ref/ removes the unorderable pair and
        // makes mdoc idempotent. Only SkiaSharp.dll ships ref/; every other package
        // is lib-only and falls back to lib/ unchanged.
        var refDirs = GetPlatformDirectories ($"{packagePath}/ref").ToList ();
        var dirs = refDirs.Any ()
            ? refDirs
            : GetPlatformDirectories ($"{packagePath}/lib").ToList ();
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

            // stage this moniker's assemblies for mdoc to read. Several TFM folders
            // feed the same family moniker (e.g. every SkiaSharp.Views.* ->
            // skiasharp-views), and different TFMs can ship an assembly with the SAME
            // file name but a different API surface. For example SkiaSharp.Views.iOS.dll
            // exists for both net*-ios (which includes SKGLView / SKGLLayer /
            // SKPaintGLSurfaceEventArgs) and net*-maccatalyst (which excludes them via
            // #if !__MACCATALYST__). A plain copy lets whichever TFM is staged last win,
            // so the GL-less MacCatalyst build can clobber the richer iOS build and
            // mdoc's --delete then drops those real types from the committed docs. Keep
            // the assembly with the most types on a name collision so no platform's API
            // surface is lost.
            var o = $"{docsTempPathFrameowrks}/{moniker}";
            EnsureDirectoryExists (o);
            foreach (var dll in GetFiles ($"{path}/*.dll")) {
                FilePath dest = $"{o}/{dll.GetFilename ()}";
                if (FileExists (dest) && CountAssemblyTypes (dll.FullPath) <= CountAssemblyTypes (dest.FullPath)) {
                    Verbose ("Keeping richer staged '{0}' for moniker '{1}'; skipping copy from '{2}'.", dll.GetFilename (), moniker, path);
                    continue;
                }
                CopyFile (dll, dest);
            }
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
    // above. The comparer supplies mdoc's --lib search paths for the THIRD-PARTY
    // reference assemblies (Microsoft.iOS/macOS/MacCatalyst/tvOS refs, Maui, GTK,
    // WindowsAppSDK, ...) that SkiaSharp.Views.* assemblies depend on; mdoc fails hard
    // ("Failed to resolve assembly") without them, and they are restored into the
    // package cache rather than shipped in output/nugets.
    var comparer = await CreateNuGetDiffAsync ();

    // The SkiaSharp assemblies also reference EACH OTHER (SkiaSharp.Skottie -> SkiaSharp,
    // SkiaSharp.Views.* -> SkiaSharp, SkiaSharp.Views.Maui.Controls -> .Maui.Core,
    // SkiaSharp.HarfBuzz -> HarfBuzzSharp, ...). These self-references must resolve to the
    // EXACT build being documented (e.g. SkiaSharp 4.150.0.0), so add every staged moniker
    // directory — the output assemblies laid out above — as a --lib path. This replaces the
    // old self-dependency glob over every cached NuGet version (removed from
    // CreateNuGetDiffAsync for determinism, since Mono.Cecil binds by simple name and the
    // first cached version won): the staged output assemblies are the precise, single,
    // host-independent set, so mdoc resolves inter-SkiaSharp types deterministically.
    var selfLibs = GetSubDirectories (docsTempPathFrameowrks)
        .Select (d => d.FullPath)
        .OrderBy (p => p, StringComparer.Ordinal);
    var refArgs = string.Join (" ",
        comparer.SearchPaths.Concat (selfLibs).Select (r => $"--lib=\"{r}\""));
    var fw = MakeAbsolute ((FilePath) fwxml);
    RunMdoc (
        $"update --debug --delete --out=\"{DOCS_PATH}\" --lang=DocId --frameworks={fw} {refArgs}",
        docsTempPathFrameowrks);

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

    // Load the authoritative set of assemblies that were actually built and
    // documented this run, straight from the freshly generated FrameworksIndex.
    // mdoc accumulates an <AssemblyInfo> per assembly that has ever documented a
    // type/member and never removes them, so when a package is dropped or renamed
    // (e.g. the old SkiaSharp.Views.Gtk split into Gtk3/Gtk4, or the obsolete
    // SkiaSharp.Views.Maui.Controls.Compatibility) its <AssemblyInfo> lingers on
    // types that still exist in a current assembly. The loop below uses this set to
    // strip those stale assembly references.
    var validAssemblies = new HashSet<string> (StringComparer.Ordinal);
    var frameworksIndexPath = $"{DOCS_PATH}/FrameworksIndex";
    if (DirectoryExists (frameworksIndexPath)) {
        foreach (var indexFile in GetFiles ($"{frameworksIndexPath}/*.xml")) {
            foreach (var asm in XDocument.Load (indexFile.FullPath).Descendants ("Assembly"))
                validAssemblies.Add (asm.Attribute ("Name")?.Value);
        }
    }

    // Load the set of namespaces that still contain at least one type, straight
    // from this run's index.xml. Like assemblies above, mdoc never removes a
    // <Namespace> entry (nor its ns-*.xml stub file) when the last type in it goes
    // away, so a dropped package leaves behind an empty <Namespace></Namespace> in
    // index.xml and an orphan ns-*.xml. The Overview pass below removes the empty
    // index.xml entries; the Namespace pass deletes any ns-*.xml not in this set.
    var validNamespaces = new HashSet<string> (StringComparer.Ordinal);
    var indexFilePath = $"{DOCS_PATH}/index.xml";
    if (System.IO.File.Exists (indexFilePath)) {
        foreach (var ns in XDocument.Load (indexFilePath).Root.Elements ("Types").Elements ("Namespace")) {
            if (ns.Elements ("Type").Any ())
                validNamespaces.Add (ns.Attribute ("Name")?.Value);
        }
    }

    foreach (var file in docFiles) {
        Debug("Processing {0}...", file.FullPath);

        var xdoc = XDocument.Load (file.FullPath);

        // Delete orphan namespace stub files (ns-*.xml) whose namespace no longer
        // has any types in index.xml (see validNamespaces above). mdoc leaves these
        // behind when a package stops being built; this keeps the stub files in
        // lockstep with the live namespaces in index.xml. The global namespace
        // (Name="", file ns-.xml) is always emitted by mdoc and is kept as-is.
        if (xdoc.Root.Name == "Namespace") {
            var nsName = xdoc.Root.Attribute ("Name")?.Value ?? "";
            if (nsName.Length > 0 && !validNamespaces.Contains (nsName)) {
                DeleteFile (file);
                continue;
            }
        }

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

        // Drop stale entries that mdoc accumulated in index.xml for packages that
        // are no longer built: <Assembly> overview entries whose assembly is not in
        // this run's build (see validAssemblies), and <Namespace> entries left empty
        // because every type in them was deleted. The orphan ns-*.xml stub files for
        // those emptied namespaces are deleted by the Namespace pass above.
        if (xdoc.Root.Name == "Overview") {
            if (validAssemblies.Count > 0) {
                xdoc.Root
                    .Elements ("Assemblies")
                    .Elements ("Assembly")
                    .Where (e => !validAssemblies.Contains (e.Attribute ("Name")?.Value))
                    .Remove ();
            }
            xdoc.Root
                .Elements ("Types")
                .Elements ("Namespace")
                .Where (e => !e.Elements ("Type").Any ())
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

        // Strip <AssemblyInfo> blocks for assemblies that no longer exist in the
        // current build (not present in this run's FrameworksIndex). A type/member
        // can be documented by several assemblies at once; when one is dropped or
        // renamed, mdoc leaves its stale <AssemblyInfo> behind. Remove those, then
        // drop any member that is left with none (it only lived in the dropped
        // assembly) and delete the file if the type itself ends up with none. Members
        // that never carried an <AssemblyInfo> (they inherit the type's) are untouched.
        if (xdoc.Root.Name == "Type" && validAssemblies.Count > 0) {
            bool IsStale (XElement info) =>
                !validAssemblies.Contains (info.Element ("AssemblyName")?.Value);

            foreach (var member in xdoc.Root.Elements ("Members").Elements ("Member").ToArray ()) {
                var infos = member.Elements ("AssemblyInfo").ToArray ();
                if (infos.Length == 0)
                    continue;
                infos.Where (IsStale).Remove ();
                if (!member.Elements ("AssemblyInfo").Any ())
                    member.Remove ();
            }

            xdoc.Root.Elements ("AssemblyInfo").Where (IsStale).ToArray ().Remove ();
            if (!xdoc.Root.Elements ("AssemblyInfo").Any ()) {
                DeleteFile (file);
                continue;
            }
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

        // Drop the obsolete SkiaSharp.GrVkYcbcrConversionInfo doc page. It is a soft
        // [Obsolete] forwarder that differs from its replacement,
        // SkiaSharp.GRVkYcbcrConversionInfo, only by the case of a single letter
        // ('r' vs 'R'). mdoc documents both (soft-obsolete types are kept), producing two
        // ECMA-XML files whose names differ only by case. Those cannot coexist on the
        // case-insensitive filesystem the Learn/OpenPublishing build runs on - one shadows
        // the other, so crefs to the shadowed type fail with 'Cross reference not found'.
        // This is the only such case-colliding pair in the API, and the legacy type merely
        // forwards to the modern one (its only members are the two implicit conversion
        // operators), so drop its page so only the modern type is published and its xref
        // always resolves. Mirrors the SkiaSharp.Views.Android.Resource handling above.
        if (xdoc.Root.Name == "Type") {
            var ycbcrName = xdoc.Root.Attribute ("FullName")?.Value;
            if (ycbcrName == "SkiaSharp.GrVkYcbcrConversionInfo") {
                DeleteFile (file);
                continue;
            }
        }
        if (xdoc.Root.Name == "Overview") {
            xdoc.Root.Descendants ("Type")
                .Where (t => t.Attribute ("Name")?.Value == "GrVkYcbcrConversionInfo")
                .ToArray ()
                .Remove ();
        }
        if (xdoc.Root.Name == "Framework") {
            xdoc.Root.Descendants ("Type")
                .Where (t => t.Attribute ("Name")?.Value == "SkiaSharp.GrVkYcbcrConversionInfo")
                .ToArray ()
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

// ===========================================================================
// Docs QA gates (no LLM) — used by the api-docs skill + auto-api-docs-writer.
//
// Agents edit the ECMA/mdoc XML directly, so these three targets give the
// deterministic gates the skill relies on:
//
//   docs-resolve-scope   List the docs to work on (+ candidate C# source path)
//                        for sharding. Driven by --scope=<all|new|changed|file:PATH>.
//   docs-lint            Objective defect scan -> machine-parseable findings.
//   docs-validate        Post-edit structural safety vs the git baseline
//                        (fails the build on any regression).
//
// Output is a stable, machine-parseable line contract (FILE/COUNT/SEVERITY/
// LINT-SUMMARY/VALIDATE/VALIDATE-SUMMARY) consumed by the skill + workflow.
//
// Paths default to the in-repo submodule layout but can be overridden so the
// tool works when the docs repo is the *primary* checkout (the gh-aw
// auto-api-docs-writer sandbox clones SkiaSharp as a secondary repo and
// downloads the docs separately). QA_REPO_ROOT stays pointed at the SkiaSharp
// clone so binding/ source lookups keep working while docs come from elsewhere.
// ===========================================================================

string QA_REPO_ROOT = ROOT_PATH.FullPath;
string QA_DOCS_GIT = MakeAbsolute (Directory (
    Argument ("docsGitRoot", EnvironmentVariable ("DOCS_GIT_ROOT") ?? DOCS_ROOT_PATH.FullPath))).FullPath;
string QA_DOCS_XML = MakeAbsolute (Directory (
    Argument ("docsDir", EnvironmentVariable ("DOCS_DIR") ?? DOCS_PATH.FullPath))).FullPath;
string QA_OBSOLETE_MAP = MakeAbsolute (File (
    Argument ("obsoleteMap", EnvironmentVariable ("OBSOLETE_MAP")
        ?? ROOT_PATH.CombineWithFilePath (".agents/skills/api-docs/references/obsolete-api-map.md").FullPath))).FullPath;
string QA_REVIEW_BASE = Argument ("docsReviewBase", EnvironmentVariable ("DOCS_REVIEW_BASE") ?? "origin/main");
string QA_SCOPE = Argument ("scope", "");

var QA_MISSPELL = new Dictionary<string, string> {
    { "teh", "the" }, { "recieve", "receive" }, { "seperate", "separate" }, { "occured", "occurred" },
    { "paramter", "parameter" }, { "retreive", "retrieve" }, { "initalize", "initialize" },
    { "lenght", "length" }, { "widht", "width" }, { "colour", "color" }, { "visable", "visible" },
    { "arguement", "argument" }, { "depricated", "deprecated" }, { "existant", "existent" }
};
var QA_CREF_PREFIXES = new [] { "T:", "M:", "P:", "F:", "E:", "N:", "Overload:" };

bool QaIsGenerated (string path)
{
    var name = System.IO.Path.GetFileName (path);
    if (name == "index.xml" || name == "_filter.xml") return true;
    if (name.StartsWith ("ns-") && name.EndsWith (".xml")) return true;
    if (Regex.IsMatch (path, @"[\\/]FrameworksIndex[\\/]")) return true;
    return false;
}

XmlDocument QaLoadXml (string path)
{
    var doc = new XmlDocument { PreserveWhitespace = true };
    doc.Load (path);
    return doc;
}

int QaCountSignatures (XmlDocument doc) =>
    doc.SelectNodes ("//MemberSignature").Count + doc.SelectNodes ("//TypeSignature").Count;

string QaRelTo (string root, string abs)
{
    var full = abs;
    try { full = System.IO.Path.GetFullPath (abs); } catch { }
    var r = root.Replace ('\\', '/').TrimEnd ('/');
    var f = full.Replace ('\\', '/');
    if (f.StartsWith (r)) return f.Substring (r.Length).TrimStart ('/');
    return f;
}

string QaRelToRepo (string abs) => QaRelTo (QA_REPO_ROOT, abs);
string QaRelToDocs (string abs) => QaRelTo (QA_DOCS_GIT, abs);

string QaAttr (XmlNode n, string name) => (n as XmlElement)?.GetAttribute (name);

// Best-effort C# source path for a docs xml file.
string QaSourcePath (string xmlPath)
{
    var type = System.IO.Path.GetFileNameWithoutExtension (xmlPath);
    var ns = System.IO.Path.GetFileName (System.IO.Path.GetDirectoryName (xmlPath));
    var candidates = new [] {
        $"{QA_REPO_ROOT}/binding/{ns}/{type}.cs",
        $"{QA_REPO_ROOT}/binding/SkiaSharp/{type}.cs",
        $"{QA_REPO_ROOT}/binding/HarfBuzzSharp/{type}.cs",
    };
    foreach (var c in candidates)
        if (System.IO.File.Exists (c)) return QaRelToRepo (c);
    var hit = GetFiles ($"{QA_REPO_ROOT}/binding/**/{type}.cs").FirstOrDefault ();
    if (hit != null) return QaRelToRepo (hit.FullPath);
    return "NONE";
}

List<string> QaGit (string args, string wd, out int exit)
{
    IEnumerable<string> output = null;
    var settings = new ProcessSettings {
        Arguments = args,
        WorkingDirectory = wd,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    try { exit = StartProcess ("git", settings, out output); }
    catch { exit = -1; output = null; }
    return output?.ToList () ?? new List<string> ();
}

// Scope core (shared by docs-resolve-scope, docs-lint, docs-validate).
List<string> QaScopeFiles (string sel)
{
    if (string.IsNullOrEmpty (sel))
        throw new Exception ("a mode is required (--scope=all|new|changed|file:PATH)");

    var files = new List<string> ();
    var m = Regex.Match (sel, @"^file:(.+)$");
    if (m.Success) {
        var p = m.Groups[1].Value;
        if (!System.IO.Path.IsPathRooted (p)) p = System.IO.Path.Combine (QA_REPO_ROOT, p);
        if (System.IO.File.Exists (p)) files.Add (System.IO.Path.GetFullPath (p));
    } else if (sel == "all") {
        foreach (var f in GetFiles ($"{QA_DOCS_XML}/**/*.xml"))
            if (!QaIsGenerated (f.FullPath)) files.Add (f.FullPath);
    } else if (sel == "new" || sel == "changed") {
        // Files changed vs the baseline (or, when there is no baseline diff,
        // the working tree). Right after mdoc regen the freshly written stubs
        // show up here, which is how the add pass finds new placeholders.
        int exit;
        var diff = QaGit ($"diff --name-only --diff-filter=ACM {QA_REVIEW_BASE}...HEAD", QA_DOCS_GIT, out exit)
            .Where (l => !string.IsNullOrWhiteSpace (l)).ToList ();
        if (diff.Count == 0)
            diff = QaGit ("diff --name-only --diff-filter=ACM", QA_DOCS_GIT, out exit)
                .Where (l => !string.IsNullOrWhiteSpace (l)).ToList ();
        foreach (var d in diff) {
            if (!d.EndsWith (".xml")) continue;
            var abs = System.IO.Path.Combine (QA_DOCS_GIT, d);
            if (System.IO.File.Exists (abs) && !QaIsGenerated (abs))
                files.Add (System.IO.Path.GetFullPath (abs));
        }
    } else {
        throw new Exception ($"Unrecognized mode '{sel}' (use all|new|changed|file:PATH)");
    }

    return files.Distinct ().OrderBy (x => x, StringComparer.Ordinal).ToList ();
}

// Resolve a path-or-mode argument to a concrete file list (for lint/validate).
List<string> QaExpandTarget (string arg)
{
    if (string.IsNullOrEmpty (arg))
        throw new Exception ("a path or mode is required (--scope=all|new|changed|file:PATH)");
    if (arg.StartsWith ("file:") || arg == "all" || arg == "new" || arg == "changed")
        return QaScopeFiles (arg);
    var p = arg;
    if (!System.IO.Path.IsPathRooted (p)) p = System.IO.Path.Combine (QA_REPO_ROOT, p);
    if (System.IO.File.Exists (p)) return new List<string> { System.IO.Path.GetFullPath (p) };
    if (System.IO.Directory.Exists (p))
        return GetFiles ($"{p}/**/*.xml").Select (f => f.FullPath).Where (f => !QaIsGenerated (f)).ToList ();
    throw new Exception ($"Target not found: {arg}");
}

// Parse the fenced ```obsolete-map table -> member tokens flagged 'error'.
List<string> QaObsoleteErrorMembers ()
{
    var members = new List<string> ();
    if (!System.IO.File.Exists (QA_OBSOLETE_MAP)) return members;
    var inBlock = false;
    foreach (var line in System.IO.File.ReadAllLines (QA_OBSOLETE_MAP)) {
        if (line.StartsWith ("```obsolete-map")) { inBlock = true; continue; }
        if (inBlock && line.StartsWith ("```")) break;
        if (!inBlock) continue;
        var cols = line.Split ('|').Select (c => c.Trim ()).ToArray ();
        if (cols.Length < 4) continue;
        if (cols[0] == "Type" || cols[0] == "") continue;
        if (cols[3] != "error") continue;
        var member = Regex.Replace (cols[1], @"\(.*$", "").Trim ();
        if (!string.IsNullOrEmpty (member)) members.Add (member);
    }
    return members.Distinct ().OrderBy (x => x, StringComparer.Ordinal).ToList ();
}

string QaMemberDocId (XmlNode member)
{
    var n = member.SelectSingleNode ("MemberSignature[@Language='DocId']");
    return n == null ? null : QaAttr (n, "Value");
}

int QaEmit (string sev, string cls, string file, string docId, string msg)
{
    if (string.IsNullOrEmpty (docId)) docId = "-";
    Console.WriteLine ($"{sev} | {cls} | {file} | {docId} | {msg}");
    return 1;
}

// Prose text for natural-language checks (repeated word, spelling). One string
// per descendant text/CDATA node, with fenced code blocks stripped out of CDATA.
// Splitting per text node means empty inline elements (e.g. <see cref=".." />)
// act as boundaries, so words on either side are never treated as adjacent.
List<string> QaProseSegments (XmlNode node)
{
    var segments = new List<string> ();
    foreach (XmlNode t in node.SelectNodes (".//text()")) {
        var val = t.Value;
        if (string.IsNullOrWhiteSpace (val)) continue;
        if (t.NodeType == XmlNodeType.CDATA)
            val = Regex.Replace (val, "(?s)```.*?```", " ");
        segments.Add (val);
    }
    return segments;
}

int QaLintFile (string xmlPath, List<string> obsoleteMembers)
{
    var rel = QaRelToRepo (xmlPath);
    var count = 0;
    XmlDocument doc;
    try { doc = QaLoadXml (xmlPath); }
    catch (Exception ex) { return QaEmit ("CRITICAL", "malformed-xml", rel, "-", $"XML will not parse: {ex.Message}"); }

    var root = doc.DocumentElement;
    var units = new List<(string docId, XmlNode docs, bool isProp, bool hasSet)> ();

    // type-level Docs
    var typeDocs = root.SelectSingleNode ("Docs");
    if (typeDocs != null) {
        var fn = QaAttr (root, "FullName");
        var tn = "T:" + (!string.IsNullOrEmpty (fn) ? fn : QaAttr (root, "Name"));
        units.Add ((tn, typeDocs, false, false));
    }
    // each Member's Docs
    foreach (XmlNode mn in root.SelectNodes ("Members/Member")) {
        var d = mn.SelectSingleNode ("Docs");
        if (d == null) continue;
        var sig = QaAttr (mn.SelectSingleNode ("MemberSignature[@Language='C#']"), "Value") ?? "";
        var mt = mn.SelectSingleNode ("MemberType")?.InnerText;
        var isProp = mt == "Property";
        var hasSet = isProp && Regex.IsMatch (sig, @"set\s*;");
        units.Add ((QaMemberDocId (mn), d, isProp, hasSet));
    }
    // MemberGroup carries shared remarks/examples for overload sets (e.g. DrawText)
    foreach (XmlNode g in root.SelectNodes ("Members/MemberGroup")) {
        var d = g.SelectSingleNode ("Docs");
        if (d == null) continue;
        var gname = QaAttr (g, "MemberName");
        var fn = QaAttr (root, "FullName");
        var tn = !string.IsNullOrEmpty (fn) ? fn : QaAttr (root, "Name");
        units.Add (($"G:{tn}.{gname}", d, false, false));
    }

    foreach (var u in units) {
        var docs = u.docs;
        var docId = u.docId;

        // Empty summary/value/returns (remarks may be self-closing)
        foreach (var tag in new [] { "summary", "value", "returns" }) {
            var node = docs.SelectSingleNode (tag);
            if (node != null && string.IsNullOrWhiteSpace (node.InnerXml))
                count += QaEmit ("IMPORTANT", "empty-tag", rel, docId, $"<{tag}> is empty");
        }

        // Per-field text checks
        foreach (XmlNode node in docs.ChildNodes) {
            if (node.NodeType != XmlNodeType.Element) continue;
            var inner = node.InnerXml;
            var text = node.InnerText;
            if (string.IsNullOrWhiteSpace (inner)) continue;

            // Placeholder
            if (Regex.IsMatch (text, @"To be added\.?"))
                count += QaEmit ("IMPORTANT", "placeholder", rel, docId, $"<{node.Name}> still 'To be added.'");
            if (inner.Contains ("[Describe ") || inner.Contains ("[Show "))
                count += QaEmit ("IMPORTANT", "placeholder", rel, docId, $"<{node.Name}> has an unfilled remarks scaffold");

            // Repeated words + spelling run on prose only (code fences stripped;
            // per text node so empty inline elements don't fuse adjacent words)
            var reportedRepeat = false;
            foreach (var seg in QaProseSegments (node)) {
                if (!reportedRepeat) {
                    var rm = Regex.Match (seg, @"(?<![-\w])([A-Za-z]{2,})\s+\1\b", RegexOptions.IgnoreCase);
                    if (rm.Success && rm.Groups[1].Value.ToLowerInvariant () != "that" && rm.Groups[1].Value.ToLowerInvariant () != "had") {
                        count += QaEmit ("CRITICAL", "repeated-word", rel, docId, $"repeated word '{rm.Groups[1].Value}' in <{node.Name}>");
                        reportedRepeat = true;
                    }
                }
                foreach (var kv in QA_MISSPELL) {
                    if (Regex.IsMatch (seg, $@"\b{kv.Key}\b", RegexOptions.IgnoreCase))
                        count += QaEmit ("CRITICAL", "spelling", rel, docId, $"'{kv.Key}' -> '{kv.Value}' in <{node.Name}>");
                }
            }

            // see cref prefix
            foreach (Match mm in Regex.Matches (inner, @"<see\s+cref=""([^""]+)""")) {
                var target = mm.Groups[1].Value;
                if (!QA_CREF_PREFIXES.Any (p => target.StartsWith (p)))
                    count += QaEmit ("IMPORTANT", "invalid-cref", rel, docId, $"<see cref='{target}'> missing DocId prefix (T:/M:/P:/F:)");
            }

            // xref prefix inside CDATA remarks
            foreach (Match mm in Regex.Matches (inner, @"<xref:(T:|M:|P:|F:)"))
                count += QaEmit ("IMPORTANT", "bad-xref", rel, docId, $"<xref:{mm.Groups[1].Value}...> uses a DocId prefix; xref takes the bare UID");
            // destroyed CDATA (escaped xref)
            if (inner.Contains ("&lt;xref:"))
                count += QaEmit ("CRITICAL", "broken-cdata", rel, docId, "escaped '&lt;xref:' — CDATA was destroyed");

            // Obsolete members in csharp fences
            foreach (Match fence in Regex.Matches (inner, @"(?s)```csharp(.*?)```")) {
                var code = fence.Groups[1].Value;
                foreach (var om in obsoleteMembers)
                    if (Regex.IsMatch (code, @"\." + Regex.Escape (om) + @"\b"))
                        count += QaEmit ("CRITICAL", "obsolete-in-example", rel, docId, $"example uses obsolete member '.{om}' (see obsolete-api-map.md)");
            }
        }

        // Accessor verb mismatch (properties)
        if (u.isProp) {
            var summary = docs.SelectSingleNode ("summary");
            if (summary != null && !string.IsNullOrWhiteSpace (summary.InnerText)) {
                var s = summary.InnerText.TrimStart ();
                if (u.hasSet) {
                    if (Regex.IsMatch (s, @"^Gets\b", RegexOptions.IgnoreCase) && !Regex.IsMatch (s, @"^Gets or sets\b", RegexOptions.IgnoreCase))
                        count += QaEmit ("IMPORTANT", "accessor-verb", rel, docId, "settable property summary should be 'Gets or sets'");
                } else {
                    if (Regex.IsMatch (s, @"^Gets or sets\b", RegexOptions.IgnoreCase))
                        count += QaEmit ("IMPORTANT", "accessor-verb", rel, docId, "read-only property summary should be 'Gets' (not 'Gets or sets')");
                }
            }
        }
    }
    return count;
}

// Clone, blank every <Docs> body, return OuterXml of everything-but-Docs.
string QaStrippedShape (XmlDocument doc)
{
    var clone = new XmlDocument { PreserveWhitespace = true };
    clone.LoadXml (doc.OuterXml);
    foreach (XmlNode d in clone.SelectNodes ("//Docs")) d.InnerXml = "";
    return clone.OuterXml.Replace ("\r\n", "\n").Trim ();
}

bool QaValidateFile (string xmlPath)
{
    var rel = QaRelToRepo (xmlPath);
    XmlDocument cur;
    try { cur = QaLoadXml (xmlPath); }
    catch (Exception ex) { Console.WriteLine ($"VALIDATE | FAIL | {rel} | not well-formed: {ex.Message}"); return false; }

    var docsRel = QaRelToDocs (xmlPath);
    int exit;
    var baseLines = QaGit ($"show HEAD:{docsRel}", QA_DOCS_GIT, out exit);
    if (exit != 0 || baseLines.Count == 0) {
        Console.WriteLine ($"VALIDATE | OK | {rel} | new file (well-formed; no baseline)");
        return true;
    }

    var baseDoc = new XmlDocument { PreserveWhitespace = true };
    baseDoc.LoadXml (string.Join ("\n", baseLines));

    var cb = QaCountSignatures (baseDoc);
    var cc = QaCountSignatures (cur);
    if (cb != cc) {
        Console.WriteLine ($"VALIDATE | FAIL | {rel} | signature count changed ({cb} -> {cc})");
        return false;
    }
    if (QaStrippedShape (baseDoc) != QaStrippedShape (cur)) {
        Console.WriteLine ($"VALIDATE | FAIL | {rel} | content changed outside <Docs>");
        return false;
    }
    Console.WriteLine ($"VALIDATE | OK | {rel} | docs-only, signatures preserved");
    return true;
}

Task ("docs-resolve-scope")
    .Description ("List the docs to work on (+ candidate C# source path). --scope=all|new|changed|file:PATH")
    .Does (() =>
{
    var files = QaScopeFiles (QA_SCOPE);
    foreach (var f in files)
        Console.WriteLine ($"FILE | {QaRelToRepo (f)} | source:{QaSourcePath (f)}");
    Console.WriteLine ($"COUNT | {files.Count}");
});

Task ("docs-lint")
    .Description ("Objective defect scan over docs XML -> machine findings. --scope=all|new|changed|file:PATH")
    .Does (() =>
{
    var targets = QaExpandTarget (QA_SCOPE);
    var obs = QaObsoleteErrorMembers ();
    int total = 0, n = 0;
    foreach (var t in targets) { total += QaLintFile (t, obs); n++; }
    Console.WriteLine ($"LINT-SUMMARY | files:{n} | findings:{total}");
});

Task ("docs-validate")
    .Description ("Post-edit structural safety vs the git baseline. --scope=all|new|changed|file:PATH")
    .Does (() =>
{
    var targets = QaExpandTarget (QA_SCOPE);
    int fail = 0, n = 0;
    foreach (var t in targets) { if (!QaValidateFile (t)) fail++; n++; }
    Console.WriteLine ($"VALIDATE-SUMMARY | files:{n} | failures:{fail}");
    if (fail > 0)
        throw new Exception ($"docs-validate failed for {fail} file(s)");
});

Task ("update-docs")
    .Description ("Regenerate all docs.")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");

Task ("Default")
    .IsDependentOn ("update-docs");


RunTarget(TARGET);
