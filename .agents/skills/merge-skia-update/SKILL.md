---
name: merge-skia-update
description: >
  Run the post-approval merge checklist for a reviewed Skia milestone bump or
  servicing sync across mono/skia and mono/SkiaSharp. Use only after a
  maintainer has accepted a current review for the exact PR heads and explicitly
  asks to merge, land, finalize, or complete the pair. Preserves the previous
  release line, merges mono/skia first with a merge commit, repins the parent to
  the actual merged SHA, requires refreshed approval after head-changing work,
  validates exact-head CI and packages, and monitors post-merge CI. Use
  review-skia-update for review or readiness assessment and update-skia to
  create or refresh the update.
compatibility: Requires git, gh, Python 3, and access to mono/skia and mono/SkiaSharp.
---

# Merge Skia Update

Execute the landing checklist for an already reviewed and approved Skia update.
This skill is the merger, not the reviewer.

```
approved exact heads
  -> preserve the previous milestone as release/A.B.x
  -> merge mono/skia with a merge commit
  -> repin SkiaSharp to the actual merged SHA
  -> refresh review and approval for the final parent head
  -> merge mono/SkiaSharp
  -> verify post-merge CI
```

This skill covers `chrome/mN` milestone bumps and same-milestone servicing
syncs. Use `update-skia` for a bleeding-edge Google Skia `main` sync.

## Scope boundary

- Start only when the maintainer says the reviewed pair is approved and asks to
  land it.
- For "is this ready?", "review this", or an unapproved pair, stop and invoke
  `review-skia-update`. Return here after the maintainer accepts that review.
- Do not repeat the deep audit of fork patches, DEPS, Component Governance,
  generated bindings, managed APIs, release notes, compatibility workarounds,
  or version semantics. Those belong to the review.
- Do verify that the accepted review covers those areas, has no unresolved
  blockers, and identifies the exact live PR heads.
- Approval is SHA-bound. Any push, rebase, repin, metadata/product edit, or
  upstream movement that changes reviewed evidence invalidates approval.
  Re-run the reviewer and obtain explicit maintainer approval before continuing.

## Safety contract

- Never merge without explicit maintainer approval for the exact heads.
- Never merge around a failed checklist item. Report the blocker and stop.
- Never merge mono/SkiaSharp before mono/skia.
- Never squash or rebase the mono/skia PR; use GitHub's merge-commit strategy.
- Use `--force-with-lease` for a reviewed feature-branch rebase; never use an
  unguarded force push.
- Never replace a native source build with `externals-download`.
- Do not weaken full cross-platform CI for a servicing sync.
- A draft is not a blocker. Mark a PR ready only immediately before its approved
  merge.
- Delete branches only after both PRs merge.

## 1. Accept the approved handoff

Record:

- the mono/skia PR, base, approved head SHA, and target upstream ref/SHA;
- the mono/SkiaSharp PR, base, and approved head SHA;
- the reciprocal PR links and shared `skia-sync/...` branch;
- the persisted, schema-valid `review-skia-update` report or explicit
  independent human review evidence accepted by the maintainer;
- the maintainer's explicit approval for those exact heads;
- the accepted disposition of review findings, compatibility limitations and
  tracking issues, support rotation, and any competing work.

PR-body self-attestation is not independent review evidence. If the handoff is
missing, stale, schema-invalid, or has unresolved findings, stop and return to
`review-skia-update`.

The accepted review must account for:

- upstream integrity and genuine two-parent ancestry;
- fork patches, DEPS, generated interop, managed API/ownership, and tests;
- version surfaces, skipped milestones, and HarfBuzz bucket progression;
- enabled tracked Component Governance registrations and their nested
  `skia_dependency.revision`, `version_reviewed_identity`, and
  `version_source`;
- temporary feature/backend disables, source exclusions, GN flags, platform
  workarounds, and backports;
- reachable release notes;
- the explicit `scripts/infra/docs/versions.json` support-rotation decision.

Do not recreate these audits in this skill; require their accepted result.

## 2. Revalidate volatile merge facts

Immediately before any write:

1. Re-fetch both PRs, both base branches, and the target upstream ref.
2. Require both live heads to equal the approved SHAs.
3. Require the upstream ref to equal the approved upstream SHA.
4. Verify both PRs still have the accepted labels and milestone:
   - both: `type/milestone-sync` and `partner/agentic-workflows`;
   - true bump: also `type/milestone-bump`, with the parent assigned the release
     milestone;
   - servicing sync: no `type/milestone-bump`.
5. Search for work opened since the review. Block only demonstrated competition:
   - the same sync line, head, or intended base;
   - a duplicate gitlink or cgmanifest repin;
   - overlapping DEPS dependency/revision/hunks that invalidate one PR.
   A shared filename alone is non-blocking. Every demonstrated competitor must
   already be merged, closed, or explicitly superseded.
6. Require both heads to be mergeable and current with their bases.
7. Require exact-head required CI to be green. Native CI must cover Windows,
   Linux, Apple, Android, Tizen, and WASM.
8. Require package validation from the exact successful parent build: expected
   IDs and versions, dependency ranges, archive integrity, representative
   native assets/symbols, restore, and runtime loading.

If any live fact differs from the approved handoff, stop. Run the appropriate
refresh/review workflow and obtain new approval rather than interpreting the
change during landing.

## 3. Preserve the previous milestone

For a true milestone bump, preserve the replaced line before either merge. A
same-milestone servicing sync does not create another release line.

1. Derive `release/A.B.x` from the previous SkiaSharp product line.
2. Record the current mono/skia and mono/SkiaSharp base tips.
3. Require both tips to identify the previous milestone and the parent base
   gitlink to equal the native base tip.
4. Preflight both destination refs before any write. Each must be absent or
   already equal its intended source SHA; never move an existing branch.
5. Present both refs and exact source SHAs and obtain confirmation.
6. Re-fetch both source tips after confirmation. If either changed, restart the
   preflight and confirmation.
7. Create the mono/skia branch first, then the matching mono/SkiaSharp branch,
   with guarded non-force ref creation.
8. Verify both branch SHAs and that the parent release gitlink equals the native
   release tip.

## 4. Merge mono/skia

1. Re-fetch both PRs, bases, and upstream.
2. For a true bump, require both base tips still to equal the recorded release
   branch SHAs.
3. Require the native head and upstream SHA still to equal the approved values.
4. Require the native merge base to equal the current base tip and `behind_by`
   to be zero.
5. Confirm the approved head contains the genuine two-parent upstream merge.
6. Compute the prospective GitHub merge tree and require it to equal the
   approved PR-head tree.
7. Invoke `pr-commit-message`, mark the PR ready if necessary, and merge with
   GitHub's **merge commit** strategy.
8. Fetch the resulting base commit. Require its tree to equal the approved head
   tree and its history to contain the authoritative two-parent upstream merge.

Record the GitHub-created base SHA. It is authoritative even when it differs
from the PR head.

## 5. Refresh and reapprove the parent

1. Rebase the parent feature branch onto the latest parent base.
2. Apply only the support-rotation decision already accepted during review.
3. As the last product-affecting parent operation before final CI, check out
   `externals/skia` at the actual merged mono/skia base SHA, then run
   `update-skia/scripts/update_versions.py` with the approved milestone and
   upstream inputs plus the recorded pre-update parent and native base SHAs.
   The helper reads the checked-out submodule HEAD when writing the cgmanifest
   git registration, so never run it against the old PR-head checkout and repin
   later.
4. Verify the resulting diff and rerun the metadata checks. Require both the
   gitlink and cgmanifest registration to equal the actual merged SHA while the
   reviewed upstream ref and merge SHA remain unchanged.
5. Commit and push with a guarded lease. A tiny dependent PR targeting the sync
   branch is allowed only when branch policy requires it, and must merge before
   final CI.
6. Verify GitHub lists only the intended commits and files.
7. Run `review-skia-update` against the refreshed exact pair, including the
   final parent head. Obtain explicit maintainer approval for that final head.

Do not merge the parent using the original approval: the required rebase and
actual-SHA repin changed its head.

## 6. Merge mono/SkiaSharp

After refreshed approval:

1. Re-fetch the parent PR and base; require the live head to equal the newly
   approved SHA, `behind_by == 0`, and the merge base to equal the base tip.
2. Require the gitlink and cgmanifest registration to equal the actual merged
   mono/skia base SHA.
3. Require exact-head CI to be green. If the package-producing tree changed,
   validate packages again from this build.
4. Invoke `pr-commit-message`, mark the PR ready if needed, and merge using the
   repository's normal strategy only after explicit approval.

## 7. Verify completion

Confirm:

- mono/skia's base contains the approved upstream merge and reviewed tree;
- mono/SkiaSharp's base contains the final approved parent commit;
- the merged parent gitlink equals the merged mono/skia SHA;
- both PRs are merged and no demonstrated competing sync PR remains open;
- required CI on the actual parent base-branch merge/squash commit is green;
- branch cleanup occurs only now, after both merges.

Do not report completion from pre-merge CI. If post-merge CI fails, open and
land a repair.

Report:

```text
Approved review: <report/evidence and exact approved SHAs>
Upstream: <ref> @ <sha>
Previous line: <release/A.B.x native sha and parent sha>
mono/skia: <PR> -> <merged sha>
Final parent approval: <evidence and exact head SHA>
mono/SkiaSharp: <PR> -> <merged sha>
CI: <exact successful head builds>
Post-merge CI: <base commit and successful build>
Packages: <versions and validation>
Rotation: <accepted decision>
```

## Stop conditions

Stop without the next merge when:

- the maintainer has not explicitly approved the exact live head;
- review evidence is missing, invalid, stale, or has unresolved findings;
- upstream, a PR head, or reviewed evidence changed;
- a demonstrated competitor remains open;
- labels, milestone, mergeability, base currency, CI, or packages fail;
- a true bump has not preserved the previous line in matching branches;
- the native prospective or resulting merge tree differs from the approved
  tree, or two-parent ancestry is missing;
- the parent does not point to the actual merged mono/skia SHA;
- refreshed review and approval for the final parent head are missing;
- required post-parent-merge CI is pending or failing.
