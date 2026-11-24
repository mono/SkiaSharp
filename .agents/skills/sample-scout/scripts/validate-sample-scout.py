#!/usr/bin/env python3
"""Validate a Sample Scout JSON report against sample-scout-schema.json.

Usage: python3 validate-sample-scout.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from collections import Counter
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-sample-scout.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).resolve().parent / "../references/sample-scout-schema.json"
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
    print("   Falling back to structural checks only")

# --- Extra checks beyond schema ---

meta = data.get("meta", {})
summary = data.get("summary", {})
findings = data.get("findings", [])

# Check totalFindings matches actual
actual_total = len(findings)
if summary.get("totalFindings", 0) != actual_total:
    errors.append(
        f"summary.totalFindings ({summary.get('totalFindings')}) != actual findings count ({actual_total})"
    )

# Check byInterest counts
by_interest = summary.get("byInterest", {})
actual_interest = Counter(f.get("interesting") for f in findings)
for k, v in by_interest.items():
    if actual_interest.get(k, 0) != v:
        warnings.append(f"summary.byInterest.{k} ({v}) != actual ({actual_interest.get(k, 0)})")

# Check byApiAvailability counts
by_api = summary.get("byApiAvailability", {})
actual_avail = sum(1 for f in findings if f.get("apis_available"))
actual_blocked = sum(1 for f in findings if not f.get("apis_available"))
if by_api.get("available", 0) != actual_avail:
    warnings.append(f"summary.byApiAvailability.available ({by_api.get('available')}) != actual ({actual_avail})")
if by_api.get("blocked", 0) != actual_blocked:
    warnings.append(f"summary.byApiAvailability.blocked ({by_api.get('blocked')}) != actual ({actual_blocked})")

# Check bySampleStatus counts
by_ss = summary.get("bySampleStatus", {})
actual_ss = Counter(f.get("sampleStatus") for f in findings)
for k, v in by_ss.items():
    if actual_ss.get(k, 0) != v:
        warnings.append(f"summary.bySampleStatus.{k} ({v}) != actual ({actual_ss.get(k, 0)})")

# Ensure names are unique-ish (file should be unique)
files = [f.get("file") for f in findings]
dupes = [n for n in set(files) if files.count(n) > 1]
if dupes:
    warnings.append(f"Duplicate file entries: {dupes[:5]}")

# Check opportunities count
opp_actual = sum(
    1 for f in findings
    if f.get("interesting") == "high"
    and f.get("apis_available") == True
    and f.get("sampleStatus") == "none"
)
if summary.get("opportunities") is not None and summary["opportunities"] != opp_actual:
    warnings.append(f"summary.opportunities ({summary['opportunities']}) != actual ({opp_actual})")

# --- Output ---
if warnings:
    for w in warnings:
        print(f"  ⚠️  {w}")

if errors:
    print(f"\n❌ {len(errors)} validation error(s) in {path.name}:\n")
    for e in errors:
        print(f"  ❌ {e}")
    sys.exit(1)

total = meta.get("totalFiles", "?")
high = actual_interest.get("high", 0)
none_count = actual_ss.get("none", 0)
print(
    f"✅ {path.name} is valid ({actual_total} findings, {high} high interest, "
    f"{none_count} new opportunities, {opp_actual} actionable)"
)
sys.exit(0)
