---
name: merge-skia-update
description: >
  Safely land a reviewed Skia milestone bump or servicing sync across the
  paired mono/skia and mono/SkiaSharp pull requests. Use whenever a maintainer
  asks to merge, land, finalize, or complete a Skia bump/sync, or asks what is
  still required before those PRs can merge. Verifies upstream freshness,
  two-parent ancestry, fork and dependency decisions, parent rebase state,
  release-note reachability, prior-line preservation, CI, packages, merge
  order, and the supported-line rotation. Use update-skia to create or refresh
  the update and review-skia-update for the deep review; this skill owns the
  final landing.
  Do not use it to merge Google Skia into the fork or for bleeding-edge
  upstream-main syncs.
compatibility: Requires git, gh, Python 3, and access to mono/skia and mono/SkiaSharp.
---

# Merge Skia Update

Land the two-repository update without losing upstream ancestry or leaving
SkiaSharp pointed at an orphaned mono/skia PR commit.

This skill covers `chrome/mN` milestone bumps and same-milestone servicing
syncs. Use `update-skia` for a bleeding-edge Google Skia `main` sync because a
continuously moving upstream tip needs a different landing policy.

```
reviewed PRs
  -> preserve the previous milestone as release/A.B.x
  -> merge mono/skia with a merge commit
  -> point SkiaSharp at the resulting skiasharp commit
  -> rebase and validate the parent
  -> merge mono/SkiaSharp
```

## Safety contract

- Treat "is this ready?" as read-only. Merge only when the user explicitly asks
  to merge or land the PRs.
- Never merge around a failed gate. Report the blocker and stop.
- Never squash or rebase the mono/skia PR. Its upstream merge ancestry is part
  of the sync record.
- Never merge the SkiaSharp PR first.
- Use `--force-with-lease` when a reviewed feature branch must be rebased;
  never use an unguarded force push.
- Do not replace a native source build with `externals-download`.
- Do not weaken review or cross-platform CI gates for a same-milestone servicing
  sync. Historical fast merges are not a supported landing path.
- A draft PR is expected while gates remain open and is not by itself a
  readiness blocker. Mark it ready only immediately before an approved merge.

## 1. Resolve the pair

Identify and cross-check:

- the mono/skia PR, its `skiasharp` or release-line base, and head SHA;
- the mono/SkiaSharp companion PR, its parent base, and head SHA;
- the milestone, upstream `chrome/mN` ref and exact SHA;
- the shared `skia-sync/...` feature branch.

Read both PR bodies, commits, changed paths, reviews, inline comments, and
checks. The reciprocal PR links must identify this exact pair.

Search both repositories for competing open work before proceeding. Treat
another PR as competing only when evidence shows at least one of:

- another Skia sync shares the same sync line, head, or intended sync base;
- a duplicate `externals/skia` or `cgmanifest.json` repin;
- overlapping DEPS dependency names, revisions, or hunks that would invalidate
  one PR if the other landed.

Record an explicit merge, close, or supersede disposition for every competing
PR, then require that disposition to be completed before landing: the PR must
already be merged, closed, or explicitly superseded. Do not leave any
demonstrated competing PR open. Merely touching `DEPS`, the gitlink, or another
commonly changed file is not enough: report unrelated parallel work as
non-blocking with the evidence that distinguishes it.

Verify repository metadata on both PRs:

- every bump and servicing sync has `type/milestone-sync` and
  `partner/agentic-workflows`;
- a true milestone bump additionally has `type/milestone-bump`, and the parent
  PR is assigned the release milestone (historically
  `4.N.0-preview.1`);
- a same-milestone servicing sync is not labeled as a milestone bump.

## 2. Recheck the review gates

Do not rely on a review captured before the latest push.

1. Compare the live Google Skia ref with the recorded
   `cgmanifest.json` `upstream_merge_commit`.
   - If upstream moved, stop and run `update-skia` to refresh the existing PR.
   - A milestone number match is not enough; compare exact ancestry.
2. Confirm mono/skia contains a genuine two-parent merge whose upstream parent
   is the target SHA and whose fork parent is the recorded base.
3. Require either a current, persisted, schema-valid `review-skia-update`
   report for the exact live heads or explicit evidence of an independent
   human review. A PR-body self-attestation is not review evidence. If the
   evidence is missing, invalid, or stale after any push or rebase, stop and
   run or rerun `review-skia-update`.
4. Resolve every finding from that review:
   - added, removed, or changed fork patches;
   - DEPS URLs, SHAs, and enabled/commented states;
   - C API and generated binding changes;
   - restored legacy code or dangling references;
   - wrapper ownership, ABI, and test coverage.
5. Confirm release-note sidecars mention only behavior reachable through the
   current C and managed bindings.
6. Confirm the parent PR is based directly on the current target branch:
   - `behind_by == 0`;
   - its merge base is the current base tip;
   - GitHub lists only update commits and intended files, not commits already
     on the base branch.
7. Verify every version surface, including:
   - Skia milestone and upstream SHA;
   - native soname and `SK_C_INCREMENT`;
   - SkiaSharp assembly/file/package versions;
   - every crossed milestone when the update skips milestones;
   - the HarfBuzzSharp milestone bucket, advanced by 100 for every crossed
     milestone when native HarfBuzz did not change.
8. Audit temporary compatibility work introduced to make CI green: disabled
   features or backends, temporary source exclusions, GN flags, platform
   workarounds, and fork backports. Restore prior support before merge whenever
   possible. Otherwise require an explicitly accepted limitation and a linked
   tracking issue.
9. Mechanically inspect every enabled tracked DEPS Component Governance
   registration's nested `skia_dependency` object. Require non-empty
   `revision`, `version_reviewed_identity`, and `version_source` values, and
   confirm the registered semantic version matches that evidence. Do not infer
   missing evidence from top-level `component` fields. Treat legacy compliance
   backfills as a separate gate even when the dependency version itself did
   not change.

## 3. Make the supported-line decision

Read `scripts/infra/docs/versions.json`. Its `support` block is hand-maintained
release policy and also drives the scheduled Skia-sync rotation.

Before landing a new milestone, report:

- the current stable and preview lines;
- the rotation produced by `stable + preview + highest-supported-plus-one`;
- whether the newly merged main milestone and intended next milestone will be
  monitored.

Do not infer support from Chrome channels or edit the file automatically.
After reporting the computed rotation, ask for and consume an explicit
maintainer decision. If approved, update support in the parent PR and
revalidate the documentation/index behavior that consumes it. If declined,
record that decision. If no decision exists, or the rotation would omit a line
the maintainer intends to service, stop. History is not policy: M151 updated
this file during landing while M152 omitted it and left the rotation stale.

## 4. Validate the pre-merge heads

Both current heads must be mergeable and free of pending or failing required
checks. CI must cover the repository's native, managed, test, visual, package,
and sample stages.

A source build on the available host is necessary but insufficient. Full native
CI across Windows, Linux, Apple platforms, Android, Tizen, and WASM is the
primary detector for compiler, SDK, linker, GN, and packaging fallout. Require
that matrix for both true bumps and same-milestone servicing syncs.

Validate packages from the exact successful parent build:

- package IDs and expected SkiaSharp/HarfBuzzSharp versions;
- dependency ranges;
- archive integrity;
- representative native assets and symbols;
- restore plus runtime loading on the available host.

PR artifacts are normally unsigned; signing is a publication-pipeline concern.
Retry a failed CI job only when its logs prove an infrastructure failure such
as a network timeout. Do not classify a compile, test, or rendering failure as
a flake.

## 5. Preserve the previous milestone

For a true milestone bump, preserve the line being replaced before either PR
merges. A same-milestone servicing sync does not create another release line.

1. Derive `release/A.B.x` from the previous SkiaSharp product line; for example,
   an M153 bump preserves `release/4.152.x`.
2. Re-fetch and record the current mono/skia base and mono/SkiaSharp base tips.
3. Before any write, verify both source tips identify the previous milestone
   and the parent base's `externals/skia` gitlink equals the recorded native
   base tip.
4. Preflight both destination refs before creating either branch. Each must be
   absent or already resolve to its intended source SHA; stop before any write
   if either conflicts.
5. Present both planned refs and exact source SHAs and obtain explicit
   maintainer confirmation.
6. Create any missing mono/skia release branch first from the recorded native
   base tip, then create the missing identically named mono/SkiaSharp release
   branch from the recorded parent base tip. Use guarded non-force ref creation.
7. Never force or move an existing release branch.
8. Verify the parent release branch's `externals/skia` gitlink equals the native
   release branch tip and both branches still identify the previous milestone.

Record both branch SHAs. This split keeps the previous milestone serviceable
after the default branches advance.

## 6. Merge mono/skia first

1. Re-fetch both PRs and upstream immediately before merging.
2. For a true bump, verify both current base tips still equal the recorded
   `release/A.B.x` branch SHAs. If either base advanced after the split, stop
   and reconcile the release-line decision rather than merging stale state.
3. Verify the native PR head is based on the current base tip: its merge base
   must equal that tip and it must not be behind. Compute the prospective merge
   tree and require it to equal the reviewed PR-head tree before performing the
   irreversible merge.
4. Invoke `pr-commit-message` for the mono/skia PR using the final head,
   companion PR, compare range, review findings, and validation evidence.
5. Mark the PR ready if needed.
6. Merge with GitHub's **merge commit** strategy. Never squash or rebase it.
7. Fetch the resulting base-branch tip and record its exact SHA and tree.
8. Verify that resulting base-branch commit has exactly the reviewed PR-head
   tree and that its history contains the authoritative two-parent upstream
   merge. Stop on any tree difference or missing ancestry.

The resulting `skiasharp` commit may differ from the PR head because GitHub can
create a merge commit. That resulting base-branch SHA is authoritative.

## 7. Refresh the SkiaSharp parent

1. Check out the companion feature branch.
2. Run `update-skia/scripts/update_versions.py` with the original current and
   target milestones, exact upstream SHA, parent-base SHA, and Skia-base SHA.
   The helper rewrites and validates final metadata, so inspect its diff; do
   not treat it as read-only and do not hand-edit generated bindings.
3. Apply an approved `versions.json` support decision and revalidate its
   documentation/index consumers.
4. Rebase the parent feature branch onto the latest target-branch tip.
5. As the last product-affecting parent change before final CI, update
   `externals/skia` and its `cgmanifest.json` registration to the exact merged
   mono/skia base-branch SHA. Preserve the reviewed upstream ref and upstream
   merge SHA, and rerun the metadata checks. Do not repin earlier: M150 and
   M151 repeatedly demonstrated that an early repin goes stale.
6. Commit the final repin directly in the companion branch under the normal
   workflow. When branch ownership or review policy requires isolation, a tiny
   dependent PR targeting the sync branch is allowed, but it must merge before
   the parent PR and before final CI.
7. Push with a guarded lease and verify GitHub now shows only intended commits
   and files.

Never leave the parent pointing at the old PR-head commit.

## 8. Run the final parent gate

Wait for CI on the refreshed parent head. Re-download and validate packages if
the package-producing tree changed. Update both PR descriptions with durable
SHAs, merge decisions, build/test evidence, package results, and remaining
platform limitations.

Invoke `pr-commit-message` for the parent PR, mark it ready, and merge it using
the repository's normal strategy only after explicit approval.

## 9. Verify and report

Confirm:

- mono/skia's base contains the reviewed upstream merge;
- mono/SkiaSharp's base contains the final parent commit;
- the parent gitlink equals the merged mono/skia commit;
- both PRs are merged and no stale open sync PR remains;
- required CI on the actual mono/SkiaSharp base-branch merge or squash commit is
  green;
- optional branch deletion happens only after both merges.

Monitor post-merge CI against the actual parent base-branch commit, not the
pre-merge head. Do not report the landing complete until required post-merge
CI is green. If it fails, open and land a repair; do not substitute pre-merge
checks as evidence. M152 demonstrates why this gate is required.

Report:

```text
Upstream: <ref> @ <sha>
Previous line: <release/A.B.x native sha and parent sha>
mono/skia: <PR> -> <merged sha>
mono/SkiaSharp: <PR> -> <merged sha>
Review: <report and unresolved findings>
CI: <exact successful build>
Post-merge CI: <base commit and successful build>
Packages: <versions and validation>
Rotation: <stable, preview, next targets and decision>
```

## Stop conditions

Stop without merging when any of these is true:

- upstream has advanced beyond the reviewed SHA;
- the mono/skia merge is not a verified two-parent merge;
- the native PR is behind its base or its prospective merge tree differs from
  the reviewed PR-head tree;
- persisted review evidence is missing, schema-invalid, or stale;
- any demonstrated competing PR remains open or lacks a completed merge, close,
  or supersede disposition;
- required PR labels or the parent release milestone are wrong;
- a fork patch, dependency, API, ownership, or release-note finding is open;
- temporary compatibility work lacks restored support or an accepted limitation
  with a tracking issue;
- a crossed milestone, HarfBuzz bucket, or Component Governance registration is
  unaccounted for;
- a true bump has not preserved the previous milestone in matching
  `release/A.B.x` branches, the branch tips conflict, or the parent release
  gitlink does not equal the native release tip;
- the parent is behind its base or includes unrelated base commits;
- required CI is pending or failing;
- packages are missing, inconsistent, or cannot load;
- the supported-line rotation needs a maintainer decision;
- mono/skia merged but the parent has not been updated to its resulting commit;
- the merged mono/skia base tree differs from the reviewed PR-head tree;
- required post-parent-merge CI is pending or failing.
