#!/usr/bin/env python3
"""Render a CI status report JSON as a Markdown file.

Usage:
    python3 render-ci-status-md.py <path-to-json> [output.md]

Produces a comprehensive markdown report suitable for AI consumption,
including all analysis sections, pipeline health, and recommendations.
"""
import json
import sys
from pathlib import Path


def sev_emoji(sev):
    s = (sev or "").lower()
    if s == "critical":
        return "🔴"
    elif s == "high":
        return "🟠"
    elif s == "medium":
        return "🟡"
    return "⚪"


def status_emoji(status):
    m = {
        "healthy": "✅",
        "failing": "❌",
        "degraded": "⚠️",
        "stale": "⏳",
        "skipped": "⏭️",
    }
    return m.get(status, "❓")


def sanitize_cell(text):
    if not text:
        return ""
    text = text.replace("\r\n", " ").replace("\n", " ").replace("\r", " ")
    text = text.replace("|", "\\|")
    return text


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-ci-status-md.py <path-to-json> [output.md]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    if len(sys.argv) >= 3:
        output_path = Path(sys.argv[2])
    else:
        output_path = json_path.with_suffix(".md")

    with open(json_path, encoding="utf-8") as f:
        data = json.load(f)

    # Verify this is an augmented report JSON, not raw collector output
    required_keys = ("meta", "verdict", "azdoHealth", "githubActions", "recommendations")
    missing = [k for k in required_keys if k not in data]
    if missing:
        print(f"❌ Missing required keys: {missing}")
        print(f"   This looks like raw collector output. Run Step 3 (AI assembles report JSON) first.")
        print(f"   Expected: augmented ci-status-YYYY-MM-DD.json, not ci-status-data.json")
        sys.exit(1)

    lines = render(data)
    content = "\n".join(lines)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"✅ {output_path.name} ({len(content)} chars, {len(lines)} lines)")
    print(f"   Output: {output_path}")


def render(data):
    lines = []
    meta = data.get("meta", {})
    verdict = data.get("verdict", {})
    azdo = data.get("azdoHealth", {})
    chain = data.get("chainAnalysis", [])
    root_causes = data.get("rootCauses", [])
    gha = data.get("githubActions", {})
    flakes = data.get("flakes", [])
    release_risk = data.get("releaseRisk", [])
    recs = data.get("recommendations", [])

    # Header
    lines.append("# SkiaSharp CI Status Report")
    lines.append("")
    lines.append(f"> Generated: **{meta.get('timestamp', '?')}**  ")
    lines.append(f"> Window: {meta.get('window', {}).get('buildsPerPipeline', '?')} builds × "
                 f"{meta.get('window', {}).get('branchesCount', '?')} branches  ")
    lines.append(f"> Branches: {', '.join(f'`{b}`' for b in meta.get('branches', []))}")
    lines.append("")

    # Verdict
    lines.append("## Executive Summary")
    lines.append("")
    lines.append(f"{verdict.get('emoji', '?')} **{verdict.get('status', '?').upper()}** — {verdict.get('summary', 'No summary available.')}")
    lines.append("")

    # AzDO Health Matrix
    lines.append("## Azure DevOps Health")
    lines.append("")
    azdo_branches = azdo.get("branches", [])
    if azdo_branches:
        # Collect all pipeline names
        all_pipe_names = []
        for b in azdo_branches:
            for p in b.get("pipelines", []):
                if p["name"] not in all_pipe_names:
                    all_pipe_names.append(p["name"])

        header = "| Branch | " + " | ".join(all_pipe_names) + " | Risk |"
        sep = "|--------|" + "|".join(["------" for _ in all_pipe_names]) + "|------|"
        lines.append(header)
        lines.append(sep)

        for branch in azdo_branches:
            pipe_map = {p["name"]: p for p in branch.get("pipelines", [])}
            cells = []
            for pname in all_pipe_names:
                p = pipe_map.get(pname)
                if p:
                    icon = p.get("latestIcon", "❓")
                    result = p.get("latestResult", "?")
                    rate = p.get("passRate")
                    rate_str = f" ({rate:.0f}%)" if rate is not None and rate < 100 else ""
                    cells.append(f"{icon} {result}{rate_str}")
                else:
                    cells.append("— no data")

            risk_map = {"low": "🟢 Low", "medium": "🟡 Medium", "high": "🔴 High"}
            risk = risk_map.get(branch.get("risk"), branch.get("risk", "?"))
            lines.append(f"| `{branch['name']}` | " + " | ".join(cells) + f" | {risk} |")
        lines.append("")

    # Regressions
    regressions = azdo.get("regressions", [])
    if regressions:
        lines.append("### Regressions Detected")
        lines.append("")
        lines.append("| Branch | Pipeline | Last Green | First Red | Suspects |")
        lines.append("|--------|----------|-----------|-----------|----------|")
        for reg in regressions:
            suspects = reg.get("suspects", [])
            suspect_str = ", ".join(f"`{s.get('id', '?')[:7]}`" for s in suspects[:3])
            if len(suspects) > 3:
                suspect_str += f" +{len(suspects)-3} more"
            url = reg.get("firstRedUrl", "")
            first_red = f"[`{reg.get('firstRedBuildNumber', '?')}`]({url})" if url else f"`{reg.get('firstRedBuildNumber', '?')}`"
            lines.append(f"| `{reg.get('branch', '?')}` | {reg.get('pipeline', '?')} | "
                        f"`{reg.get('lastGreenBuildNumber', '?')}` | {first_red} | {suspect_str} |")
        lines.append("")

    # Chain Analysis
    if chain:
        lines.append("## Pipeline Chain Analysis")
        lines.append("")
        lines.append("The internal chain is sequential: **Native → Managed → Tests**. "
                     "Failures may cascade downstream.")
        lines.append("")
        for entry in chain:
            verdict_icon = {"cascade": "🔗", "independent": "🔀", "mixed": "🔀🔗"}.get(entry.get("verdict"), "❓")
            lines.append(f"- {verdict_icon} **`{entry.get('branch', '?')}`** [{entry.get('verdict', '?')}]: "
                        f"{entry.get('summary', 'No summary')}")
            if entry.get("cascadedPipelines"):
                lines.append(f"  - Cascaded: {', '.join(entry['cascadedPipelines'])}")
        lines.append("")

    # Root Causes
    if root_causes:
        lines.append("## Root Causes")
        lines.append("")
        for rc in root_causes:
            sev = rc.get("severity", "?")
            cat = rc.get("category", "?").replace("_", " ")
            lines.append(f"### [{rc.get('id', '?')}] {sev_emoji(sev)} {rc.get('title', 'Untitled')}")
            lines.append("")
            lines.append(f"- **Category:** {cat}")
            lines.append(f"- **Severity:** {sev}")
            footprint = rc.get("footprint", {})
            lines.append(f"- **Branches:** {', '.join(f'`{b}`' for b in footprint.get('branches', []))}")
            lines.append(f"- **Pipelines:** {', '.join(footprint.get('pipelines', []))}")
            lines.append(f"- **First seen:** {rc.get('firstSeen', '?')}")
            lines.append(f"- **Last seen:** {rc.get('lastSeen', '?')}")
            lines.append("")
            sample = rc.get("sampleError", "")
            if sample:
                lines.append(f"```")
                lines.append(sanitize_cell(sample[:500]))
                lines.append(f"```")
                lines.append("")
            evidence = rc.get("buildEvidence", [])
            if evidence:
                lines.append("Evidence: " + ", ".join(
                    f"[{e.get('id', '?')}]({e.get('url', '')})" for e in evidence[:5]
                ))
                lines.append("")

    # GitHub Actions
    lines.append("## GitHub Actions")
    lines.append("")
    gha_summary = gha.get("summary", {})
    lines.append(f"**Total:** {gha_summary.get('total', '?')} workflows | "
                 f"✅ {gha_summary.get('healthy', '?')} healthy | "
                 f"❌ {gha_summary.get('failing', '?')} failing | "
                 f"⏳ {gha_summary.get('stale', 0)} stale")
    lines.append("")

    workflows = gha.get("workflows", [])
    if workflows:
        # Group by severity
        for severity in ("high", "medium", "low"):
            group = [w for w in workflows if w.get("severity") == severity]
            if not group:
                continue
            sev_label = {"high": "🟠 High Severity", "medium": "🟡 Medium Severity", "low": "⚪ Low Severity"}
            lines.append(f"### {sev_label.get(severity, severity)}")
            lines.append("")
            lines.append("| Workflow | Repo | Status | Details |")
            lines.append("|----------|------|--------|---------|")
            for wf in group:
                status = wf.get("status", "?")
                icon = status_emoji(status)
                detail = ""
                if wf.get("scope") == "branch":
                    branch_bits = []
                    for b in wf.get("branches", []):
                        b_icon = {"success": "✅", "failure": "❌"}.get(b.get("latestResult"), "❓")
                        branch_bits.append(f"{b_icon} {b['name']}")
                    detail = " ".join(branch_bits)
                else:
                    # Show latest run info
                    failed_jobs = wf.get("failedJobs", [])
                    if failed_jobs:
                        detail = "; ".join(f"{j['name']}" for j in failed_jobs[:2])
                    elif wf.get("branches"):
                        # branch-scoped data in flat form
                        b = wf["branches"][0] if wf["branches"] else {}
                        detail = b.get("latestResult", "")
                lines.append(f"| {wf['name']} | `{wf.get('repo', '?')}` | {icon} {status} | {sanitize_cell(detail)} |")
            lines.append("")

    # Failed job details
    failed_wfs = [w for w in workflows if w.get("failedJobs")]
    if failed_wfs:
        lines.append("### Failed Job Details")
        lines.append("")
        for wf in failed_wfs:
            lines.append(f"**{wf['name']}** (`{wf.get('repo', '?')}`) [{wf.get('trigger', '?')}]")
            lines.append("")
            for job in wf.get("failedJobs", []):
                steps = ", ".join(f"`{s}`" for s in job.get("failedSteps", []))
                run_url = job.get("runUrl", "")
                run_link = f" — [run {job.get('runId', '?')}]({run_url})" if run_url else ""
                lines.append(f"- **{job['name']}**: {steps}{run_link}")
            lines.append("")

    # Flakes
    if flakes:
        lines.append("## Flake Detection")
        lines.append("")
        lines.append("| Branch | Pipeline | Pattern | Confidence | Description |")
        lines.append("|--------|----------|---------|------------|-------------|")
        for f in flakes:
            lines.append(f"| `{f.get('branch', '?')}` | {f.get('pipeline', '?')} | "
                        f"{f.get('pattern', '?')} | {f.get('confidence', '?')} | "
                        f"{sanitize_cell(f.get('description', ''))} |")
        lines.append("")

    # Release Risk
    if release_risk:
        lines.append("## Release Risk Assessment")
        lines.append("")
        lines.append("| Branch | Shippable | Days Since Green | Blockers | Recommendation |")
        lines.append("|--------|-----------|-----------------|----------|----------------|")
        for rr in release_risk:
            ship = "✅ Yes" if rr.get("shippable") else "❌ No"
            days = rr.get("daysSinceGreen", "—")
            blockers = ", ".join(rr.get("blockers", [])) or "—"
            rec = rr.get("recommendation", "?")
            lines.append(f"| `{rr.get('branch', '?')}` | {ship} | {days} | {blockers} | {rec} |")
        lines.append("")

    # Recommendations
    if recs:
        lines.append("## Top Recommendations")
        lines.append("")
        for rec in recs:
            sev = rec.get("severity", "?")
            priority = rec.get("priority", "?")
            url = rec.get("buildUrl", "")
            link = f" → [{url.split('/')[-1] if '/' in url else 'link'}]({url})" if url else ""
            lines.append(f"{priority}. {sev_emoji(sev)} **{rec.get('action', '?')}**")
            lines.append(f"   - {rec.get('reason', '?')}")
            lines.append(f"   - Target: `{rec.get('target', '?')}`{link}")
            lines.append("")

    # Footer
    lines.append("---")
    lines.append("")
    lines.append(f"*Report generated {meta.get('timestamp', '?')} • Schema v{meta.get('schemaVersion', '?')}*")
    lines.append("")

    return lines


if __name__ == "__main__":
    main()
