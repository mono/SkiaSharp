#!/usr/bin/env bash
# Category: Missing Null Argument Validation

cat << 'GUIDANCE'
## Category: Missing Null Argument Validation

### What to look for
Public methods in binding/SkiaSharp/ that accept SK*/GR* reference type parameters
without an ArgumentNullException guard before passing Handle to native code.
A null passed through to a P/Invoke call causes AccessViolationException or a native
crash instead of a clear, debuggable exception.

### How to fix
Add null checks consistent with existing patterns:
  if (paint == null)
      throw new ArgumentNullException(nameof(paint));

For code that must compile on both netstandard2.0 and net6+, use the simple
if-null-throw pattern (works everywhere). Do NOT use ArgumentNullException.ThrowIfNull()
without a #if NET6_0_OR_GREATER guard.

### ABI safety
Adding argument validation does not change the method signature. Always ABI-safe.

### What NOT to flag
- Parameters explicitly documented or designed to accept null (e.g., SKPaint paint = null)
- Overloads that delegate to a primary overload which already validates
- Private, internal, or protected methods
- Value type parameters (structs: SKPoint, SKRect, SKColor, SKMatrix, etc.)
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Public methods with SK*/GR* reference parameters in binding/SkiaSharp/"
echo "### (agent must read the full method to verify no null check exists)"
grep -rn 'public.*\(.*SK[A-Z]\|public.*\(.*GR[A-Z]' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | shuf | head -25 || echo "None found"
echo ""
echo "### Existing ArgumentNullException patterns (for reference on correct style)"
grep -rn 'throw new ArgumentNullException' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -10 || echo "None found"
echo "### Total existing null checks"
grep -rn 'throw new ArgumentNullException' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | wc -l || true
