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
//                         the feed (the §2.2 step-1 the unified workflow runs).
//   docs-api-diff       — current: diff the freshly built, unpublished CI packages
//                         against the feed as a build-pipeline gate (spec §5.3);
//                         writes only transient output/api-diff, never releases/.
//
// Shared machinery (the NuGet-diff comparer, layout helpers, versions.json loading)
// lives in scripts/infra/shared/api-diff-tools.cake and is #loaded below. The
// release-line / baseline / supersession rules are identical to the Python engine
// (spec §1); see versions.json.
// ─────────────────────────────────────────────────────────────────────────────

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../../.."));

#load "../../../../scripts/infra/shared/shared.cake"
#load "../../../../scripts/infra/shared/download.cake"
#load "../../../../scripts/infra/shared/api-diff-tools.cake"

// All committed API diffs live inside the docfx site so docfx renders them and the
// human pages can link to them with internal links (spec §3).
DirectoryPath RELEASES_PATH = MakeAbsolute (ROOT_PATH.Combine ("documentation/docfx/releases"));

// The HarfBuzz family emits into a parallel lowercase tree (spec §3.4); everything
// else (including the managed SkiaSharp.HarfBuzz binding) is the SkiaSharp family
// (spec §1.5). The folder name is the family-tree root, distinct from the package id.
bool IsHarfBuzzFamily (string id) =>
    id == "HarfBuzzSharp" || id.StartsWith ("HarfBuzzSharp.");


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
    var hbLinesEmitted = new SortedSet<string> ();
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

        // Decide which lines actually get a changelog emitted (spec §1.4):
        //   - a line that shipped stable: always (the historical record);
        //   - a preview-only line that was abandoned (superseded in versions.json,
        //     e.g. 4.147): never — it is absorbed into its successor's diff;
        //   - a preview-only line ahead of the last stable: yes (active dev line);
        //   - any other preview-only line (old, never shipped): no.
        var emit = lines
            .Where (l => !l.rep.IsPrerelease
                || (!IsVersionSuperseded (versionsConfig, l.rep.ToNormalizedString ())
                    && (latestStable == null || l.rep.CompareTo (latestStable) > 0)))
            .ToList ();

        for (var idx = 0; idx < emit.Count; idx++) {
            // The package we actually diff (e.g. 4.148.0-rc.1.2) and the folder we
            // write it to (e.g. 4.148.0).
            var version = emit [idx].rep.ToNormalizedString ();
            var changelogVersion = emit [idx].key;

            // Pick the baseline to diff against (spec §1.3):
            //   1. An explicit compare_to override in versions.json wins
            //      (e.g. 4.148 -> 3.119.4, deliberately skipping 4.147).
            //   2. Otherwise diff against the previous EMITTED line, so skipped
            //      previews and pruned lines are transparent.
            var previous = FindCompareToBaseline (versionsConfig, version, allVersions);
            if (previous == null && idx > 0)
                previous = emit [idx - 1].rep.ToNormalizedString ();

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

            if (isHarfBuzz)
                hbLinesEmitted.Add (changelogVersion);

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

    WriteCoReleaseMap (skiaHarfBuzzDeps, hbLinesEmitted);

    // clean up after working
    CleanDirectories (baseDir);
});


////////////////////////////////////////////////////////////////////////////////////////////////////
// API-DIFF OUTPUT HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

// Clear only the API-diff folders this engine owns (spec §3.5): the per-line
// <line>/ package folders under releases/ and the entire harfbuzzsharp/ tree. A
// "<line>" folder is a directory whose name is a version core (starts with a
// digit); the human pages are FILES (<line>.md) and the TOC.yml/index.md are left
// untouched. This is deliberately conservative so the Python-owned pages survive.
void ClearOwnedApiDiffFolders ()
{
    if (!DirectoryExists (RELEASES_PATH))
        return;

    foreach (var dir in GetSubDirectories (RELEASES_PATH)) {
        var name = dir.GetDirectoryName ();
        if (name == "harfbuzzsharp" || (name.Length > 0 && char.IsDigit (name [0])))
            DeleteDirectory (dir, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
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
    // bracketed range like "[8.3.0, )"; take the first version token and reduce it
    // to its major.minor.patch core (the §1.1 line key).
    var range = ((string)dep.Attribute ("version") ?? "").Trim ('[', '(', ']', ')', ' ');
    var token = range.Split (',') [0].Trim ();
    if (string.IsNullOrEmpty (token))
        return null;
    var v = new NuGetVersion (token);
    return $"{v.Major}.{v.Minor}.{v.Patch}";
}

// Write the co-release map sidecar (spec §3.6): one entry per emitted SkiaSharp
// line giving the HarfBuzz line that ships with it and the site-relative link to
// its API-diff folder. When a SkiaSharp line ships an UNCHANGED HarfBuzz (no folder
// of its own emitted), the link points at the most recent DISTINCT HarfBuzz folder
// that did change (spec §3.4). This is the only thing that crosses from this engine
// into the Python release-notes engine.
void WriteCoReleaseMap (Dictionary<string, string> skiaHarfBuzzDeps, SortedSet<string> hbLinesEmitted)
{
    EnsureDirectoryExists (RELEASES_PATH);
    var sidecar = RELEASES_PATH.CombineWithFilePath ("co-release-map.json");

    var entries = new JArray ();
    foreach (var kvp in skiaHarfBuzzDeps.OrderBy (k => k.Key)) {
        var skiaLine = kvp.Key;
        var hbLine = kvp.Value;

        // If the exact HarfBuzz line wasn't emitted as its own folder (unchanged
        // co-release), fall back to the newest emitted HarfBuzz folder at or below
        // it, so the link still resolves to a real folder.
        var linkLine = hbLinesEmitted.Contains (hbLine)
            ? hbLine
            : hbLinesEmitted.Reverse ().FirstOrDefault (h => string.Compare (h, hbLine, StringComparison.Ordinal) <= 0)
                ?? hbLinesEmitted.LastOrDefault ();

        var entry = new JObject ();
        entry ["skia_line"] = skiaLine;
        entry ["hb_line"] = hbLine;
        entry ["hb_link"] = linkLine != null ? $"harfbuzzsharp/{linkLine}/" : null;
        entries.Add (entry);
    }

    System.IO.File.WriteAllText (sidecar.FullPath, entries.ToString (Newtonsoft.Json.Formatting.Indented));
    Information ($"Wrote co-release map sidecar with {entries.Count} entries: {sidecar.FullPath}");
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
