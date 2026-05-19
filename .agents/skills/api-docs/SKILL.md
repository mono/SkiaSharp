---
name: api-docs
description: >
  Write and review XML API documentation for SkiaSharp following .NET guidelines.
  
  Triggers: "document class", "add XML docs", "write XML documentation", "add triple-slash comments",
  "review documentation quality", "check docs for errors", "fix doc issues", "fill in missing docs",
  "remove To be added placeholders", API documentation requests.
---

# API Documentation

Write and review XML API documentation for SkiaSharp.

## Key Concepts

- The `docs/` directory is a **Git submodule** pointing to [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs)
- XML files are **generated from NuGet assemblies** using `mdoc` — if new APIs were added, run Phase 1 before editing
- For existing APIs, you can edit the XML files directly without regenerating (start at Phase 2)

## File Locations

```
docs/SkiaSharpAPI/
├── SkiaSharp/              # Main namespace
│   ├── SKCanvas.xml        # One XML file per type
│   ├── SKPaint.xml
│   ├── SKImage.xml
│   └── ...
├── HarfBuzzSharp/          # HarfBuzz namespace
├── SkiaSharp.Views.*/      # Platform-specific views
└── index.xml               # Extension methods (auto-synced, don't edit)
```

## Phases

### Phase 1: Regenerate Stubs (optional)

When new APIs have been added, XML doc files must be regenerated before documenting them.

```bash
dotnet tool restore
dotnet cake --target=docs-download-output   # Download latest NuGets from CI feed
dotnet cake --target=update-docs            # Regenerate XML docs (api-diff + mdoc update + format)
```

New members appear with "To be added." placeholders. **Skip this phase** if:
- You're editing existing docs (no new APIs)
- The automated workflow already ran stub generation as a pre-agent step

### Phase 2: Extract

Extract placeholders to JSON using the docs tool:
```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 extract docs/SkiaSharpAPI/ -Output output/docs-work/
```
This produces one JSON file per XML file, containing only members with "To be added." placeholders. The `output/` directory is gitignored so local runs don't pollute the repo.

### Phase 3: Discover (Orchestrator)

This phase is lightweight — you are an **orchestrator**, not a writer. Read only what you need to plan the work, then delegate immediately.

1. **Read the manifest** — `output/docs-work/manifest.json` tells you how many files, types, and fields need documentation. Use this to plan the write phase.

2. **Read the reference docs** — these will be passed to writer agents:
   - [references/patterns.md](references/patterns.md) — .NET XML documentation formatting rules
   - [references/skia-patterns.md](references/skia-patterns.md) — SkiaSharp/HarfBuzz domain knowledge

3. **Do NOT pre-read JSON files or source code** — the writer agents will do their own discovery. Pre-reading is wasted work that duplicates what writers do.

### Phase 4: Write and Review (Parallel)

Split the work across **multiple parallel writer agents** for speed. Each writer independently reads its assigned JSON files, reads the corresponding C# source, fills the documentation, AND self-reviews its own work before finishing. This combined write+review approach is more token-efficient than separate phases.

**Splitting strategy:**
- Sort files from manifest by `fieldCount` descending
- Distribute files round-robin across 3 groups to balance total fields per group
- Launch 3 parallel background `general-purpose` agents, one per group

**Writer agent prompt template** (customize the file list for each agent):

```
You are an API DOCUMENTATION WRITER and REVIEWER. Fill all placeholder fields in
your assigned JSON files, then self-review your work before finishing.

ASSIGNED FILES: [list the filenames for this agent's group]

PHASE A — WRITE:
1. Read the patterns file at .agents/skills/api-docs/references/patterns.md
2. Read the domain knowledge at .agents/skills/api-docs/references/skia-patterns.md
3. For each assigned file:
   a. Read the JSON file from output/docs-work/
   b. Find and READ the corresponding C# source in binding/ (ESSENTIAL for accuracy)
   c. Fill all "To be added." fields following the rules below
   d. Write the completed JSON back to the same path

PHASE B — SELF-REVIEW (do this AFTER writing all files):
For each file you wrote, re-read the JSON and verify:
1. Every code example uses real APIs (grep source to confirm method exists)
2. No C# reserved keywords used as variable names (override, base, event, etc.)
3. "Gets or sets" vs "Gets" matches actual property accessors in source
4. Parameter constraints match source validation logic (does it throw? clamp? pad?)
5. No remaining "To be added." or [bracketed placeholders]
6. Boolean params use "true to..." pattern; nullable params use <see langword="null" />
7. Type-level remarks have real content (not template text)
8. Domain facts match skia-patterns.md (color formats, naming, byte layouts)

Fix any issues found during self-review by rewriting the JSON file.

WRITING RULES:
- NEVER invent API calls — verify every method/overload exists by reading C# source
- NEVER guess numeric values — read MemberValue from JSON, cross-reference source
- Fill ALL params in ALL overloads — each overload independently complete
- Read-only properties use "Gets" — check source for { get; } vs { get; set; }
- Use <see langword="null" /> not <see langword="default" /> for nullable params
- Use <see langword="true" /> and <see langword="false" /> for boolean literals in prose

REMARKS RULES:
- Type-level entries (memberType=type) have remarksRequired=true with a CDATA template.
  Complete it fully — never leave [bracketed placeholders]. Include description,
  disposal note if applicable, and a code example.
- Member-level: set remarks to "" for simple members, or write rich markdown for
  important methods (factory methods, Draw*, etc.)

CONTENT FORMAT:
- XML refs: <see cref="T:..." />, <paramref name="..." />, <see langword="null" />
- In CDATA remarks: use <xref:...> instead of <see cref>
- Set remarks to "" for self-closing <remarks />

TRUST HIERARCHY for native type facts (bit layouts, byte orders):
1. Native C/C++ header in repo (if you can find and read it) — AUTHORITATIVE
2. skia-patterns.md reference file — PRE-VERIFIED, trust it
3. Your own knowledge — DO NOT USE for byte layouts. Never invent macro expansions.

Do all work directly. Do NOT launch sub-agents or delegate.
```

Wait for all writer agents to complete before proceeding to Phase 5.

### Phase 5: Merge

After all writers complete and self-review, merge the JSON back into XML:

```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
```

The merge tool has built-in safety checks — it counts `MemberSignature` and `TypeSignature` elements before and after merging each file and aborts with a fatal error if any were lost. It also validates the output XML is well-formed.

Then run formatting to clean up:

```bash
dotnet cake --target=docs-format-docs
```

## Resources

- [references/patterns.md](references/patterns.md) — .NET XML documentation syntax, verb conventions, and formatting
- [references/skia-patterns.md](references/skia-patterns.md) — SkiaSharp/HarfBuzz domain knowledge (color types, naming, threading)
- [references/checklist.md](references/checklist.md) — Review severity criteria (CRITICAL / IMPORTANT / MINOR)
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation
