# Phases 01–03: Research

## Phase 1 — establish the exact range

When automation supplied the runtime variables, verify them and skip branch re-detection.
For a standalone run:

1. Read current milestone from `SkMilestone.h`, `VERSIONS.txt`, and `cgmanifest.json`.
2. Check both repositories for an existing update PR.
3. Fetch the target upstream ref.
4. Use `main`/`skiasharp` for the newest line. For an older supported milestone, use an
   existing matching release branch in **both** repositories; never create release branches here.

The three supported modes are:

| Mode | Upstream | Parent base | mono/skia base | Head |
|---|---|---|---|---|
| Milestone | `chrome/m{TARGET}` | `main` | `skiasharp` | `skia-sync/m{TARGET}` |
| Release bug-fix | `chrome/m{TARGET}` | matching release | matching release | `skia-sync/release-…` |
| Tip | `main` | `main` | `skiasharp` | `skia-sync/main` |

## Phase 2 — analyze behavior, not only signatures

Read [../breaking-changes-checklist.md](../breaking-changes-checklist.md).

Use the base branch's recorded upstream commit:

```bash
BASE_UPSTREAM_SHA="${SKIA_BASE_UPSTREAM_SHA:-$(git -C ../.. show "origin/{BASE_BRANCH}:cgmanifest.json" |
  jq -r '.registrations[] | select(.component.other.name == "skia") | .upstream_merge_commit')}"
TARGET_UPSTREAM_REF="upstream/{UPSTREAM_REF}"
git cat-file -e "${BASE_UPSTREAM_SHA}^{commit}"
git rev-parse --verify "${TARGET_UPSTREAM_REF}^{commit}"
DIFF_RANGE="${BASE_UPSTREAM_SHA}..${TARGET_UPSTREAM_REF}"
```

Audit:

- Release notes and public headers.
- Removed/moved APIs and every C API reference.
- Asserted C/C++ struct layouts.
- Added/deleted source files and build lists.
- Shared, Ganesh, Graphite, Dawn, and backend implementation paths.
- Wrapped factories/context creation: unchanged signatures can still add a required context
  field, remove fallback behavior, or gain a null-return path.
- `DEPS` differences between `origin/{SKIA_BASE_BRANCH}` and the target.

Write `$ARTIFACT_DIR/skia-breaking-change-analysis.md`. It must identify every HIGH/MEDIUM
risk and state why each relevant implementation path is safe or needs adaptation.

## Phase 3 — independent discrepancy review

Launch one synchronous independent validator:

```text
task(
  agent_type="explore",
  model="claude-sonnet-5",
  mode="sync",
  prompt=<../validation-prompt.md with variables substituted>
)
```

The validator reads the primary analysis and repository, then reports only missed items,
incorrect classifications, or unsafe dependency decisions. It does not repeat the analysis.

Write its result to `$ARTIFACT_DIR/skia-validation-review.md` and integrate every finding into
the primary analysis.

## Gate

- Exact diff range verified.
- Primary analysis exists.
- Independent review exists.
- No unresolved HIGH/MEDIUM finding remains.

Stop here until both report files exist and the review is integrated. Do not load the merge
phase, create either branch, or begin the merge before this gate passes.
