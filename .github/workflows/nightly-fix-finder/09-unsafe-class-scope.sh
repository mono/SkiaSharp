#!/usr/bin/env bash
# Category: Class-Level unsafe Over-Scoping

cat << 'GUIDANCE'
## Category: Class-Level unsafe Over-Scoping

### What to look for
Classes marked as 'public unsafe class' or 'internal unsafe class' in binding/SkiaSharp/
where only a small number of methods actually require unsafe code. Marking an entire
class as unsafe is overly broad — it grants unsafe permissions to all code in the class
when only a few methods need them. This is a "permission creep" pattern.

### How to fix
Remove 'unsafe' from the class declaration and add it only to the specific methods
or blocks that require it:
  // Before
  public unsafe class SKCanvas : SKObject
  {
      public void SafeMethod() { ... }
      public void UnsafeMethod() { fixed (byte* p = ...) { ... } }
  }

  // After
  public class SKCanvas : SKObject
  {
      public void SafeMethod() { ... }
      public unsafe void UnsafeMethod() { fixed (byte* p = ...) { ... } }
  }

### ABI safety
Moving unsafe from class level to method level is ABI-safe — no signature change.

### What NOT to flag
- Classes where the majority of methods genuinely use unsafe patterns
- *.generated.cs files — never touch those
- Classes in externals/ (upstream Skia code)
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Classes with class-level unsafe modifier in binding/SkiaSharp/"
grep -rn 'public unsafe class\|internal unsafe class' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -30 || echo "None found"
echo ""
echo "### Total class-level unsafe class declarations"
grep -rn 'public unsafe class\|internal unsafe class' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | wc -l || true
echo ""
echo "### Method-level unsafe usage in binding/SkiaSharp/ (for comparison)"
grep -rn '^\s*\(public\|private\|internal\|protected\) unsafe ' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -10 || echo "None found"
