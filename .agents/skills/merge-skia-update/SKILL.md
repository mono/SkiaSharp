---
name: merge-skia-update
description: >
  Safely land a reviewed Skia milestone bump or servicing sync across the
  paired mono/skia and mono/SkiaSharp pull requests. Use whenever a maintainer
  asks to merge, land, finalize, or complete a Skia bump/sync, or asks what is
  still required before those PRs can merge. Verifies upstream freshness,
  two-parent ancestry, fork and dependency decisions, parent rebase state,
  release-note reachability, CI, packages, merge order, and the supported-line
  rotation. Use update-skia to create or refresh the update and
  review-skia-update for the deep review; this skill owns the final landing.
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

## 1. Resolve the pair

Identify and cross-check:

- the mono/skia PR, its `skiasharp` or release-line base, and head SHA;
- the mono/SkiaSharp companion PR, its parent base, and head SHA;
- the milestone, upstream `chrome/mN` ref and exact SHA;
- the shared `skia-sync/...` feature branch.

Read both PR bodies, commits, changed paths, reviews, inline comments, and
checks. The reciprocal PR links must identify this exact pair.

## 2. Recheck the review gates

Do not rely on a review captured before the latest push.

1. Compare the live Google Skia ref with the recorded
   `cgmanifest.json` `upstream_merge_commit`.
   - If upstream moved, stop and run `update-skia` to refresh the existing PR.
   - A milestone number match is not enough; compare exact ancestry.
2. Confirm mono/skia contains a genuine two-parent merge whose upstream parent
   is the target SHA and whose fork parent is the recorded base.
3. Run or inspect the final `review-skia-update` report. Resolve every finding:
   - added, removed, or changed fork patches;
   - DEPS URLs, SHAs, and enabled/commented states;
   - C API and generated binding changes;
   - restored legacy code or dangling references;
   - wrapper ownership, ABI, and test coverage.
4. Confirm release-note sidecars mention only behavior reachable through the
   current C and managed bindings.
5. Confirm the parent PR is based directly on the current target branch:
   - `behind_by == 0`;
   - its merge base is the current base tip;
   - GitHub lists only update commits and intended files, not commits already
     on the base branch.
6. Verify every version surface, including:
   - Skia milestone and upstream SHA;
   - native soname and `SK_C_INCREMENT`;
   - SkiaSharp assembly/file/package versions;
   - the HarfBuzzSharp milestone bucket when native HarfBuzz did not change.

## 3. Make the supported-line decision

Read `scripts/infra/docs/versions.json`. Its `support` block is hand-maintained
release policy and also drives the scheduled Skia-sync rotation.

Before landing a new milestone, report:

- the current stable and preview lines;
- the rotation produced by `stable + preview + highest-supported-plus-one`;
- whether the newly merged main milestone and intended next milestone will be
  monitored.

Do not infer support from Chrome channels or edit the file automatically. If
the rotation would omit a line the maintainer intends to service, stop for an
explicit support-list decision.

## 4. Validate the pre-merge heads

Both current heads must be mergeable and free of pending or failing required
checks. CI must cover the repository's native, managed, test, visual, package,
and sample stages.

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

## 5. Merge mono/skia first

1. Re-fetch both PRs and upstream immediately before merging.
2. Invoke `pr-commit-message` for the mono/skia PR using the final head,
   companion PR, compare range, review findings, and validation evidence.
3. Mark the PR ready if needed.
4. Merge with GitHub's **merge commit** strategy. Never squash or rebase it.
5. Fetch the resulting base-branch tip and record its exact SHA and tree.

The resulting `skiasharp` commit may differ from the PR head because GitHub can
create a merge commit. That resulting base-branch SHA is authoritative.

## 6. Refresh the SkiaSharp parent

1. Check out the companion feature branch.
2. Update `externals/skia` to the exact merged mono/skia base-branch commit.
3. Update the Skia git registration in `cgmanifest.json` to the same SHA.
   Preserve the reviewed upstream ref and upstream merge SHA.
4. Run `update-skia/scripts/update_versions.py` with the original current and
   target milestones, exact upstream SHA, parent-base SHA, and Skia-base SHA.
   The helper rewrites and validates final metadata, so inspect its diff; do
   not treat it as read-only and do not hand-edit generated bindings.
5. Rebase the parent feature branch onto the latest target-branch tip.
6. Push with a guarded lease and verify GitHub now shows only intended commits
   and files.

Never leave the parent pointing at the old PR-head commit.

## 7. Run the final parent gate

Wait for CI on the refreshed parent head. Re-download and validate packages if
the package-producing tree changed. Update both PR descriptions with durable
SHAs, merge decisions, build/test evidence, package results, and remaining
platform limitations.

Invoke `pr-commit-message` for the parent PR, mark it ready, and merge it using
the repository's normal strategy only after explicit approval.

## 8. Verify and report

Confirm:

- mono/skia's base contains the reviewed upstream merge;
- mono/SkiaSharp's base contains the final parent commit;
- the parent gitlink equals the merged mono/skia commit;
- both PRs are merged and no stale open sync PR remains;
- optional branch deletion happens only after both merges.

Report:

```text
Upstream: <ref> @ <sha>
mono/skia: <PR> -> <merged sha>
mono/SkiaSharp: <PR> -> <merged sha>
Review: <report and unresolved findings>
CI: <exact successful build>
Packages: <versions and validation>
Rotation: <stable, preview, next targets and decision>
```

## Stop conditions

Stop without merging when any of these is true:

- upstream has advanced beyond the reviewed SHA;
- the mono/skia merge is not a verified two-parent merge;
- a fork patch, dependency, API, ownership, or release-note finding is open;
- the parent is behind its base or includes unrelated base commits;
- required CI is pending or failing;
- packages are missing, inconsistent, or cannot load;
- the supported-line rotation needs a maintainer decision;
- mono/skia merged but the parent has not been updated to its resulting commit.
