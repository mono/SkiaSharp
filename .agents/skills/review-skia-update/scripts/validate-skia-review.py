#!/usr/bin/env python3
"""Validate a skia-review JSON file against skia-review-schema.json.

Exit codes: 0=valid, 1=fixable, 2=fatal.
Requires: pip install jsonschema
"""
import json
import re
import sys
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 validate-skia-review.py <path-to-json>")
        sys.exit(2)

    path = Path(sys.argv[1])
    if not path.exists():
        print(f"❌ File not found: {path}")
        sys.exit(2)

    schema_path = Path(__file__).parent.parent / "references" / "skia-review-schema.json"
    if not schema_path.exists():
        print(f"❌ Schema not found: {schema_path}")
        sys.exit(2)
    try:
        import jsonschema
    except ImportError:
        print("❌ jsonschema not installed. Run: pip install jsonschema")
        sys.exit(2)

    with open(path) as f:
        data = json.load(f)
    with open(schema_path) as f:
        schema = json.load(f)

    errors = []
    validator = jsonschema.Draft202012Validator(schema)
    for error in sorted(validator.iter_errors(data), key=lambda e: list(map(str, e.path))):
        field = ".".join(str(p) for p in error.absolute_path) or "(root)"
        errors.append(f"Schema: {field}: {error.message}")

    if not isinstance(data, dict):
        print(f"❌ Schema: root must be an object, got {type(data).__name__}")
        sys.exit(1)

    if not isinstance(data.get("summary"), str) or len(data["summary"]) < 50:
        errors.append("Top-level summary is too short (need 50+ chars)")
    if not isinstance(data.get("recommendations"), list) or not data["recommendations"]:
        errors.append("Top-level recommendations array is empty")

    for section in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr"):
        value = data.get(section)
        if not isinstance(value, dict):
            continue
        if not value.get("summary"):
            errors.append(f"{section}.summary is missing or empty")
        recs = value.get("recommendations")
        if recs is None:
            errors.append(f"{section}.recommendations is missing")
        elif isinstance(recs, list) and not recs:
            errors.append(f"{section}.recommendations is empty")

    for section in ("upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr"):
        value = data.get(section)
        if isinstance(value, dict) and value.get("status") == "REVIEW_REQUIRED":
            categories = ("added", "changed") if section == "companionPr" else ("added", "removed", "changed")
            if not any(value.get(cat) for cat in categories):
                errors.append(f"{section}.status is REVIEW_REQUIRED but {'/'.join(categories)} all empty")

    for section in ("upstreamIntegrity", "interopIntegrity", "companionPr"):
        value = data.get(section)
        if not isinstance(value, dict):
            continue
        for cat in ("added", "removed", "changed"):
            if cat == "removed" and section == "companionPr":
                continue
            for item in value.get(cat, []) if isinstance(value.get(cat, []), list) else []:
                if not isinstance(item, dict):
                    continue
                if not item.get("path"):
                    errors.append(f"{section}.{cat} item missing 'path'")
                if not item.get("summary"):
                    errors.append(f"{section}.{cat} item '{item.get('path','')}' missing 'summary'")
                for diff_field in ("diff", "oldDiff", "newDiff", "patchDiff"):
                    text = item.get(diff_field)
                    if isinstance(text, str) and text.startswith("see "):
                        errors.append(f"{section}.{cat} '{item.get('path','')}' {diff_field} contains file reference instead of actual diff content")

    deps = data.get("depsAudit")
    if isinstance(deps, dict):
        for cat in ("added", "removed", "changed"):
            for item in deps.get(cat, []) if isinstance(deps.get(cat, []), list) else []:
                if isinstance(item, dict):
                    if not item.get("name"):
                        errors.append(f"depsAudit.{cat} item missing 'name'")
                    if not item.get("summary"):
                        errors.append(f"depsAudit.{cat} item '{item.get('name','')}' missing 'summary'")

    def status(section):
        value = data.get(section)
        return value.get("status") if isinstance(value, dict) else None

    high = status("generatedFiles") == "FAIL" or status("upstreamIntegrity") == "REVIEW_REQUIRED"
    if high and data.get("riskAssessment") != "HIGH":
        errors.append("riskAssessment should be HIGH")
    medium = any(status(s) == "REVIEW_REQUIRED" for s in ("depsAudit", "interopIntegrity", "companionPr"))
    if not high and medium and data.get("riskAssessment") == "LOW":
        errors.append("riskAssessment should be MEDIUM or HIGH, not LOW")
    if all(status(s) == "PASS" for s in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr")) and data.get("riskAssessment") != "LOW":
        errors.append("riskAssessment should be LOW when all sections are PASS")

    meta = data.get("meta")
    shas = meta.get("shas") if isinstance(meta, dict) else None
    if isinstance(shas, dict):
        for field in ("prHead", "base", "upstream"):
            sha = shas.get(field)
            if isinstance(sha, str) and sha and not re.fullmatch(r"[0-9a-f]{40}", sha):
                errors.append(f"meta.shas.{field} is not a valid 40-char hex SHA: {sha}")
    json_text = json.dumps(data)
    for match in re.findall(r'"[^"]*(?:/Users/|/home/|C:\\Users\\)[^"]*"', json_text):
        errors.append(f"Absolute path found: {match}")

    if not errors:
        print(f"✅ {path.name} is valid (PR #{meta.get('skiaPrNumber', '?')}, risk: {data.get('riskAssessment', '?')})")
        sys.exit(0)
    print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
    for error in errors:
        print(f"  {error}")
    sys.exit(1)


if __name__ == "__main__":
    main()
