# Milestones

This document defines SkiaSharp's milestone naming convention and how milestones are automatically assigned to PRs.

## Naming Convention

**Rule: the milestone name IS the version string, derivable from the branch name.**

| Release type | Milestone name | Branch name | NuGet version |
|---|---|---|---|
| Preview | `4.148.0-preview.1` | `release/4.148.0-preview.1` | `4.148.0-preview.1.{build}` |
| RC | `4.148.0-rc.1` | `release/4.148.0-rc.1` | `4.148.0-rc.1.{build}` |
| Stable | `4.148.0` | `release/4.148.0` | `4.148.0-stable.{build}` |
| Servicing | `3.119.x` | `release/3.119.x` | varies |
| Backlog | `Backlog` | — | — |

### Rules

1. **No `v` prefix** — milestones are `4.148.0`, not `v4.148.0`
2. **Hyphens and dots only** — no spaces, no "Preview 1", no "RC 1", no "GA", no "Stable"
3. **Exact version** — `4.148.0-preview.1` not `4.148.x Preview 1`
4. **Servicing uses `.x`** — `3.119.x` represents a rolling servicing stream
5. **Derivable from branch** — strip `release/` from the branch name → milestone name

### Why?

- **Predictable** — anyone can look at a branch and know the milestone
- **Automatable** — the auto-milestone workflow needs zero configuration
- **Consistent** — matches NuGet package versions and git tags
- **Grep-friendly** — `gh issue list --milestone 4.148.0-preview.1` just works

## Auto-assignment

When a PR is merged, the milestone is automatically assigned by the [`auto-milestone.yml`](../../.github/workflows/auto-milestone.yml) workflow:

| PR target branch | Milestone assigned | How |
|---|---|---|
| `main` | Lowest open milestone for current version | Reads version from `scripts/VERSIONS.txt` (the `SkiaSharp nuget` line), finds lowest open milestone ≥ that version |
| `release/X.Y.Z-label.N` | `X.Y.Z-label.N` | Strip `release/` prefix |
| `release/X.Y.x` | `X.Y.x` | Strip `release/` prefix |

No configuration file is needed — release branches are self-describing, and `main` derives its milestone from the repo's version metadata and open milestones.

The logic is in [`.github/workflows/auto-milestone/auto-milestone.js`](../../.github/workflows/auto-milestone/auto-milestone.js) and can be tested locally:

```bash
# Run unit tests
node .github/workflows/auto-milestone/auto-milestone.test.js

# Dry-run against a real PR (uses gh CLI — coming soon)
# node .github/workflows/auto-milestone/auto-milestone.js --dry-run --pr 4046
```

### Examples

| I merged a PR to... | It gets milestone... |
|---|---|
| `main` | `4.148.0-preview.1` (current lowest open) |
| `release/3.119.x` | `3.119.x` |
| `release/3.119.4` | `3.119.4` |
| `release/3.119.4-preview.1` | `3.119.4-preview.1` |
| `release/2.88.x` | `2.88.x` |

## Lifecycle

1. **Created** — when the `release-branch` skill creates a release branch, it also creates the milestone
2. **Assigned** — automatically on PR merge by `auto-milestone.yml`
3. **Closed** — when the `release-publish` skill publishes to NuGet and creates the GitHub release

## Special Milestones

| Milestone | Purpose |
|---|---|
| `Backlog` | Issues/PRs not targeting any specific release |
| `X.Y.x` (e.g., `3.119.x`) | Servicing stream — collects all patches for a major.minor series |
