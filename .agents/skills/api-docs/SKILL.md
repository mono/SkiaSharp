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
       "remarks": "<format type=\"text/markdown\"><![CDATA[\n## Remarks\n\n`SKPathBuilder` provides a step-by-step way to construct paths. Use `MoveTo` to set the starting point, then `LineTo`, `QuadTo`, or `CubicTo` to add segments. Call `Close` to connect back to the start.\n\nThis type wraps a native Skia resource and implements `IDisposable`. Always dispose when done.\n\n## Examples\n\n```csharp\nusing var builder = new SKPathBuilder();\nbuilder.MoveTo(10, 10);\nbuilder.LineTo(100, 50);\nbuilder.LineTo(50, 100);\nbuilder.Close();\n\nusing var path = builder.Snapshot();\ncanvas.DrawPath(path, paint);\n```\n]]></format>"
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

Then launch a **background agent** to review your changes for quality issues. This catches mistakes that the writing agent may be biased toward missing:

```
Use a background explore agent to review the changed XML files against
the checklist in .agents/skills/api-docs/references/checklist.md.
Report any CRITICAL or IMPORTANT issues found.
```

Fix any issues the reviewer finds before finishing.

## Resources

- [references/patterns.md](references/patterns.md) — XML syntax, verb conventions, and examples
- [references/checklist.md](references/checklist.md) — Review severity criteria (CRITICAL / IMPORTANT / MINOR)
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation
