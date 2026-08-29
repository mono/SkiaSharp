---
name: release-testing
description: >
  Smoke-test an exact public SkiaSharp release package set on the current host.
  Use when a maintainer asks to smoke test a NuGet.org version. This is optional
  human validation, not a required step in the automated Prepare/Finish release
  workflows.
---

# Release Smoke Testing

Use this skill for host/device validation that benefits from human inspection:
native loading, console use, Linux containers, Blazor rendering, Android, iOS,
Mac Catalyst, and Windows rendering.

The supported release path is documented in
[releasing.md](../../../documentation/dev/releasing.md). Branch creation,
package publication, tags, GitHub Releases, and milestones are owned by
workflows and deterministic scripts, not this skill.

## Contract

- Start from one exact public SkiaSharp version.
- The public-version planner verifies the complete NuGet.org receipt before
  producing test commands.
- Never substitute a newer version, branch, feed, runtime, image, simulator, or
  device.
- Present the host-appropriate matrix and obtain approval before preparation or
  execution.
- Execute every approved item even when an earlier item fails.
- Product assertions and rendering differences remain failures.
- Do not add skips, change expected output, or override package pins.
- Each runner owns its setup and cleanup. Never delete user-owned devices.
- Report screenshots and all initial/retry outcomes.
- A passing report informs the maintainer's team-pipeline decision; it does not
  mutate or unlock either GitHub release workflow.

## Plan

For a public package version:

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  {exact-public-skiasharp-version}
```

The planner:

1. locked-restores and builds the standalone C# release tool at the current
   checkout without downloading native Skia artifacts;
2. invokes its read-only public NuGet receipt verification using NuGet SDK APIs
   and `GH_TOKEN`/`GITHUB_TOKEN` for GitHub state;
3. derives the exact HarfBuzzSharp version;
4. pins both package versions in every runner command;
5. reports available and host-inapplicable coverage.

Render:

```markdown
## Release smoke-test plan

**Version:** `{release.publicPackages.SkiaSharp}`
**Commit:** `{release.commit}`
**Packages:** SkiaSharp `{version}`, HarfBuzzSharp `{version}`
**Source:** NuGet.org

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |
```

Use `ask_user` to approve the full available matrix, customize it, or cancel.

## Execute

Run preparation once:

```bash
python3 .agents/skills/release-testing/scripts/prepare-test-run.py
```

Then run each approved matrix item's emitted command sequentially. Record:

- item ID and exact command;
- initial result and duration;
- failure phase and diagnostics;
- environment repairs;
- retry result;
- expected screenshot paths and review status.

Use [setup.md](references/setup.md) for prerequisites,
[monitoring.md](references/monitoring.md) for progress, and
[troubleshooting.md](references/troubleshooting.md) only after a failure.

## Report

The final report must include the immutable public version/source commit, every
approved item, omitted host coverage, initial and retry outcomes, and screenshot
review. State plainly that smoke testing is advisory unless a separate protected
team-pipeline gate requires the result.
