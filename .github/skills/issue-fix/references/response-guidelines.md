# Fix Response Guidelines

How to communicate fix findings on PRs and issues.

## PR Description Structure

Use this template for the PR body:

```
## Fix [short description]

Fixes #NNNN

### Problem
[1-2 paragraphs: what's broken, who's affected, what symptoms]

### Root Cause
[1-2 paragraphs: the precise technical cause, with code references]

### Fix
[Brief description of the change, with before/after code if helpful]

### Results
[For functional bugs: "Bug no longer reproduces"]
[For performance bugs: before/after table with metrics]

### Testing
- Build verified: [command]
- Tests pass: [command + result]
- Regression test added: [test name]
- Manual verification: [what you checked]

### What Changed
- `path/to/file.cs` — [one-line description of change]
```

## Issue Comment Structure (Summary)

Keep issue comments SHORT — the PR has the detail:

```
## [Fix/Investigation] Summary

**Root cause found and fixed in #NNNN.**

[1-2 sentence explanation of the root cause]

[Key results table if performance]

### Interesting findings
- [Bullet points of surprising discoveries]
- [Things that were NOT the cause]

Full investigation details on the PR: #NNNN.
```

## Performance Results Formatting

Always use tables for before/after comparison:

```markdown
| Metric | Before | After | Native C++ | Speedup |
|---|---|---|---|---|
| FPS (C12) | 5.3 | **77-93** | 61 | **15.6×** |
| render | 62ms | 6.9ms | 6.9ms | 9× |
| flush | 125ms | 1.8ms | — | 69× |
```

Include:
- Multiple complexity levels if available
- Native baseline for comparison
- Per-phase breakdown

## Where to Post What

| Content | Where | Why |
|---------|-------|-----|
| Code changes + description | PR body | Reviewers need this |
| Full investigation process | PR comment | Detailed, reviewable |
| Summary + key findings | Issue comment | Community visibility |
| Tools used + methodology | PR comment | Future reproducibility |

## Risk Assessment

| Risk | When |
|------|------|
| Low | Single file, no public API change, isolated fix |
| Medium | Multiple files, touches public surface, cross-platform |
| High | ABI impact, native code, behavior change for existing users |

## Tone
- Be factual and precise
- Include code references (file:line)
- Show your work (data, not assertions)
- Acknowledge what you tried that DIDN'T work
- Credit the reporter's investigation
