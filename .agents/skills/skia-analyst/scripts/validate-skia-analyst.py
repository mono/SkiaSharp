#!/usr/bin/env python3
"""Validate a Skia Analyst JSON report against skia-analyst-schema.json.

Usage: python3 validate-skia-analyst.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from pathlib import Path
from collections import Counter

if len(sys.argv) != 2:
    print("Usage: python3 validate-skia-analyst.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).resolve().parent / "../references/skia-analyst-schema.json"
if not schema_path.exists():
    print(f"❌ Schema not found: {schema_path}")
    sys.exit(2)

try:
    with open(path) as f:
        data = json.load(f)
except json.JSONDecodeError as e:
    print(f"❌ Invalid JSON: {e}")
    sys.exit(2)

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

# --- Extra checks ---
meta = data.get("meta", {})
summary = data.get("summary", {})
findings = data.get("findings", [])

actual_total = len(findings)
if summary.get("totalFindings", 0) != actual_total:
    errors.append(f"summary.totalFindings ({summary.get('totalFindings')}) != actual ({actual_total})")

# Check summary counts
for field, getter in [
    ("byChangeType", lambda f: f.get("changeType")),
    ("byImportance", lambda f: f.get("importance")),
    ("byBindingStatus", lambda f: f.get("bindingStatus")),
    ("byImpact", lambda f: f.get("impact")),
    ("byPriority", lambda f: f.get("priority")),
    ("bySource", lambda f: f.get("source")),
]:
    expected = summary.get(field, {})
    actual = Counter(getter(f) for f in findings)
    for k, v in expected.items():
        if actual.get(k, 0) != v:
            warnings.append(f"summary.{field}.{k} ({v}) != actual ({actual.get(k, 0)})")

# Unique names
names = [f.get("name") for f in findings]
dupes = [n for n in set(names) if names.count(n) > 1]
if dupes:
    warnings.append(f"Duplicate names: {dupes[:5]}")

# --- Output ---
if warnings:
    for w in warnings:
        print(f"  ⚠️  {w}")

if errors:
    print(f"\n❌ {len(errors)} validation error(s) in {path.name}:\n")
    for e in errors:
        print(f"  ❌ {e}")
    sys.exit(1)

ms = meta.get("currentMilestone", "?")
mode = meta.get("scanMode", "?")
impact = Counter(f.get("impact") for f in findings)
missing = sum(1 for f in findings if f.get("bindingStatus") == "missing")
print(f"✅ {path.name} is valid (m{ms}, {mode}, {actual_total} findings, {missing} missing, "
      f"{impact.get('transformative', 0)} transformative)")
sys.exit(0)
