#!/usr/bin/env bash
# Category: Inconsistent SK* Parameter Nullability

cat << 'GUIDANCE'
## Category: Inconsistent SK* Parameter Nullability

### What to look for
Public methods in binding/SkiaSharp/ where an SK*/GR* reference parameter is
passed directly to native code via .Handle without any null check. When a caller
passes null they get AccessViolationException or a native crash instead of a clear,
debuggable ArgumentNullException.

There are three valid patterns — only pattern (c) is a bug:
  (a) Null accepted: SKPaint paint = null — documented nullable, intentional
  (b) Null validated: if (paint == null) throw new ArgumentNullException(nameof(paint));
  (c) Null unguarded: SkiaApi.sk_canvas_draw_X(Handle, ..., paint.Handle) — BUG

### How to fix
For methods where null is NOT valid input, add a guard before the .Handle access:
  if (paint == null)
      throw new ArgumentNullException(nameof(paint));

### ABI safety
Adding argument validation does not change the method signature. Always ABI-safe.

### What NOT to flag
- Parameters explicitly designed to accept null (= null default, or paint?.Handle patterns)
- Overloads that delegate to a primary overload which already validates
- Private, internal, or protected methods
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Methods with SK*/GR* params where .Handle is accessed (may lack null guard)"
echo "### Agent must read the full method to confirm no null check is present"
grep -rn '\.Handle' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null \
    | grep -v '// \|Handle =\|Handle !\|IntPtr\|nint\|var \|= Handle\|this\.Handle\|\.Handle =\|Handle ==\|Handle !=\|?\.Handle' \
    | shuf | head -25 || echo "None found"
echo ""
echo "### Parameters explicitly designed to accept null (for contrast)"
grep -rn 'SK[A-Z][a-zA-Z]* [a-zA-Z]* = null' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -15 || echo "None found"
echo ""
echo "### Existing null-safe Handle access patterns (for reference)"
grep -rn '?\.Handle\|?? IntPtr' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -10 || echo "None found"
