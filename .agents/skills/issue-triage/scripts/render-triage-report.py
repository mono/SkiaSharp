#!/usr/bin/env python3
"""Render an issue-triage JSON report to sidecar Markdown and HTML files.

Usage:
    python3 render-triage-report.py <path-to-json> [output.html]

If output path is omitted, writes to the same directory as the JSON using the
same basename (e.g., 3400.json -> 3400.md and 3400.html).
"""

from __future__ import annotations

import html
import json
import os
import sys
from pathlib import Path
from typing import Any


def esc(value: Any) -> str:
    return html.escape("" if value is None else str(value))


def fmt_confidence(value: Any) -> str:
    if isinstance(value, (int, float)):
        return f"{value:.2f} ({round(value * 100)}%)"
    return "—"


def fmt_list(items: Any) -> str:
    if not items:
        return "—"
    if isinstance(items, list):
        return ", ".join(str(x) for x in items)
    return str(items)


def md_table(rows: list[tuple[str, str]]) -> str:
    lines = ["| Field | Value |", "|-------|-------|"]
    for key, value in rows:
        lines.append(f"| {key} | {value} |")
    return "\n".join(lines)


def md_code_block(text: str, lang: str = "") -> str:
    if not text:
        return ""
    fence = "```"
    return f"{fence}{lang}\n{text.rstrip()}\n{fence}\n"


def render_markdown(data: dict[str, Any]) -> str:
    meta = data.get("meta", {})
    classification = data.get("classification", {})
    evidence = data.get("evidence", {})
    analysis = data.get("analysis", {})
    output = data.get("output", {})
    actionability = output.get("actionability", {})
    bug = evidence.get("bugSignals", {})
    repro = evidence.get("reproEvidence", {})
    version = evidence.get("versionAnalysis", {})
    regression = evidence.get("regression", {})
    fix = evidence.get("fixStatus", {})
    resolution = analysis.get("resolution", {})

    lines: list[str] = []
    number = meta.get("number", "?")
    lines.append(f"# Issue Triage Report — #{number}")
    lines.append("")
    lines.append(md_table([
        ("Repository", meta.get("repo", "—")),
        ("Analyzed", meta.get("analyzedAt", "—")),
        ("Type", f'{classification.get("type", {}).get("value", "—")} ({fmt_confidence(classification.get("type", {}).get("confidence"))})'),
        ("Area", f'{classification.get("area", {}).get("value", "—")} ({fmt_confidence(classification.get("area", {}).get("confidence"))})'),
        ("Suggested action", f'{actionability.get("suggestedAction", "—")} ({fmt_confidence(actionability.get("confidence"))})'),
    ]))
    lines.append("")

    if meta.get("currentLabels"):
        lines.append(f"**Current labels:** {fmt_list(meta['currentLabels'])}")
        lines.append("")

    lines.append("## Reporter Summary")
    lines.append("")
    lines.append(data.get("summary", "—"))
    lines.append("")

    lines.append("## Classification")
    lines.append("")
    lines.append(md_table([
        ("Type", classification.get("type", {}).get("value", "—")),
        ("Area", classification.get("area", {}).get("value", "—")),
        ("Platforms", fmt_list(classification.get("platforms"))),
        ("Backends", fmt_list(classification.get("backends"))),
        ("Tenets", fmt_list(classification.get("tenets"))),
        ("Partner", classification.get("partner", "—")),
    ]))
    lines.append("")

    lines.append("## Evidence")
    lines.append("")

    if repro:
        lines.append("### Reproduction")
        lines.append("")
        if repro.get("stepsToReproduce"):
            lines.extend([f"1. {step}" for step in repro["stepsToReproduce"]])
            lines.append("")
        if repro.get("environmentDetails"):
            lines.append(f"**Environment:** {repro['environmentDetails']}")
            lines.append("")
        if repro.get("relatedIssues"):
            lines.append(f"**Related issues:** {', '.join(f'#{n}' for n in repro['relatedIssues'])}")
            lines.append("")
        if repro.get("repoLinks"):
            lines.append("**Repository links:**")
            lines.extend([f"- {item.get('url')} — {item.get('description', '').strip() or 'linked repro'}" for item in repro["repoLinks"]])
            lines.append("")
        if repro.get("screenshots"):
            lines.append("**Screenshots:**")
            lines.extend([f"- {item.get('url')} — {item.get('description', '').strip() or 'screenshot'}" for item in repro["screenshots"]])
            lines.append("")
        if repro.get("attachments"):
            lines.append("**Attachments:**")
            lines.extend([f"- {item.get('filename')} — {item.get('url')} {('— ' + item.get('description')) if item.get('description') else ''}" for item in repro["attachments"]])
            lines.append("")
        if repro.get("codeSnippets"):
            lines.append("**Code snippets:**")
            lines.append("")
            for snippet in repro["codeSnippets"]:
                lines.append(md_code_block(snippet, "csharp").rstrip())
                lines.append("")

    if bug:
        lines.append("### Bug Signals")
        lines.append("")
        lines.append(md_table([
            ("Severity", bug.get("severity", "—")),
            ("Regression claimed", str(bug.get("regressionClaimed", "—"))),
            ("Error type", bug.get("errorType", "—")),
            ("Error message", bug.get("errorMessage", "—")),
            ("Repro quality", bug.get("reproQuality", "—")),
            ("Target frameworks", fmt_list(bug.get("targetFrameworks"))),
        ]))
        lines.append("")
        if bug.get("stackTrace"):
            lines.append("**Stack trace:**")
            lines.append("")
            lines.append(md_code_block(bug["stackTrace"], "text").rstrip())
            lines.append("")

    if version:
        lines.append("### Version Analysis")
        lines.append("")
        lines.append(md_table([
            ("Mentioned versions", fmt_list(version.get("mentionedVersions"))),
            ("Worked in", version.get("workedIn", "—")),
            ("Broke in", version.get("brokeIn", "—")),
            ("Current relevance", version.get("currentRelevance", "—")),
            ("Relevance reason", version.get("relevanceReason", "—")),
        ]))
        lines.append("")

    if regression:
        lines.append("### Regression")
        lines.append("")
        lines.append(md_table([
            ("Is regression", str(regression.get("isRegression", "—"))),
            ("Confidence", fmt_confidence(regression.get("confidence"))),
            ("Reason", regression.get("reason", "—")),
            ("Worked in version", regression.get("workedInVersion", "—")),
            ("Broke in version", regression.get("brokeInVersion", "—")),
        ]))
        lines.append("")

    if fix:
        lines.append("### Fix Status")
        lines.append("")
        lines.append(md_table([
            ("Likely fixed", str(fix.get("likelyFixed", "—"))),
            ("Confidence", fmt_confidence(fix.get("confidence"))),
            ("Reason", fix.get("reason", "—")),
            ("Related PRs", fmt_list([f"#{n}" for n in fix.get("relatedPRs", [])])),
            ("Related commits", fmt_list(fix.get("relatedCommits"))),
            ("Fixed in version", fix.get("fixedInVersion", "—")),
        ]))
        lines.append("")

    lines.append("## Analysis")
    lines.append("")
    lines.append("### Technical Summary")
    lines.append("")
    lines.append(analysis.get("summary", "—"))
    lines.append("")

    if analysis.get("rationale"):
        lines.append("### Rationale")
        lines.append("")
        lines.append(analysis["rationale"])
        lines.append("")

    if analysis.get("keySignals"):
        lines.append("### Key Signals")
        lines.append("")
        for signal in analysis["keySignals"]:
            text = signal.get("text", "").strip()
            src = signal.get("source", "").strip()
            interp = signal.get("interpretation", "").strip()
            line = f'- "{text}" — **{src}**'
            if interp:
                line += f" ({interp})"
            lines.append(line)
        lines.append("")

    if analysis.get("codeInvestigation"):
        lines.append("### Code Investigation")
        lines.append("")
        lines.append("| File | Lines | Relevance | Finding |")
        lines.append("|------|-------|-----------|---------|")
        for entry in analysis["codeInvestigation"]:
            lines.append(
                f"| `{entry.get('file', '')}` | {entry.get('lines', '—')} | {entry.get('relevance', '—')} | {entry.get('finding', '—')} |"
            )
        lines.append("")

    if analysis.get("errorFingerprint"):
        lines.append(f"**Error fingerprint:** `{analysis['errorFingerprint']}`")
        lines.append("")

    if analysis.get("workarounds"):
        lines.append("### Workarounds")
        lines.append("")
        lines.extend([f"- {item}" for item in analysis["workarounds"]])
        lines.append("")

    if analysis.get("nextQuestions"):
        lines.append("### Next Questions")
        lines.append("")
        lines.extend([f"- {item}" for item in analysis["nextQuestions"]])
        lines.append("")

    if resolution:
        lines.append("### Resolution Proposals")
        lines.append("")
        if resolution.get("hypothesis"):
            lines.append(f"**Hypothesis:** {resolution['hypothesis']}")
            lines.append("")
        for index, proposal in enumerate(resolution.get("proposals", []), start=1):
            title = proposal.get("title", f"Proposal {index}")
            meta_bits = [
                proposal.get("category"),
                f"confidence {fmt_confidence(proposal.get('confidence'))}" if "confidence" in proposal else None,
                proposal.get("effort"),
                f"validated={proposal.get('validated')}" if proposal.get("validated") else None,
            ]
            meta_text = ", ".join(bit for bit in meta_bits if bit)
            lines.append(f"{index}. **{title}**{f' — {meta_text}' if meta_text else ''}")
            lines.append(f"   - {proposal.get('description', '—')}")
            if proposal.get("codeSnippet"):
                lines.append("")
                lines.append(md_code_block(proposal["codeSnippet"], "csharp").rstrip())
                lines.append("")
        if resolution.get("recommendedProposal"):
            lines.append(f"**Recommended proposal:** {resolution['recommendedProposal']}")
        if resolution.get("recommendedReason"):
            lines.append(f"**Why:** {resolution['recommendedReason']}")
        lines.append("")

    lines.append("## Output")
    lines.append("")
    lines.append("### Actionability")
    lines.append("")
    lines.append(md_table([
        ("Suggested action", actionability.get("suggestedAction", "—")),
        ("Confidence", fmt_confidence(actionability.get("confidence"))),
        ("Reason", actionability.get("reason", "—")),
        ("Suggested repro platform", actionability.get("suggestedReproPlatform", "—")),
    ]))
    lines.append("")

    if output.get("missingInfo"):
        lines.append("### Missing Info")
        lines.append("")
        lines.extend([f"- {item}" for item in output["missingInfo"]])
        lines.append("")

    lines.append("### Automatable Actions")
    lines.append("")
    lines.append("| Type | Risk | Confidence | Description | Details |")
    lines.append("|------|------|------------|-------------|---------|")
    for action in output.get("actions", []):
        details: list[str] = []
        if action.get("labels"):
            details.append(f"labels={', '.join(action['labels'])}")
        if action.get("linkedIssue"):
            details.append(f"linkedIssue=#{action['linkedIssue']}")
        if action.get("stateReason"):
            details.append(f"stateReason={action['stateReason']}")
        if action.get("category"):
            details.append(f"category={action['category']}")
        if action.get("milestone"):
            details.append(f"milestone={action['milestone']}")
        lines.append(
            f"| {action.get('type', '—')} | {action.get('risk', '—')} | {fmt_confidence(action.get('confidence'))} | {action.get('description', '—')} | {'; '.join(details) or '—'} |"
        )
        if action.get("comment"):
            lines.append("")
            lines.append(f"**Comment draft for `{action.get('type')}`:**")
            lines.append("")
            lines.append(md_code_block(action["comment"], "markdown").rstrip())
            lines.append("")

    lines.append("## Raw JSON")
    lines.append("")
    lines.append(md_code_block(json.dumps(data, indent=2, ensure_ascii=False), "json").rstrip())
    lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def main() -> None:
    if len(sys.argv) < 2:
        print("Usage: python3 render-triage-report.py <path-to-json> [output.html]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    if len(sys.argv) >= 3:
        html_path = Path(sys.argv[2])
    else:
        html_path = json_path.with_suffix(".html")
    md_path = html_path.with_suffix(".md")

    try:
        with open(json_path, encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"❌ Invalid JSON: {e}")
        sys.exit(1)

    for key in ("meta", "summary", "classification", "evidence", "analysis", "output"):
        if key not in data:
            print(f"❌ Missing required key: {key}")
            sys.exit(1)

    template_path = Path(__file__).parent / "viewer.html"
    if not template_path.exists():
        print(f"❌ Template not found: {template_path}")
        sys.exit(1)

    with open(template_path, encoding="utf-8") as f:
        template = f.read()

    json_str = json.dumps(data, ensure_ascii=False)
    json_str = json_str.replace("</script>", "<\\/script>")
    json_str = json_str.replace("</Script>", "<\\/Script>")
    json_str = json_str.replace("</SCRIPT>", "<\\/SCRIPT>")

    marker = "<!--INJECT_DATA_HERE-->"
    if marker not in template:
        print(f"❌ Injection marker '{marker}' not found in template")
        sys.exit(1)

    html_doc = template.replace(marker, f"<script>const DATA = {json_str};</script>")
    meta = data.get("meta", {})
    number = meta.get("number", "?")
    action = data.get("output", {}).get("actionability", {}).get("suggestedAction", "?")
    html_doc = html_doc.replace(
        "<title>Issue Triage Report</title>",
        f"<title>Issue Triage — #{number} ({action})</title>",
    )

    markdown = render_markdown(data)

    md_path.parent.mkdir(parents=True, exist_ok=True)
    with open(md_path, "w", encoding="utf-8") as f:
        f.write(markdown)

    html_path.parent.mkdir(parents=True, exist_ok=True)
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html_doc)

    print(f"✅ {md_path.name} ({os.path.getsize(md_path) / 1024:.0f} KB)")
    print(f"✅ {html_path.name} ({os.path.getsize(html_path) / 1024:.0f} KB)")
    print(
        f"   Issue #{number} • {data.get('classification', {}).get('type', {}).get('value', '?')} • "
        f"{data.get('classification', {}).get('area', {}).get('value', '?')} • "
        f"{action}"
    )
    print(f"   Output: {json_path.with_suffix('').as_posix()}.[json|md|html]")


if __name__ == "__main__":
    main()
