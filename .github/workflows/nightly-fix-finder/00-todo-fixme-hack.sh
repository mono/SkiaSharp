#!/usr/bin/env bash
# Category: TODO/FIXME/HACK Comments

cat << 'GUIDANCE'
## Category: TODO/FIXME/HACK Comments

### What to look for
Stale TODO/FIXME/HACK/XXX comments in binding/ and source/ with a concrete,
actionable resolution — either implement what's described or remove the comment
if it's no longer relevant.

### Good candidates
- TODOs referencing a bug number or feature that has since been completed
- TODOs with a clear description of what needs doing (not just "fix this someday")
- FIXMEs that indicate a known problem without a workaround
- Prefer binding/SkiaSharp/ over source/ (higher impact, more likely to be noticed)

### How to fix
Either implement the TODO or remove the comment if it's no longer relevant.
Include a brief explanation in the issue body of why it was resolved or removed.

### ABI safety
Implementing a TODO may add new public APIs (always ABI-safe as an addition).
Removing a comment is trivially safe. Never change existing method signatures while
resolving a TODO.

### What NOT to flag
- TODOs inside generated files (*.generated.cs) — never touch those
- TODOs that are clearly still valid and require multi-PR effort
- Vague TODO comments with no actionable content
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Sample TODO/FIXME/HACK comments in binding/ and source/"
grep -rn 'TODO\|FIXME\|HACK\|XXX' \
    --include='*.cs' \
    --exclude-dir=obj --exclude-dir=bin \
    --exclude='*.generated.cs' \
    binding/ source/ 2>/dev/null | shuf | head -25 || echo "None found"
echo ""
echo "### Total count"
grep -rn 'TODO\|FIXME\|HACK\|XXX' \
    --include='*.cs' \
    --exclude-dir=obj --exclude-dir=bin \
    --exclude='*.generated.cs' \
    binding/ source/ 2>/dev/null | wc -l || true
