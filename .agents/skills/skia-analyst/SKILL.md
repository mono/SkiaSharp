---
name: skia-analyst
description: >
  Analyze Skia features for SkiaSharp - produces both a changelog (what shipped, marketing slides,
  migration guides) and a gap analysis (what's missing, impact/priority/effort scoring, hidden APIs).
  Given any combination of git refs, milestones, or no input at all, it scans upstream Skia release
  notes, diffs bindings, and checks C++ headers to produce a unified report. Use whenever the user
  asks to "write release notes", "generate changelog", "what changed between versions", "what are
  we missing", "feature gap analysis", "what should we bind next", "what's new in Skia", "scout
  features", "diff two tags", "what shipped in this release", "summarize changes since v3.x",
  "prepare release announcement", or any request to analyze SkiaSharp versions or Skia features.
  Also use proactively when the user mentions a Skia milestone bump, finishes a release cycle, or
  asks what went into a specific version.
---

# Skia Analyst

You analyze Skia features for SkiaSharp from two angles simultaneously:
1. **What shipped** - changelog, marketing slides, PR links, upstream engine benefits, migration guides
2. **What's missing** - gap analysis with impact/priority/effort scoring, hidden API scan, action items

Every run produces both. No mode selection needed.

## Key References

- **[references/schema-cheatsheet.md](references/schema-cheatsheet.md)** - Human-readable schema
- **[references/skia-analyst-schema.json](references/skia-analyst-schema.json)** - JSON Schema (Draft 2020-12)
- **[references/analysis-instructions.md](references/analysis-instructions.md)** - Classification criteria for both lenses

## Input Flexibility

The user may specify anything - infer the scan mode:

| User says | Mode | What to do |
|-----------|------|------------|
| Nothing, "scan everything" | `full` | All milestones, full gap analysis |
| "what's new since m133" | `windowed` | milestoneFrom=133, milestoneTo=current |
| "between m133 and m147" | `windowed` | milestoneFrom=133, milestoneTo=147 |
| "diff v3.119.4..origin/main" | `diff` | Git diff + milestone analysis |
| "what changed in v4.147.0" | `diff` | Auto-detect previous tag, diff |
| "things before m120" | `windowed` | milestoneTo=120 |

If unclear, ask. But try to infer first.

## Workflow

```
Phase 1: Setup (determine scan range, locate C API)
Phase 2: Launch dual-model agents (Opus + GPT in parallel)
Phase 3: Synthesize - merge findings, dedupe, both lenses
Phase 4: Generate outputs (JSON -> validate -> HTML)
Phase 5: Present results
```

### Phase 1: Setup

**1a. Determine scan range**

Based on user input, establish:
- `scanMode`: full, windowed, or diff
- For windowed: `milestoneFrom` and `milestoneTo`
- For diff: `refFrom` and `refTo` (resolve SHAs, dates, commit count)
- `currentMilestone`: from `externals/skia/include/core/SkMilestone.h` or commit messages

**1b. Fetch Skia release notes**

```
https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
```

Fetch in 20KB chunks. Save to a temp file for agents.

**1c. Locate C API**

```bash
ls externals/skia/include/c/ 2>/dev/null
# Fallback to main repo checkout
```

### Phase 2: Launch Dual-Model Agents

Launch **two** background agents simultaneously on **different models**. Each does the complete job independently. Their blind spots don't overlap.

Give each agent this prompt (adapted with actual scan range):

```
task agent_type=general-purpose mode=background model=claude-opus-4.7 name=analyst-opus:
task agent_type=general-purpose mode=background model=gpt-5.4 name=analyst-gpt:

  "You are analyzing Skia for SkiaSharp. Current milestone: M{current}.
   Scan mode: {mode}. Range: {range details}.

   FIRST: Read .agents/skills/skia-analyst/references/analysis-instructions.md

   THEN do ALL of these:
   1. RELEASE NOTES SCAN - Read the Skia release notes. Extract features for the relevant milestone range.
   2. HIDDEN API SCAN - Fetch upstream C++ headers, compare against C API.
   3. BINDING VERIFICATION - Check C API and C# wrappers exist.
   4. GIT DIFF (if diff mode) - Analyze the git diff for API/build/dep changes.

   For EVERY finding, classify with BOTH lenses:
   - Changelog: changeType + importance
   - Gap: bindingStatus + impact + priority + effort

   Save to {output_path}"
```

### Phase 3: Synthesize

When both agents complete, merge their findings:

1. **Deduplicate** by name/skiaApi - keep richer data from each
2. **Resolve conflicts** - more cautious bindingStatus wins, higher impact wins
3. **Union hidden APIs** - combine both agents' header scan discoveries
4. **Generate slides** - marketing bullets for major+ importance findings
5. **Generate changelog** - grouped by: Breaking Changes (first), New APIs, Bug Fixes, Performance, Security, Engine Improvements, Deprecations, Dependencies, Platform Changes
6. **Build nextSteps** - prioritized action items from gap analysis

### Phase 4: Generate Outputs

**4a. Generate JSON**

Produce JSON following the schema. Save to `/tmp/skia-analyst-YYYY-MM-DD.json`.

**4b. Validate**

```bash
python3 .agents/skills/skia-analyst/scripts/validate-skia-analyst.py <path-to-json>
```

**4c. Render HTML**

```bash
python3 .agents/skills/skia-analyst/scripts/render-skia-analyst.py <path-to-json>
```

### Phase 5: Present Results

Show both perspectives inline:

**Changelog highlights:**
- Marketing slides (top items)
- Breaking changes (if any)
- New APIs summary

**Gap analysis highlights:**
- Top opportunities by action score (impact x priority x 1/effort)
- Transformative gaps
- Quick wins (partial binding status)

Then offer next steps:
1. "Want me to investigate any finding in more detail?"
2. "Should I use the add-api skill to start binding a feature?"
3. "Want to upload the viewer to a gist for sharing?"
