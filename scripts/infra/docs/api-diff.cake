// READ FIRST: documentation/dev/release-notes-and-api-diffs.md is the behavior
// spec for this engine. Change the spec first, then make this code match it.
//
// ─────────────────────────────────────────────────────────────────────────────
// API-diff engine (spec §5)
//
// This is the Cake half of the release-notes skill. It regenerates the per-family
// API-diff trees under documentation/docfx/releases/ (spec §3.3/§3.4) by diffing
// every published version of each TRACKED_NUGETS package with
// Mono.ApiTools.NuGetDiff, and writes the co-release map sidecar (spec §3.6) the
// Python release-notes engine consumes.
//
// One target: `docs-api-diff` regenerates the COMMITTED releases/ trees from the
// feed (the Cake generator the §2.2 Prepare phase runs). It is INCREMENTAL and
// SCOPED — like release_notes/generate.py it takes --force / --minVersion / --maxVersion:
// by default it skips a line whose api-diff folder already exists (a shipped
// version's diff never changes), only computing missing/forced lines in range.
// `--force` with no scope regenerates every line at or above its configured
// history floor (e.g. after the api-diff tools themselves change).
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


Task ("docs-api-diff")
    .Does (async () =>
{
    // Incremental + scoped controls (forwarded by prepare.sh; mirror release_notes/generate.py).
    //   --force              rebuild a line even when its api-diff folder exists
    //   --minVersion X       lower bound (inclusive) on the line core to process
    //   --maxVersion Y       upper bound (inclusive); set == min for a single version
    // By default (no --force) a line whose folder already exists is SKIPPED — a
    // shipped version's api diff never changes, so it is a committed cache. A scoped
    // run touches only lines in [min,max]; everything else stays exactly as committed.
    var force = Argument ("force", false);
    var minVersion = Argument ("minVersion", "");
    var maxVersion = Argument ("maxVersion", "");
    var isScoped = !string.IsNullOrEmpty (minVersion) || !string.IsNullOrEmpty (maxVersion);
    RequireScopeAtOrAboveHistoryFloor (minVersion, maxVersion);

    var baseDir = $"{ROOT_PATH}/output/api-diffs-past";
    CleanDirectories (baseDir);

    // A FULL forced rebuild is authoritative: wipe every owned folder up front so a
    // stale *.breaking.md (after a baseline change) or a removed package is pruned.
    // An incremental/scoped run must NOT wipe cached lines — it clears each line's
    // folder individually right before rebuilding it (below), leaving skipped lines
    // (and, in a scoped run, out-of-range lines) exactly as committed. The human
    // pages (<line>.md, TOC.yml, index.md) are owned by the Python engine either way.
    if (force && !isScoped)
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
    // enumerated. Emission is still collapsed to one api diff per release line
    // below — prereleases are needed as candidates, not as their own folders.
    var filter = new NuGetVersions.Filter {
        IncludePrerelease = NUGET_DIFF_PRERELEASE
    };

    // Accumulates the SkiaSharp-line → emitted-HarfBuzz-line mapping as we discover
    // it, so we can write the co-release map sidecar (spec §3.6) at the end. Keyed
    // by SkiaSharp line core; value is the HarfBuzz line core shipping with it.
    var skiaHarfBuzzDeps = ReadCoReleaseMap ();
    var feedSkiaHarfBuzzLines = new HashSet<string> (StringComparer.OrdinalIgnoreCase);

    // Every tracked package in a family writes beneath the same release-line folder.
    // A scoped rebuild must clear that shared folder once, before the first package
    // writes it, rather than once per package (which would erase earlier diffs).
    var clearedLineDirs = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
    // Capture committed markdown before any forced line clearing. A rebuilt path is
    // not "new" merely because ClearGeneratedApiDiffsIn removed it earlier in this
    // run; only paths absent at startup should receive first-write normalization.
    var existingApiDiffFiles = DirectoryExists (RELEASES_PATH)
        ? new HashSet<string> (
            System.IO.Directory.EnumerateFiles (
                RELEASES_PATH.FullPath,
                "*.md",
                System.IO.SearchOption.AllDirectories),
            StringComparer.OrdinalIgnoreCase)
        : new HashSet<string> (StringComparer.OrdinalIgnoreCase);

    // Process every SkiaSharp package (including SkiaSharp.HarfBuzz) before the
    // HarfBuzzSharp family. Scoped HarfBuzz lines are derived from the SkiaSharp
    // co-release dependencies, so those mappings must exist before HarfBuzz runs.
    var packageIds = TRACKED_NUGETS.Keys
        .Where (id => !id.Contains ("NativeAssets"))
        .OrderBy (id => IsHarfBuzzFamily (id) ? 1 : 0)
        .ToList ();
    var inflightSkia = new NuGetVersion (GetVersion ("SkiaSharp"))
        .ToNormalizedString ().Split ('-') [0];
    var inflightHb = new NuGetVersion (GetVersion ("HarfBuzzSharp"))
        .ToNormalizedString ().Split ('-') [0];
    var inflightMappingRecorded = false;

    foreach (var id in packageIds) {
        var isHarfBuzz = IsHarfBuzzFamily (id);
        var versionsConfig = isHarfBuzz ? hbConfig : skiaConfig;
        var family = isHarfBuzz ? "harfbuzzsharp" : "skiasharp";

        // Every managed SkiaSharp package has now been inspected. Add the working-tree
        // fallback before the first HarfBuzz package is scoped so a brand-new in-flight
        // HarfBuzz line can be generated on its first run. Published SkiaSharp.HarfBuzz
        // lines were marked below even when their API-diff folders were cached, so the
        // fallback cannot overwrite an immutable feed-derived mapping.
        if (isHarfBuzz && !inflightMappingRecorded) {
            if (!string.IsNullOrEmpty (inflightSkia)
                    && !string.IsNullOrEmpty (inflightHb)
                    && !feedSkiaHarfBuzzLines.Contains (inflightSkia)) {
                Information ($"Recording in-flight co-release from working tree: SkiaSharp {inflightSkia} → HarfBuzzSharp {inflightHb}.");
                skiaHarfBuzzDeps [inflightSkia] = inflightHb;
            }
            inflightMappingRecorded = true;
        }

        Information ($"Comparing the assemblies in '{id}'...");

        var allVersions = await NuGetVersions.GetAllAsync (id, filter);

        // The newest stable release on the feed. It is the cut-off for preview
        // emission: a preview-only line is only worth an api diff while it is
        // still ahead of the last shipped stable (the active dev line, e.g.
        // 4.148/4.150). Older preview-only lines that never shipped stay pruned.
        var latestStable = allVersions
            .Where (v => !v.IsPrerelease)
            .OrderByDescending (v => v)
            .FirstOrDefault ();

        // Collapse the feed into one entry per release LINE, keyed by the numeric
        // version core with the prerelease label stripped (4.148.0-rc.1.2 ->
        // 4.148.0; the 4th digit of a real 4-part stable like 1.49.2.1 is kept).
        // Each line's api diff is a rollup named by that core, diffed against the
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

        // Decide which lines actually get an api diff emitted (spec §1.4). A line
        // is emitted when ANY of these holds, and not otherwise:
        //   1. it shipped stable (the permanent historical record); or
        //   2. it is listed in versions.json — an intentionally tracked line, e.g. a
        //      superseded preview-only line like 4.147 or 3.0.0 (spec §1.4 rule 2).
        //      "superseded" only skips a line as a *baseline* (§1.3); it does NOT
        //      drop the line's own page — a shipped preview still needs its diff; or
        //   3. it is a preview-only line ahead of the last stable (active dev line); or
        //   4. for HarfBuzzSharp, it is referenced by a SkiaSharp co-release. A
        //      HarfBuzz package can remain prerelease-only even though it shipped inside
        //      a published SkiaSharp preview line, so the co-release is the durable
        //      signal that its API-diff folder must exist.
        // Any other preview-only line (old, never shipped, not listed) is dropped.
        // The history floor (spec §1.4) then removes any line below the configured
        // minimum — the obsolete back-catalogue whose committed folders we keep but
        // do not rebuild (ClearOwnedApiDiffFolders skips them symmetrically). We keep
        // the pre-floor `emittable` set too: the floor line's baseline lives below the
        // floor and must be resolvable from it (§1.3 "baselines are unaffected").
        var emittable = lines
            .Where (l => !l.rep.IsPrerelease
                || IsVersionListed (versionsConfig, l.rep.ToNormalizedString ())
                || latestStable == null
                || l.rep.CompareTo (latestStable) > 0
                || (isHarfBuzz
                    && IsCoReleasedHarfBuzzLine (l.key, skiaHarfBuzzDeps)))
            .ToList ();
        var emit = emittable
            .Where (l => !IsBelowHistoryFloor (l.key, family))
            .ToList ();

        for (var idx = 0; idx < emit.Count; idx++) {
            // The package we actually diff (e.g. 4.148.0-rc.1.2) and the folder we
            // write it to (e.g. 4.148.0).
            var version = emit [idx].rep.ToNormalizedString ();
            var apiDiffVersion = emit [idx].key;

            // Mark every published SkiaSharp.HarfBuzz line before incremental/scoped
            // gating. Its committed co-release mapping remains authoritative even when
            // the API-diff folder is cached and no package extraction is needed.
            if (id == "SkiaSharp.HarfBuzz")
                feedSkiaHarfBuzzLines.Add (apiDiffVersion);

            // The committed folder for this line: SkiaSharp family -> releases/<line>/<id>/…;
            // HarfBuzz family -> releases/harfbuzzsharp/<hb-line>/<id>/… (spec §3.3/§3.4).
            var lineDir = isHarfBuzz
                ? RELEASES_PATH.Combine ("harfbuzzsharp").Combine (apiDiffVersion)
                : RELEASES_PATH.Combine (apiDiffVersion);
            var lineIndex = lineDir.CombineWithFilePath ("index.md");
            // A scoped run still repairs a missing HarfBuzz landing page referenced
            // by any committed SkiaSharp page. The final renderer validates the whole
            // release corpus so a selected page cannot be published alongside an
            // already-broken co-release link. Existing out-of-range folders remain
            // untouched; this exception creates only missing deterministic artifacts.
            var missingCoReleaseApiDiff = isHarfBuzz
                && IsCoReleasedHarfBuzzLine (apiDiffVersion, skiaHarfBuzzDeps)
                && !FileExists (lineIndex);

            // Incremental + scoped gating. We only skip the DIFF WORK, never the line's
            // presence in the `emit` sequence, so a skipped/out-of-range line is still
            // available as a baseline for the lines that roll up past it (below).
            //   - out of [minVersion, maxVersion]  -> leave exactly as committed
            //   - folder already exists and !force -> a shipped diff never changes; reuse
            if (isScoped && !FamilyCoreInRange (
                    apiDiffVersion, isHarfBuzz, minVersion, maxVersion,
                    skiaHarfBuzzDeps) && !missingCoReleaseApiDiff) {
                Debug ($"Skipping '{apiDiffVersion}' of '{id}' (outside --minVersion/--maxVersion).");
                continue;
            }
            if (missingCoReleaseApiDiff)
                Information ($"Backfilling missing co-release API diff '{apiDiffVersion}' of '{id}'.");
            if (!force && FileExists (lineIndex)) {
                Information ($"Skipping '{apiDiffVersion}' of '{id}' (api-diff folder exists; --force to rebuild).");
                continue;
            }

            // Pick the baseline to diff against (spec §1.3):
            //   1. An explicit compare_to override in versions.json wins
            //      (e.g. 4.148 -> 3.119.4, deliberately skipping 4.147).
            //   2. Otherwise diff against the most recent previous EMITTED line that
            //      is NOT itself superseded — a superseded line still gets its own
            //      page but must never serve as a baseline (spec §1.2/§1.3), so the
            //      next line diffs past it and rolls its work up.
            //   3. The LOWEST emitted line (the history-floor line) has no emitted
            //      predecessor: its real baseline sits BELOW the floor and was filtered
            //      out of `emit`. Falling through with a null baseline would diff it
            //      against an empty assembly (0.0.0.0) and re-emit its ENTIRE API as
            //      "new" — a huge, wrong, every-run churn. A baseline may live below the
            //      floor (spec §1.4: "baselines are unaffected"), so resolve it from the
            //      pre-floor `emittable` set and download it FOR COMPARISON ONLY — the
            //      below-floor line is used as a baseline, never emitted itself. This is
            //      one already-cached package (the floor line's immediate predecessor,
            //      also the baseline of the next line up), so the floor's perf win — not
            //      rebuilding the whole obsolete back-catalogue — is preserved.
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
            if (previous == null) {
                // No emitted predecessor -> this is the floor line. Reach below the
                // floor in the pre-floor `emittable` set (lines that would ship a diff
                // if the floor were absent) for the most recent non-superseded line.
                var currentRep = emit [idx].rep;
                var below = emittable
                    .Where (l => l.rep.CompareTo (currentRep) < 0)
                    .OrderByDescending (l => l.rep);
                foreach (var l in below) {
                    var candidate = l.rep.ToNormalizedString ();
                    if (!IsVersionSuperseded (versionsConfig, candidate)) {
                        previous = candidate;
                        break;
                    }
                }
            }

            Information ($"Comparing version '{previous}' vs '{version}' of '{id}' (api diff '{apiDiffVersion}')...");

            // pre-cache so we can have better logs
            Debug ($"Caching version '{version}' of '{id}'...");
            var versionRoot = await comparer.ExtractCachedPackageAsync (id, version);
            if (previous != null) {
                Debug ($"Caching version '{previous}' of '{id}'...");
                await comparer.ExtractCachedPackageAsync (id, previous);
            }

            // generate the diff and copy to the committed releases/ tree
            Debug ($"Running a diff on '{previous}' vs '{version}' of '{id}'...");
            var diffRoot = $"{baseDir}/{id}/{apiDiffVersion}";
            // Incremental runs skipped the up-front ClearOwnedApiDiffFolders, so clear
            // just THIS line's generated files before rebuilding it — a stale
            // *.breaking.md must not survive when the line is regenerated. Multiple
            // packages share lineDir, so clear it only before the first package writes
            // there; clearing for every package would leave only the final package's
            // diff. (A full forced rebuild already wiped everything up front, and a
            // brand-new line has no folder yet.)
            if (!(force && !isScoped)
                    && clearedLineDirs.Add (lineDir.FullPath)
                    && DirectoryExists (lineDir))
                ClearGeneratedApiDiffsIn (lineDir.FullPath);
            // Stage this package's own SkiaSharp/HarfBuzz dependencies at the versions it
            // was built against (read from its nuspec) so inherited types resolve to the
            // contemporaneous assembly, then remove them so they never leak into the next
            // line (spec §5.2). versionRoot is the new-side extracted package.
            var stagedSelfDeps = await StageSelfDepsFromNuspecAsync (comparer, versionRoot);
            try {
                await RunBreakingAndFullDiff (
                    comparer, id, previous, version, lineDir, diffRoot,
                    existingApiDiffFiles);
            } finally {
                UnstageSearchPaths (comparer, stagedSelfDeps);
            }

            // Record the co-release mapping (spec §1.5/§3.6): the HarfBuzz version
            // that ships with a SkiaSharp line is the HarfBuzzSharp dependency of the
            // managed SkiaSharp.HarfBuzz binding at that line. We read it straight
            // from the extracted package's nuspec.
            if (id == "SkiaSharp.HarfBuzz") {
                var hbDep = ReadHarfBuzzDependencyLine (versionRoot);
                if (!string.IsNullOrEmpty (hbDep)) {
                    skiaHarfBuzzDeps [apiDiffVersion] = hbDep;
                }
            }

            Debug ($"Diff complete of version '{version}' of '{id}'.");
        }
        Information ($"Diff complete of '{id}'.");
    }

    // Write the per-line API-diff index.md landing pages (spec §3.3/§3.4) and the
    // co-release map sidecar (spec §3.6) the Python release-notes engine consumes.
    WriteApiDiffFolderIndexes (existingApiDiffFiles);
    WriteCoReleaseMap (skiaHarfBuzzDeps);

    // clean up after working
    CleanDirectories (baseDir);
});


////////////////////////////////////////////////////////////////////////////////////////////////////
// API-DIFF OUTPUT HELPERS
////////////////////////////////////////////////////////////////////////////////////////////////////

// Marker that prefixes the first line of every file the comparer writes (the
// <assembly>.md / <assembly>.breaking.md diffs and the "No changes." stub in
// CopyApiDiffs). Hand-authored files nested in a line folder do not carry it.
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
            // line folder individually.
            foreach (var lineDir in GetSubDirectories (dir)) {
                var lineKey = lineDir.GetDirectoryName ();
                // History floor (spec §1.4): keep committed obsolete folders intact.
                if (lineKey.Length > 0 && char.IsDigit (lineKey [0])
                        && !IsBelowHistoryFloor (lineKey, "harfbuzzsharp"))
                    ClearGeneratedApiDiffsIn (lineDir.FullPath);
            }
            DeleteEmptyDirectories (dir.FullPath);
            continue;
        }

        // SkiaSharp family: a line folder is a top-level directory named by a version core.
        // A folder below the history floor (spec §1.4) is left exactly as committed —
        // not cleared here and not re-emitted above — so the obsolete back-catalogue
        // survives a floored regen instead of being wiped.
        if (name.Length > 0 && char.IsDigit (name [0])
                && !IsBelowHistoryFloor (name, "skiasharp")) {
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

// True iff `core` is within the inclusive [minVersion, maxVersion] range. Empty bounds
// are open. Used to scope an incremental run to a version range (mirrors release_notes/generate.py's
// --min-version/--max-version), comparing on the NuGet version core.
bool CoreInRange (string core, string minVersion, string maxVersion)
{
    var v = new NuGetVersion (core);
    if (!string.IsNullOrEmpty (minVersion) && v.CompareTo (new NuGetVersion (minVersion)) < 0)
        return false;
    if (!string.IsNullOrEmpty (maxVersion) && v.CompareTo (new NuGetVersion (maxVersion)) > 0)
        return false;
    return true;
}

// Scoped arguments are SkiaSharp page versions. HarfBuzzSharp has its own version
// line, so map the selected SkiaSharp lines through the co-release dependencies
// instead of comparing e.g. 14.2.1.2 directly with 4.150.0.
bool FamilyCoreInRange (
    string core,
    bool isHarfBuzz,
    string minVersion,
    string maxVersion,
    IDictionary<string, string> skiaHarfBuzzDeps)
{
    if (!isHarfBuzz)
        return CoreInRange (core, minVersion, maxVersion);

    return skiaHarfBuzzDeps.Any (kvp =>
        CoreInRange (kvp.Key, minVersion, maxVersion)
        && string.Equals (kvp.Value, core, StringComparison.OrdinalIgnoreCase));
}

bool IsCoReleasedHarfBuzzLine (
    string core,
    IDictionary<string, string> skiaHarfBuzzDeps)
{
    return skiaHarfBuzzDeps.Values.Any (value =>
        string.Equals (value, core, StringComparison.OrdinalIgnoreCase));
}

// Write the co-release map sidecar (spec §3.6): a plain { "skia_line": "hb_line" } object,
// one entry per SkiaSharp line giving the HarfBuzz line that ships with it. The api-diff
// LINK is pure derivation (`harfbuzzsharp/<hb_line>/index.md`), so the Python engine builds
// it — we do not store it. `hb_line` is authoritative at full §1.1 granularity. This is the
// only thing that crosses from this engine into the Python release-notes engine.
//
// MERGE, don't overwrite: an incremental/scoped run only recomputed the lines it processed;
// every other line's mapping is immutable (a shipped package's HarfBuzz dependency never
// changes), so preserve the committed entries and overlay only what this run recomputed.
SortedDictionary<string, string> ReadCoReleaseMap ()
{
    var sourcesDir = RELEASES_PATH.Combine ("_sources");
    var sidecar = sourcesDir.CombineWithFilePath ("co-release-map.json");
    var result = new SortedDictionary<string, string> (StringComparer.Ordinal);
    if (FileExists (sidecar)) {
        var existing = JToken.Parse (System.IO.File.ReadAllText (sidecar.FullPath));
        if (existing is JObject obj) {
            foreach (var prop in obj.Properties ())
                result [prop.Name] = (string) prop.Value;
        } else if (existing is JArray arr) {
            // Legacy array-of-objects format ({skia_line, hb_line, hb_link}); read hb_line.
            foreach (var e in arr.OfType<JObject> ()) {
                var k = (string) e ["skia_line"];
                var v = (string) e ["hb_line"];
                if (!string.IsNullOrEmpty (k) && !string.IsNullOrEmpty (v))
                    result [k] = v;
            }
        }
    }
    return result;
}

void WriteCoReleaseMap (IDictionary<string, string> skiaHarfBuzzDeps)
{
    EnsureDirectoryExists (RELEASES_PATH);
    var sourcesDir = RELEASES_PATH.Combine ("_sources");
    EnsureDirectoryExists (sourcesDir);
    var sidecar = sourcesDir.CombineWithFilePath ("co-release-map.json");

    var merged = ReadCoReleaseMap ();
    foreach (var kvp in skiaHarfBuzzDeps)
        merged [kvp.Key] = kvp.Value;

    var outObj = new JObject ();
    foreach (var kvp in merged)
        outObj [kvp.Key] = kvp.Value;

    System.IO.File.WriteAllText (sidecar.FullPath, outObj.ToString (Newtonsoft.Json.Formatting.Indented));
    Information ($"Wrote co-release map sidecar with {outObj.Count} entries: {sidecar.FullPath}");
}

// Write the generated index.md landing page for every emitted API-diff folder (spec
// §3.3/§3.4): the deterministic target of each hub page's API-changes link (§4.4). For
// each line folder (SkiaSharp `releases/<line>/` and HarfBuzz
// `releases/harfbuzzsharp/<hb-line>/`) it lists every `<package>/<assembly>.md` diff,
// flags any with a `<assembly>.breaking.md` sibling as breaking, and links back to the
// `../<line>.md` hub. It carries the API_DIFF_MARKER like every other generated file, so
// the §3.5 wipe regenerates it each run.
void WriteApiDiffFolderIndexes (ISet<string> existingApiDiffFiles)
{
    if (!DirectoryExists (RELEASES_PATH))
        return;

    // SkiaSharp family: line folders directly under releases/ (name starts with a digit).
    foreach (var dir in GetSubDirectories (RELEASES_PATH)) {
        var name = dir.GetDirectoryName ();
        if (name.Length > 0 && char.IsDigit (name [0]))
            WriteApiDiffFolderIndex (
                dir, name, $"../{name}.md", existingApiDiffFiles);
    }

    // HarfBuzz family: line folders one level deeper, under releases/harfbuzzsharp/.
    var hbRoot = RELEASES_PATH.Combine ("harfbuzzsharp");
    if (DirectoryExists (hbRoot)) {
        foreach (var dir in GetSubDirectories (hbRoot)) {
            var name = dir.GetDirectoryName ();
            if (name.Length > 0 && char.IsDigit (name [0]))
                WriteApiDiffFolderIndex (
                    dir, name, null, existingApiDiffFiles);
        }
    }
}

// Write one line folder's index.md. SkiaSharp folders link to their matching
// release page. HarfBuzzSharp has no standalone human page: its summary and API
// diff link live on the co-shipping SkiaSharp page, so its index has no backlink.
void WriteApiDiffFolderIndex (
    DirectoryPath lineDir,
    string line,
    string releaseNotesHref,
    ISet<string> existingApiDiffFiles)
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
    var backLink = string.IsNullOrEmpty (releaseNotesHref)
        ? ""
        : $"> Back to [release notes]({releaseNotesHref}).{n}{n}";
    var text = $"{API_DIFF_MARKER} {line}{n}{n}{backLink}{body}";
    var indexPath = lineDir.CombineWithFilePath ("index.md");
    var isNewIndex = !existingApiDiffFiles.Contains (indexPath.FullPath);
    System.IO.File.WriteAllText (indexPath.FullPath, text);
    if (isNewIndex)
        NormalizeGeneratedMarkdown (indexPath);
}

void NormalizeGeneratedMarkdown (FilePath path)
{
    var text = System.IO.File.ReadAllText (path.FullPath);
    System.IO.File.WriteAllText (
        path.FullPath,
        text.TrimEnd ('\r', '\n') + Environment.NewLine);
}

// Copy the generated diff markdown into a line folder: {lineDir}/{id}/{assembly}.md
// (+ {assembly}.breaking.md), the package-namespaced per-assembly shape of spec
// §3.3/§3.4. Also mirrors into output/logs/ for build-log inspection (transient).
void CopyApiDiffs (
    DirectoryPath diffRoot,
    string id,
    DirectoryPath lineDir,
    ISet<string> existingApiDiffFiles)
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
            var apiDiffPath = lineDir.Combine (id).CombineWithFilePath ($"{dllName}.md");
            var isNewApiDiff = !existingApiDiffFiles.Contains (apiDiffPath.FullPath);
            EnsureDirectoryExists (apiDiffPath.GetDirectory ());
            CopyFile (file, apiDiffPath);
            // Preserve byte-for-byte history for existing generated diffs during a
            // forced rebuild. Only a newly created file needs its inherited tool
            // padding normalized before it becomes committed history.
            if (isNewApiDiff)
                NormalizeGeneratedMarkdown (apiDiffPath);
            var apiDiffOutputPath = (FilePath)$"{ROOT_PATH}/output/logs/api-diffs/{id}/{lineDir.GetDirectoryName ()}/{dllName}.md";
            EnsureDirectoryExists (apiDiffOutputPath.GetDirectory ());
            CopyFile (file, apiDiffOutputPath);
        }
    }
}

// Run the standard two-pass diff (breaking-only, then full/non-breaking) and copy
// the resulting markdown into {lineDir}/{id}. The two passes produce the
// {dll}.breaking.md and {dll}.md files respectively. NEW side is a published feed
// version. apiDiffVersion (the line folder) differs from newVersion (the actual
// package diffed, e.g. 4.148.0-rc.1.2) whenever a line is still in preview.
async Task RunBreakingAndFullDiff (
    NuGetDiff comparer,
    string id,
    string oldVersion,
    string newVersion,
    DirectoryPath lineDir,
    string diffRoot,
    ISet<string> existingApiDiffFiles)
{
    comparer.MarkdownDiffFileExtension = ".breaking.md";
    comparer.IgnoreNonBreakingChanges = true;
    await comparer.SaveCompleteDiffToDirectoryAsync (id, oldVersion, newVersion, diffRoot);

    comparer.MarkdownDiffFileExtension = null;
    comparer.IgnoreNonBreakingChanges = false;
    await comparer.SaveCompleteDiffToDirectoryAsync (id, oldVersion, newVersion, diffRoot);

    CopyApiDiffs (diffRoot, id, lineDir, existingApiDiffFiles);
}

RunTarget(TARGET);
