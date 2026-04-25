#!/usr/bin/env python3
"""Validate a Skia Feature Scout JSON report against feature-scout-schema.json.

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
items = data.get("items", [])
hidden = data.get("hiddenApis", [])
deprecations = data.get("deprecations", [])
perf = data.get("performance", [])
next_steps = data.get("nextSteps", [])

# Check summary counts match actual items
actual_total = len(items)
if summary.get("totalItems", 0) != actual_total:
    errors.append(
        f"summary.totalItems ({summary.get('totalItems')}) != actual items count ({actual_total})"
    )

# Count binding statuses and verify
status_counts = {}
for item in items:
    bs = item.get("bindingStatus", "")
    status_counts[bs] = status_counts.get(bs, 0) + 1

for status_key, summary_key in [
    ("full", "full"),
    ("partial", "partial"),
    ("missing", "missing"),
    ("action_needed", "actionNeeded"),
    ("correctly_absent", "correctlyAbsent"),
    ("not_applicable", "notApplicable"),
]:
    actual = status_counts.get(status_key, 0)
    reported = summary.get(summary_key, 0)
    if actual != reported:
        warnings.append(
            f"summary.{summary_key} ({reported}) != actual {status_key} count ({actual})"
        )

# Count priorities and verify
pri_counts = {}
for item in items:
    p = item.get("priority", "")
    pri_counts[p] = pri_counts.get(p, 0) + 1

for pri in ["critical", "high", "medium", "low"]:
    actual = pri_counts.get(pri, 0)
    reported = summary.get(pri, 0)
    if actual != reported:
        warnings.append(
            f"summary.{pri} ({reported}) != actual {pri} priority count ({actual})"
        )

# Ensure IDs are unique and sequential
item_ids = [i.get("id") for i in items]
if len(item_ids) != len(set(item_ids)):
    errors.append("Duplicate item IDs found")

# Milestone sanity
current_ms = meta.get("currentMilestone", 0)
latest_ms = meta.get("latestUpstreamMilestone", 0)
if current_ms > latest_ms:
    errors.append(
        f"currentMilestone ({current_ms}) > latestUpstreamMilestone ({latest_ms})"
    )

# Items with bindingStatus=full should have cApiStatus=present and csharpStatus=present
for item in items:
    if item.get("bindingStatus") == "full":
        if item.get("cApiStatus") != "present":
            warnings.append(
                f"Item {item.get('id')} '{item.get('name')}': bindingStatus=full but cApiStatus != present"
            )
        if item.get("csharpStatus") != "present":
            warnings.append(
                f"Item {item.get('id')} '{item.get('name')}': bindingStatus=full but csharpStatus != present"
            )

# Items with bindingStatus=partial should have cApiStatus=present and csharpStatus=missing
for item in items:
    if item.get("bindingStatus") == "partial":
        if item.get("cApiStatus") != "present":
            warnings.append(
                f"Item {item.get('id')} '{item.get('name')}': bindingStatus=partial but cApiStatus != present"
            )

# High/critical missing items should appear in nextSteps
high_missing = {
    i.get("name")
    for i in items
    if i.get("priority") in ("high", "critical")
    and i.get("bindingStatus") in ("missing", "action_needed")
}
next_step_actions = " ".join(s.get("action", "") for s in next_steps)
for name in high_missing:
    # Loose check: any word from the name appears in any next step
    words = name.split()[:3]
    if not any(w.lower() in next_step_actions.lower() for w in words if len(w) > 3):
        warnings.append(
            f"High/critical missing item '{name}' has no matching nextStep"
        )

# Check hidden API counts
if summary.get("hiddenApiCount", 0) != len(hidden):
    warnings.append(
        f"summary.hiddenApiCount ({summary.get('hiddenApiCount', 0)}) != actual hiddenApis count ({len(hidden)})"
    )

# Check performance counts
if summary.get("performanceCount", 0) != len(perf):
    warnings.append(
        f"summary.performanceCount ({summary.get('performanceCount', 0)}) != actual performance count ({len(perf)})"
    )

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
    f"✅ {path.name} is valid ({mode} scan, {ms_info}, "
    f"{len(items)} items, {len(hidden)} hidden, {len(perf)} perf)"
)
sys.exit(0)
