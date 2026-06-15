#!/usr/bin/env bash
# Category: Obsolete API Internal Callers

cat << 'GUIDANCE'
## Category: Obsolete API Internal Callers

### What to look for
Internal callers in binding/ and source/ using #pragma warning disable CS0618 to
suppress obsolete-member warnings. These are places where our own code calls our own
obsolete APIs instead of the modern replacements.

### How to fix
Look at the [Obsolete] message to find the recommended replacement, then update the
internal caller to use the modern API. Remove the #pragma warning disable once updated.

### ABI safety
Only changes the implementation, not the method signature. Always ABI-safe.
Do NOT remove the [Obsolete] attribute itself — that would break external callers.

### What NOT to flag
- Assembly-level suppressions in AssemblyInfo.cs or Properties/ files
- Test code that intentionally tests the obsolete API itself
- Cases where the modern replacement is not yet fully implemented
GUIDANCE

echo ""
echo "## Scan Data"
echo "### Internal callers using #pragma warning disable CS0618 in binding/ and source/"
grep -rn 'CS0618' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | shuf | head -20 || echo "None found"
echo ""
echo "### [Obsolete] declarations with messages (to find recommended replacements)"
grep -rn '\[Obsolete.*"' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | head -15 || echo "None found"
echo ""
echo "### Total CS0618 suppressions"
grep -rn 'CS0618' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | wc -l || true
