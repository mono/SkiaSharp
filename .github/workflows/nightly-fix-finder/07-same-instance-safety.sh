#!/usr/bin/env bash
# Category: Same-Instance-Return Safety

cat << 'GUIDANCE'
## Category: Same-Instance-Return Safety

### What to look for
Calls to Subset(), ToRasterImage(), or ToRasterImage(false) where the caller disposes
the source object without first checking if the result and source are the same instance.

### Why it matters
These methods may return the SAME instance rather than a new object. Disposing the
source when result == source causes a use-after-free crash that is hard to diagnose.

### Correct pattern
  var result = source.Subset(bounds);
  if (result != source)
      source.Dispose();
  return result;

  // Or with try/finally:
  var raster = image.ToRasterImage(true);
  try {
      // ... use raster ...
  } finally {
      if (raster != image)
          raster.Dispose();
  }

### ABI safety
Only changes internal implementation logic. Always ABI-safe.

### What NOT to flag
- Cases where the source is not disposed at all (separate memory leak issue)
- Internal SKObject.GetObject() wrappers — those handle reference counting correctly
- Cases where result is used but source is intentionally kept alive
GUIDANCE

echo ""
echo "## Scan Data"
echo "### All calls to Subset() or ToRasterImage() across the codebase"
grep -rn '\.Subset\s*(\|\.ToRasterImage\s*(' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ tests/ samples/ 2>/dev/null | shuf | head -20 || echo "None found"
echo ""
echo "### Calls to Subset/ToRasterImage with Dispose() visible nearby (potential bug)"
grep -rnA3 '\.Subset\|\.ToRasterImage' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ tests/ samples/ 2>/dev/null \
    | grep -B3 'Dispose' | head -30 || echo "None found"
