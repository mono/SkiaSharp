# 01–03 — Resolve and research

Complete this file before creating either feature branch.

## Phase 01 — resolve the exact update

### Local preflight

For a developer-run update:

1. Confirm the parent and `externals/skia` worktrees are clean.
2. Initialize submodules and verify `git`, `gh`, Python 3, and the .NET SDK are available.
3. Fetch the parent base candidates and paired Skia base candidates.
4. Ensure the paired Skia repository has an `upstream` remote for `https://github.com/google/skia.git`.
5. Check both repositories for an existing PR or branch for the requested target. Continue an
   existing update only when its bases and upstream ref match; never overwrite unrelated work.

Resolve these values, including `IS_RELEASE`, and export the corresponding `SKIA_SYNC_*`
variables from `SKILL.md`:

| Request | Parent base | Paired Skia base | Upstream ref | Head |
|---|---|---|---|---|
| Newest milestone | `main` | `skiasharp` | `chrome/m{TARGET}` | `skia-sync/m{TARGET}` |
| Older supported milestone | existing matching release branch in both repos | same release branch | `chrome/m{TARGET}` | `skia-sync/release-…` |
| Upstream tip | `main` | `skiasharp` | `main` | `skia-sync/main` |

Never create a release branch here. If an older supported line lacks a matching branch in either
repository, stop and report that the release process must create it.

Read `{CURRENT}` from the selected parent base's `scripts/VERSIONS.txt`; cross-check
`externals/skia/include/core/SkMilestone.h` and the Skia registration in `cgmanifest.json`.
For automation, consume the supplied values instead and verify the referenced branches exist.

### Fetch and verify the range

Fetch:

- `origin/{BASE_BRANCH}` in the parent.
- `origin/{SKIA_BASE_BRANCH}` in the paired Skia repository.
- `upstream/{UPSTREAM_REF}` in the paired Skia repository.
- The base branch's recorded `upstream_merge_commit`, by SHA.

Export the exact parent-base submodule pointer as `SKIA_SYNC_SKIA_BASE_SHA`; use it instead of a
moving remote branch ref in later integrity checks.

Use the base branch's recorded commit, not a milestone label:

```bash
BASE_UPSTREAM_SHA="${SKIA_BASE_UPSTREAM_SHA:-$(git show "origin/{BASE_BRANCH}:cgmanifest.json" |
  jq -r '.registrations[] | select(.component.other.name == "skia") | .upstream_merge_commit')}"
TARGET_UPSTREAM_REF="upstream/{UPSTREAM_REF}"
git -C externals/skia cat-file -e "${BASE_UPSTREAM_SHA}^{commit}"
git -C externals/skia rev-parse --verify "${TARGET_UPSTREAM_REF}^{commit}"
TARGET_UPSTREAM_SHA=$(git -C externals/skia rev-parse "${TARGET_UPSTREAM_REF}^{commit}")
export SKIA_SYNC_BASE_UPSTREAM_SHA="$BASE_UPSTREAM_SHA"
export SKIA_SYNC_TARGET_UPSTREAM_SHA="$TARGET_UPSTREAM_SHA"
DIFF_RANGE="${BASE_UPSTREAM_SHA}..${TARGET_UPSTREAM_REF}"
```

Use ancestry, not SHA equality, to determine whether the target is already contained in the
selected paired Skia base. If it is, stop before branching. A matching milestone number alone is
not proof of no work; same-milestone bug-fix commits still count.

## Phase 02 — analyze behavior, not only signatures

Read [../breaking-changes-checklist.md](../breaking-changes-checklist.md). Audit the authoritative
`DIFF_RANGE`:

- Release notes and public headers.
- Removed/moved APIs and every C API reference.
- Asserted C/C++ struct layouts.
- Added/deleted source files and build lists.
- Shared, Ganesh, Graphite, Dawn, and platform backend implementation paths.
- Wrapped factories/context creation: unchanged signatures can still add required context state,
  remove fallback behavior, or gain a null-return path.
- `DEPS` differences between `origin/{SKIA_BASE_BRANCH}` and the target.

Write `$ARTIFACT_DIR/skia-breaking-change-analysis.md`. Identify every HIGH/MEDIUM risk and state
why each relevant implementation path is safe or needs adaptation. Do not predict a fix without
source evidence.

Also write `$ARTIFACT_DIR/skia-dependency-decisions.md` before review. Account for every fork-base
versus target revision and enabled/commented-state difference, with a provisional preserve,
accept-target, or compatibility-roll decision backed by source/fork evidence. Phase 05 finalizes
this report during conflict resolution; Phase 07 updates it when a build disproves a decision.
Treat enabled/commented state and revision choice as separate decisions: preserving the fork's
dependency shape does not by itself prove that an upstream revision roll is safe.

The final table must contain: dependency, base URL/SHA/state, final URL/SHA/state, decision,
cgmanifest component (or `not tracked`), authoritative version source, derived semantic version,
and manifest action. Phase 07 fills the final URL/SHA/version columns from the deterministic
`skia-dependency-changes.json` signal rather than relying on memory of earlier merge work.

## Phase 03 — independent discrepancy review

Launch one synchronous, read-only validator using
[../validation-prompt.md](../validation-prompt.md), substituting the exact range,
`$ARTIFACT_DIR/skia-breaking-change-analysis.md`, and
`$ARTIFACT_DIR/skia-dependency-decisions.md`. When the `task` tool is available, use
`agent_type="explore"`, `model="claude-sonnet-5"`, and `mode="sync"`. Locally, use an independent
reviewer if available; otherwise perform a distinct second pass and record that limitation.

The review reports only missed items, incorrect classifications, unsafe dependency decisions, and
a concise confirmation checklist. Write it to `$ARTIFACT_DIR/skia-validation-review.md` and
integrate every finding into the primary analysis.

## Gate

- All runtime values and refs are verified.
- The exact diff range is authoritative.
- Primary analysis, dependency decisions, and independent review artifacts exist.
- No unresolved HIGH/MEDIUM finding remains.

Do not load Phase 04, create branches, or begin the merge before this gate passes.
