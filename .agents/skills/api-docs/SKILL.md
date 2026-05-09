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

## Phases

The full docs lifecycle has 5 phases. Interactive use typically starts at Phase 2 or 3.
The automated workflow (`auto-api-docs-writer`) runs all phases, with Phase 1 handled by pre-agent steps.

### Phase 1: Regenerate Stubs

When new APIs have been added, XML doc files must be regenerated before documenting them.

```bash
dotnet tool restore
dotnet cake --target=docs-download-output   # Download latest NuGets from CI feed
dotnet cake --target=update-docs            # Regenerate XML docs (api-diff + mdoc update + format)
```

New members appear with "To be added." placeholders. **Skip this phase** if:
- You're editing existing docs (no new APIs)
- The automated workflow already ran stub generation as a pre-agent step
- You were told "Phase 1 is pre-computed"

### Phase 2: Discover Placeholders

Find files needing documentation:

```bash
# Find all "To be added." placeholders
grep -rl "To be added" docs/SkiaSharpAPI/ | sort

# Count total placeholders
grep -r "To be added" docs/SkiaSharpAPI/ | wc -l

# Find empty/self-closing tags
grep -rE "<(summary|value|returns)\s*/>" docs/SkiaSharpAPI/
grep -rE "<(summary|value|returns)>\s*</" docs/SkiaSharpAPI/
```

Prioritize by namespace:
1. `docs/SkiaSharpAPI/SkiaSharp/` — core namespace (most important)
2. `docs/SkiaSharpAPI/HarfBuzzSharp/` — text shaping
3. Other namespaces (Views, etc.)

### Phase 3: Write Documentation

For each file with placeholders:

1. **Read the XML file** to understand the type and its members
2. **Read the corresponding C# source** from `binding/` to understand what each API does:
   - `docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml` → search `binding/SkiaSharp/` for `SKCanvas`
   - `docs/SkiaSharpAPI/HarfBuzzSharp/Buffer.xml` → search `binding/HarfBuzzSharp/` for `Buffer`
3. **Replace "To be added." placeholders** with proper documentation following patterns below
4. **Validate XML** after editing each file:
   ```bash
   xmllint --noout docs/SkiaSharpAPI/<Namespace>/<TypeName>.xml
   ```

### Phase 4: Validate and Format

Run the formatting target to validate and clean up:

```bash
dotnet cake --target=docs-format-docs
```

Fix any errors it reports. This also syncs extension method docs in `index.xml`.

### Phase 5: Commit

Commit changes inside the `docs/` submodule:

```bash
cd docs
git add -A
git config user.name "github-actions[bot]"
git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
git commit -m "Fill API documentation placeholders"
cd ..
```

For interactive use, push to your fork and open a PR against `mono/SkiaSharp-API-docs`.

## Key Concepts

- The `docs/` directory is a **Git submodule** pointing to [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs) — changes must be committed and PR'd to that repository
- XML files are **generated from NuGet assemblies** using `mdoc` — if new APIs were added, run Phase 1 before editing
- For existing APIs, you can edit the XML files directly without regenerating (start at Phase 3)

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

## Reviewing Documentation

When reviewing (not writing from scratch), search for issues:

```bash
# Find spelling errors (common ones)
grep -riE "teh|recieve|seperate|occured|paramter" docs/SkiaSharpAPI/

# Find whitespace issues
grep -rE " </|  </" docs/SkiaSharpAPI/
```

Classify by severity using [references/checklist.md](references/checklist.md) and fix following [references/patterns.md](references/patterns.md).

## Resources

- [references/patterns.md](references/patterns.md) — XML syntax and examples
- [references/checklist.md](references/checklist.md) — Review severity criteria
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation

## XML Validation

**CRITICAL**: After completing all edits to an XML file, validate its XML syntax before moving to the next file.

```bash
# Validate a single file
xmllint --noout docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml

# Validate all XML files in a namespace
find docs/SkiaSharpAPI/SkiaSharp -name "*.xml" -exec xmllint --noout {} \;

# Validate entire docs directory
find docs/SkiaSharpAPI -name "*.xml" -exec xmllint --noout {} \;
```

### Common XML Errors to Avoid

1. **Duplicate closing tags**: `</param></param>` — happens when copy/pasting
2. **Mismatched tags**: `<summary>...</param>` — tag names must match
3. **Unescaped characters**: `<`, `>`, `&` must be `&lt;`, `&gt;`, `&amp;`
4. **Missing closing tags**: `<summary>text` without `</summary>`
