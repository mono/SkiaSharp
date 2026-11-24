#!/usr/bin/env python3
"""Validate a CI status report JSON against the schema.

Usage: python3 validate-ci-status.py <report.json>
Exits: 0=valid, 1=fixable errors (retry), 2=fatal.
"""
import json
import sys
from pathlib import Path

if len(sys.argv) != 2:
    print("Usage: python3 validate-ci-status.py <report.json>")
    sys.exit(2)

path = Path(sys.argv[1])
if not path.exists():
    print(f"❌ File not found: {path}")
    sys.exit(2)

try:
    with open(path) as f:
        data = json.load(f)
except json.JSONDecodeError as e:
    print(f"❌ Invalid JSON: {e}")
    sys.exit(2)

errors = []
warnings = []

# --- Required top-level keys ---
REQUIRED_KEYS = ["meta", "verdict", "azdoHealth", "chainAnalysis", "rootCauses",
                 "githubActions", "flakes", "releaseRisk", "recommendations"]

for key in REQUIRED_KEYS:
    if key not in data:
        errors.append(f"Missing required top-level key: '{key}'")

if errors:
    # Fatal — can't validate further without structure
    print(f"❌ {len(errors)} error(s):")
    for e in errors:
        print(f"   ❌ {e}")
    sys.exit(1)

# --- meta validation ---
meta = data["meta"]
if not meta.get("date"):
    errors.append("meta.date is required")
if not meta.get("timestamp"):
    errors.append("meta.timestamp is required")
if meta.get("schemaVersion") != "1.0":
    errors.append(f"meta.schemaVersion must be '1.0' (got {meta.get('schemaVersion')!r})")
if not meta.get("window"):
    errors.append("meta.window is required")
elif not isinstance(meta["window"].get("buildsPerPipeline"), int):
    errors.append("meta.window.buildsPerPipeline must be an integer")
if not meta.get("branches") or not isinstance(meta["branches"], list):
    errors.append("meta.branches must be a non-empty array of branch names")

# --- verdict validation ---
verdict = data["verdict"]
valid_statuses = ("healthy", "degraded", "broken")
if verdict.get("status") not in valid_statuses:
    errors.append(f"verdict.status must be one of {valid_statuses} (got {verdict.get('status')!r})")
if not verdict.get("summary"):
    errors.append("verdict.summary is required (1-2 sentence health summary)")
if verdict.get("emoji") not in ("🟢", "🟡", "🔴"):
    warnings.append(f"verdict.emoji should be 🟢/🟡/🔴 (got {verdict.get('emoji')!r})")

# --- azdoHealth validation ---
azdo = data["azdoHealth"]
if "branches" not in azdo or not isinstance(azdo["branches"], list):
    errors.append("azdoHealth.branches must be an array")
else:
    for i, branch in enumerate(azdo["branches"]):
        if not branch.get("name"):
            errors.append(f"azdoHealth.branches[{i}].name is required")
        if branch.get("risk") not in ("low", "medium", "high"):
            errors.append(f"azdoHealth.branches[{i}].risk must be low/medium/high (got {branch.get('risk')!r})")
        if "pipelines" not in branch or not isinstance(branch["pipelines"], list):
            errors.append(f"azdoHealth.branches[{i}].pipelines must be an array")

if "regressions" not in azdo:
    warnings.append("azdoHealth.regressions missing — should be an array (empty if none)")

# --- chainAnalysis validation ---
chain = data["chainAnalysis"]
if not isinstance(chain, list):
    errors.append("chainAnalysis must be an array")
else:
    for i, entry in enumerate(chain):
        if not entry.get("branch"):
            errors.append(f"chainAnalysis[{i}].branch is required")
        if entry.get("verdict") not in ("cascade", "independent", "mixed"):
            errors.append(f"chainAnalysis[{i}].verdict must be cascade/independent/mixed")
        if not entry.get("summary"):
            errors.append(f"chainAnalysis[{i}].summary is required")

# --- rootCauses validation ---
root_causes = data["rootCauses"]
if not isinstance(root_causes, list):
    errors.append("rootCauses must be an array")
else:
    valid_categories = ("code_regression", "flake", "infra_network", "quota_resource", "chain_blockage", "unknown")
    valid_severities = ("critical", "high", "medium", "low")
    ids_seen = set()
    for i, rc in enumerate(root_causes):
        if not rc.get("id"):
            errors.append(f"rootCauses[{i}].id is required")
        elif rc["id"] in ids_seen:
            errors.append(f"rootCauses[{i}].id '{rc['id']}' is duplicate")
        else:
            ids_seen.add(rc["id"])
        if not rc.get("title"):
            errors.append(f"rootCauses[{i}].title is required")
        if rc.get("category") not in valid_categories:
            errors.append(f"rootCauses[{i}].category must be one of {valid_categories}")
        if rc.get("severity") not in valid_severities:
            errors.append(f"rootCauses[{i}].severity must be one of {valid_severities}")
        if not rc.get("sampleError"):
            warnings.append(f"rootCauses[{i}].sampleError should include verbatim error text")

# --- githubActions validation ---
gha = data["githubActions"]
if "workflows" not in gha or not isinstance(gha["workflows"], list):
    errors.append("githubActions.workflows must be an array")
else:
    valid_gha_statuses = ("healthy", "failing", "degraded", "stale", "skipped")
    valid_severities_gha = ("high", "medium", "low")
    for i, wf in enumerate(gha["workflows"]):
        if not wf.get("name"):
            errors.append(f"githubActions.workflows[{i}].name is required")
        if wf.get("status") not in valid_gha_statuses:
            errors.append(f"githubActions.workflows[{i}].status must be one of {valid_gha_statuses}")
        if wf.get("severity") not in valid_severities_gha:
            errors.append(f"githubActions.workflows[{i}].severity must be one of {valid_severities_gha}")

if "summary" not in gha:
    errors.append("githubActions.summary is required")
else:
    summary = gha["summary"]
    for field in ("total", "healthy", "failing"):
        if not isinstance(summary.get(field), int):
            errors.append(f"githubActions.summary.{field} must be an integer")

# --- flakes validation ---
flakes = data["flakes"]
if not isinstance(flakes, list):
    errors.append("flakes must be an array")

# --- releaseRisk validation ---
release_risk = data["releaseRisk"]
if not isinstance(release_risk, list):
    errors.append("releaseRisk must be an array")
else:
    valid_recs = ("ship", "wait", "cherry-pick", "investigate")
    for i, rr in enumerate(release_risk):
        if not rr.get("branch"):
            errors.append(f"releaseRisk[{i}].branch is required")
        if not isinstance(rr.get("shippable"), bool):
            errors.append(f"releaseRisk[{i}].shippable must be a boolean")
        if rr.get("recommendation") not in valid_recs:
            errors.append(f"releaseRisk[{i}].recommendation must be one of {valid_recs}")

# --- recommendations validation ---
recs = data["recommendations"]
if not isinstance(recs, list):
    errors.append("recommendations must be an array")
elif len(recs) > 5:
    warnings.append(f"recommendations has {len(recs)} items — should be at most 5")
else:
    for i, rec in enumerate(recs):
        if not isinstance(rec.get("priority"), int):
            errors.append(f"recommendations[{i}].priority must be an integer")
        if not rec.get("action"):
            errors.append(f"recommendations[{i}].action is required")
        if not rec.get("reason"):
            errors.append(f"recommendations[{i}].reason is required")
    # Check sorted by priority
    priorities = [r.get("priority", 999) for r in recs]
    if priorities != sorted(priorities):
        warnings.append("recommendations not sorted by priority")

# --- Cross-reference checks ---
# releaseRisk.blockers should reference existing rootCauses.id
root_cause_ids = {rc.get("id") for rc in root_causes}
for rr in release_risk:
    for blocker in rr.get("blockers", []):
        if blocker not in root_cause_ids:
            errors.append(f"releaseRisk '{rr.get('branch')}' references rootCause '{blocker}' which doesn't exist")

# recommendations.rootCauseId should reference existing rootCauses.id
for rec in recs:
    rcid = rec.get("rootCauseId")
    if rcid and rcid not in root_cause_ids:
        errors.append(f"recommendation '{rec.get('action', '?')[:40]}' references rootCause '{rcid}' which doesn't exist")

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
    branch_count = len(azdo.get("branches", []))
    wf_count = len(gha.get("workflows", []))
    rc_count = len(root_causes)
    print(f"✅ Valid CI status report")
    print(f"   {meta.get('date', '?')} • {verdict.get('emoji', '?')} {verdict.get('status', '?')}")
    print(f"   {branch_count} branches • {wf_count} workflows • {rc_count} root causes • {len(recs)} recommendations")
    if warnings:
        print(f"   ({len(warnings)} warnings — review above)")
    sys.exit(0)
