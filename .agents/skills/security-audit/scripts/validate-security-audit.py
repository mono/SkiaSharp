#!/usr/bin/env python3
"""Validate a security audit JSON report against the schema.

Usage: python3 validate-security-audit.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-security-audit.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

schema_path = Path(__file__).resolve().parent / "../references/security-audit-schema.json"
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
    warnings.append("jsonschema not installed — run: pip install jsonschema (schema check skipped)")

# --- Semantic checks ---
meta = data.get("meta", {})
summary = data.get("summary", {})
findings = data.get("findings", [])
cg = data.get("cgAlerts", {})
next_steps = data.get("nextSteps", [])
versions = data.get("versionVerification", [])

# 1. Summary counts must match findings
status_counts = {"needs_attention": 0, "undiscovered": 0, "false_positive": 0, "clean": 0, "in_progress": 0, "ready_to_merge": 0}
for f in findings:
    s = f.get("status", "")
    if s in status_counts:
        status_counts[s] += 1

if summary.get("needsAttention", 0) != status_counts["needs_attention"]:
    warnings.append(f"summary.needsAttention ({summary.get('needsAttention')}) != findings count ({status_counts['needs_attention']})")
if summary.get("undiscovered", 0) != status_counts["undiscovered"]:
    warnings.append(f"summary.undiscovered ({summary.get('undiscovered')}) != findings count ({status_counts['undiscovered']})")
if summary.get("falsePositive", 0) != status_counts["false_positive"]:
    warnings.append(f"summary.falsePositive ({summary.get('falsePositive')}) != findings count ({status_counts['false_positive']})")
if summary.get("clean", 0) != status_counts["clean"]:
    warnings.append(f"summary.clean ({summary.get('clean')}) != findings count ({status_counts['clean']})")

# 2. totalCves should match sum of all CVEs across findings
total_cves = sum(len(f.get("cves", [])) + len(f.get("nonChromeCves", [])) for f in findings)
if summary.get("totalCves", 0) != total_cves:
    warnings.append(f"summary.totalCves ({summary.get('totalCves')}) != actual CVE count ({total_cves})")

# 3. CG alerts validation
if cg:
    total_alerts = cg.get("totalAlerts", 0)
    all_alerts = cg.get("alerts", []) or cg.get("allAlerts", [])
    unique_cves = cg.get("uniqueCVEs", [])
    categories = cg.get("categories", [])

    # Prefer 'alerts' array — categories alone is insufficient
    if not all_alerts and not unique_cves:
        if categories:
            errors.append("cgAlerts has 'categories' but no 'alerts' array — include the raw script output")
        else:
            errors.append("cgAlerts has totalAlerts but no 'alerts' array to enumerate them")

    # If alerts present, count must match totalAlerts
    if all_alerts and len(all_alerts) != total_alerts:
        warnings.append(f"cgAlerts.totalAlerts ({total_alerts}) != len(alerts) ({len(all_alerts)})")

    # Every alert must have id, component, severity
    for i, a in enumerate(all_alerts):
        if not a.get("id"):
            errors.append(f"cgAlerts.alerts[{i}] missing 'id'")
        if not a.get("component"):
            errors.append(f"cgAlerts.alerts[{i}] missing 'component'")
        if not a.get("severity"):
            errors.append(f"cgAlerts.alerts[{i}] missing 'severity'")

    # bySeverity counts should match
    by_sev = cg.get("bySeverity", {})
    if all_alerts and by_sev:
        from collections import Counter
        actual_sev = Counter(a.get("severity") for a in all_alerts)
        for sev, count in by_sev.items():
            actual = actual_sev.get(sev, 0)
            if actual != count:
                warnings.append(f"cgAlerts.bySeverity.{sev} ({count}) != actual ({actual})")

    # Pipelines should be present
    if not cg.get("pipelines"):
        warnings.append("cgAlerts.pipelines is empty — should list scanned pipelines")

# 4. Findings must have unique dependencies
deps = [f.get("dependency") for f in findings]
dupes = [d for d in set(deps) if deps.count(d) > 1]
if dupes:
    warnings.append(f"Duplicate finding dependencies: {dupes}")

# 5. Every finding with status != clean/false_positive should have CVEs or action
for f in findings:
    if f.get("status") in ("needs_attention", "undiscovered", "in_progress"):
        if not f.get("cves") and not f.get("nonChromeCves") and not f.get("action"):
            warnings.append(f"Finding '{f.get('dependency')}' has status={f.get('status')} but no CVEs or action")

# 6. nextSteps should be sorted by priority
priorities = [s.get("priority", 999) for s in next_steps]
if priorities != sorted(priorities):
    warnings.append("nextSteps not sorted by priority")

# 7. Meta validation
if not meta.get("date"):
    errors.append("meta.date is required")
if not meta.get("skiaMilestone"):
    errors.append("meta.skiaMilestone is required")

# 8. versionVerification should have entries
if not versions:
    warnings.append("versionVerification is empty — should list verified dependencies")

# --- Output ---
if warnings:
    print(f"⚠️  {len(warnings)} warning(s):")
    for w in warnings:
        print(f"   ⚠️  {w}")

if errors:
    print(f"\n❌ {len(errors)} error(s):")
    for e in errors:
        print(f"   ❌ {e}")
    sys.exit(1)
else:
    total_findings = len(findings)
    cg_count = cg.get("totalAlerts", 0) if cg else 0
    print(f"✅ Valid security audit report")
    print(f"   m{meta.get('skiaMilestone', '?')} • {meta.get('date', '?')}")
    print(f"   {total_findings} findings • {summary.get('totalCves', 0)} CVEs • {cg_count} CG alerts")
    if warnings:
        print(f"   ({len(warnings)} warnings — review above)")
    sys.exit(0)
