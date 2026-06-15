#!/usr/bin/env bash
# Category: Missing nameof() in Exceptions

cat << 'GUIDANCE'
## Category: Missing nameof() in Exceptions

### What to look for
throw new ArgumentNullException("paramName") or ArgumentOutOfRangeException("paramName")
using a raw string literal for the parameter name instead of nameof(paramName).
Using nameof() ensures the string stays in sync if the parameter is renamed.

### How to fix
Replace the string literal with nameof():
  // Before
  throw new ArgumentNullException("paint");
  // After
  throw new ArgumentNullException(nameof(paint));

### ABI safety
Only changes the exception message at runtime. Always ABI-safe.

### What NOT to flag
- ArgumentException("descriptive message") — these use descriptive messages, not param names
- Cases where the string is a descriptive message rather than a parameter name
- Calls to ArgumentNullException(string, Exception) — different overload
GUIDANCE

echo ""
echo "## Scan Data"
echo "### ArgumentNullException using string literals instead of nameof()"
grep -rn 'ArgumentNullException\s*("' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | grep -v 'nameof' | head -20 || echo "None found"
echo ""
echo "### ArgumentOutOfRangeException using string literals instead of nameof()"
grep -rn 'ArgumentOutOfRangeException\s*("' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | grep -v 'nameof' | head -20 || echo "None found"
echo ""
echo "### Reference: correct pattern using nameof()"
grep -rn 'throw new Argument.*Exception\s*(nameof' \
    --include='*.cs' \
    --exclude='*.generated.cs' \
    --exclude-dir=obj \
    binding/ source/ 2>/dev/null | head -5 || echo "None found"
echo "### Total string-literal exceptions (ArgumentNull + ArgumentOutOfRange)"
{ grep -rn 'ArgumentNullException\s*("' --include='*.cs' --exclude='*.generated.cs' --exclude-dir=obj binding/ source/ 2>/dev/null | grep -v 'nameof'; \
  grep -rn 'ArgumentOutOfRangeException\s*("' --include='*.cs' --exclude='*.generated.cs' --exclude-dir=obj binding/ source/ 2>/dev/null | grep -v 'nameof'; } | wc -l || true
