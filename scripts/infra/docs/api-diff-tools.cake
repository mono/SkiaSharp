// READ FIRST: documentation/dev/release-notes-and-api-diffs.md is the behavior
// spec for the API-diff engine. This file holds the *shared* Cake machinery
// that both the API-diff target (scripts/infra/docs/api-diff.cake)
// and the mdoc XML generators (scripts/infra/docs/docs.cake) depend on:
//
//   - CreateNuGetDiffAsync : the NuGet-diff comparer factory (+ its dependency loader)
//   - GetPlatformDirectories / DecompressArchive : package layout helpers
//   - versions.json loading : LoadVersionsConfig / IsVersionSuperseded / FindCompareToBaseline
//
// Per the spec (§2.1) these are the only pieces shared between the two engines, so
// they live alongside both consumers here instead of being duplicated. The heavy
// NuGet-diff #addins live here too, so only the two consumers that #load this file
// pay for them.
//
// CONSUMERS MUST #load "shared.cake" BEFORE this file: it relies on ROOT_PATH,
// PACKAGE_CACHE_PATH, GetVersion, TRACKED_NUGETS, etc. defined there.

#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.Json&version=6.0.1
#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=SharpCompress&version=0.32.2
#addin nuget:?package=Mono.Cecil&version=0.11.5
#addin nuget:?package=Mono.ApiTools.ApiInfo&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiff&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiffFormatted&version=1.4.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

using System.Xml;
using System.Xml.Linq;
using SharpCompress.Common;
using SharpCompress.Readers;
using Mono.ApiTools;
using NuGet.Packaging;
using NuGet.Versioning;


////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET-DIFF COMPARER FACTORY
////////////////////////////////////////////////////////////////////////////////////////////////////

async Task<NuGetDiff> CreateNuGetDiffAsync()
{
    var comparer = new NuGetDiff();
    comparer.PackageCache = PACKAGE_CACHE_PATH.FullPath;
    // Output determinism requires resolving every referenced assembly: an unresolved
    // reference makes Mono.ApiTools silently degrade type matching into spurious "New Type"
    // dumps whose shape depends on what is installed on the build host. Every real dependency
    // is added explicitly below, so the resolution closure is complete except for one
    // assembly that ships in no package or reference pack: the .NET-Android resource designer
    // (_Microsoft.Android.Resource.Designer), injected into every .NET-Android assembly by
    // AndroidUseDesignerAssembly (default since .NET 8). It exists on no host, so it is
    // unresolvable identically everywhere and skipping it stays host-independent; it is never
    // part of SkiaSharp's public API, so it contributes nothing to the diff.
    comparer.IgnoreResolutionErrors = true;

    // Exclude auto-generated, non-API classes that the platform tooling compiles INTO the
    // public view assemblies but that are not part of SkiaSharp's API surface:
    //   - the Android "aapt" resource designer "<RootNamespace>.Resource" and its nested
    //     R.* tables (Resource.Drawable, Resource.Styleable, ...), emitted into the
    //     Android builds of SkiaSharp.Views.Android / .Forms / .Maui.*;
    //   - the Uno/WinUI XAML "GlobalStaticResources" class emitted into the UWP/WinUI
    //     builds of SkiaSharp.Views.UWP / .Windows / .Uno.
    // Without this they surface as thousands of lines of "New Type" resource-id constants
    // in the diff (e.g. 2600+ lines for a single Forms designer), pure noise that also
    // varies with platform tooling. IgnoreMemberRegex feeds the formatted-markdown diff's
    // IgnoreAdded/IgnoreNew/IgnoreRemoved sets, so a match drops the whole type block at
    // generation time. The patterns are anchored under "SkiaSharp.Views." and require
    // "Resource"/"GlobalStaticResources" to be a whole trailing name segment, so the real
    // API lookalikes outside that namespace are untouched (SkiaSharp.Resources.*,
    // SkiaSharp.GR*TextureResourceInfo). The mdoc engine applies the equivalent exclusion
    // itself (scripts/infra/docs/docs.cake); this property only affects the api-diff path,
    // since docs.cake consumes this comparer solely for its SearchPaths.
    comparer.IgnoreMemberRegex.Add (@"^SkiaSharp\.Views\.[\w.]+\.Resource([.:+/ ]|$)");
    comparer.IgnoreMemberRegex.Add (@"^SkiaSharp\.Views\.[\w.]+\.GlobalStaticResources([.:+/ ]|$)");

    Verbose ($"Adding dependencies...");

    await AddDep("OpenTK.GLControl", "NET20");
    await AddDep("GtkSharp", "netstandard2.0");
    await AddDep("GdkSharp", "netstandard2.0");
    await AddDep("GLibSharp", "netstandard2.0");
    await AddDep("AtkSharp", "netstandard2.0");
    await AddDep("System.Memory", "netstandard2.0");
    await AddDep("System.Runtime.CompilerServices.Unsafe", "netstandard2.1");
    await AddDep("Microsoft.WindowsAppSDK", "net6.0-windows10.0.18362.0");
    await AddDep("Microsoft.Maui.Graphics", "netstandard2.0");
    await AddDep("Microsoft.Windows.SDK.NET.Ref", "net6.0");
    await AddDep("Microsoft.Windows.SDK.Contracts", "netstandard2.0");
    await AddDep("System.Runtime.WindowsRuntime", "netstandard2.0");
    await AddDep("System.Runtime.WindowsRuntime.UI.Xaml", "netstandard2.0");
    await AddDep("Microsoft.WindowsDesktop.App.Ref", "net6.0");
    await AddDep("Microsoft.AspNetCore.Components", "net6.0");
    await AddDep("Microsoft.JSInterop", "net6.0");
    await AddDep("OpenTK.GLWpfControl", "netcoreapp3.1");
    await AddDep("Microsoft.Maui.Core", "net10.0");
    await AddDep("Microsoft.Maui.Controls.Core", "net10.0");
    await AddDep("Microsoft.Maui.Controls.Compatibility", "net10.0");
    await AddDep("Microsoft.iOS.Ref.net10.0_26.0", "net10.0");
    await AddDep("Microsoft.MacCatalyst.Ref.net10.0_26.0", "net10.0");
    await AddDep("Microsoft.tvOS.Ref.net10.0_26.0", "net10.0");
    await AddDep("Microsoft.macOS.Ref.net10.0_26.0", "net10.0");
    await AddDep("Samsung.Tizen.Ref", "net10.0");
    await AddDep("GirCore.Gdk-4.0", "net10.0");
    await AddDep("GirCore.Gtk-4.0", "net10.0");
    await AddDep("GirCore.Cairo-1.0", "net10.0");
    await AddDep("GirCore.FreeType2-2.0", "net10.0");
    await AddDep("GirCore.GdkPixbuf-2.0", "net10.0");
    await AddDep("GirCore.Gio-2.0", "net10.0");
    await AddDep("GirCore.GLib-2.0", "net10.0");
    await AddDep("GirCore.GObject-2.0", "net10.0");
    await AddDep("GirCore.Graphene-1.0", "net10.0");
    await AddDep("GirCore.Gsk-4.0", "net10.0");
    await AddDep("GirCore.HarfBuzz-0.0", "net10.0");
    await AddDep("GirCore.Pango-1.0", "net10.0");
    await AddDep("GirCore.PangoCairo-1.0", "net10.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.iOS/v1.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.TVOS/v1.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.WatchOS/v1.0");
    await AddVsixDep("Xamarin.VisualStudio.Apple.Sdk", "$ReferenceAssemblies/Microsoft/Framework/Xamarin.Mac/v2.0");
    await AddVsixDep("Xamarin.Android.Sdk", "$ReferenceAssemblies/Microsoft/Framework/MonoAndroid/v1.0");
    await AddVsixDep("Xamarin.Android.Sdk", "$ReferenceAssemblies/Microsoft/Framework/MonoAndroid/v13.0");
    await AddDep("Uno.UI", "netstandard2.0");
    await AddDep("Xamarin.Forms", "netstandard2.0");
    await AddDep("Xamarin.Forms", "MonoAndroid10.0");
    await AddDep("Xamarin.Forms", "uap10.0.16299");
    await AddDep("Xamarin.Forms", "tizen40");
    // The iOS/macOS renderers (Xamarin.Forms.Platform.iOS / .macOS) ship under
    // build/XCODE11 instead of lib/ since Forms 4.6 — add that folder directly.
    await AddPackageDir("Xamarin.Forms", "build/XCODE11");
    await AddDep("Xamarin.Forms.Platform.WPF", "net461");
    await AddDep("Xamarin.Forms.Platform.GTK", "net45");
    await AddDep("Mono.GtkSharp", "net45");

    // Self-dependencies (SkiaSharp parts that reference other SkiaSharp parts — e.g.
    // SkiaSharp.Views.* → SkiaSharp, SkiaSharp.Views.Maui.Controls → ...Maui.Core) are
    // NOT added here. They must match the *version being diffed*, not a single global
    // version, so they are staged per-diff from each package's own nuspec by
    // StageSelfDepsFromNuspecAsync (see the note on that helper). Adding them globally
    // here would resolve a 1.x package's inherited types against today's assembly.

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
            Verbose ($"      ref path {refPath}");
            comparer.SearchPaths.Add(refPath);
        } else {
            Verbose ($"      no lib or ref path");
        }
    }

    // Add an arbitrary subfolder of a cached package to the search paths. Used for
    // assemblies that ship outside lib/ or ref/ (e.g. the Xamarin.Forms iOS/macOS
    // renderers under build/XCODE11). subPath is relative to the package root and uses
    // '/' separators.
    async Task AddPackageDir(string id, string subPath, string type = "release")
    {
        var version = GetVersion(id, type);
        Verbose ($"    Adding dependency {id} version {version} ({subPath})...");
        var root = await comparer.ExtractCachedPackageAsync(id, version);
        var dir = System.IO.Path.Combine(root, System.IO.Path.Combine(subPath.Split('/')));
        if (DirectoryExists(dir)) {
            Verbose ($"      dir {dir}");
            comparer.SearchPaths.Add(dir);
        } else {
            Verbose ($"      no dir at {subPath}");
        }
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////
// SELF-DEPENDENCY STAGING (contemporaneous, per-diff)
////////////////////////////////////////////////////////////////////////////////////////////////////

// A SkiaSharp/HarfBuzz package that derives its public types from another of our packages
// (e.g. SkiaSharp.Views.* : SKObject in SkiaSharp; SkiaSharp.Views.Maui.Controls →
// ...Maui.Core) needs that referenced assembly on the resolver's search path. ApiInfo
// reads a public type's base class FROM the resolved assembly (Mono.Cecil binds by simple
// name, so whichever build is on the path wins), and uses it to decide which inherited
// interfaces are public: with SkiaSharp resolved, SKObject's `internal`
// ISKReferenceCounted/ISKSkipObjectRegistration are hidden and only the public
// System.IDisposable remains; without it, those internals LEAK into the diff.
//
// The version on the path must be the one the package being diffed was actually built
// against — otherwise a historical (e.g. 1.x) diff is described using today's assembly and
// can show inherited members the old type never had. So instead of a single global
// version, stage each self-dependency at the EXACT version pinned in the package's own
// nuspec. That is contemporaneous, deterministic, and host/cache-independent (nuspec
// content is frozen). Returns the paths added so the caller removes them after the diff —
// a self-dep must never leak into the next line. `packageRoot` is the extracted root of
// the (new-side) package being diffed; the library uses one search-path set for both
// sides of a diff, and a line's old→new are adjacent same-era releases, so the new side's
// pin is the correct choice.
async Task<List<string>> StageSelfDepsFromNuspecAsync(NuGetDiff comparer, string packageRoot)
{
    var added = new List<string>();
    if (string.IsNullOrEmpty(packageRoot))
        return added;
    var nuspec = GetFiles($"{packageRoot}/*.nuspec").FirstOrDefault();
    if (nuspec == null)
        return added;

    var xdoc = XDocument.Load(nuspec.FullPath);
    XNamespace ns = xdoc.Root.GetDefaultNamespace();

    // Collect our own dependencies at their pinned minimum version. The same dependency
    // repeats once per target-framework group, so de-dup by id and keep the lowest pin
    // (deterministic, and the lowest is the version the package first shipped against).
    var selfDeps = new Dictionary<string, NuGetVersion>(StringComparer.OrdinalIgnoreCase);
    foreach (var dep in xdoc.Descendants(ns + "dependency")) {
        var depId = (string)dep.Attribute("id");
        if (string.IsNullOrEmpty(depId) || !IsSelfDependency(depId))
            continue;
        if (!VersionRange.TryParse((string)dep.Attribute("version") ?? "", out var range) || range.MinVersion == null)
            continue;
        if (!selfDeps.TryGetValue(depId, out var existing) || range.MinVersion < existing)
            selfDeps[depId] = range.MinVersion;
    }

    foreach (var id in selfDeps.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)) {
        var version = selfDeps[id].ToNormalizedString();
        Verbose($"    Staging self-dependency {id} version {version} (from nuspec)...");
        try {
            var root = await comparer.ExtractCachedPackageAsync(id, version);
            var dir = GetSelfDepLibDirectory(root);
            if (dir == null) {
                Verbose($"      no managed build found for {id} {version}");
                continue;
            }
            Verbose($"      lib path {dir}");
            comparer.SearchPaths.Add(dir);
            added.Add(dir);
        } catch (Exception ex) {
            // A single self-dep that can't be fetched/extracted (e.g. a yanked or
            // never-published transitive version) must not abort the whole run; the worst
            // case is that one line cannot resolve that assembly's inherited types.
            Warning($"      could not stage self-dependency {id} {version}: {ex.Message}");
        }
    }
    return added;
}

// Remove paths that StageSelfDepsFromNuspecAsync added, restoring the comparer to its
// shared (third-party only) search-path set before the next line is diffed.
void UnstageSearchPaths(NuGetDiff comparer, List<string> paths)
{
    foreach (var path in paths)
        comparer.SearchPaths.Remove(path);
}

// Our own MANAGED packages, identified by id prefix (SkiaSharp*, HarfBuzzSharp*). A package
// never depends on itself, so this never stages the assembly being diffed (which would
// shadow it). NativeAssets.* packages are EXCLUDED: they ship only native binaries (no
// managed assembly), so they contribute nothing to inherited-type resolution, they are by
// far the largest packages to download, and a missing/preview native build would otherwise
// abort the whole run for no benefit.
bool IsSelfDependency(string id) =>
    (id.StartsWith("SkiaSharp", StringComparison.OrdinalIgnoreCase) ||
     id.StartsWith("HarfBuzzSharp", StringComparison.OrdinalIgnoreCase)) &&
    id.IndexOf(".NativeAssets.", StringComparison.OrdinalIgnoreCase) < 0;

// Pick exactly one managed build folder of a self-dependency package to resolve inherited
// types from. Deterministic ranking: prefer netstandard (newest — the most portable,
// era-stable surface), then the newest plain .NET, then portable, then the
// alphabetically-first remaining build; look under lib/ before ref/. One build is enough
// because Mono.Cecil binds by simple assembly name.
string GetSelfDepLibDirectory(string packageRoot)
{
    foreach (var bucket in new[] { "lib", "ref" }) {
        var bucketDir = System.IO.Path.Combine(packageRoot, bucket);
        if (!DirectoryExists(bucketDir))
            continue;

        DirectoryPath best = null;
        var bestRank = (tier: -1, version: new System.Version(0, 0), name: (string)null);
        foreach (var dir in GetDirectories($"{bucketDir}/*")) {
            if (!GetFiles($"{dir}/*.dll").Any() && !GetFiles($"{dir}/*.winmd").Any())
                continue;

            var d = dir.GetDirectoryName().ToLower();
            int tier;
            System.Version version;
            var net = System.Text.RegularExpressions.Regex.Match(d, @"^net(\d+\.\d+)$");
            if (d.StartsWith("netstandard")) {
                tier = 3;
                if (!System.Version.TryParse(d.Substring("netstandard".Length), out version))
                    version = new System.Version(0, 0);
            } else if (net.Success) {
                tier = 2;
                version = System.Version.Parse(net.Groups[1].Value);
            } else if (d.StartsWith("portable")) {
                tier = 1;
                version = new System.Version(0, 0);
            } else {
                tier = 0;
                version = new System.Version(0, 0);
            }

            if (tier > bestRank.tier
                || (tier == bestRank.tier && version > bestRank.version)
                || (tier == bestRank.tier && version == bestRank.version && (bestRank.name == null || string.CompareOrdinal(d, bestRank.name) < 0))) {
                bestRank = (tier, version, d);
                best = dir;
            }
        }

        if (best != null)
            return best.FullPath;
    }
    return null;
}


////////////////////////////////////////////////////////////////////////////////////////////////////
// PACKAGE LAYOUT HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

void DecompressArchive(FilePath archive, DirectoryPath outputDir)
{
    using (var stream = System.IO.File.OpenRead(archive.FullPath))
    using (var reader = ReaderFactory.Open(stream)) {
        while(reader.MoveToNextEntry()) {
            if (!reader.Entry.IsDirectory) {
                reader.WriteEntryToDirectory(outputDir.FullPath, new ExtractionOptions {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }
}

IEnumerable<(DirectoryPath path, string platform)> GetPlatformDirectories(DirectoryPath rootDir)
{
    var platformDirs = GetDirectories($"{rootDir}/*");

    // A package that ships a cross-platform (BCL) build is documented from that
    // single assembly. Prefer the newest plain .NET build (e.g. net10.0) because
    // that is what real apps target and it exposes the most complete API surface;
    // fall back to netstandard, then portable, only when no plain .NET build exists.
    DirectoryPath crossPlatformDir = null;
    var crossPlatformRank = (tier: -1, version: new System.Version (0, 0));
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName().ToLower();
        int tier;
        System.Version version;
        // plain .NET (net5.0+); the dot excludes net4x desktop, the anchors exclude platform heads like net10.0-ios
        var net = System.Text.RegularExpressions.Regex.Match(d, @"^net(\d+\.\d+)$");
        if (net.Success) {
            tier = 2;
            version = System.Version.Parse(net.Groups[1].Value);
        } else if (d.StartsWith("netstandard")) {
            tier = 1;
            if (!System.Version.TryParse(d.Substring("netstandard".Length), out version))
                version = new System.Version (0, 0);
        } else if (d.StartsWith("portable")) {
            tier = 0;
            version = new System.Version (0, 0);
        } else {
            continue;
        }

        if (tier > crossPlatformRank.tier || (tier == crossPlatformRank.tier && version > crossPlatformRank.version)) {
            crossPlatformRank = (tier, version);
            crossPlatformDir = dir;
        }
    }

    if (crossPlatformDir != null) {
        // we just want this single cross-platform assembly
        yield return (crossPlatformDir, null);
        yield break;
    }

    // there were no cross-platform libraries, so process each platform
    foreach (var dir in platformDirs) {
        var d = dir.GetDirectoryName().ToLower();
        if (d.StartsWith("monoandroid") || (d.StartsWith("net") && d.Contains("-android")))
            yield return (dir, "android");
        else if (d.StartsWith("net4"))
            yield return (dir, "net");
        else if (d.StartsWith("uap"))
            yield return (dir, "uwp");
        else if (d.StartsWith("xamarinios") || d.StartsWith("xamarin.ios") || (d.StartsWith("net") && d.Contains("-ios")))
            yield return (dir, "ios");
        else if (d.StartsWith("xamarinmac") || d.StartsWith("xamarin.mac") || (d.StartsWith("net") && d.Contains("-macos")))
            yield return (dir, "macos");
        else if (d.StartsWith("xamarintvos") || d.StartsWith("xamarin.tvos") || (d.StartsWith("net") && d.Contains("-tvos")))
            yield return (dir, "tvos");
        else if (d.StartsWith("xamarinwatchos") || d.StartsWith("xamarin.watchos") || (d.StartsWith("net") && d.Contains("-watchos")))
            yield return (dir, "watchos");
        else if (d.StartsWith("tizen") || (d.StartsWith("net") && d.Contains("-tizen")))
            yield return (dir, "tizen");
        else if (d.StartsWith("net") && d.Contains("-windows"))
            yield return (dir, "windows");
        else if (d.StartsWith("net") && d.Contains("-maccatalyst"))
            yield return (dir, "maccatalyst");
        else if (d.StartsWith("netcoreapp"))
            continue; // skip this one for now
        else
            throw new Exception($"Unknown platform '{d}' found at '{dir}'.");
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////
// versions.json — the shared version-comparison config
//
// scripts/infra/docs/versions.json is the single source of truth for how versions
// relate to each other (spec §1.2). It is "override only": versions NOT listed fall
// back to the default behaviour of diffing against the immediately-preceding
// published version.
//
// versions.json is family-bucketed: { "skiasharp": { "<line>": {...} },
// "harfbuzzsharp": { ... } }. This loader flattens the requested family's bucket
// into the legacy JArray shape — one object per line carrying an injected "version"
// key — so IsVersionSuperseded/FindCompareToBaseline work unchanged across families.
////////////////////////////////////////////////////////////////////////////////////////////////////

JArray LoadVersionsConfig (string family = "skiasharp")
{
    var path = $"{ROOT_PATH}/scripts/infra/docs/versions.json";
    if (!FileExists (path))
        return new JArray ();
    var doc = JObject.Parse (System.IO.File.ReadAllText (path));
    var bucket = doc [family] as JObject;
    var result = new JArray ();
    if (bucket != null) {
        foreach (var prop in bucket.Properties ()) {
            var entry = (JObject)prop.Value.DeepClone ();
            entry ["version"] = prop.Name;
            result.Add (entry);
        }
    }
    return result;
}

// A "superseded" version is one that was previewed but never shipped stable
// (e.g. 4.147 was abandoned in favour of 4.148). Its ONLY effect is on baseline
// selection: it is excluded from acting as a *baseline* for other versions, so a
// later release diffs against the last real predecessor instead. A superseded line
// is NOT dropped from emission — it still gets its own artifact/page (spec §1.2 and
// §1.4 rule 2); it is simply transparent as a diff baseline. Matched on
// major.minor.patch, so an entry for "4.147.0" covers every 4.147.0-preview.* (all
// previews of that exact patch), but not a different patch.
bool IsVersionSuperseded (JArray config, string normalizedVersion)
{
    var nv = new NuGetVersion (normalizedVersion);
    var key = $"{nv.Major}.{nv.Minor}.{nv.Patch}";
    return config.Any (v => (string)v ["version"] == key && (string)v ["status"] == "superseded");
}

// True when a version line has ANY entry in versions.json (a compare_to override
// and/or a status). Per spec §1.4 rule 2 a tracked line is always EMITTED — it gets
// its own artifact even when it is a preview-only line behind the latest stable
// (e.g. the superseded 4.147 / 3.0.0 lines, which are shipped previews that still
// need their own api diff/page). Matched on major.minor.patch.
bool IsVersionListed (JArray config, string normalizedVersion)
{
    var nv = new NuGetVersion (normalizedVersion);
    var key = $"{nv.Major}.{nv.Minor}.{nv.Patch}";
    return config.Any (v => (string)v ["version"] == key);
}

// Return the explicit "compare_to" baseline declared for a version in
// versions.json (e.g. 4.148 → 3.119.4, deliberately skipping 4.147), resolved to
// the newest actual package that matches that major.minor.patch. Returns null
// when no override exists, in which case the caller falls back to a walk-back.
string FindCompareToBaseline (JArray config, string normalizedVersion, NuGetVersion[] allVersions)
{
    var nv = new NuGetVersion (normalizedVersion);
    var key = $"{nv.Major}.{nv.Minor}.{nv.Patch}";
    var entry = config.FirstOrDefault (v => (string)v ["version"] == key && v ["compare_to"] != null);
    if (entry == null)
        return null;

    var compareTo = (string)entry ["compare_to"];
    var candidates = allVersions
        .Where (v => $"{v.Major}.{v.Minor}.{v.Patch}" == compareTo)
        .OrderByDescending (v => v)
        .ToArray ();
    return candidates.Length > 0 ? candidates [0].ToNormalizedString () : null;
}
