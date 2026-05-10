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

2. **Extract placeholders to JSON** using the extract script:
   ```bash
   python3 .agents/skills/api-docs/scripts/docs-tool.py extract docs/SkiaSharpAPI/ -o docs-work/
   ```
   This produces one JSON file per XML file, containing only members with "To be added." placeholders. Each entry includes the DocId, C# signature, member type, and which fields need filling.

3. **Fill the JSON files** — for each JSON file in `docs-work/`:
   - Read the JSON to see what needs docs
   - Read the corresponding C# source from `binding/` to understand each API
   - Edit the JSON to replace "To be added." values with proper documentation
   - **Do NOT edit XML files directly** — the merge script handles that safely

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

   Set `remarks` to `""` for self-closing `<remarks />`. Leave fields as "To be added." to skip them.

4. **Merge filled JSON back into XML**:
   ```bash
   pip install lxml  # Required for CDATA-safe XML handling
   python3 .agents/skills/api-docs/scripts/docs-tool.py merge docs-work/ --validate
   ```
   The merge script uses lxml to safely modify only `<Docs>` blocks — it structurally cannot touch `<MemberSignature>` elements and preserves CDATA sections byte-for-byte.
     ```

   Common XML errors to avoid:
   - **Duplicate closing tags**: `</param></param>` — happens when copy/pasting
   - **Mismatched tags**: `<summary>...</param>` — tag names must match
   - **Unescaped characters**: `<`, `>`, `&` must be `&lt;`, `&gt;`, `&amp;`
   - **Missing closing tags**: `<summary>text` without `</summary>`

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
