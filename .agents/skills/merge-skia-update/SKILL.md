---
name: merge-skia-update
description: >
  Guide a maintainer through manually landing an already reviewed Skia update.
  Use after review-skia-update has completed and the maintainer is happy with
  the paired mono/skia and mono/SkiaSharp PRs. Optionally preserves the previous
  release line, prepares the mono/skia merge message, pauses for the manual
  native merge, repins the existing parent PR to the actual merged commit, then
  prepares the SkiaSharp merge message. This skill never merges either PR.
compatibility: Requires git, gh, and PowerShell 7 with access to mono/skia and mono/SkiaSharp.
---

# Merge Skia Update

Help with the mechanical work around two manual merges. Do not repeat the
review and do not merge either PR.

## 1. Confirm and resolve

Ask the maintainer to confirm:

- `review-skia-update` has run;
- they are happy with the review and approve the update;
- required CI was green;
- they want to begin the manual landing.

If they do not confirm, stop. Do not independently re-audit reviews, labels,
milestones, CI, packages, DEPS, bindings, or release notes.

Use GitHub and the PR bodies to resolve:

- the mono/skia PR number, head branch, and base branch;
- the mono/SkiaSharp PR number, head branch, and base branch;
- whether the parent targets `main` or an existing `release/A.B.x` branch.

Present these values before continuing.

## 2. Preserve the previous release line when targeting main

Run the script with the branch names already resolved by the AI:

```powershell
pwsh .agents/skills/merge-skia-update/scripts/Prepare-SkiaReleaseBranches.ps1 `
  -SkiaSharpBaseBranch <parent-base> `
  -SkiaBaseBranch <native-base>
```

The script determines the release action from the parent base branch:

- `main`: reads the committed SkiaSharp package version from
  `scripts/VERSIONS.txt` and derives `release/A.B.x` for the current line;
- `release/A.B.x`: reports a servicing sync and exits without creating refs.

The script always defaults to a dry run. When targeting `main`, it reads the
current SkiaSharp base tip and the exact mono/skia commit referenced by its
`externals/skia` gitlink. It requires the supplied mono/skia base branch to
point at that commit and preflights the derived release branch in both
repositories.

Show the output and obtain confirmation. Rerun with the same inputs plus
`-Push`. The script checks the source and destination refs again, creates the
mono/skia release branch first, then the mono/SkiaSharp release branch, and
verifies both. It never moves an existing branch.

## 3. Prepare the mono/skia merge

Invoke `pr-commit-message` for the resolved mono/skia PR and give its title and
body to the maintainer.

Tell the maintainer to merge mono/skia manually using **Create a merge commit**.
Squash and rebase must not be used because upstream merge ancestry is part of
the update history.

Stop. Continue only after the maintainer says mono/skia is merged.

## 4. Repin the existing SkiaSharp PR

Use a clean worktree checked out to the resolved SkiaSharp PR head branch:

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

## 5. Prepare the mono/SkiaSharp merge

Invoke `pr-commit-message` for the updated mono/SkiaSharp PR and give its title
and body to the maintainer.

Tell the maintainer to merge it manually using the repository's normal merge
method. Do not wait for another PR CI run: the repin script proved that the
merged mono/skia tree is identical to the reviewed PR tree. Main CI validates
the resulting merge.

Stop. Continue only after the maintainer says mono/SkiaSharp is merged.

## 6. Post-merge check

Confirm:

- both PRs are merged;
- mono/SkiaSharp main points its gitlink and cgmanifest at the actual mono/skia
  merge commit;
- main CI has started for the resulting SkiaSharp commit.

Report both merged SHAs and the preserved release branch, if created. Do not
wait for main CI to finish.

## Stop conditions

Stop when:

- the maintainer does not confirm the initial checklist;
- the PR pair or branch names cannot be resolved;
- the parent base is neither `main` nor `release/A.B.x`;
- the current product line cannot be derived from `scripts/VERSIONS.txt`;
- a supplied native base branch does not match the parent base gitlink;
- an existing release branch points at a different SHA;
- a release source changes between dry run and `-Push`;
- mono/skia was not merged with a two-parent merge commit;
- that merge does not contain the parent PR's reviewed native commit;
- the merged and reviewed native trees differ;
- the current worktree is not the clean, current SkiaSharp PR branch;
- the repin would change anything except `externals/skia` and
  `cgmanifest.json`.
