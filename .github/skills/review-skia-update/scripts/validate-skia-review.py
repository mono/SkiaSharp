#!/usr/bin/env python3
"""Validate a skia-review JSON file against skia-review-schema.json.

Usage: python3 validate-skia-review.py /tmp/skiasharp/skia-review/20260320-164500/170.json
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

    # Schema validation
    validator = jsonschema.Draft202012Validator(schema)
    for error in sorted(validator.iter_errors(data), key=lambda e: list(e.path)):
        field = ".".join(str(p) for p in error.absolute_path) or "(root)"
        errors.append(f"Schema: {field}: {error.message}")

    # Top-level summary and recommendations
    if not data.get("summary") or len(data.get("summary", "")) < 50:
        errors.append("Top-level summary is too short (need 50+ chars)")
    if not data.get("recommendations") or len(data.get("recommendations", [])) == 0:
        errors.append("Top-level recommendations array is empty")

    # Per-section summary and recommendations
    for section in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr"):
        if not data.get(section, {}).get("summary"):
            errors.append(f"{section}.summary is missing or empty")
        recs = data.get(section, {}).get("recommendations")
        if recs is None:
            errors.append(f"{section}.recommendations is missing")
        elif isinstance(recs, list) and len(recs) == 0:
            errors.append(f"{section}.recommendations is empty")

    # REVIEW_REQUIRED must have items
    for section in ("upstreamIntegrity", "interopIntegrity", "depsAudit"):
        if data.get(section, {}).get("status") == "REVIEW_REQUIRED":
            has = bool(data[section].get("added")) or bool(data[section].get("removed")) or bool(data[section].get("changed"))
            if not has:
                errors.append(f"{section}.status is REVIEW_REQUIRED but added/removed/changed all empty")

    # companionPr REVIEW_REQUIRED must have items (no removed category)
    if data.get("companionPr", {}).get("status") == "REVIEW_REQUIRED":
        has = bool(data["companionPr"].get("added")) or bool(data["companionPr"].get("changed"))
        if not has:
            errors.append("companionPr.status is REVIEW_REQUIRED but added/changed both empty")

    # Source items must have path + summary; diff fields must be actual content
    for section in ("upstreamIntegrity", "interopIntegrity"):
        for cat in ("added", "removed", "changed"):
            for item in data.get(section, {}).get(cat, []):
                if not item.get("path"):
                    errors.append(f"{section}.{cat} item missing 'path'")
                if not item.get("summary"):
                    errors.append(f"{section}.{cat} item '{item.get('path','')}' missing 'summary'")
                for diff_field in ("diff", "oldDiff", "newDiff", "patchDiff"):
                    val = item.get(diff_field, "")
                    if val and val.startswith("see "):
                        errors.append(f"{section}.{cat} '{item.get('path','')}' {diff_field} contains file reference instead of actual diff content")

    # Companion PR file items must have path + summary
    for cat in ("added", "changed"):
        for item in data.get("companionPr", {}).get(cat, []):
            if not item.get("path"):
                errors.append(f"companionPr.{cat} item missing 'path'")
            if not item.get("summary"):
                errors.append(f"companionPr.{cat} item '{item.get('path','')}' missing 'summary'")
            for diff_field in ("diff", "oldDiff", "newDiff", "patchDiff"):
                val = item.get(diff_field, "")
                if val and val.startswith("see "):
                    errors.append(f"companionPr.{cat} '{item.get('path','')}' {diff_field} contains file reference instead of actual diff content")

    # Dep items must have name + summary
    for cat in ("added", "removed", "changed"):
        for item in data.get("depsAudit", {}).get(cat, []):
            if not item.get("name"):
                errors.append(f"depsAudit.{cat} item missing 'name'")
            if not item.get("summary"):
                errors.append(f"depsAudit.{cat} item '{item.get('name','')}' missing 'summary'")

    # Risk consistency
    gen_fail = data.get("generatedFiles", {}).get("status") == "FAIL"
    upstream_review = data.get("upstreamIntegrity", {}).get("status") == "REVIEW_REQUIRED"
    expected_high = gen_fail or upstream_review
    if expected_high and data.get("riskAssessment") != "HIGH":
        errors.append("riskAssessment should be HIGH")

    deps_review = data.get("depsAudit", {}).get("status") == "REVIEW_REQUIRED"
    interop_review = data.get("interopIntegrity", {}).get("status") == "REVIEW_REQUIRED"
    expected_medium = deps_review or interop_review
    if not expected_high and expected_medium and data.get("riskAssessment") == "LOW":
        errors.append("riskAssessment should be MEDIUM or HIGH, not LOW")

    # If all sections PASS, riskAssessment must be LOW
    all_pass = (data.get("generatedFiles", {}).get("status") == "PASS" and
                data.get("upstreamIntegrity", {}).get("status") == "PASS" and
                data.get("interopIntegrity", {}).get("status") == "PASS" and
                data.get("depsAudit", {}).get("status") == "PASS")
    if all_pass and data.get("riskAssessment") != "LOW":
        errors.append("riskAssessment should be LOW when all sections are PASS")

    # SHA format (40-char hex)
    for sha_field in ("prHead", "base", "upstream"):
        sha = data.get("meta", {}).get("shas", {}).get(sha_field, "")
        if sha and not re.fullmatch(r"[0-9a-f]{40}", sha):
            errors.append(f"meta.shas.{sha_field} is not a valid 40-char hex SHA: {sha}")

    # No absolute paths
    json_text = json.dumps(data)
    for m in re.findall(r'"[^"]*(?:/Users/|/home/|C:\\Users\\)[^"]*"', json_text):
        errors.append(f"Absolute path found: {m}")

    if not errors:
        pr = data.get("meta", {}).get("skiaPrNumber", "?")
        risk = data.get("riskAssessment", "?")
        print(f"✅ {path.name} is valid (PR #{pr}, risk: {risk})")
        sys.exit(0)

    print(f"❌ {len(errors)} validation error(s) in {path.name}:\n")
    for e in errors:
        print(f"  {e}")
    sys.exit(1)


if __name__ == "__main__":
    main()
