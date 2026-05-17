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

### Phase 3: Discover

Before writing any documentation, build context:

1. **Read the documentation patterns** in [references/patterns.md](references/patterns.md) — this covers XML syntax rules, verb conventions, parameter/return patterns, rich remarks with code examples, and type-level documentation guidance.

2. **Study existing well-documented types** — read these XML files to understand what good docs look like:
   - `docs/SkiaSharpAPI/SkiaSharp/SKShader.xml` — rich CDATA remarks with code examples
   - `docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml` — comprehensive member documentation

3. **Review the sample gallery** — `samples/Gallery/Shared/Samples/` contains real-world usage patterns that make excellent code examples.

4. **Read the extracted JSON** — scan each file in `output/docs-work/` to understand the scope:
   - How many types need docs? Which are the most important?
   - Which entries have `"remarksRequired": true`? These need rich content.
   - Read the C# signatures to understand what each API does.

5. **Read the C# source** for the types you'll be documenting — `binding/SkiaSharp/` and `binding/HarfBuzzSharp/`. This is essential for writing accurate docs and avoiding fabrication.

### Phase 4: Write

Fill the JSON files in `output/docs-work/`. **Do NOT edit XML files directly.**

**Critical rules:**
- **NEVER invent API calls** — before writing a code example, verify the method/overload exists by reading the C# source in `binding/`. If you're unsure an API exists, don't use it in examples.
- **NEVER guess numeric values** — for enums with standard mappings (ITU-T, CICP, Vulkan), read the actual `MemberValue` from the JSON and cross-reference with the source.
- **Fill ALL params in ALL overloads** — don't skip params on later overloads. Each overload must be independently complete.
- **Read-only properties use "Gets"** — check the C# source for `{ get; }` vs `{ get; set; }`.
- **Use `<see langword="null" />`** not `<see langword="default" />` for nullable reference parameters.

**Remarks rules:**
- **Type-level entries** (`"memberType": "type"`) have `"remarksRequired": true` and a pre-filled CDATA template. You MUST complete the template — never leave `[bracketed placeholders]`. Include a description, disposal note if applicable, and a code example.
- **Member-level entries**: set `remarks` to `""` for simple members, or write rich markdown for important methods (factory methods, `Draw*`, etc.)

**Content supports XML refs**: `<see cref="T:..." />`, `<paramref name="..." />`, `<see langword="null" />`. In CDATA remarks, use `<xref:...>` instead of `<see cref>`.

Set `remarks` to `""` for self-closing `<remarks />`. Leave fields as "To be added." to skip them.
See [references/patterns.md](references/patterns.md) for full guidance on rich remarks.

### Phase 5: Review

Launch **two background agents** in parallel to validate the JSON **before** merging. Wait for both to complete. This is a write→review→fix loop — repeat until no CRITICAL issues remain.

**Agent 1: Fabrication Detector** — catches invented APIs, wrong facts, and unverified claims:

```
Launch a background general-purpose agent with this prompt:

You are a documentation accuracy auditor. Your ONLY goal is to find FABRICATED
or UNVERIFIED content in the JSON documentation files. AI documentation writers
frequently invent API methods, make false claims about type behavior, and state
"facts" without checking source code. Treat every claim as a hypothesis — verify
it against actual source, or flag it.

Do all work directly. Do NOT launch sub-agents or delegate to further background
agents.

For each JSON file in output/docs-work/:
1. Read the JSON file
2. For EVERY code example in remarks fields:
   - Extract each method call, constructor call, and property access
   - Search binding/ source code to verify it exists with that exact signature
   - Check: does the overload accept those parameter types?
   - Report any call that doesn't match actual source code
3. For EVERY enum description that cites a specific number or standard:
   - Check the MemberValue in the JSON matches what's described
   - Verify standard references (ITU-T H.273, CICP, Vulkan) are correct
4. For EVERY property summary that says "Gets or sets":
   - Check the C# signature — if it has only { get; } then it should say "Gets"
5. For EVERY factual claim in summaries and remarks, verify against source:
   - Immutability/mutability claims ("X is immutable", "X cannot be changed")
   - Thread safety claims ("X is thread-safe", "X can be shared")
   - Default value claims ("defaults to 72", "the default is X")
   - Data format claims (bit layouts, byte sizes, channel counts, packing)
   - Behavioral claims ("unlike X, which does Y")
   If the source code does not confirm a claim, flag it as unverified.
6. **Cross-library boundary check**: SkiaSharp and HarfBuzzSharp wrap DIFFERENT native
   libraries. Verify that documentation does not conflate types across these boundaries:
   - `SKColor` / `sk_color_t` is **ARGB** (0xAARRGGBB). Never describe as "RGBA".
   - `hb_color_t` is **RGBA**. Never describe as "ARGB".
   - If a doc claims a color format, check WHICH native type backs the C# property.

Output a list of issues. For each: file, docId, what's wrong, what it should be.
If no issues found, say "No fabrication issues found."
```

**Agent 2: Quality Reviewer** — catches style, completeness, and accuracy issues:

```
Launch a background general-purpose agent with this prompt:

You are a documentation quality reviewer. Read the checklist at
.agents/skills/api-docs/references/checklist.md for severity criteria.

Do all work directly. Do NOT launch sub-agents or delegate to further background
agents.

For each JSON file in output/docs-work/:
1. Check for remaining "To be added." values that should have been filled
2. Check type-level entries (memberType=type) have real remarks content,
   not template placeholders like [Describe...] or [Show...]
3. Check summaries add value beyond just restating the member name
4. Check <see cref> references use correct prefix (T: M: P: F:)
5. Check boolean params use "true to..." and boolean returns use "true if..."
6. Check nullable params use <see langword="null" /> not "default"
7. Check remarks don't make false comparisons with other types
   (e.g., "Unlike X, which is immutable" — verify before accepting)
8. Check enum member descriptions accurately describe the member's
   specific value, not a similar-looking sibling enum member

Report CRITICAL and IMPORTANT issues only. Include file, docId, and fix.
```

**Fix all CRITICAL issues** from both agents, then re-run the reviewers if you made changes. Only proceed to Phase 6 when both agents report no CRITICAL issues.

### Phase 6: Merge

Only after Phase 5 reviewers pass, merge the JSON back into XML:

```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
```

The merge tool has built-in safety checks — it counts `MemberSignature` and `TypeSignature` elements before and after merging each file and aborts with a fatal error if any were lost. It also validates the output XML is well-formed.

Then run formatting to clean up:

```bash
dotnet cake --target=docs-format-docs
```

## Resources

- [references/patterns.md](references/patterns.md) — XML syntax, verb conventions, and examples
- [references/checklist.md](references/checklist.md) — Review severity criteria (CRITICAL / IMPORTANT / MINOR)
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation
