#!/usr/bin/env bash
# Category: Test Coverage Gaps

cat << 'GUIDANCE'
## Category: Test Coverage Gaps

### What to look for
Public methods in core SkiaSharp classes — SKCanvas, SKBitmap, SKImage, SKPaint,
SKPath, SKSurface, SKFont, SKColorSpace, SKShader, SKData — that have no corresponding
reference in tests/.

### How to fix
File an issue requesting a non-trivial test for the untested method. Include a skeleton:
  [SkippableFact]
  public void MethodNameWorks()
  {
      using var surface = SKSurface.Create(new SKImageInfo(100, 100));
      var canvas = surface.Canvas;
      // actual test logic
      Assert.Equal(expected, actual);
  }

### ABI safety
Adding tests does not touch the public API. Always ABI-safe.

### What NOT to flag
- Simple property getters trivially verified by other tests
- GPU-specific methods unavailable in CI
- Methods requiring platform-specific hardware
- Trivial Assert.NotNull suggestions — the test must verify real behavior
GUIDANCE

echo ""
echo "## Scan Data"
for cls in SKCanvas SKBitmap SKImage SKPaint SKPath SKSurface SKFont SKColorSpace SKShader SKData; do
    FILE="binding/SkiaSharp/${cls}.cs"
    if [ -f "$FILE" ]; then
        grep -n 'public.*[a-zA-Z]\+\s*(' "$FILE" 2>/dev/null \
            | grep -v 'class\|interface\|enum\|delegate\|struct\|//' \
            | sed "s|^|$FILE:|" \
            | awk -F'(' '{print $1}' \
            | awk '{print $NF, "|", $0}'
    fi
done | sort -u > /tmp/gh-aw/agent/public-methods.txt

echo "### All extracted public methods:"
cat /tmp/gh-aw/agent/public-methods.txt
echo ""
echo "### Methods NOT found (whole-word) in any test file:"
while IFS='|' read -r method source; do
    method=$(echo "$method" | tr -d ' ')
    if [ ${#method} -gt 3 ] && ! grep -rwq "$method" --include='*.cs' tests/ 2>/dev/null; then
        echo "  UNTESTED: $method  (from $source)"
    fi
done < /tmp/gh-aw/agent/public-methods.txt | head -25
