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

### Phase 2: Discover and Write

1. **Read the documentation patterns** in [references/patterns.md](references/patterns.md) — this covers XML syntax rules, verb conventions, parameter/return patterns, **rich remarks with code examples**, and type-level documentation guidance.

   Key quality expectations:
   - **Types** (classes/structs): need rich remarks with overview, usage example, disposal notes
   - **Factory/important methods**: need code examples showing common usage
   - **Look at existing well-documented types** (`docs/SkiaSharpAPI/SkiaSharp/SKShader.xml`, `SKCanvas.xml`) as a quality target
   - **Look at sample gallery** (`samples/Gallery/Shared/Samples/`) for real-world usage patterns
   - Simple getters/setters and enum members only need summaries

   **Critical rules to avoid common mistakes:**
   - **NEVER invent API calls** — before writing a code example, verify the method/overload exists by reading the C# source in `binding/`. If you're unsure an API exists, don't use it in examples.
   - **NEVER guess numeric values** — for enums with standard mappings (ITU-T, CICP, Vulkan), read the actual `MemberValue` from the XML and cross-reference with the source.
   - **Fill ALL params in ALL overloads** — don't skip params on later overloads just because you documented them on the first overload. Each overload's params must be independently complete.
   - **Read-only properties use "Gets"** — check the C# source for `{ get; }` vs `{ get; set; }` before writing "Gets or sets".
   - **Use `<see langword="null" />`** not `<see langword="default" />` for nullable reference parameters.

2. **Extract placeholders to JSON** using the docs tool:
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 extract docs/SkiaSharpAPI/ -Output output/docs-work/
   ```
   This produces one JSON file per XML file, containing only members with "To be added." placeholders. Each entry includes the DocId, C# signature, member type, and which fields need filling. The `output/` directory is gitignored so local runs don't pollute the repo.

3. **Fill the JSON files** — for each JSON file in `output/docs-work/`:
   - Read the JSON to see what needs docs
   - Read the corresponding C# source from `binding/` to understand each API
   - Edit the JSON to replace "To be added." values with proper documentation
   - **Do NOT edit XML files directly** — the merge script handles that safely

   **Remarks rules:**
   - **Type-level entries** (`"memberType": "type"`) have `"remarksRequired": true` and a pre-filled CDATA template. You MUST complete the template with real content — never leave the `[bracketed placeholders]`. Include a description, disposal note if applicable, and a code example.
   - **Member-level entries**: set `remarks` to `""` for simple members, or write rich markdown for important methods (factory methods, `Draw*`, etc.)
   - Look at `samples/Gallery/Shared/Samples/` and the C# source to write realistic code examples

   Example JSON entry (before):
   ```json
   {
     "docId": "M:SkiaSharp.SKCanvas.DrawRect(SkiaSharp.SKRect,SkiaSharp.SKPaint)",
     "memberType": "Method",
     "signature": "public void DrawRect (SkiaSharp.SKRect rect, SkiaSharp.SKPaint paint);",
     "fields": {
       "summary": "To be added.",
       "params": { "rect": "To be added.", "paint": "To be added." },
       "remarks": "To be added."
     }
   }
   ```

   After filling (supports XML refs like `<see cref>`, `<paramref>`, `<see langword>`):
   ```json
   {
     "docId": "M:SkiaSharp.SKCanvas.DrawRect(SkiaSharp.SKRect,SkiaSharp.SKPaint)",
     "memberType": "Method",
     "signature": "public void DrawRect (SkiaSharp.SKRect rect, SkiaSharp.SKPaint paint);",
     "fields": {
       "summary": "Draws a rectangle using the specified paint.",
       "params": {
         "rect": "The <see cref=\"T:SkiaSharp.SKRect\" /> to draw.",
         "paint": "The <see cref=\"T:SkiaSharp.SKPaint\" /> that controls color and style."
       },
       "remarks": ""
     }
   }
   ```

   For **types**, the extract script pre-fills a CDATA template. Complete it:
   ```json
   {
     "docId": null,
     "memberType": "type",
     "remarksRequired": true,
     "fields": {
       "summary": "Provides a mutable builder for constructing <see cref=\"T:SkiaSharp.SKPath\" /> objects.",
       "remarks": "<format type=\"text/markdown\"><![CDATA[\n## Remarks\n\n`SKPaint` controls how drawing operations render on the canvas, including color, stroke width, anti-aliasing, blend modes, shaders, and text properties.\n\nThis type wraps a native Skia resource and implements `IDisposable`. Always dispose when done, either with a `using` statement or by calling `Dispose()` directly.\n\n## Examples\n\n```csharp\nusing var paint = new SKPaint\n{\n    Color = SKColors.CornflowerBlue,\n    IsAntialias = true,\n    Style = SKPaintStyle.Fill,\n};\ncanvas.DrawCircle(128, 128, 80, paint);\n```\n]]></format>"
     }
   }
   ```

   For **simple members**, set `remarks` to `""`. Leave fields as "To be added." to skip them.
   See [references/patterns.md](references/patterns.md) for full guidance on rich remarks.

4. **Merge filled JSON back into XML**:
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
   ```
   The merge script uses .NET XmlDocument to safely modify only `<Docs>` blocks — it structurally cannot touch `<MemberSignature>` elements and preserves CDATA sections.

### Phase 3: Validate

Run the formatting target to validate and clean up:

```bash
dotnet cake --target=docs-format-docs
```

Fix any errors it reports. This also syncs extension method docs in `index.xml`.

Then launch **two background agents** in parallel to validate your work. Wait for both to complete and fix any issues they find.

**Agent 1: Fabrication Detector** — catches invented APIs and wrong facts:

```
Launch a background general-purpose agent with this prompt:

You are a documentation accuracy auditor. Your ONLY goal is to find FABRICATED
content in the JSON documentation files. AI documentation writers frequently
invent API methods, overloads, and constructor signatures that don't actually
exist. You must catch every instance.

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

Output a list of issues. For each: file, docId, what's wrong, what it should be.
If no issues found, say "No fabrication issues found."
```

**Agent 2: Quality Reviewer** — catches style and completeness issues:

```
Launch a background general-purpose agent with this prompt:

You are a documentation quality reviewer. Read the checklist at
.agents/skills/api-docs/references/checklist.md for severity criteria.

For each JSON file in output/docs-work/:
1. Check for remaining "To be added." values that should have been filled
2. Check type-level entries (memberType=type) have real remarks content,
   not template placeholders like [Describe...] or [Show...]
3. Check summaries add value beyond just restating the member name
4. Check <see cref> references use correct prefix (T: M: P: F:)
5. Check boolean params use "true to..." and boolean returns use "true if..."
6. Check nullable params use <see langword="null" /> not "default"

Report CRITICAL and IMPORTANT issues only. Include file, docId, and fix.
```

Fix all CRITICAL issues from both agents before finishing. IMPORTANT issues should be fixed if time allows.

## Resources

- [references/patterns.md](references/patterns.md) — XML syntax, verb conventions, and examples
- [references/checklist.md](references/checklist.md) — Review severity criteria (CRITICAL / IMPORTANT / MINOR)
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation
