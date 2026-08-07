---
name: release-branch
description: >
  Create SkiaSharp release branches and start release CI. Use when the user says
  "release X", "start release X", "create release branch for X", "release now",
  or asks for the next preview on main or a maintenance line. This is the first
  step of releasing: detect an exact version when necessary, audit the complete
  operation, obtain confirmation, then reconcile and push the release branches.
---

# Release Branch

This skill is **Step 1 of 5**:

**release-branch** → [release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

Use the scripts for all detection, validation, reconciliation, and writes. The
agent's role is to choose the correct input, turn script JSON into a concise
human summary, obtain confirmation, and run the exact approved command.

## Safety and ownership

Read this before running any command:

- Treat remote release branches and the CI run they trigger as irreversible.
- Never force-update or move an existing release ref.
- Never commit directly to protected branches:
  - `mono/SkiaSharp`: `main`
  - `mono/skia`: `main`, `skiasharp`
- The script may create and push `release/*` branches in both repositories.
- Pushing the **SkiaSharp** release branch starts the release CI pipeline.
- For a regular stable release, the script may create and push a
  `bump-version-*` branch and open a PR to the maintenance branch.
- The script **never merges or auto-merges a PR**. A maintainer reviews and
  merges protected-branch PRs.
- Ask for user confirmation after the exact-version audit and before execution.
  The scripts do not prompt interactively.

## What the scripts guarantee

### Read-only detector

`detect-release-version.py` accepts only `main` or `release/X.Y.x`. It refreshes
remote-tracking refs and calculates an exact next-preview version. It cannot
checkout, initialize submodules, commit, or push.

### Exact-version release script

`create-release-branches.py` accepts only an exact release version. It rejects
`main`, `origin/main`, and `release/X.Y.x`.

With `--dry-run`, it audits current state and returns JSON without changing the
worktree, local branches, commits, submodules, or remotes. The audit is
repeatable and reports each operation independently:

| Status | Meaning |
|--------|---------|
| `done` | The operation already exists and was validated. |
| `pending` | Execution still needs to perform the operation. |
| `awaiting-user` | Automation is complete; a maintainer action remains, such as merging a PR. |

The audit treats an existing remote SkiaSharp release branch as authoritative.
This allows it to inspect an in-progress or completed release even after the
integration branch has advanced. It validates:

- Exact SkiaSharp and HarfBuzzSharp file/NuGet versions.
- `SKIASHARP_VERSION` and `PREVIEW_LABEL`.
- The pinned `externals/skia` gitlink.
- Matching SkiaSharp and mono/skia remote release refs.
- Any prepared local version commit or uncommitted expected version changes.
- Stable post-cut bump branch and PR state.

Execution uses the immutable base and Skia SHAs from the approved audit. It
performs only `pending` operations and safely skips validated `done` operations.
This makes retries safe after a partial push or interruption.

## Release model

Each release line has an integration branch:

| Integration branch | Purpose |
|--------------------|---------|
| `main` | Newest in-development line before it is forked. |
| `release/X.Y.x` | Established or maintenance line. |

Integration branches normally contain the next unreleased version with
`PREVIEW_LABEL: preview.0`.

| Exact version | Base | Resulting label |
|---------------|------|-----------------|
| `X.Y.Z-preview.N` | `release/X.Y.x`, or `main` before the line is forked | `preview.N` |
| `X.Y.Z-rc.N` | `release/X.Y.x`, or `main` before the line is forked | `rc.N` |
| `X.Y.Z` | `release/X.Y.x` | `stable` |
| `X.Y.Z.F-preview.N` / `-rc.N` | Tag `vX.Y.Z` | `preview.N` / `rc.N` |
| `X.Y.Z.F` | Latest matching hotfix preview/RC branch | `stable` |

Important rules:

- A regular stable is cut from `release/X.Y.x`, not from its latest preview.
- Preview/RC iteration numbers start at 1; `.0` is not a release.
- Every SkiaSharp `release/{version}` branch has an identically named
  mono/skia branch at the exact pinned gitlink.
- Stable public packages use the bare base version. CI stable packages use
  `{base}-stable.{build}` until publication.
- Immediately after a regular stable cut, the maintenance line advances to the
  next patch through a PR opened by the release script.

## Presenting an audit to the user

Script stdout is JSON for reliable agent parsing. Never dump the raw JSON into
the conversation. Render a concise status summary:

```markdown
## Release status

**Release:** `{version}` ({type})
**Cut from:** `{baseRef}` at `{baseSha}`
**Branches:** `mono/SkiaSharp:{releaseBranch}` and `mono/skia:{releaseBranch}`
**Pinned Skia commit:** `{skiaSha}`

### Completed
- Summarize each `operations[]` entry with status `done`.

### Pending
- Summarize each entry with status `pending`.

### Awaiting maintainer
- Link entries with status `awaiting-user`, such as the stable bump PR.

### Warnings
- Include every `warnings[]` entry verbatim.
```

Omit empty sections, but never omit warnings. Mention package versions only
when their update is pending. Clearly call out a pending SkiaSharp push because
it starts CI.

If all operations are `done` or `awaiting-user`, do not ask to run execution.
Report the remaining maintainer action or hand off to release-status.

## Runbook

### 1. Resolve an exact version

If the user supplied an exact version, use it directly.

If the user asked for the next release without a version, determine the
integration line. Ask whether they mean `main` or a specific `release/X.Y.x`
when ambiguous, then run:

```bash
python3 .agents/skills/release-branch/scripts/detect-release-version.py {integration-branch}
```

Read `releaseVersion` from the JSON and use that exact value for every remaining
command.

### 2. Audit the exact release

```bash
python3 .agents/skills/release-branch/scripts/create-release-branches.py \
  {exact-version} \
  --dry-run
```

Render the JSON using the user summary above.

### 3. Decide whether execution is needed

- If every operation is `done` or `awaiting-user`, report the current state and
  skip execution.
- If any operation is `pending`, ask the user to confirm the summarized pending
  operations.

### 4. Execute after confirmation

Run the exact `executionCommand` emitted by the approved audit. It includes the
approved immutable `baseSha` and `skiaSha`:

```bash
python3 .agents/skills/release-branch/scripts/create-release-branches.py \
  {exact-version} \
  --expect-base-sha {baseSha} \
  --expect-skia-sha {skiaSha}
```

If execution fails, show the failed operation. Run the dry-run again to
reconcile state before retrying; do not manually force or move refs.

### 5. Hand off

After execution, use `statusCommand` from the JSON result to start
[release-status](../release-status/SKILL.md). For a stable release, also report
the post-stable PR URL and state that it awaits maintainer review and merge.

## Files

- [scripts/detect-release-version.py](scripts/detect-release-version.py) —
  read-only next-preview detection.
- [scripts/create-release-branches.py](scripts/create-release-branches.py) —
  exact-version audit and reconciliation.
- [scripts/tests/test_release_scripts.py](scripts/tests/test_release_scripts.py)
  — scenario and recovery tests.
- [releasing.md](../../../documentation/dev/releasing.md) — complete release
  process reference.
