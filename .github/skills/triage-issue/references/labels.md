# Label Taxonomy

SkiaSharp uses prefixed GitHub labels. AI triage values must match the exact label suffix so that `prefix/ + value` reconstructs the GitHub label.

**To get current valid values, always run the script first:**

```bash
bash .github/skills/triage-issue/scripts/get-labels.sh          # all groups
bash .github/skills/triage-issue/scripts/get-labels.sh type/     # type labels
bash .github/skills/triage-issue/scripts/get-labels.sh area/     # area labels
bash .github/skills/triage-issue/scripts/get-labels.sh backend/  # backend labels
bash .github/skills/triage-issue/scripts/get-labels.sh os/       # platform labels
```

The script output is the source of truth. The tables below provide guidance on when to use each label.

## Cardinality Rules

| Prefix | Cardinality | Rule |
|--------|-------------|------|
| `type/` | **Single** | One type per issue — pick the best fit |
| `area/` | **Single** | Primary component — "where in the codebase?" |
| `backend/` | **Multiple** | All rendering backends affected |
| `os/` | **Multiple** | All platforms affected |
| `tenet/` | **Multiple** | All quality tenets that apply |
| `partner/` | **Single** | One third-party partner flag |
| `status/` | **Multiple** | Can stack (informational, not set by AI) |
| `backport/` | N/A | Excluded from triage — tooling label |

## Usage Guidance

### `type/` — Issue Type

| Value | When to Use |
|-------|-------------|
| `bug` | Something is broken — crashes, wrong output, performance regressions |
| `feature-request` | New functionality that doesn't exist yet — new API, new platform |
| `enhancement` | Improvement to existing functionality — faster, easier, better errors |
| `question` | Support question, how-to, "can SkiaSharp do X?" |
| `documentation` | User needs docs, samples, or examples — confusion, not a defect |

### `area/` — Component Area

Pick the most specific match. If it's about SKCanvasView in MAUI, use `SkiaSharp.Views.Maui` not `SkiaSharp`. Use `libSkiaSharp.native` for DllNotFoundException / native loading issues.

### `backend/` — Rendering Backend

Only include if the issue is backend-specific. General SkiaSharp API issues don't need a backend.

**Note**: `backend/Android` is the Android rendering pipeline, distinct from `os/Android` which is the operating system.

### `os/` — Platform

Only include if the issue is platform-specific. Cross-platform API issues don't need a platform.

### `tenet/` — Quality Tenet

- `compatibility` — Compatibility with previous versions or cross-platform consistency
- `performance` — Speed, memory, throughput, file size
- `reliability` — Crashes, data corruption, unexpected behavior

### `partner/` — Partner Flag

Flag for a third-party partner team to look at. Only set when the issue clearly falls in their domain.

## Classification Tips

- If an issue has `[BUG]` in the title but is actually a question, classify as `question` — trust the content, not the prefix
- If ambiguous between `enhancement` and `feature-request`: enhancement improves what exists, feature-request adds something new
- If ambiguous between `question` and `documentation`: question asks "how?", documentation says "we need docs/samples for X"
