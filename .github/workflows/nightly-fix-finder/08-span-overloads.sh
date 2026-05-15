#!/usr/bin/env bash
# Category: Missing Span<T> / ReadOnlySpan<T> Overloads

cat << 'GUIDANCE'
## Category: Missing Span<T> / ReadOnlySpan<T> Overloads

### What to look for
Public methods in binding/SkiaSharp/ that accept T[] array parameters where a
ReadOnlySpan<T> or Span<T> overload is absent. Span-based overloads are preferred
because they:
- Accept arrays, stack-allocated buffers, and Memory<T> slices without allocation
- Eliminate the need for callers to copy data into a new array
- Follow modern .NET API conventions

### How to fix
Add a Span overload alongside the existing array overload (ABI-safe, purely additive):
  // Existing — keep as-is
  public void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint) { ... }

  // New overload to add
  public void DrawPoints(SKPointMode mode, ReadOnlySpan<SKPoint> points, SKPaint paint)
  {
      unsafe {
          fixed (SKPoint* p = points)
              SkiaApi.sk_canvas_draw_points(Handle, mode, (IntPtr)points.Length, p, paint.Handle);
      }
  }

### ABI safety
Adding new overloads is always ABI-safe. Never remove or modify existing array overloads.

### What NOT to flag
- Methods that already have a Span overload
- Methods where Span makes no sense (single object, non-collection parameters)
- Private, internal, or protected methods
- Generated binding code (*.generated.cs)
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Public methods with T[] array parameters in binding/SkiaSharp/"
grep -rn '\[\] ' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null \
    | grep 'public' \
    | grep -v 'params\|override\|//' \
    | shuf | head -25 || echo "None found"
echo ""
echo "### Existing ReadOnlySpan/Span usage (for reference on correct implementation patterns)"
grep -rn 'ReadOnlySpan\|Span<' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/SkiaSharp/ 2>/dev/null | head -15 || echo "None found"
