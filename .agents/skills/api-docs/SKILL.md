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

1. **Read the documentation patterns** in [references/patterns.md](references/patterns.md) — this contains all XML syntax rules, verb conventions, parameter/return patterns, and common mistakes.

2. **Find files with placeholders**:
   ```bash
   grep -rl "To be added" docs/SkiaSharpAPI/ | sort
   ```
   Prioritize: `SkiaSharp/` (core) → `HarfBuzzSharp/` → other namespaces.

3. **For each file with placeholders**:
   - Read the XML file to understand the type and its members
   - Read the corresponding C# source from `binding/` to understand what each API does:
     - `docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml` → search `binding/SkiaSharp/` for `SKCanvas`
     - `docs/SkiaSharpAPI/HarfBuzzSharp/Buffer.xml` → search `binding/HarfBuzzSharp/` for `Buffer`
   - Replace "To be added." placeholders with proper documentation following the patterns
   - **NEVER modify `<MemberSignature>`, `<TypeSignature>`, `<MemberType>`, `<AssemblyInfo>`, `<ReturnValue>`, `<Parameters>`, or `<Attributes>` elements** — only edit content inside `<Docs>` blocks (`<summary>`, `<param>`, `<returns>`, `<value>`, `<remarks>`)
   - **Validate XML** after editing each file:
     ```bash
     xmllint --noout docs/SkiaSharpAPI/<Namespace>/<TypeName>.xml
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
