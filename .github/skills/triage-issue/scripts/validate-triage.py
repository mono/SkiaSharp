#!/usr/bin/env python3
"""Validate an ai-triage JSON file against the triage JSON Schema.

Usage:
    validate-triage.py <path-to-triage-file.json>
    validate-triage.py ai-triage/2794.json

Validates against references/triage-schema.json using jsonschema (draft 2020-12).
Exits 0 if valid, 1 if errors found. Prints all errors to stdout.

Requires: pip install jsonschema
"""

import json
import sys
from pathlib import Path

try:
    from jsonschema import Draft202012Validator, FormatChecker, ValidationError
except ImportError:
    print("❌ Missing dependency: pip install jsonschema")
    sys.exit(2)

SCRIPT_DIR = Path(__file__).parent
SCHEMA_FILE = SCRIPT_DIR / "../references/triage-schema.json"


def format_error(error: ValidationError) -> str:
    """Format a validation error into a readable message."""
    path = "/".join(str(p) for p in error.absolute_path)
    location = path if path else "(root)"

    # For oneOf failures, show the most specific sub-error
    if error.context:
        # Find the most specific (deepest path) sub-error
        best = max(error.context, key=lambda e: len(list(e.absolute_path)))
        sub_path = "/".join(str(p) for p in best.absolute_path)
        if sub_path:
            return f"  {sub_path}: {best.message}"
        return f"  {location}: {best.message}"

    return f"  {location}: {error.message}"


def validate(schema: dict, data: dict) -> list[str]:
    """Validate data against schema, return list of error messages."""
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(data), key=lambda e: list(e.absolute_path))

    messages = []
    for error in errors:
        messages.append(format_error(error))

    return messages


def main():
    if len(sys.argv) != 2:
        print(f"Usage: {sys.argv[0]} <triage-file.json>")
        sys.exit(2)

    path = Path(sys.argv[1])
    if not path.exists():
        print(f"❌ File not found: {path}")
        sys.exit(2)

    if not SCHEMA_FILE.exists():
        print(f"❌ Schema not found: {SCHEMA_FILE}")
        sys.exit(2)

    try:
        data = json.loads(path.read_text())
    except json.JSONDecodeError as e:
        print(f"❌ Invalid JSON: {e}")
        sys.exit(1)

    schema = json.loads(SCHEMA_FILE.read_text())
    errors = validate(schema, data)

    if errors:
        print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
        for e in errors:
            print(e)
        sys.exit(1)
    else:
        print(f"✅ {path.name} is valid (issue #{data.get('number', '?')})")
        sys.exit(0)


if __name__ == "__main__":
    main()
