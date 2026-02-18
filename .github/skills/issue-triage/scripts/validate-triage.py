#!/usr/bin/env python3
"""Validate an ai-triage JSON file against triage-schema.json.

Usage: python3 validate-triage.py ai-triage/2794.json
Exits: 0=valid, 1=fixable (retry), 2=fatal.
"""
import json, re, sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-triage.py <triage.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).parent / "../references/triage-schema.json"
if not schema_path.exists():
    print(f"❌ Schema not found: {schema_path}")
    sys.exit(2)

with open(path) as f:
    data = json.load(f)
with open(schema_path) as f:
    schema = json.load(f)

errors = []

# --- Schema validation ---
try:
    from jsonschema import Draft202012Validator
    validator = Draft202012Validator(schema)
    for e in validator.iter_errors(data):
        p = ".".join(str(x) for x in e.absolute_path) or "(root)"
        errors.append(f"Schema: {p}: {e.message}")
except ImportError:
    print("⚠️  jsonschema not installed — run: pip install jsonschema")
    print("   Falling back to JSON-only checks (no schema validation)")

# --- Extra checks beyond schema ---

classification_type = data.get("classification", {}).get("type", {}).get("value", "")

# codeInvestigation is mandatory for bugs
ci = data.get("analysis", {}).get("codeInvestigation", [])
if classification_type == "type/bug" and len(ci) == 0:
    errors.append("Bug issue has no codeInvestigation entries (mandatory for type/bug)")

# bugSignals should exist for bugs (warning only)
if classification_type == "type/bug" and not data.get("evidence", {}).get("bugSignals"):
    print("⚠️  Warning: Bug issue has no evidence.bugSignals (recommended for bugs)")

# Absolute path check in codeInvestigation
abs_pattern = re.compile(r"(/Users/|/home/|C:\\Users\\)")
for entry in ci:
    if abs_pattern.search(entry.get("file", "")):
        errors.append(f"codeInvestigation file '{entry['file']}' contains absolute path")

if not errors:
    number = data.get("meta", {}).get("number", "?")
    ctype = classification_type or "?"
    print(f"✅ {path.name} is valid (issue #{number}, type: {ctype})")
    sys.exit(0)

print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
for e in errors:
    print(f"  {e}")
sys.exit(1)
