#!/usr/bin/env bash
# Category: Missing readonly on Struct Methods

cat << 'GUIDANCE'
## Category: Missing readonly on Struct Methods

### What to look for
Methods and property getters on SkiaSharp value types (structs) that do NOT mutate
the struct's fields but are not marked 'readonly'. In C# 8+, omitting 'readonly' on
non-mutating struct members causes the compiler to create a defensive copy of the
struct before each call when read through a read-only reference — a hidden performance
cost and source of subtle bugs.

Target structs: SKPoint, SKPointI, SKSize, SKSizeI, SKRect, SKRectI, SKColor,
               SKColorF, SKMatrix, SKMatrix44, SKRoundRect

### How to fix
Add the readonly modifier to methods and property getters that only read fields:
  // Before
  public float Length => (float)Math.Sqrt(x * x + y * y);

  // After
  public readonly float Length => (float)Math.Sqrt(x * x + y * y);

### ABI safety
Adding readonly to a struct method is a compile-time-only annotation. ABI-safe.
Do NOT add readonly to methods that assign to any field (e.g., Offset(), Inflate(),
Scale()) — those are intentionally mutating.

### What NOT to flag
- Methods that assign to struct fields — they are intentionally mutable
- Methods inside class types — readonly only applies to struct members
- Already-readonly members
- Constructors
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Struct definitions in binding/SkiaSharp/"
grep -rn 'public partial struct\|public struct\|internal struct' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -20 || echo "None found"
echo ""
echo "### Non-readonly public members in MathTypes.cs (candidate list)"
grep -n '^\s*public [a-zA-Z]' \
    binding/SkiaSharp/MathTypes.cs 2>/dev/null \
    | grep -v 'readonly\|static\|struct\|class\|//' \
    | shuf | head -25 || echo "None found"
echo ""
echo "### Existing readonly members on structs (for reference style)"
grep -rn 'public readonly' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -10 || echo "None found"
