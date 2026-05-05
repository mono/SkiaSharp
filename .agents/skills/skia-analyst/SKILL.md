---
name: skia-analyst
description: >
  Analyze Skia features for SkiaSharp - produces a unified analysis of what shipped (upstream
  engine benefits, PR links, migration guides) and what's missing (impact/priority/effort scoring,
  hidden APIs). Given any combination of git refs, milestones, or no input at all, it scans upstream
  Skia release notes, diffs bindings, and checks C++ headers to produce a unified report. Use
  whenever the user asks to "write release notes", "generate changelog", "what changed between
  versions", "what are we missing", "feature gap analysis", "what should we bind next", "what's
  new in Skia", "scout features", "diff two tags", "what shipped in this release", "summarize
  changes since v3.x", "prepare release announcement", or any request to analyze SkiaSharp versions
  or Skia features. Also use proactively when the user mentions a Skia milestone bump, finishes a
  release cycle, or asks what went into a specific version.
---

# Skia Analyst

You analyze Skia features for SkiaSharp from two angles simultaneously:
1. **What shipped** — upstream engine benefits, PR links, migration guides
2. **What's missing** — gap analysis with impact/priority/effort scoring, hidden API scan, action items

Every run produces both. Output is structured JSON and rendered GitHub-flavored Markdown.

This skill always runs in a SkiaSharp checkout. It uses:
- `externals/skia/` submodule for the C API (our fork at `mono/skia`)
- `binding/SkiaSharp/SkiaApi.generated.cs` for the C API reflected as P/Invoke externs
- `binding/SkiaSharp/*.cs` for the C# wrappers
- Upstream `google/skia` headers fetched via GitHub for hidden API comparison

## Key References

- **[references/schema-cheatsheet.md](references/schema-cheatsheet.md)** — Human-readable schema
- **[references/skia-analyst-schema.json](references/skia-analyst-schema.json)** — JSON Schema (Draft 2020-12)
- **[references/analysis-instructions.md](references/analysis-instructions.md)** — Classification criteria

## Input Flexibility

The user may specify anything — infer the scan mode:

| User says | Mode | What to do |
|-----------|------|------------|
| Nothing, "scan everything" | `full` | All milestones, full gap analysis |
| "what's new since m133" | `windowed` | milestoneFrom=133, milestoneTo=current |
| "between m133 and m147" | `windowed` | milestoneFrom=133, milestoneTo=147 |
| "diff v3.119.4..origin/main" | `diff` | Git diff + milestone analysis |
| "what changed in v4.147.0" | `diff` | Auto-detect previous tag, diff |

If unclear, ask. But try to infer first.

## Workflow

```
Phase 1: Setup (determine scan range, prepare sources)
Phase 2: Launch dual-model agents (Opus + GPT in parallel)
Phase 3: Synthesize — merge findings, dedupe, both lenses
Phase 4: Generate outputs (JSON → validate → Markdown)
Phase 5: Present results
```

### Phase 1: Setup

**1a. Determine scan range**

Based on user input, establish:
- `scanMode`: full, windowed, or diff
- For windowed: `milestoneFrom` and `milestoneTo`
- For diff: `refFrom` and `refTo` (resolve SHAs, dates, commit count)

**1b. Determine current milestone**

```bash
cat externals/skia/include/core/SkMilestone.h
```

Or check commit messages for the latest Skia bump PR.

**1c. Fetch Skia release notes**

Fetch `RELEASE_NOTES.md` from `google/skia` using the GitHub MCP tool or `gh api`:

```bash
gh api repos/google/skia/contents/RELEASE_NOTES.md -H "Accept: application/vnd.github.raw" > skia-release-notes.md
```

Save to a file in the working directory for agents to reference.

**1d. Prepare upstream headers for hidden API scan**

Agents will fetch upstream C++ headers directly from GitHub during their scan:
```
github-mcp-server-get_file_contents owner=google repo=skia path=include/core/SkImage.h
```

**1e. Locate binding sources**

The C API is reflected in `binding/SkiaSharp/SkiaApi.generated.cs` as P/Invoke extern methods.
The C# wrappers are in `binding/SkiaSharp/*.cs`. Both are in the worktree.

For the C API headers (our fork), check `externals/skia/include/c/` and `externals/skia/src/c/`.
If the submodule isn't checked out, agents can grep `SkiaApi.generated.cs` for `sk_*` and `gr_*`
extern function names — this reflects the full C API surface.

### Phase 2: Launch Dual-Model Agents

Launch **two** background agents simultaneously on **different models**:

```
task agent_type=general-purpose mode=background model=claude-opus-4.7 name=analyst-opus:
task agent_type=general-purpose mode=background model=gpt-5.4 name=analyst-gpt:
```

Each agent does the complete job independently:
1. **Release notes scan** — read Skia RELEASE_NOTES.md, extract features
2. **Hidden API scan** — fetch upstream C++ headers from `google/skia`, compare against
   binding/SkiaSharp/SkiaApi.generated.cs for P/Invoke externs and binding/SkiaSharp/*.cs for wrappers
3. **Binding verification** — grep the actual code to set bindingStatus
4. **Git diff** (if diff mode) — analyze API/build/dep changes between refs

For EVERY finding, classify with BOTH lenses:
- Changelog: `changeType` + `importance`
- Gap: `bindingStatus` + `impact` + `priority` + `effort`

### Phase 3: Synthesize

When both agents complete, merge their findings:

1. **Deduplicate** by name/skiaApi — keep richer data from each
2. **Resolve conflicts** — more cautious bindingStatus wins, higher impact wins
3. **Union hidden APIs** — combine both agents' header scan discoveries

### Phase 4: Generate Outputs

**4a. Generate JSON**

Save to `skia-analyst-report.json` in the working directory.

**4b. Validate**

```bash
python3 .agents/skills/skia-analyst/scripts/validate-skia-analyst.py skia-analyst-report.json
```

**4c. Render Markdown**

```bash
python3 .agents/skills/skia-analyst/scripts/render-skia-analyst.py skia-analyst-report.json skia-analyst-report.md
```

This produces a GitHub-flavored Markdown file with collapsible details, suitable for pasting
into a GitHub issue or sharing as a gist.

### Phase 5: Present Results

Show highlights inline:

- **Top gaps** — transformative and significant items
- **Quick wins** — partial binding status (C API exists, just needs C# wrapper)
- **Breaking changes** — if any findings have `importance: breaking`

Then offer next steps:
1. "Want me to investigate any finding in more detail?"
2. "Should I use the api-add-review skill to start binding a feature?"
3. "Want to upload the markdown to a gist for sharing?"
4. "Should I create a GitHub issue with the gap analysis?"
