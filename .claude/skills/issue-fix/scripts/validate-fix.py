#!/usr/bin/env python3
"""Validate an ai-fix JSON file against fix-schema.json.

Usage: python3 validate-fix.py ai-fix/2997.json
Exits: 0=valid, 1=fixable (retry), 2=fatal.
"""
import json, re, sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-fix.py <fix.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).parent / "../references/fix-schema.json"
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
status = data.get("status", {}).get("value", "")

# If status is 'fixed', verification should not be failed
if status == "fixed":
    if data.get("verification", {}).get("reproScenario") == "failed":
        errors.append("Status is 'fixed' but verification.reproScenario is 'failed'")
    if data.get("tests", {}).get("result") == "failed":
        errors.append("Status is 'fixed' but tests.result is 'failed'")

# If regressionTestAdded, testsAdded should have entries
tests = data.get("tests", {})
if tests.get("regressionTestAdded") and not tests.get("testsAdded"):
    errors.append("regressionTestAdded is true but testsAdded is missing or empty")

# changes.files should have entries when status is 'fixed'
files = data.get("changes", {}).get("files", [])
if status == "fixed" and not files:
    errors.append("Status is 'fixed' but changes.files is empty")

# Absolute path check
abs_pattern = re.compile(r"(/Users/|/home/|C:\\Users\\)")
for f in files:
    if abs_pattern.search(f.get("path", "")):
        errors.append(f"File path '{f['path']}' contains absolute path")

if not errors:
    number = data.get("meta", {}).get("number", "?")
    print(f"✅ {path.name} is valid (issue #{number}, status: {status})")
    sys.exit(0)

print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
for e in errors:
    print(f"  {e}")
sys.exit(1)
