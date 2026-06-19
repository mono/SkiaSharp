// READ FIRST: documentation/dev/release-notes-and-changelogs.md is the behavior
// spec for this engine. Change the spec first, then make this code match it.
//
// ─────────────────────────────────────────────────────────────────────────────
// API-changelog engine (spec §5)
//
// This is the Cake half of the release-notes skill. It regenerates the per-family
// API-diff trees under documentation/docfx/releases/ (spec §3.3/§3.4) by diffing
// every published version of each TRACKED_NUGETS package with
// Mono.ApiTools.NuGetDiff, and writes the co-release map sidecar (spec §3.6) the
// Python release-notes engine consumes.
//
// Two targets, one engine:
//   docs-api-diff-past  — historical: regenerate the COMMITTED releases/ trees from
//                         the feed (the Cake generator the §2.2 Prepare phase runs).
//   docs-api-diff       — current: diff the freshly built, unpublished CI packages
//                         against the feed as a build-pipeline gate (spec §5.3);
//                         writes only transient output/api-diff, never releases/.
//
// Shared machinery (the NuGet-diff comparer, layout helpers, versions.json loading)
// lives alongside this file in scripts/infra/docs/api-diff-tools.cake and is #loaded
// below. The
// release-line / baseline / supersession rules are identical to the Python engine
// (spec §1); see versions.json.
// ─────────────────────────────────────────────────────────────────────────────

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/download.cake"
#load "api-diff-tools.cake"

// All committed API diffs live inside the docfx site so docfx renders them and the
// human pages can link to them with internal links (spec §3).
DirectoryPath RELEASES_PATH = MakeAbsolute (ROOT_PATH.Combine ("documentation/docfx/releases"));

// The HarfBuzz family emits into a parallel lowercase tree (spec §3.4); everything
// else (including the managed SkiaSharp.HarfBuzz binding) is the SkiaSharp family
// (spec §1.5). The folder name is the family-tree root, distinct from the package id.
bool IsHarfBuzzFamily (string id) =>
    id == "HarfBuzzSharp" || id.StartsWith ("HarfBuzzSharp.");

// --nugetDiffMinVersion support (spec §5): a local-iteration floor. When set, only
// changelog LINES whose version core is >= the floor are (re)generated and cleared;
// everything below is left exactly as committed. Baselines are still chosen from the
// full release history (the emit list below is never truncated), so each regenerated
// line is byte-identical to what an unfloored full run would produce — this only ever
// narrows the work, never changes output. Empty (the CI default) = no floor.
bool IsLineBelowMin (string lineKey)
{
    if (string.IsNullOrEmpty (NUGET_DIFF_MIN_VERSION))
        return false;
    if (!NuGetVersion.TryParse (lineKey, out var v))
        return false;
    if (!NuGetVersion.TryParse (NUGET_DIFF_MIN_VERSION, out var min))
        throw new Exception ($"--nugetDiffMinVersion='{NUGET_DIFF_MIN_VERSION}' is not a valid version.");
    return v.CompareTo (min) < 0;
}


Task ("docs-api-diff")
    .Does (async () =>
{
    // working version
    var baseDir = $"{OUTPUT_NUGETS_PATH}/api-diff";
    CleanDirectories (baseDir);

    // pretty version
    var diffDir = $"{ROOT_PATH}/output/api-diff";
    EnsureDirectoryExists (diffDir);
    CleanDirectories (diffDir);

    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    // Shared version-comparison config — same source of truth used by
    // docs-api-diff-past. Here it lets us pick a sensible baseline for the
    // unpublished local build instead of blindly diffing against the newest
    // feed version (which could be a superseded preview). Per family (spec §1.5).
    var skiaConfig = LoadVersionsConfig ("skiasharp");
    var hbConfig = LoadVersionsConfig ("harfbuzzsharp");

    var filter = new NuGetVersions.Filter {
        IncludePrerelease = NUGET_DIFF_PRERELEASE
    };

    foreach (var id in SUPPORTED_NUGETS.Keys) {
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        var versionsConfig = IsHarfBuzzFamily (id) ? hbConfig : skiaConfig;

        Information ($"Comparing the assemblies in '{id}'...");

        var version = GetVersion (id);
        if (string.IsNullOrEmpty(version)) {
            Information ($"Skipping '{id}' — no version found in VERSIONS.txt.");
            continue;
        }
        var localNugetVersion = PREVIEW_ONLY_NUGETS.Contains(id)
            ? $"{version}-{PREVIEW_NUGET_SUFFIX}"
            : version;

        var localNupkgPath = $"{OUTPUT_NUGETS_PATH}/{id}.{localNugetVersion}.nupkg";
        if (!FileExists(localNupkgPath)) {
            Information ($"Skipping '{id}' — local nupkg not found: {localNupkgPath}");
            continue;
        }

        // Pick the baseline to diff the local build against:
        //   1. An explicit compare_to override in versions.json wins.
        //   2. Otherwise use the newest published version that is NOT superseded
        //      and is not the build's own version — this skips abandoned preview
        //      lines (e.g. 4.147) the same way docs-api-diff-past does.
        var allVersions = await NuGetVersions.GetAllAsync (id, filter);
        var latestVersion = FindCompareToBaseline (versionsConfig, version, allVersions);
        if (latestVersion == null) {
            foreach (var candidate in allVersions.OrderByDescending (v => v)) {
                var normalized = candidate.ToNormalizedString ();
                if (normalized == localNugetVersion)
                    continue;
                if (IsVersionSuperseded (versionsConfig, normalized))
                    continue;
                latestVersion = normalized;
                break;
            }
        }
        Debug ($"Version '{latestVersion}' is the baseline for '{id}'...");

        // pre-cache so we can have better logs
        if (!string.IsNullOrEmpty (latestVersion)) {
            Debug ($"Caching version '{latestVersion}' of '{id}'...");
            await comparer.ExtractCachedPackageAsync (id, latestVersion);
        }

        // generate the diff (current build is a transient CI gate — it writes only
        // output/api-diff, never the committed releases/ tree, spec §5.3)
        Debug ($"Running a diff on '{latestVersion}' vs '{localNugetVersion}' of '{id}'...");
        var diffRoot = $"{baseDir}/{id}";
        using (var reader = new PackageArchiveReader ($"{OUTPUT_NUGETS_PATH}/{id}.{localNugetVersion}.nupkg")) {
            comparer.MarkdownDiffFileExtension = ".breaking.md";
            comparer.IgnoreNonBreakingChanges = true;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, latestVersion, reader, diffRoot);

            comparer.MarkdownDiffFileExtension = null;
            comparer.IgnoreNonBreakingChanges = false;
            await comparer.SaveCompleteDiffToDirectoryAsync (id, latestVersion, reader, diffRoot);
        }

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
    var baseDir = $"{ROOT_PATH}/output/api-diffs-past";
    CleanDirectories (baseDir);

    // Make the regenerated set authoritative: clear ONLY the API-diff folders this
    // engine owns (spec §3.5) — the per-line <line>/ package folders and the whole
    // harfbuzzsharp/ tree — so anything that should no longer exist (a stale
    // *.breaking.md after a baseline change in versions.json, or a package removed
    // from TRACKED_NUGETS) is pruned instead of left to drift. The human pages
    // (<line>.md, <line>-unreleased.md, TOC.yml, index.md) are owned by the Python
    // engine and MUST NOT be touched here.
    ClearOwnedApiDiffFolders ();

    // Shared version-comparison config, per family (spec §1.2/§1.5).
    Information ("Loading versions.json...");
    var skiaConfig = LoadVersionsConfig ("skiasharp");
    var hbConfig = LoadVersionsConfig ("harfbuzzsharp");

    Information ($"Creating comparer...");
    var comparer = await CreateNuGetDiffAsync ();
    comparer.SaveAssemblyApiInfo = true;
    comparer.SaveAssemblyMarkdownDiff = true;

    // Include prerelease packages so the active development lines (which ship
    // only as previews/rcs until they go stable, e.g. 4.148/4.150) can be
    // enumerated. Emission is still collapsed to one changelog per release line
    // below — prereleases are needed as candidates, not as their own folders.
    var filter = new NuGetVersions.Filter {
        IncludePrerelease = NUGET_DIFF_PRERELEASE
    };

    // Accumulates the SkiaSharp-line → emitted-HarfBuzz-line mapping as we discover
    // it, so we can write the co-release map sidecar (spec §3.6) at the end. Keyed
    // by SkiaSharp line core; value is the HarfBuzz line core shipping with it.
    var skiaHarfBuzzDeps = new Dictionary<string, string> ();

    foreach (var id in TRACKED_NUGETS.Keys) {
        // skip doc generation for NativeAssets as that has nothing but a native binary
        if (id.Contains ("NativeAssets"))
            continue;

        var isHarfBuzz = IsHarfBuzzFamily (id);
        var versionsConfig = isHarfBuzz ? hbConfig : skiaConfig;

        Information ($"Comparing the assemblies in '{id}'...");

        var allVersions = await NuGetVersions.GetAllAsync (id, filter);

        // The newest stable release on the feed. It is the cut-off for preview
        // emission: a preview-only line is only worth a changelog while it is
        // still ahead of the last shipped stable (the active dev line, e.g.
        // 4.148/4.150). Older preview-only lines that never shipped stay pruned.
        var latestStable = allVersions
            .Where (v => !v.IsPrerelease)
            .OrderByDescending (v => v)
            .FirstOrDefault ();

        // Collapse the feed into one entry per release LINE, keyed by the numeric
        // version core with the prerelease label stripped (4.148.0-rc.1.2 ->
        // 4.148.0; the 4th digit of a real 4-part stable like 1.49.2.1 is kept).
        // Each line's changelog is a rollup named by that core, diffed against the
        // line's representative package: the newest stable if it shipped,
        // otherwise the newest prerelease. This mirrors the release-notes pages,
        // which are stable-named rollups of all the previews in between.
        var lines = allVersions
            .GroupBy (v => v.ToNormalizedString ().Split ('-') [0])
            .Select (g => {
                var stable = g.Where (v => !v.IsPrerelease).OrderByDescending (v => v).FirstOrDefault ();
                return (key: g.Key, rep: stable ?? g.OrderByDescending (v => v).First ());
            })
            .OrderBy (l => l.rep)
            .ToList ();

        // Decide which lines actually get a changelog emitted (spec §1.4). A line
        // is emitted when ANY of these holds, and not otherwise:
        //   1. it shipped stable (the permanent historical record); or
        //   2. it is listed in versions.json — an intentionally tracked line, e.g. a
        //      superseded preview-only line like 4.147 or 3.0.0 (spec §1.4 rule 2).
        //      "superseded" only skips a line as a *baseline* (§1.3); it does NOT
        //      drop the line's own page — a shipped preview still needs its diff; or
        //   3. it is a preview-only line ahead of the last stable (active dev line).
        // Any other preview-only line (old, never shipped, not listed) is dropped.
        var emit = lines
            .Where (l => !l.rep.IsPrerelease
                || IsVersionListed (versionsConfig, l.rep.ToNormalizedString ())
                || latestStable == null
                || l.rep.CompareTo (latestStable) > 0)
            .ToList ();

        for (var idx = 0; idx < emit.Count; idx++) {
            // The package we actually diff (e.g. 4.148.0-rc.1.2) and the folder we
            // write it to (e.g. 4.148.0).
            var version = emit [idx].rep.ToNormalizedString ();
            var changelogVersion = emit [idx].key;

            // --nugetDiffMinVersion floor: skip GENERATING this line when it is below the
            // floor, but only after it has had its chance to be a baseline for later lines
            // (the emit list is intact, so the walk-back below still sees it). Its committed
            // files were already spared by ClearOwnedApiDiffFolders, so they stay untouched.
            if (IsLineBelowMin (changelogVersion)) {
                Debug ($"Skipping generation of '{id}' line '{changelogVersion}' (below --nugetDiffMinVersion={NUGET_DIFF_MIN_VERSION}).");
                continue;
            }

            // Pick the baseline to diff against (spec §1.3):
            //   1. An explicit compare_to override in versions.json wins
            //      (e.g. 4.148 -> 3.119.4, deliberately skipping 4.147).
            //   2. Otherwise diff against the most recent previous EMITTED line that
            //      is NOT itself superseded — a superseded line still gets its own
            //      page but must never serve as a baseline (spec §1.2/§1.3), so the
            //      next line diffs past it and rolls its work up.
            var previous = FindCompareToBaseline (versionsConfig, version, allVersions);
            if (previous == null) {
                for (var j = idx - 1; j >= 0; j--) {
                    var candidate = emit [j].rep.ToNormalizedString ();
                    if (!IsVersionSuperseded (versionsConfig, candidate)) {
                        previous = candidate;
                        break;
                    }
                }
            }

            Information ($"Comparing version '{previous}' vs '{version}' of '{id}' (changelog '{changelogVersion}')...");

            // pre-cache so we can have better logs
            Debug ($"Caching version '{version}' of '{id}'...");
            var versionRoot = await comparer.ExtractCachedPackageAsync (id, version);
            if (previous != null) {
                Debug ($"Caching version '{previous}' of '{id}'...");
                await comparer.ExtractCachedPackageAsync (id, previous);
            }

            // The committed folder for this line: SkiaSharp family -> releases/<line>/<id>/…;
            // HarfBuzz family -> releases/harfbuzzsharp/<hb-line>/<id>/… (spec §3.3/§3.4).
            var lineDir = isHarfBuzz
                ? RELEASES_PATH.Combine ("harfbuzzsharp").Combine (changelogVersion)
                : RELEASES_PATH.Combine (changelogVersion);

            // generate the diff and copy to the committed releases/ tree
            Debug ($"Running a diff on '{previous}' vs '{version}' of '{id}'...");
            var diffRoot = $"{baseDir}/{id}/{changelogVersion}";
            await RunBreakingAndFullDiff (comparer, id, previous, version, lineDir, diffRoot);

            // Record the co-release mapping (spec §1.5/§3.6): the HarfBuzz version
            // that ships with a SkiaSharp line is the HarfBuzzSharp dependency of the
            // managed SkiaSharp.HarfBuzz binding at that line. We read it straight
            // from the extracted package's nuspec.
            if (id == "SkiaSharp.HarfBuzz") {
                var hbDep = ReadHarfBuzzDependencyLine (versionRoot);
                if (!string.IsNullOrEmpty (hbDep))
                    skiaHarfBuzzDeps [changelogVersion] = hbDep;
            }

            Debug ($"Diff complete of version '{version}' of '{id}'.");
        }
        Information ($"Diff complete of '{id}'.");
    }

    // Record the in-flight SkiaSharp line's HarfBuzz co-release from the WORKING TREE
    // (spec §3.6/§5.1): the line under active development (VERSIONS.txt) has no published
    // package yet, so the feed loop above could not contribute it. We add it from
    // VERSIONS.txt — SkiaSharp's line and the HarfBuzzSharp line it builds against — but
    // ONLY when the feed did not already supply this SkiaSharp line (an on-feed line is
    // always read from its own published package, never overridden). This is what lets an
    // in-flight HarfBuzz line be recorded before it publishes (§4.5).
    var inflightSkia = new NuGetVersion (GetVersion ("SkiaSharp")).ToNormalizedString ().Split ('-') [0];
    var inflightHb = new NuGetVersion (GetVersion ("HarfBuzzSharp")).ToNormalizedString ().Split ('-') [0];
    if (!string.IsNullOrEmpty (inflightSkia) && !string.IsNullOrEmpty (inflightHb)
            && !skiaHarfBuzzDeps.ContainsKey (inflightSkia)) {
        Information ($"Recording in-flight co-release from working tree: SkiaSharp {inflightSkia} → HarfBuzzSharp {inflightHb}.");
        skiaHarfBuzzDeps [inflightSkia] = inflightHb;
    }

    // Write the per-line API-diff index.md landing pages (spec §3.3/§3.4) and the
    // co-release map sidecar (spec §3.6) the Python release-notes engine consumes.
    //
    // A floored (--nugetDiffMinVersion) run is a partial, local-iteration regeneration:
    // it deliberately did not visit the lines below the floor, so it has NOTHING to say
    // about them. Rewriting the index pages or the co-release sidecar from this partial
    // set would drop their below-floor entries, corrupting the committed tree. So in a
    // floored run we leave BOTH untouched — only an unfloored (authoritative) run, which
    // visits every line, is allowed to rewrite them.
    if (string.IsNullOrEmpty (NUGET_DIFF_MIN_VERSION)) {
        WriteApiDiffFolderIndexes ();
        WriteCoReleaseMap (skiaHarfBuzzDeps);
    } else {
        Information ($"--nugetDiffMinVersion={NUGET_DIFF_MIN_VERSION}: leaving index pages and the co-release map untouched (partial run).");
    }

    // clean up after working
    CleanDirectories (baseDir);
});


////////////////////////////////////////////////////////////////////////////////////////////////////
// API-DIFF OUTPUT HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

// Marker that prefixes the first line of every file the comparer writes (the
// <assembly>.md / <assembly>.breaking.md diffs and the "No changes." stub in
// CopyChangelogs). Hand-authored files nested in a line folder do not carry it.
const string API_DIFF_MARKER = "# API diff:";

// Clear only the GENERATED API-diff files this engine owns (spec §3.5), inside the
// per-line <line>/ package folders under releases/ and the harfbuzzsharp/ tree. A
// "<line>" folder is a directory whose name is a version core (starts with a digit).
//
// We delete a *.md file only when it is the engine's own generated output, identified
// by TWO conditions that must both hold:
//   1. its first line starts with API_DIFF_MARKER (so a hand-authored file shaped like
//      an assembly diff, e.g. 1.68.0/SkiaSharp/gpu-migration.md, is preserved — a plain
//      "*.md" glob could not tell it apart from SkiaSharp.md), AND
//   2. it is NOT a "*.humanreadable.md" file — that is a retired legacy format outside
//      the two patterns (<assembly>.md / <assembly>.breaking.md) this engine emits, so
//      it is left untouched. (Some humanreadable files happen to carry the marker and
//      some do not; excluding by name keeps all of them treated consistently.)
//
// Everything else (the human <line>.md pages and TOC.yml/index.md at the releases/
// root, plus any hand-authored extras) is preserved. After deleting, empty directories
// are pruned so a removed/superseded version leaves no stragglers behind.
void ClearOwnedApiDiffFolders ()
{
    if (!DirectoryExists (RELEASES_PATH))
        return;

    foreach (var dir in GetSubDirectories (RELEASES_PATH)) {
        var name = dir.GetDirectoryName ();

        if (name == "harfbuzzsharp") {
            // The HarfBuzz family keeps its line folders one level deeper; clear each
            // line folder individually so --nugetDiffMinVersion can spare older ones.
            foreach (var lineDir in GetSubDirectories (dir)) {
                var lineKey = lineDir.GetDirectoryName ();
                if (lineKey.Length > 0 && char.IsDigit (lineKey [0]) && !IsLineBelowMin (lineKey))
                    ClearGeneratedApiDiffsIn (lineDir.FullPath);
            }
            DeleteEmptyDirectories (dir.FullPath);
            continue;
        }

        // SkiaSharp family: a line folder is a top-level directory named by a version core.
        if (name.Length > 0 && char.IsDigit (name [0])) {
            if (IsLineBelowMin (name))
                continue;
            ClearGeneratedApiDiffsIn (dir.FullPath);
            DeleteEmptyDirectories (dir.FullPath);
        }
    }
}

// Delete every generated API-diff *.md (see IsGeneratedApiDiff) anywhere under one line
// folder, leaving hand-authored files in place.
void ClearGeneratedApiDiffsIn (string lineDir)
{
    foreach (var md in System.IO.Directory.EnumerateFiles (lineDir, "*.md", System.IO.SearchOption.AllDirectories)) {
        if (IsGeneratedApiDiff (md))
            System.IO.File.Delete (md);
    }
}

// True iff the file is one of this engine's generated diffs: it carries the
// API_DIFF_MARKER on its first line AND is not a retired "*.humanreadable.md" legacy
// file. An empty/unreadable file is treated as not-generated and therefore preserved.
bool IsGeneratedApiDiff (string path)
{
    if (path.EndsWith (".humanreadable.md", StringComparison.OrdinalIgnoreCase))
        return false;
    foreach (var line in System.IO.File.ReadLines (path))
        return line.StartsWith (API_DIFF_MARKER, StringComparison.Ordinal);
    return false;
}

// Recursively remove directories that hold no files at all (e.g. a removed or
// superseded version whose every file was generated). A directory that still holds a
// preserved hand-authored file is kept.
void DeleteEmptyDirectories (string dir)
{
    foreach (var sub in System.IO.Directory.EnumerateDirectories (dir))
        DeleteEmptyDirectories (sub);

    if (!System.IO.Directory.EnumerateFileSystemEntries (dir).Any ())
        System.IO.Directory.Delete (dir);
}

// Read the HarfBuzzSharp dependency line core from an extracted SkiaSharp.HarfBuzz
// package's nuspec (spec §1.5 co-release mapping). Returns e.g. "8.3.0" or null.
string ReadHarfBuzzDependencyLine (string packageRoot)
{
    var nuspec = GetFiles ($"{packageRoot}/*.nuspec").FirstOrDefault ();
    if (nuspec == null)
        return null;

    var xdoc = XDocument.Load (nuspec.FullPath);
    XNamespace ns = xdoc.Root.GetDefaultNamespace ();
    var dep = xdoc.Descendants (ns + "dependency")
        .FirstOrDefault (d => (string)d.Attribute ("id") == "HarfBuzzSharp");
    if (dep == null)
        return null;

    // The dependency range is normally a plain minimum version like "8.3.0" or a
    // bracketed range like "[8.3.0, )"; take the first version token and reduce it to
    // its §1.1 line key. Keep the FULL line granularity (do NOT collapse to
    // Major.Minor.Patch): the emitted HarfBuzz folder key is the package version's
    // normalized core (`ToNormalizedString().Split('-')[0]`, e.g. a 4-part stable like
    // 8.3.1.5 is preserved), so the dependency line must be computed the same way or it
    // will not match its folder in the co-release map (spec §3.6).
    var range = ((string)dep.Attribute ("version") ?? "").Trim ('[', '(', ']', ')', ' ');
    var token = range.Split (',') [0].Trim ();
    if (string.IsNullOrEmpty (token))
        return null;
    return new NuGetVersion (token).ToNormalizedString ().Split ('-') [0];
}

// Write the co-release map sidecar (spec §3.6): one entry per emitted SkiaSharp line
// (including the in-flight line) giving the HarfBuzz line that ships with it and the
// site-relative link to that HarfBuzz line's API-diff index. `hb_link` is a pure
// mechanical mirror of `hb_line` — `harfbuzzsharp/<hb-line>/index.md` (§3.3/§3.4) —
// regardless of whether that folder exists yet; Python checks the filesystem and, when
// the folder is absent (an in-flight HarfBuzz line, §4.5), drives an in-flight page
// instead of linking. `hb_line` is authoritative at full §1.1 granularity. This is the
// only thing that crosses from this engine into the Python release-notes engine.
void WriteCoReleaseMap (Dictionary<string, string> skiaHarfBuzzDeps)
{
    EnsureDirectoryExists (RELEASES_PATH);
    var sidecar = RELEASES_PATH.CombineWithFilePath ("co-release-map.json");

    var entries = new JArray ();
    foreach (var kvp in skiaHarfBuzzDeps.OrderBy (k => k.Key)) {
        var entry = new JObject ();
        entry ["skia_line"] = kvp.Key;
        entry ["hb_line"] = kvp.Value;
        entry ["hb_link"] = $"harfbuzzsharp/{kvp.Value}/index.md";
        entries.Add (entry);
    }

    System.IO.File.WriteAllText (sidecar.FullPath, entries.ToString (Newtonsoft.Json.Formatting.Indented));
    Information ($"Wrote co-release map sidecar with {entries.Count} entries: {sidecar.FullPath}");
}

// Write the generated index.md landing page for every emitted API-diff folder (spec
// §3.3/§3.4): the deterministic target of each hub page's API-changes link (§4.4). For
// each line folder (SkiaSharp `releases/<line>/` and HarfBuzz
// `releases/harfbuzzsharp/<hb-line>/`) it lists every `<package>/<assembly>.md` diff,
// flags any with a `<assembly>.breaking.md` sibling as breaking, and links back to the
// `../<line>.md` hub. It carries the API_DIFF_MARKER like every other generated file, so
// the §3.5 wipe regenerates it each run.
void WriteApiDiffFolderIndexes ()
{
    if (!DirectoryExists (RELEASES_PATH))
        return;

    // SkiaSharp family: line folders directly under releases/ (name starts with a digit).
    foreach (var dir in GetSubDirectories (RELEASES_PATH)) {
        var name = dir.GetDirectoryName ();
        if (name.Length > 0 && char.IsDigit (name [0]))
            WriteApiDiffFolderIndex (dir, name);
    }

    // HarfBuzz family: line folders one level deeper, under releases/harfbuzzsharp/.
    var hbRoot = RELEASES_PATH.Combine ("harfbuzzsharp");
    if (DirectoryExists (hbRoot)) {
        foreach (var dir in GetSubDirectories (hbRoot)) {
            var name = dir.GetDirectoryName ();
            if (name.Length > 0 && char.IsDigit (name [0]))
                WriteApiDiffFolderIndex (dir, name);
        }
    }
}

// Write one line folder's index.md. `line` is the folder name (== the §1.1 line core),
// so the hub back-link is always `../<line>.md` for both families. Skips folders that
// hold no per-assembly diffs (nothing to land on).
void WriteApiDiffFolderIndex (DirectoryPath lineDir, string line)
{
    var body = new System.Text.StringBuilder ();
    var hasContent = false;

    foreach (var pkgDir in GetSubDirectories (lineDir).OrderBy (d => d.GetDirectoryName (), StringComparer.Ordinal)) {
        var pkg = pkgDir.GetDirectoryName ();
        var rows = new List<string> ();
        foreach (var md in System.IO.Directory.EnumerateFiles (pkgDir.FullPath, "*.md").OrderBy (p => p, StringComparer.Ordinal)) {
            var file = System.IO.Path.GetFileName (md);
            if (file == "index.md" || file.EndsWith (".breaking.md", StringComparison.Ordinal))
                continue;
            var assembly = file.Substring (0, file.Length - ".md".Length);
            var breaking = System.IO.File.Exists (System.IO.Path.Combine (pkgDir.FullPath, assembly + ".breaking.md"));
            rows.Add ($"- [{assembly}]({pkg}/{file}){(breaking ? " — ⚠️ breaking" : "")}");
        }
        if (rows.Count == 0)
            continue;
        hasContent = true;
        body.AppendLine ($"## {pkg}");
        body.AppendLine ();
        foreach (var row in rows)
            body.AppendLine (row);
        body.AppendLine ();
    }

    if (!hasContent)
        return;

    var n = Environment.NewLine;
    var text = $"{API_DIFF_MARKER} {line}{n}{n}> Back to [release notes](../{line}.md).{n}{n}{body}";
    var indexPath = lineDir.CombineWithFilePath ("index.md");
    System.IO.File.WriteAllText (indexPath.FullPath, text);
}

// Copy the generated diff markdown into a line folder: {lineDir}/{id}/{assembly}.md
// (+ {assembly}.breaking.md), the package-namespaced per-assembly shape of spec
// §3.3/§3.4. Also mirrors into output/logs/ for build-log inspection (transient).
void CopyChangelogs (DirectoryPath diffRoot, string id, DirectoryPath lineDir)
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

        // now copy the markdown files into the line's package folder
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
            var changelogPath = lineDir.Combine (id).CombineWithFilePath ($"{dllName}.md");
            EnsureDirectoryExists (changelogPath.GetDirectory ());
            CopyFile (file, changelogPath);
            var changelogOutputPath = (FilePath)$"{ROOT_PATH}/output/logs/changelogs/{id}/{lineDir.GetDirectoryName ()}/{dllName}.md";
            EnsureDirectoryExists (changelogOutputPath.GetDirectory ());
            CopyFile (file, changelogOutputPath);
        }
    }
}

// Run the standard two-pass diff (breaking-only, then full/non-breaking) and copy
// the resulting markdown into {lineDir}/{id}. The two passes produce the
// {dll}.breaking.md and {dll}.md files respectively. NEW side is a published feed
// version. changelogVersion (the line folder) differs from newVersion (the actual
// package diffed, e.g. 4.148.0-rc.1.2) whenever a line is still in preview.
async Task RunBreakingAndFullDiff (NuGetDiff comparer, string id, string oldVersion, string newVersion, DirectoryPath lineDir, string diffRoot)
{
    comparer.MarkdownDiffFileExtension = ".breaking.md";
    comparer.IgnoreNonBreakingChanges = true;
    await comparer.SaveCompleteDiffToDirectoryAsync (id, oldVersion, newVersion, diffRoot);

    comparer.MarkdownDiffFileExtension = null;
    comparer.IgnoreNonBreakingChanges = false;
    await comparer.SaveCompleteDiffToDirectoryAsync (id, oldVersion, newVersion, diffRoot);

    CopyChangelogs (diffRoot, id, lineDir);
}

RunTarget(TARGET);
