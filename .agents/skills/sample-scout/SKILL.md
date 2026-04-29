---
name: sample-scout
description: >
  Scout Skia GM (golden master) samples in the externals/skia submodule to find demos worth
  porting to the SkiaSharp Gallery. Reads .cpp files directly from the checked-out submodule,
  analyzes what each demonstrates, checks whether the required APIs exist in SkiaSharp, and
  cross-references against existing Gallery samples to identify gaps. Produces a structured JSON
  report and a GitHub-flavored Markdown report for filtering by interest level, API availability,
  and sample coverage status. Use this skill whenever the user asks about "what samples should we
  build", "what demos are we missing", "find interesting Skia GMs", "sample gap analysis", "what
  can we port from Skia", "gallery ideas", "scout GM samples", or any request to discover demo
  opportunities from upstream Skia. Also use proactively after adding new APIs to find samples
  that showcase them.
---

# Sample Scout

You analyze Skia GM (golden master) sample files from the `externals/skia` submodule to discover
demos worth porting to the SkiaSharp Gallery. The goal is to find visually impressive, educationally
valuable samples that showcase SkiaSharp's capabilities — and identify which ones we can build today
vs. which need new APIs first.

## Why This Matters

Skia has 400+ GM samples that exercise every API and visual technique. These are a goldmine for
the SkiaSharp Gallery — each one is a proven, tested visual that demonstrates something users would
want to learn. But nobody can manually review 400+ C++ files to find the gems. This skill automates
the discovery.

## Key References

- **[references/analysis-instructions.md](references/analysis-instructions.md)** — How to classify samples
- **[references/sample-scout-schema.json](references/sample-scout-schema.json)** — JSON Schema for validation
- **[references/schema-cheatsheet.md](references/schema-cheatsheet.md)** — Human-readable schema docs

## Workflow

```
Phase 1: Setup (list GM files, list existing Gallery samples)
Phase 2: Analyze GM files (parallel agents, each handles a chunk)
Phase 3: Cross-reference with existing Gallery samples
Phase 4: Validate and render
```

### Phase 1: Setup

**1a. Ensure the submodule is checked out**

The GM files live in `externals/skia/gm/`. If the submodule isn't initialized:

```bash
git submodule update --init --depth=1 externals/skia
```

**1b. List all GM files**

```bash
ls externals/skia/gm/*.cpp | xargs -n1 basename | sort > gm-files.txt
wc -l < gm-files.txt
```

**1c. List existing Gallery samples**

```bash
find samples/Gallery -name "*.cs" -path "*/Samples/*" | sort
```

For each sample, extract the `Title`, `Description`, and `Category` to build a coverage map.

**1d. Split into chunks for parallel processing**

With 400+ files, split into 5 chunks of ~80-90 files each for parallel analysis.

### Phase 2: Analyze GM Files

Launch **5 parallel background agents** (general-purpose), each analyzing one chunk. Each agent:

1. For each `.cpp` file in its chunk, read it directly from the submodule:
   ```bash
   cat externals/skia/gm/{filename}
   ```

2. Read the file and extract:
   - **What it demonstrates** (1-2 sentences)
   - **Key Skia APIs used** (class::method names)
   - **Interest level**: high / medium / low
   - **API availability**: check if the required APIs exist in SkiaSharp by grepping `binding/SkiaSharp/`
   - **Missing APIs**: list any APIs not available in SkiaSharp
   - **Notes**: GPU-only, Graphite-specific, bug regression, etc.

3. Save findings as JSON array to a temp file.

See [references/analysis-instructions.md](references/analysis-instructions.md) for the classification
criteria and decision guidelines.

**Agent prompt template:**
```
Analyze Skia GM sample files. For EACH file, read it from externals/skia/gm/FILENAME
and produce a JSON entry.

Files: {comma-separated list}

Read .agents/skills/sample-scout/references/analysis-instructions.md for classification criteria.

For each file output: file, name, description, interesting (high/medium/low),
apis_available (true/false), missing_apis [], key_apis [], notes,
visualGoal (what the rendered output looks like), suggestedControls [],
category (Gallery category), skiaSharpApis [] (C# equivalents).

Check API availability by grepping binding/SkiaSharp/ for the C# equivalents.
Save as JSON array to {output_path}.
Must produce exactly {N} entries — count at the end to confirm.
```

### Phase 3: Cross-Reference with Existing Gallery Samples

After all agents complete, merge their findings and cross-reference against existing Gallery samples:

For each GM entry, check if an existing Gallery sample covers the same topic:
- **`existing`** — A Gallery sample directly covers this GM's main feature
- **`similar`** — A Gallery sample covers a related topic (e.g., gradient GM → Gradient sample exists)
- **`none`** — No Gallery sample covers this

Tag each finding with `sampleStatus` and `matchedSample`.

Save the merged findings as `sample-scout-report.json` in the working directory.

### Phase 4: Validate and Render

**4a. Validate**

```bash
python3 .agents/skills/sample-scout/scripts/validate-sample-scout.py sample-scout-report.json
```

**4b. Render Markdown**

```bash
python3 .agents/skills/sample-scout/scripts/render-sample-scout.py sample-scout-report.json sample-scout-report.md
```

This produces a `.md` file with `###`/`####` headers suitable for GitHub issues.

 
