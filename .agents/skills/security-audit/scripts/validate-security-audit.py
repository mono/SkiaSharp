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
    errors.append(f"summary.needsAttention ({summary.get('needsAttention')}) != findings count ({status_counts['needs_attention']}) — fix the count")
if summary.get("undiscovered", 0) != status_counts["undiscovered"]:
    errors.append(f"summary.undiscovered ({summary.get('undiscovered')}) != findings count ({status_counts['undiscovered']}) — fix the count")
if summary.get("falsePositive", 0) != status_counts["false_positive"]:
    errors.append(f"summary.falsePositive ({summary.get('falsePositive')}) != findings count ({status_counts['false_positive']}) — fix the count")
if summary.get("clean", 0) != status_counts["clean"]:
    errors.append(f"summary.clean ({summary.get('clean')}) != findings count ({status_counts['clean']}) — fix the count")

# 2. totalCves should match sum of all CVEs across findings
total_cves = sum(len(f.get("cves", [])) + len(f.get("nonChromeCves", [])) for f in findings)
if summary.get("totalCves", 0) != total_cves:
    warnings.append(f"summary.totalCves ({summary.get('totalCves')}) != actual CVE count ({total_cves})")

# 3. CG alerts validation
if not cg:
    warnings.append("cgAlerts missing — Step 6 is mandatory for a complete security audit")
elif cg:
    total_alerts = cg.get("totalAlerts", 0)
    all_alerts = cg.get("alerts", [])

    if "alerts" not in cg:
        errors.append("cgAlerts missing 'alerts' key — include the raw query-cg-alerts.py output")

    # Count must match totalAlerts
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

# 4. Findings must have unique dependencies — ONE finding per dependency, all CVEs inside it
deps = [f.get("dependency") for f in findings]
dupes = [d for d in set(deps) if deps.count(d) > 1]
if dupes:
    errors.append(
        f"Duplicate finding dependencies: {dupes} — each dependency MUST have exactly ONE "
        "finding object. Put all CVEs (affected, already_fixed, false_positive) in a single "
        "finding's cves[] array. Use each CVE's 'assessment' field to distinguish status."
    )

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
skia_milestone = meta.get("skiaMilestone")
if skia_milestone is None:
    errors.append("meta.skiaMilestone is required")
elif not isinstance(skia_milestone, int) or skia_milestone <= 0:
    errors.append(f"meta.skiaMilestone must be an integer > 0 (got {skia_milestone!r})")
skia_sha = meta.get("skiaSubmoduleCommit")
if not skia_sha:
    errors.append("meta.skiaSubmoduleCommit is required")
elif not (isinstance(skia_sha, str) and len(skia_sha) >= 7 and all(c in "0123456789abcdefABCDEF" for c in skia_sha)):
    errors.append(f"meta.skiaSubmoduleCommit must look like a git SHA (got {skia_sha!r})")

# 8. versionVerification should have entries
if not versions:
    warnings.append("versionVerification is empty — should list verified dependencies")

# 9. Skia CVE completeness — every CVE in a skia finding must be fully resolved.
#    Pattern: each essential field is either a concrete value OR has an accompanying
#    *Note field explaining why it's missing. Silent nulls are forbidden.
def _has_note(cve, key):
    v = cve.get(key)
    return isinstance(v, str) and v.strip() != ""

for f in findings:
    if f.get("dependency") != "skia":
        continue
    if f.get("status") == "clean":
        continue
    for j, cve in enumerate(f.get("cves", [])):
        cid = cve.get("id", f"cves[{j}]")
        loc = f"finding 'skia' / {cid}"

        # Always-required basics
        if not cve.get("description"):
            errors.append(f"{loc}: missing 'description'")
        if not cve.get("source"):
            errors.append(f"{loc}: missing 'source'")
        if not cve.get("assessment"):
            errors.append(f"{loc}: missing 'assessment'")
        sev = cve.get("severity")
        if sev not in ("CRITICAL", "HIGH", "MEDIUM", "LOW"):
            errors.append(f"{loc}: severity must be CRITICAL/HIGH/MEDIUM/LOW (got {sev!r})")

        # Value OR Note pairs (errors)
        if cve.get("bugId") in (None, "") and not _has_note(cve, "bugIdNote"):
            errors.append(f"{loc}: bugId is null but bugIdNote is missing — explain why no bug ID was found")
        if cve.get("fixCommit") in (None, "") and not _has_note(cve, "fixCommitNote"):
            errors.append(f"{loc}: fixCommit is null but fixCommitNote is missing — explain why no fix commit was located")

        # Value OR Note pair (warning — CVSS often pending)
        if cve.get("cvss") is None and not _has_note(cve, "severityNote"):
            warnings.append(f"{loc}: cvss is null but severityNote is missing — explain the severity source")

        # bugUrl can only be null if bugId is null
        if cve.get("bugId") and not cve.get("bugUrl"):
            warnings.append(f"{loc}: bugId is set but bugUrl is missing — derive from issues.chromium.org/issues/{cve.get('bugId')}")

        # Assessment == affected requires complete resolution
        if cve.get("assessment") == "affected":
            if cve.get("cherryPicksCleanly") is None and not _has_note(cve, "cherryPickNote"):
                errors.append(f"{loc}: assessment='affected' requires cherryPicksCleanly or cherryPickNote")
            if cve.get("reachability") is None and not _has_note(cve, "reachabilityNote"):
                errors.append(f"{loc}: assessment='affected' requires reachability or reachabilityNote")
            if cve.get("onUpstreamMilestone") is None and not _has_note(cve, "branchNote"):
                errors.append(f"{loc}: assessment='affected' requires onUpstreamMilestone or branchNote")
            if cve.get("inOurTree") is None:
                errors.append(f"{loc}: assessment='affected' requires inOurTree (boolean, never null)")

# 10. Minimum CVE count check — the NVD "Skia" query typically returns 15+ recent CVEs.
#     If the report has significantly fewer, the agent may have dropped "already fixed" ones.
skia_cve_count = 0
for f in findings:
    if f.get("dependency") == "skia":
        skia_cve_count += len(f.get("cves", []))
        skia_cve_count += len(f.get("nonChromeCves", []))

if skia_cve_count < 10:
    warnings.append(
        f"Only {skia_cve_count} Skia CVEs in report — NVD typically returns 15+ recent CVEs. "
        "Verify that 'already_fixed' and 'false_positive' CVEs were not silently dropped."
    )

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
