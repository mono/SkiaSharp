#!/usr/bin/env bash
# Category: Inconsistent Dispose Patterns

cat << 'GUIDANCE'
## Category: Inconsistent Dispose Patterns

### What to look for
SKObject-derived instances created in tests/ and samples/ using 'new SK...' but
disposed via an explicit .Dispose() call rather than a 'using' statement or
declaration. The 'using' form is preferred because it disposes on exception too.

### How to fix
Replace:
  var paint = new SKPaint();
  // ... use paint ...
  paint.Dispose();

With:
  using var paint = new SKPaint();
  // ... use paint ...

### ABI safety
No API change — purely an implementation style improvement. Always ABI-safe.

### What NOT to flag
- Value types — SKPoint, SKSize, SKRect, SKRectI, SKColor, SKColorF, SKMatrix are
  structs and do NOT need dispose at all
- Tests that intentionally control disposal timing (e.g. to test double-dispose behavior)
- Dispose() calls in finally blocks that exist for a specific cleanup reason
- Override, virtual, or protected Dispose() implementations
- Calls to base.Dispose()
- binding/ code (more carefully managed; scope is tests/ and samples/ only)
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Direct .Dispose() calls in tests/ and samples/ (not in using/override patterns)"
grep -rn '\.Dispose()' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    tests/ samples/ 2>/dev/null \
    | grep -v 'using\|override\|protected\|virtual\|base\.' \
    | shuf | head -20 || echo "None found"
echo ""
echo "### SKObject instances created with 'new' without 'using' in tests/ and samples/"
grep -rn 'new SK[A-Z][a-zA-Z]*(' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    tests/ samples/ 2>/dev/null \
    | grep -v 'using\|=>' \
    | shuf | head -15 || echo "None found"
