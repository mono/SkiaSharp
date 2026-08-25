---
name: release-branch
description: >
  Create SkiaSharp release branches and start release CI. Use when the user says
  "release X", "start release X", "create release branch for X", "release now",
  or asks for the next preview on main or a maintenance line. This is the first
  release step: resolve an exact version, audit immutable branch inputs, obtain
  approval, and reconcile both SkiaSharp and mono/skia release branches.
---

# Release Branch

This skill is **Step 1 of 5**:

**release-branch** → [release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Use scripts for detection, validation, reconciliation, and writes.
- Treat remote release refs and the CI run triggered by the SkiaSharp push as
  irreversible. Never force-update or move an existing ref.
- Keep the current checkout unchanged and never commit directly to protected
  `main`/`skiasharp` branches.
- Audit the exact version first; execute only after the user approves every
  pending operation.
- Preserve the audited base SHA and Skia gitlink SHA during execution.
- The script may create matching `release/{version}` refs in both repositories.
- A regular stable release may create a protected-branch bump PR. Automation
  opens it; a maintainer reviews and merges it.
- This skill never merges PRs or publishes packages/releases.

## Release model

| Exact version | Base |
|---------------|------|
| `X.Y.Z-preview.N` / `-rc.N` | `main` before line creation, otherwise `release/X.Y.x` |
| `X.Y.Z` | `release/X.Y.x` |
| `X.Y.Z.F-preview.N` / `-rc.N` | Tag `vX.Y.Z` |
| `X.Y.Z.F` | Latest matching hotfix preview/RC branch |

Preview/RC iterations begin at 1. Every SkiaSharp release branch has an
identically named mono/skia branch at the exact pinned gitlink. Stable Build and
BAR package versions are exact `X.Y.Z`; `stable` is only the pipeline sentinel
that selects release versioning and is never emitted as a package suffix.

Historical lines use the same relationship: `release/4.150.x` and
`release/4.151.x` are integration/maintenance inputs that produce exact child
branches such as `release/4.150.4` and `release/4.151.3`. An existing exact
branch such as `release/4.152.0-rc.1` is not an integration branch and proceeds
to release-status after its minimal Arcade release backport lands.

Do not confuse future integration state with the immediate untagged release
snapshots: `release/4.150.3` and `release/4.151.2` are exact sibling branches,
not ancestors of their advanced `.x` lines, and can proceed directly to
release-status after their own backports.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/detect-release-version.py` | Read-only next-preview calculation from `main` or `release/X.Y.x`. |
| `scripts/create-release-branches.py` | Exact-version dry-run, validation, reconciliation, push, and stable bump PR. |

Operation statuses:

| Status | Response |
|--------|----------|
| `done` | Validated; no write needed. |
| `pending` | Include in approval and execution. |
| `awaiting-user` | Automation is complete; report the maintainer action. |

## Workflow

### 1. Resolve the exact version

Use a supplied exact version directly. When the user requests the next release,
choose `main` or an exact `release/X.Y.x` integration branch, asking only when
that line is ambiguous:

```bash
python3 .agents/skills/release-branch/scripts/detect-release-version.py \
  {integration-branch}
```

Pin the returned `releaseVersion`.

### 2. Audit

```bash
python3 .agents/skills/release-branch/scripts/create-release-branches.py \
  {exact-version} \
  --dry-run
```

Render:

```markdown
## Release branch audit

**Release:** `{version}` ({type})
**Base:** `{baseRef}` at `{baseSha}`
**Skia:** `{skiaSha}`
**Branches:** `mono/SkiaSharp:{releaseBranch}`,
`mono/skia:{releaseBranch}`

| Operation | Status | Detail |
|-----------|--------|--------|
| `{operations[].id}` | `{operations[].status}` | `{operations[].detail}` |
```

Include every warning and call out that a pending SkiaSharp push starts CI.

### 3. Approve and execute

If no operation is pending, report any maintainer action and continue to the
handoff. Otherwise obtain approval and run the emitted `executionCommand`, which
pins `baseSha` and `skiaSha`.

If execution fails, rerun the dry-run to reconcile partial state before retrying
the emitted command.

### 4. Hand off

Run the returned `statusCommand` with
[release-status](../release-status/SKILL.md). For stable releases, also report
the bump PR URL and its maintainer-owned merge state.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
