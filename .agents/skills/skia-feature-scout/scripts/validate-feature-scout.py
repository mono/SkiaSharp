#!/usr/bin/env python3
"""Validate a Skia Feature Scout JSON report (v3) against feature-scout-schema.json.

Usage: python3 validate-feature-scout.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-feature-scout.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).parent / "../references/feature-scout-schema.json"
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
next_steps = data.get("nextSteps", [])

# Check summary.totalFindings matches actual
actual_total = len(findings)
if summary.get("totalFindings", 0) != actual_total:
    errors.append(
        f"summary.totalFindings ({summary.get('totalFindings')}) != actual findings count ({actual_total})"
    )

# Check byStatus counts
by_status = summary.get("byStatus", {})
for status_val, expected_count in by_status.items():
    actual = sum(1 for f in findings if f.get("bindingStatus") == status_val)
    if actual != expected_count:
        warnings.append(f"summary.byStatus.{status_val} ({expected_count}) != actual ({actual})")

# Check byPriority counts
by_priority = summary.get("byPriority", {})
for pri_val, expected_count in by_priority.items():
    actual = sum(1 for f in findings if f.get("priority") == pri_val)
    if actual != expected_count:
        warnings.append(f"summary.byPriority.{pri_val} ({expected_count}) != actual ({actual})")

# Check bySource counts
by_source = summary.get("bySource", {})
for src_val, expected_count in by_source.items():
    actual = sum(1 for f in findings if f.get("source") == src_val)
    if actual != expected_count:
        warnings.append(f"summary.bySource.{src_val} ({expected_count}) != actual ({actual})")

# Ensure names are unique
names = [f.get("name") for f in findings]
dupes = [n for n in set(names) if names.count(n) > 1]
if dupes:
    warnings.append(f"Duplicate finding names: {dupes[:5]}")

# Milestone sanity
current_ms = meta.get("currentMilestone", 0)
latest_ms = meta.get("latestUpstreamMilestone", 0)
if current_ms > latest_ms:
    errors.append(f"currentMilestone ({current_ms}) > latestUpstreamMilestone ({latest_ms})")

# Binding status consistency
for f in findings:
    name = f.get("name", "?")
    bs = f.get("bindingStatus")
    if bs == "full" and not f.get("cApiFunction") and not f.get("csharpMethod"):
        warnings.append(f"'{name}': bindingStatus=full but no cApiFunction or csharpMethod")
    if bs == "partial" and not f.get("cApiFunction"):
        warnings.append(f"'{name}': bindingStatus=partial but no cApiFunction")

# milestones should be int arrays
for f in findings:
    ms = f.get("milestones")
    if ms is not None and not isinstance(ms, list):
        errors.append(f"'{f.get('name')}': milestones should be array, got {type(ms).__name__}")
    if ms and not all(isinstance(m, int) for m in ms):
        errors.append(f"'{f.get('name')}': milestones array contains non-integers")

# --- Output ---
if warnings:
    for w in warnings:
        print(f"  ⚠️  {w}")

if errors:
    print(f"\n❌ {len(errors)} validation error(s) in {path.name}:\n")
    for e in errors:
        print(f"  ❌ {e}")
    sys.exit(1)

mode = meta.get("mode", "?")
ms_info = f"m{meta.get('currentMilestone', '?')}"
if mode == "windowed":
    ms_info = f"m{meta.get('previousMilestone', '?')}→{ms_info}"
print(
    f"✅ {path.name} is valid ({mode} scan, {ms_info}, {len(findings)} findings)"
)
sys.exit(0)
