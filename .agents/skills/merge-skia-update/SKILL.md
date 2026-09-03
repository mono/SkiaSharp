---
name: merge-skia-update
description: >
  Guide a maintainer through manually landing an already reviewed Skia update.
  Use after review-skia-update has completed and the maintainer is happy with
  the paired mono/skia and mono/SkiaSharp PRs. Optionally preserves the previous
  release line, prepares the mono/skia merge message, pauses for the manual
  native merge, repins the existing parent PR to the actual merged commit, then
  prepares the SkiaSharp merge message. This skill never merges either PR.
compatibility: Requires git, gh, and PowerShell 7.4+ with access to mono/skia and mono/SkiaSharp.
---

# Merge Skia Update

Help with the mechanical work around two manual merges. Do not repeat the
review and do not merge either PR.

## 1. Resolve and confirm

Resolve the PR pair before asking for any landing confirmation.

If the maintainer supplied either PR, use its reciprocal link to find the
companion PR. If they supplied both PRs, require each reciprocal link to point
to the other; stop on any mismatch. Otherwise, search the open Skia update PRs
and pair them using their reciprocal links and shared sync branches.

- If exactly one pair is found, present it.
- If multiple pairs are found, present the candidates and ask the maintainer
  which pair to land.
- If no pair can be resolved, ask for either PR number and stop until it is
  provided.

For the selected pair, resolve and present:

- the mono/skia PR number, head branch, and base branch;
- the mono/SkiaSharp PR number, head branch, and base branch;
- whether the parent targets `main` or an existing `release/A.B.x` branch.

Only after the maintainer sees the exact selected pair, ask them to confirm:

- `review-skia-update` has run;
- they are happy with the review and approve the update;
- required CI was green;
- they want to begin the manual landing.

If they do not confirm, stop. Do not independently re-audit reviews, labels,
milestones, CI, packages, DEPS, bindings, or release notes.

## 2. Preserve the previous release line for milestone bumps

Run the script with the branch names already resolved by the AI:

```powershell
pwsh .agents/skills/merge-skia-update/scripts/Prepare-SkiaReleaseBranches.ps1 `
  -SkiaSharpBaseBranch <parent-base> `
  -SkiaSharpHeadBranch <parent-head> `
  -SkiaBaseBranch <native-base>
```

The script determines the release action from the parent base branch:

- `main`: compares the committed `chrome_milestone` on the parent base and head;
  when the milestone increases, it reads the base's committed SkiaSharp package
  version from `scripts/VERSIONS.txt` and derives `release/A.B.x` for the
  previous line;
- `main` with no milestone change: reports a same-milestone sync and exits
  without creating refs;
- `release/A.B.x`: reports a servicing sync and exits without creating refs.

The script always defaults to a dry run. For a milestone bump targeting `main`,
it reads the current SkiaSharp base tip and the exact mono/skia commit referenced
by its `externals/skia` gitlink. It requires the supplied mono/skia base branch
to point at that commit and preflights the derived release branch in both
repositories.

When the script reports a milestone bump, show the output and obtain
confirmation. Rerun with the same inputs plus `-Push`. The script checks the
source and destination refs again, creates the mono/skia release branch first,
then the mono/SkiaSharp release branch, and verifies both. It never moves an
existing branch. When it reports a same-milestone or servicing sync, continue
without a push confirmation.

## 3. Prepare the mono/skia merge

Invoke `pr-commit-message` for the resolved mono/skia PR and give its title and
body to the maintainer.

Tell the maintainer to merge mono/skia manually using **Create a merge commit**.
Squash and rebase must not be used because upstream merge ancestry is part of
the update history.

Stop. Continue only after the maintainer says mono/skia is merged.

## 4. Repin the existing SkiaSharp PR

Check whether the current checkout is clean, on the resolved SkiaSharp PR head
branch, and matches its remote tip. Never discard or overwrite local changes.

If the current checkout is suitable, use it directly:

```powershell
pwsh .agents/skills/merge-skia-update/scripts/Update-SkiaSharpSkiaCommit.ps1 `
  -SkiaSharpBranch <parent-head> `
  -SkiaBranch <native-base>
```

The default is a dry run. The script:

1. reads the reviewed mono/skia commit from the parent PR gitlink;
2. resolves the new tip of the supplied mono/skia base branch;
3. requires that tip to be a two-parent merge containing the reviewed commit;
4. requires the merged tree to equal the reviewed tree.

Show the output. If it is correct, rerun with `-Push`. The script updates only
`externals/skia` and the mono/skia `commitHash` in `cgmanifest.json`, commits the
change, and pushes the existing SkiaSharp PR branch without force.

If the current checkout is unsuitable, ask whether the maintainer wants to
check out the parent PR branch or avoid a local checkout. For the no-checkout
option, first verify the same two-parent ancestry and tree-equality conditions,
record the exact verified mono/skia merge SHA, then trigger the existing
workflow:

```shell
REPOSITORY=$(python3 scripts/infra/repository_identity.py get repository)
gh workflow run auto-skia-submodule-sync.yml --repo "$REPOSITORY" \
  -f target_branch=<parent-head> \
  -f skia_branch=<native-base>
```

The workflow opens or updates a small dependent PR targeting the existing
SkiaSharp PR branch. Before showing that PR to the maintainer, confirm it
changes only `externals/skia` and `cgmanifest.json` and that both files point to
the exact verified merge SHA. Recheck that SHA's ancestry and tree equality
against the reviewed native commit. Stop if the workflow captured any SHA
other than the recorded verified one. Wait for the maintainer to merge the
dependent PR manually, then verify the parent PR still points to that exact
SHA. Do not generate the parent merge message until the dependent PR is merged.

## 5. Prepare the mono/SkiaSharp merge

Invoke `pr-commit-message` for the updated mono/SkiaSharp PR and give its title
and body to the maintainer.

Tell the maintainer to merge it manually using the repository's normal merge
method. Do not wait for another PR CI run: the repin script proved that the
merged mono/skia tree is identical to the reviewed PR tree. CI on the resolved
parent base branch validates the resulting merge.

Stop. Continue only after the maintainer says mono/SkiaSharp is merged.

## 6. Post-merge check

Confirm:

- both PRs are merged;
- the resolved mono/SkiaSharp parent base branch points its gitlink and
  cgmanifest at the actual mono/skia merge commit;
- CI has started on that parent base branch for the resulting SkiaSharp commit.

Report both merged SHAs and the preserved release branch, if created. Do not
wait for the parent base branch CI to finish.

## Stop conditions

Stop when:

- the maintainer does not confirm the initial checklist;
- the PR pair or branch names cannot be resolved or the maintainer has not
  selected one of multiple candidate pairs;
- supplied PR numbers do not reciprocally link to each other;
- the parent base is neither `main` nor `release/A.B.x`;
- the current product line cannot be derived from `scripts/VERSIONS.txt`;
- a supplied native base branch does not match the parent base gitlink;
- an existing release branch points at a different SHA;
- a release source changes between dry run and `-Push`;
- the parent head milestone regresses;
- mono/skia was not merged with a two-parent merge commit;
- that merge does not contain the parent PR's reviewed native commit;
- the merged and reviewed native trees differ;
- local repin was selected but the current checkout is not clean, on the parent
  PR branch, and at its remote tip;
- the workflow repin changes unexpected files, captures a different native SHA,
  or its dependent PR is not merged;
- the repin would change anything except `externals/skia` and
  `cgmanifest.json`.
