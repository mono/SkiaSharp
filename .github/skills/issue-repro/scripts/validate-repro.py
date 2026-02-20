#!/usr/bin/env python3
"""Validate an ai-repro JSON file against repro-schema.json.

Usage: python3 validate-repro.py ai-repro/2997.json
Exits: 0=valid, 1=fixable (retry), 2=fatal.
"""
import json, re, sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-repro.py <repro.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).parent / "../references/repro-schema.json"
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
abs_pattern = re.compile(r"(/Users/|/home/|C:\\Users\\)")
steps = data.get("reproductionSteps", [])

# Step numbering
for i, s in enumerate(steps):
    if s.get("stepNumber") != i + 1:
        errors.append(f"Step [{i}] has stepNumber {s.get('stepNumber')}, expected {i + 1}")

for s in steps:
    n = s.get("stepNumber", "?")
    output = s.get("output", "")
    command = s.get("command", "")
    result = s.get("result", "")
    exit_code = s.get("exitCode")

    if output and len(output) > 4096:
        errors.append(f"Step {n} output is {len(output)} chars (max 4096)")
    if output and abs_pattern.search(output):
        errors.append(f"Step {n} output contains absolute path — redact usernames")
    if command and abs_pattern.search(command):
        errors.append(f"Step {n} command contains absolute path — redact usernames")

    # exitCode consistency
    if command and result and result != "skip" and exit_code is not None:
        if exit_code == 0 and result == "failure":
            errors.append(f"Step {n} has exitCode 0 but result 'failure'")
        if exit_code > 0 and result == "success":
            errors.append(f"Step {n} has exitCode {exit_code} but result 'success'")

# Stack trace length
st = data.get("errorMessages", {}).get("stackTrace", "")
if st and len(st) > 5000:
    errors.append(f"stackTrace is {len(st)} chars (max 5000)")

# Conclusion ↔ step-result consistency
conclusion = data.get("conclusion", "")
results = [s.get("result") for s in steps if s.get("result")]

if conclusion == "reproduced":
    if "failure" not in results and "wrong-output" not in results:
        errors.append("Conclusion is 'reproduced' but no step has result 'failure' or 'wrong-output'")
elif conclusion == "not-reproduced":
    if "success" not in results:
        errors.append("Conclusion is 'not-reproduced' but no step has result 'success'")
    if "failure" in results or "wrong-output" in results:
        errors.append("Conclusion is 'not-reproduced' but step(s) have 'failure'/'wrong-output'")
elif conclusion in ("needs-platform", "needs-hardware", "partial", "inconclusive"):
    blockers = data.get("blockers", [])
    if not blockers:
        errors.append(f"Conclusion is '{conclusion}' but blockers is missing or empty")

if not errors:
    number = data.get("meta", {}).get("number", "?")
    print(f"✅ {path.name} is valid (issue #{number}, conclusion: {conclusion})")
    sys.exit(0)

print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
for e in errors:
    print(f"  {e}")
sys.exit(1)
