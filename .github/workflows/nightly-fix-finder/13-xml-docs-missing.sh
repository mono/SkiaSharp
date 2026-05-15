#!/usr/bin/env bash
# Category: Missing XML Documentation on Public APIs

cat << 'GUIDANCE'
## Category: Missing XML Documentation on Public APIs

### What to look for
Public constructors, methods, or properties in core SkiaSharp binding classes that
have no /// <summary> XML documentation comment. Focus on high-traffic classes:
SKCanvas, SKBitmap, SKImage, SKPaint, SKPath, SKSurface, SKFont, SKColorSpace,
SKShader, SKData. File one issue per class, scoped to a meaningful batch (not every
single member in one PR).

### How to fix
Add meaningful /// <summary> XML doc comments. Reference skia.org docs for accuracy:
  /// <summary>
  /// Creates a new <see cref="SKBitmap"/> with the specified dimensions.
  /// </summary>
  /// <param name="width">The width of the bitmap in pixels.</param>
  /// <param name="height">The height of the bitmap in pixels.</param>
  public SKBitmap(int width, int height) { ... }

### ABI safety
XML doc comments have zero runtime impact. Always ABI-safe.

### What NOT to flag
- Internal, private, or protected members
- Generated files (*.generated.cs) — never touch those
- source/ directory — focus on binding/SkiaSharp/ only
- Already-documented members
- Suggest documenting every overload identically — better to file one issue per class
  for a focused batch of the most-used constructors and methods
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Public members with no preceding /// <summary> in core binding classes"
for cls in SKCanvas SKBitmap SKImage SKPaint SKPath SKSurface SKFont SKColorSpace SKShader SKData; do
    FILE="binding/SkiaSharp/${cls}.cs"
    if [ -f "$FILE" ]; then
        echo "--- $cls ---"
        awk '
            /^\s*\/\/\//{has_doc=1}
            /^\s*public /{
                if (!has_doc) print FILENAME ":" NR ": " $0
                has_doc=0
            }
            !/^\s*\/\/\// { has_doc=0 }
        ' "$FILE" 2>/dev/null | grep -v 'class\b\|{$\|^\s*public\s*{' | head -10
    fi
done
echo ""
echo "### Undocumented public member counts per file in binding/SkiaSharp/"
for f in binding/SkiaSharp/*.cs; do
    [ -f "$f" ] || continue
    [[ "$f" == *generated* ]] && continue
    awk '
        /^\s*\/\/\//{has_doc=1}
        /^\s*public /{
            if (!has_doc) count++
            has_doc=0
        }
        !/^\s*\/\/\// { has_doc=0 }
        END { if (count > 0) print count "\t" FILENAME }
    ' "$f" 2>/dev/null
done | sort -rn | head -15
