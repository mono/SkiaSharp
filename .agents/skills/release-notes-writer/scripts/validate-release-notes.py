#!/usr/bin/env python3
"""Validate a Release Notes JSON report against release-notes-schema.json.

Usage: python3 validate-release-notes.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-release-notes.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).parent / "../references/release-notes-schema.json"
if not schema_path.exists():
    print(f"❌ Schema not found: {schema_path}")
    sys.exit(2)

with open(path) as f:
    data = json.load(f)
with open(schema_path) as f:
    schema = json.load(f)

errors = []
warnings = []

# --- JSON Schema validation ---
try:
    from jsonschema import Draft202012Validator
    validator = Draft202012Validator(schema)
    for e in validator.iter_errors(data):
        p = ".".join(str(x) for x in e.absolute_path) or "(root)"
        errors.append(f"Schema: {p}: {e.message}")
except ImportError:
    print("⚠️  jsonschema not installed — run: pip install jsonschema")
    print("   Falling back to structural checks only")

# --- Extra checks beyond schema ---

meta = data.get("meta", {})
summary = data.get("summary", {})
findings = data.get("findings", [])

# Check summary.totalFindings matches actual
actual_total = len(findings)
if summary.get("totalFindings", 0) != actual_total:
    errors.append(
        f"summary.totalFindings ({summary.get('totalFindings')}) != actual findings count ({actual_total})"
    )

# Check byChangeType counts
by_ct = summary.get("byChangeType", {})
for ct_val, expected_count in by_ct.items():
    actual = sum(1 for f in findings if f.get("changeType") == ct_val)
    if actual != expected_count:
        warnings.append(f"summary.byChangeType.{ct_val} ({expected_count}) != actual ({actual})")

# Check byImportance counts
by_imp = summary.get("byImportance", {})
for imp_val, expected_count in by_imp.items():
    actual = sum(1 for f in findings if f.get("importance") == imp_val)
    if actual != expected_count:
        warnings.append(f"summary.byImportance.{imp_val} ({expected_count}) != actual ({actual})")

# Check byLabel counts
by_label = summary.get("byLabel", {})
for label_val, expected_count in by_label.items():
    actual = sum(1 for f in findings if label_val in (f.get("labels") or []))
    if actual != expected_count:
        warnings.append(f"summary.byLabel.{label_val} ({expected_count}) != actual ({actual})")

# Ensure names are unique
names = [f.get("name") for f in findings]
dupes = [n for n in set(names) if names.count(n) > 1]
if dupes:
    warnings.append(f"Duplicate finding names: {dupes[:5]}")

# Every finding should reference a PR or commit
unlinked = [f.get("name") for f in findings if not f.get("pr") and not f.get("commit")]
if unlinked:
    warnings.append(f"{len(unlinked)} finding(s) without PR or commit reference: {unlinked[:3]}")

# Breaking changes should have migration guides
breaking_no_guide = [
    f.get("name") for f in findings
    if f.get("importance") == "breaking" and not f.get("migrationGuide")
]
if breaking_no_guide:
    warnings.append(f"Breaking change(s) without migrationGuide: {breaking_no_guide[:3]}")

# Slides and changelog should be non-empty
if not data.get("slides", "").strip():
    errors.append("slides field is empty")
if not data.get("changelog", "").strip():
    errors.append("changelog field is empty")

# Ref sanity
if meta.get("refFrom") == meta.get("refTo"):
    errors.append("refFrom and refTo are identical")

# --- Output ---
if warnings:
    for w in warnings:
        print(f"  ⚠️  {w}")

if errors:
    print(f"\n❌ {len(errors)} validation error(s) in {path.name}:\n")
    for e in errors:
        print(f"  ❌ {e}")
    sys.exit(1)

ref_from = meta.get("refFrom", "?")
ref_to = meta.get("refTo", "?")
commit_count = meta.get("commitCount", "?")
print(
    f"✅ {path.name} is valid ({ref_from}..{ref_to}, {commit_count} commits, {len(findings)} findings)"
)
sys.exit(0)
