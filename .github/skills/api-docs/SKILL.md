---
name: api-docs
description: Write and review XML API documentation for SkiaSharp following .NET guidelines. Use when user asks to document a class, add XML docs, write XML documentation, add triple-slash comments, review documentation quality, check docs for errors, fix doc issues, fill in missing docs, remove "To be added" placeholders, or mentions API documentation.
---

# API Documentation

Write and review XML API documentation for SkiaSharp.

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

## Writing Documentation

1. Find the type's XML file in `docs/SkiaSharpAPI/<Namespace>/<TypeName>.xml`
2. Edit `<summary>`, `<param>`, `<returns>`, `<value>` tags within `<Docs>` sections
3. Follow patterns in [references/patterns.md](references/patterns.md)
4. **After editing a file, validate XML syntax** (see [XML Validation](#xml-validation) below)
5. Run `dotnet cake --target=docs-format-docs` to validate and format

## Reviewing Documentation

1. Search for issues using grep patterns below
2. Classify by severity using [references/checklist.md](references/checklist.md)
3. Fix issues following [references/patterns.md](references/patterns.md)
4. Run `dotnet cake --target=docs-format-docs` to validate

### Quick Issue Search

```bash
# Find placeholders
grep -r "To be added" docs/SkiaSharpAPI/

# Find empty tags (self-closing)
grep -rE "<(summary|value|returns)\s*/>" docs/SkiaSharpAPI/

# Find empty tags (open/close with optional whitespace)
grep -rE "<(summary|value|returns)>\s*</" docs/SkiaSharpAPI/

# Find spelling errors (common ones)
grep -riE "teh|recieve|seperate|occured|paramter" docs/SkiaSharpAPI/

# Find whitespace issues
grep -rE " </|  </" docs/SkiaSharpAPI/
```

## Resources

- [references/patterns.md](references/patterns.md) - XML syntax and examples
- [references/checklist.md](references/checklist.md) - Review severity criteria

## XML Validation

**CRITICAL**: After completing all edits to an XML file, validate its XML syntax before moving to the next file.

### Validation Command

```bash
# Validate a single file
xmllint --noout docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml

# Validate all XML files in a namespace
find docs/SkiaSharpAPI/SkiaSharp -name "*.xml" -exec xmllint --noout {} \;

# Validate entire docs directory
find docs/SkiaSharpAPI -name "*.xml" -exec xmllint --noout {} \;
```

### Common XML Errors to Avoid

1. **Duplicate closing tags**: `</param></param>` - happens when copy/pasting
2. **Mismatched tags**: `<summary>...</param>` - tag names must match
3. **Unescaped characters**: `<`, `>`, `&` must be `&lt;`, `&gt;`, `&amp;`
4. **Missing closing tags**: `<summary>text` without `</summary>`

### Workflow

1. Make all edits to a file
2. Run `xmllint --noout <file>` to validate
3. If errors, fix them before proceeding
4. Move to next file
