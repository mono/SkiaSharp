#!/usr/bin/env bash
# Category: [Obsolete] Declarations Missing Replacement Guidance

cat << 'GUIDANCE'
## Category: [Obsolete] Declarations Missing Replacement Guidance

### What to look for
Public APIs marked with [Obsolete] or [Obsolete("")] that do not include a message
telling callers what to use instead. Poor [Obsolete] attributes are a DX problem:
external consumers see "deprecated" in IntelliSense but don't know the correct
replacement, leading to confusion and unnecessary support requests.

### How to fix
Add a clear migration message:
  // Before
  [Obsolete]
  public void OldMethod(SKPaint paint) { ... }

  // After
  [Obsolete("Use NewMethod(SKPaint) instead.")]
  public void OldMethod(SKPaint paint) { ... }

If a method is obsolete with no replacement:
  [Obsolete("This method will be removed in a future version. There is no direct replacement.")]

### ABI safety
Changing only the [Obsolete] message attribute. Always ABI-safe.

### What NOT to flag
- [Obsolete] attributes that already have a non-empty message
- [Obsolete] inside generated files (*.generated.cs)
GUIDANCE

echo ""
echo "## Scan Data"
echo "### [Obsolete] with no message (blank attribute)"
grep -rn '\[Obsolete\]' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | head -20 || echo "None found"
echo ""
echo "### [Obsolete] with empty string message"
grep -rn '\[Obsolete("")]' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | head -20 || echo "None found"
echo ""
echo "### All [Obsolete] declarations for context (including ones with messages)"
grep -rn '\[Obsolete' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | head -30 || echo "None found"
