#!/usr/bin/env python3
"""Render a security-audit JSON report as a Markdown file.

Usage:
    python3 render-security-audit-md.py <path-to-json> [output.md]

Produces a comprehensive markdown report suitable for AI consumption,
including all findings, Chrome Releases data, CG alerts, and next steps.
"""
import json
import sys
from pathlib import Path


def sev_emoji(sev):
    s = (sev or "").upper()
    if s == "CRITICAL":
        return "🔴"
    elif s == "HIGH":
        return "🟠"
    elif s == "MEDIUM":
        return "🟡"
    return "⚪"


def status_emoji(status):
    m = {
        "needs_attention": "🔴",
        "in_progress": "🟡",
        "ready_to_merge": "✅",
        "undiscovered": "🆕",
        "false_positive": "⚪",
        "clean": "✅",
        "cg_tracked": "🛡️",
    }
    return m.get(status, "❓")


def status_label(status):
    m = {
        "needs_attention": "Needs attention",
        "in_progress": "In progress",
        "ready_to_merge": "Ready to merge",
        "undiscovered": "Undiscovered",
        "false_positive": "False positive",
        "clean": "Clean",
        "cg_tracked": "CG Tracked",
    }
    return m.get(status, status or "Unknown")


def sanitize_cell(text):
    """Sanitize text for use in a markdown table cell."""
    if not text:
        return ""
    # Replace newlines with spaces
    text = text.replace("\r\n", " ").replace("\n", " ").replace("\r", " ")
    # Escape pipe characters
    text = text.replace("|", "\\|")
    return text


def render_md(data):
    meta = data["meta"]
    summary = data["summary"]
    findings = data["findings"]
    next_steps = data.get("nextSteps", [])
    ver = data.get("versionVerification", [])
    cg = data.get("cgAlerts", {})
    cr = data.get("chromeReleases", {})

    lines = []

    # Header
    lines.append(f"# Security Audit Report")
    lines.append("")
    lines.append(f"**Date:** {meta.get('date', 'N/A')}")
    lines.append(f"**Skia Milestone:** m{meta.get('skiaMilestone', '?')}")
    lines.append(f"**Fork Commit:** `{meta.get('skiaSubmoduleCommit', 'N/A')[:12]}`")
    lines.append(f"**Upstream Verified:** {'Yes' if meta.get('upstreamVerified') else 'No'}")
    lines.append("")

    # Summary
    lines.append("## Summary")
    lines.append("")
    lines.append("| Status | Count |")
    lines.append("|--------|-------|")
    lines.append(f"| 🔴 Needs Attention | {summary.get('needsAttention', 0)} |")
    lines.append(f"| 🆕 Undiscovered | {summary.get('undiscovered', 0)} |")
    lines.append(f"| ⚪ False Positive | {summary.get('falsePositive', 0)} |")
    lines.append(f"| ✅ Clean | {summary.get('clean', 0)} |")
    lines.append(f"| **Total CVEs** | **{summary.get('totalCves', 0)}** |")
    lines.append(f"| **Highest Severity** | **{summary.get('highestSeverity', 'N/A')}** |")
    lines.append("")

    # Findings
    lines.append("## Findings")
    lines.append("")

    # Sort findings: needs_attention/in_progress first
    status_order = {"needs_attention": 0, "in_progress": 1, "undiscovered": 2, "ready_to_merge": 3, "false_positive": 4, "clean": 5}
    sorted_findings = sorted(findings, key=lambda f: status_order.get(f.get("status", ""), 9))

    for i, f in enumerate(sorted_findings, 1):
        dep = f.get("dependency", "?")
        status = f.get("status", "?")
        emoji = status_emoji(status)
        label = status_label(status)
        cves = f.get("cves", [])
        non_chrome = f.get("nonChromeCves", [])

        lines.append(f"### {i}. {emoji} {dep} — {label}")
        lines.append("")

        # Info table
        lines.append("| Field | Value |")
        lines.append("|-------|-------|")
        if f.get("currentVersion"):
            lines.append(f"| Current | {f['currentVersion']} |")
        if f.get("fixVersion"):
            lines.append(f"| Min Fix | {f['fixVersion']} |")
        if f.get("latestVersion"):
            lines.append(f"| Latest | {f['latestVersion']} |")
        lines.append(f"| CVEs | {len(cves) + len(non_chrome)} |")
        if f.get("issues"):
            issues_str = ", ".join(
                f"#{i['number'] if isinstance(i, dict) else i}" for i in f["issues"]
            )
            lines.append(f"| Issues | {issues_str} |")
        if f.get("prs"):
            prs_str = ", ".join(
                f"#{p['number'] if isinstance(p, dict) else p}" for p in f["prs"]
            )
            lines.append(f"| PRs | {prs_str} |")
        lines.append("")

        # CVE table
        if cves:
            # Sort: affected first, then by severity
            assess_order = {"potentially_affected": 0, "affected": 0, "needs_attention": 0, "undiscovered": 1, "already_fixed": 2, "false_positive": 3}
            sev_order_map = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
            cves_sorted = sorted(cves, key=lambda c: (
                assess_order.get((c.get("assessment") or "").lower(), 1),
                sev_order_map.get((c.get("severity") or "").upper(), 9)
            ))

            has_fixed_in = any(c.get("fixedIn") or c.get("fixMilestone") for c in cves_sorted)
            if has_fixed_in:
                lines.append("| Severity | CVE | Fixed In | Assessment | Description |")
                lines.append("|----------|-----|----------|------------|-------------|")
            else:
                lines.append("| Severity | CVE | Assessment | Description |")
                lines.append("|----------|-----|------------|-------------|")

            for c in cves_sorted:
                sev = c.get("severity", "?")
                cve_id = c.get("id", "?")
                fixed = c.get("fixedIn") or c.get("fixMilestone") or ""
                assess = c.get("assessment", "?")
                desc = sanitize_cell(c.get("description", ""))
                if has_fixed_in:
                    lines.append(f"| {sev_emoji(sev)} {sev} | {cve_id} | {fixed} | {assess} | {desc} |")
                else:
                    lines.append(f"| {sev_emoji(sev)} {sev} | {cve_id} | {assess} | {desc} |")
            lines.append("")

        # Non-Chrome CVEs
        if non_chrome:
            lines.append("**Non-Chrome CVEs (Manual Review):**")
            lines.append("")
            lines.append("| Severity | CVE | Source | Assessment | Description |")
            lines.append("|----------|-----|--------|------------|-------------|")
            for c in non_chrome:
                sev = c.get("severity", "?")
                cve_id = c.get("id", "?")
                source = c.get("source", "?")
                assess = c.get("assessment", "?")
                desc = sanitize_cell(c.get("description", ""))
                lines.append(f"| {sev_emoji(sev)} {sev} | {cve_id} | {source} | {assess} | {desc} |")
            lines.append("")

        # Action
        if f.get("action"):
            lines.append(f"**Action:** {f['action']}")
            lines.append("")

        # Notes
        if f.get("notes"):
            lines.append(f"> {f['notes']}")
            lines.append("")

        lines.append("---")
        lines.append("")

    # CG Alerts
    if cg and cg.get("totalAlerts"):
        lines.append("## Component Governance Alerts")
        lines.append("")
        lines.append(f"**Total Alerts:** {cg.get('totalAlerts', 0)}")
        sev_data = cg.get("bySeverity", {})
        lines.append(f"**Breakdown:** Critical={sev_data.get('Critical', 0)}, High={sev_data.get('High', 0)}, Medium={sev_data.get('Medium', 0)}, Low={sev_data.get('Low', 0)}")
        lines.append("")

        alerts = cg.get("alerts", [])
        if alerts:
            # Group by severity, show top ones
            crit_high = [a for a in alerts if a.get("severity") in ("Critical", "High")]
            if crit_high:
                lines.append("### Critical & High Severity")
                lines.append("")
                lines.append("| Severity | CVE/Advisory | Component |")
                lines.append("|----------|--------------|-----------|")
                for a in crit_high[:30]:
                    lines.append(f"| {sev_emoji(a.get('severity',''))} {a.get('severity','')} | {a.get('id','')} | {a.get('component','')} |")
                if len(crit_high) > 30:
                    lines.append(f"| ... | +{len(crit_high)-30} more | |")
                lines.append("")

        pipelines = cg.get("pipelines", [])
        if pipelines:
            lines.append(f"**Pipelines scanned:** {', '.join(p.get('name','') for p in pipelines)}")
            lines.append("")

    # Chrome Releases (after CG, before Version Verification)
    if cr and cr.get("skiaRelevantCves"):
        lines.append("## Chrome Releases Blog")
        lines.append("")
        lines.append(f"**Queried:** {cr.get('monthsQueried', '?')} months ({cr.get('postsReviewed', '?')} posts reviewed)")
        lines.append(f"**Total CVEs extracted:** {cr.get('totalCvesExtracted', '?')}")
        lines.append(f"**Skia-relevant CVEs:** {cr.get('skiaRelevantCves', '?')}")
        lines.append("")

        structured = cr.get("structuredCves", [])
        milestone = meta.get("skiaMilestone", 0)
        above = [c for c in structured if (c.get("milestone") or 0) > milestone]

        if above:
            lines.append(f"### CVEs Above Current Milestone (m{milestone})")
            lines.append("")
            lines.append("| Severity | CVE | Component | Fixed In | Bug | Blog |")
            lines.append("|----------|-----|-----------|----------|-----|------|")

            # Sort: Skia first, then by severity
            comp_order = {"Skia": 0, "ANGLE": 1, "Canvas": 2, "Fonts": 3, "GPU": 4, "Compositing": 5, "WebGL": 6}
            sev_order = {"Critical": 0, "High": 1, "Medium": 2, "Low": 3}
            above.sort(key=lambda c: (comp_order.get(c.get("component", ""), 99), sev_order.get(c.get("severity", ""), 9)))

            for c in above:
                sev = c.get("severity", "?")
                cve_id = c.get("cveId", "?")
                comp = c.get("component", "?")
                ms = c.get("milestone", "?")
                bug = c.get("bugId", "")
                bug_link = f"[{bug}](https://issues.chromium.org/issues/{bug})" if bug else "N/A"
                blog = c.get("blogPostUrl", "")
                blog_link = f"[Post]({blog})" if blog else ""
                lines.append(f"| {sev_emoji(sev)} {sev} | {cve_id} | {comp} | m{ms} | {bug_link} | {blog_link} |")
            lines.append("")

        # Summary by component
        comp_counts = {}
        for c in structured:
            comp = c.get("component", "Other")
            comp_counts[comp] = comp_counts.get(comp, 0) + 1
        lines.append("### By Component (all milestones)")
        lines.append("")
        lines.append("| Component | CVEs |")
        lines.append("|-----------|------|")
        for comp, count in sorted(comp_counts.items(), key=lambda x: -x[1]):
            lines.append(f"| {comp} | {count} |")
        lines.append("")

    # Version Verification
    if ver:
        lines.append("## Version Verification")
        lines.append("")
        lines.append("| Dependency | Source | Verified Version | cgmanifest | Match |")
        lines.append("|------------|--------|------------------|------------|-------|")
        for v in ver:
            dep = v.get("name") or v.get("dependency") or "?"
            source = v.get("source", "?")
            verified = v.get("verifiedVersion", "?")
            cgver = v.get("cgmanifestVersion") or "N/A"
            match = "✅" if v.get("match") else "❌"
            lines.append(f"| {dep} | {source} | {verified} | {cgver} | {match} |")
        lines.append("")

    # Next Steps
    if next_steps:
        lines.append("## Next Steps")
        lines.append("")
        for i, step in enumerate(next_steps, 1):
            sev = step.get("severity", "")
            action = step.get("action", "")
            reason = step.get("reason", "")
            cmd = step.get("command", "")
            lines.append(f"{i}. **{action}** ({sev})")
            if reason:
                lines.append(f"   - {reason}")
            if cmd:
                lines.append(f"   - `{cmd}`")
            lines.append("")

    return "\n".join(lines)


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-security-audit-md.py <path-to-json> [output.md]")
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

    md = render_md(data)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(md)

    print(f"✅ {output_path.name} ({len(md) / 1024:.1f} KB)")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
