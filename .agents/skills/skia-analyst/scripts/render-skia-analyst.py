#!/usr/bin/env python3
"""Render a Skia Analyst JSON report as GitHub-flavored Markdown.

Usage: python3 render-skia-analyst.py <path-to-json> [output.md]

Headers start at ### so the output can be embedded directly in a GitHub issue
(# and ## are reserved for issue titles).
"""
import json
import os
import sys
from pathlib import Path


IMPACT_EMOJI = {'transformative': '🚀', 'significant': '⭐', 'moderate': '📦', 'minor': '🔹'}
IMPACT_DESC = {
    'transformative': 'Unlocks entirely new app categories',
    'significant': 'Major visible improvement users notice immediately',
    'moderate': 'Useful gap fill, improves safety or completeness',
    'minor': 'Small utility or completeness item',
}
PRI_EMOJI = {'critical': '🔴', 'high': '🟠', 'medium': '🟡', 'low': '⚪'}
PRI_ORDER = {'critical': 0, 'high': 1, 'medium': 2, 'low': 3}


def render_item(f):
    pri = PRI_EMOJI.get(f.get('priority', 'low'), '⚪')
    ms_num = f['milestone'] if f.get('milestone') else 0
    if ms_num:
        ms_text = f"m{ms_num}"
        pad = "&nbsp;" * max(0, 4 - len(ms_text))
        ms = f"<code>{pad}{ms_text}</code>"
    else:
        ms = "<code>&nbsp;&nbsp;&nbsp;&nbsp;</code>"

    lines = ["<details>", f"<summary>{pri} {ms} <b>{f['name']}</b></summary>\n",
             f"> {f['description']}  "]
    detail = []
    if f.get('skiaApi'):
        detail.append(f"**Skia API:** `{f['skiaApi']}`")
    if f.get('cppClass') and f.get('cppMethod'):
        detail.append(f"**C++:** `{f['cppClass']}::{f['cppMethod']}`")
    elif f.get('cppClass'):
        detail.append(f"**C++:** `{f['cppClass']}`")
    if f.get('userValue'):
        detail.append(f"**User value:** {f['userValue']}")
    if f.get('milestone'):
        ms_detail = f"m{f['milestone']}"
        if f.get('milestones'):
            ms_detail += ", " + ", ".join(f"m{m}" for m in f['milestones'])
        detail.append(f"**Milestones:** {ms_detail}")
    if f.get('effort'):
        detail.append(f"**Effort:** {f['effort']}")
    if f.get('skillToUse'):
        detail.append(f"**Skill:** `{f['skillToUse']}`")
    if f.get('replacement'):
        detail.append(f"**Replacement:** `{f['replacement']}`")
    if f.get('notes'):
        detail.append(f"**Notes:** {f['notes']}")
    if detail:
        lines.append(">  \n> " + "  \n> ".join(detail))
    lines.append("\n</details>\n")
    return '\n'.join(lines)


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-skia-analyst.py <path-to-json> [output.md]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    output_path = Path(sys.argv[2]) if len(sys.argv) >= 3 else json_path.with_suffix(".md")

    try:
        with open(json_path, encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"❌ Invalid JSON: {e}")
        sys.exit(1)

    all_findings = data.get("findings", [])
    actionable = [f for f in all_findings if f.get('bindingStatus') in ('missing', 'partial', 'action_needed')]
    full_items = [f for f in all_findings if f.get('bindingStatus') == 'full']
    na_items = [f for f in all_findings if f.get('bindingStatus') in ('not_applicable', 'correctly_absent')]
    meta = data.get("meta", {})

    # Derive subtitle from scan mode
    mode = meta.get("scanMode", "full")
    if mode == "diff" and meta.get("refFrom"):
        subtitle = f"{meta['refFrom']}..{meta.get('refTo', 'HEAD')}"
    elif mode == "windowed" and meta.get("milestoneFrom"):
        subtitle = f"m{meta['milestoneFrom']}-m{meta.get('milestoneTo', meta.get('currentMilestone', '?'))}"
    else:
        subtitle = f"m{meta.get('currentMilestone', '?')} (full scan)"

    missing_count = sum(1 for f in actionable if f['bindingStatus'] == 'missing')
    partial_count = sum(1 for f in actionable if f['bindingStatus'] == 'partial')
    action_count = sum(1 for f in actionable if f['bindingStatus'] == 'action_needed')

    md = []
    md.append("### Skia Analyst Report\n")
    md.append(f"> **{len(actionable)} actionable items** · {len(full_items)} already bound · {len(na_items)} no action needed  ")
    md.append(f"> {subtitle} · Generated {meta.get('date', '?')}\n")

    # Collapsed legend
    md.append("<details>")
    md.append("<summary><b>Legend</b></summary>\n")
    md.append("**Status** (squares)\n")
    md.append("| Symbol | Meaning | Count |")
    md.append("|--------|---------|-------|")
    md.append(f"| 🟥 | Missing - no C API or C# wrapper | {missing_count} |")
    md.append(f"| 🟨 | Partial - C API exists, needs C# wrapper | {partial_count} |")
    md.append(f"| 🟧 | Action needed - wraps deprecated/removed API | {action_count} |")
    md.append(f"| 🟩 | Already bound | {len(full_items)} |")
    md.append(f"| ⬜ | No action needed | {len(na_items)} |")
    md.append("")
    md.append("**Priority** (circles)\n")
    md.append("| Symbol | Meaning |")
    md.append("|--------|---------|")
    md.append("| 🔴 | Critical - will break on next Skia bump |")
    md.append("| 🟠 | High - major capability or highly requested |")
    md.append("| 🟡 | Medium - useful addition |")
    md.append("| ⚪ | Low - minor utility |")
    md.append("")
    md.append("</details>\n")

    # Impact summary
    md.append("| Impact | Count | What it means |")
    md.append("|--------|-------|---------------|")
    for imp in ['transformative', 'significant', 'moderate', 'minor']:
        items = [f for f in actionable if f.get('impact') == imp]
        if items:
            md.append(f"| {IMPACT_EMOJI[imp]} **{imp.title()}** | {len(items)} | {IMPACT_DESC[imp]} |")
    md.append("")

    # Findings by impact
    for imp in ['transformative', 'significant', 'moderate', 'minor']:
        items = [f for f in actionable if f.get('impact') == imp]
        if not items:
            continue
        md.append(f"#### {IMPACT_EMOJI[imp]} {imp.title()}\n")
        for f in sorted(items, key=lambda x: (PRI_ORDER.get(x.get('priority', 'low'), 3), x['name'])):
            md.append(render_item(f))

    # Already bound
    md.append("<details>")
    md.append(f"<summary><h4>✅ Already Bound ({len(full_items)})</h4></summary>\n")
    md.append("These Skia features are fully available in SkiaSharp.\n")
    for f in sorted(full_items, key=lambda x: x['name']):
        ms = f" (m{f['milestone']})" if f.get('milestone') else ""
        md.append(f"- **{f['name']}**{ms} - {f['description'][:100]}")
    md.append("\n</details>\n")

    # No action needed
    md.append("<details>")
    md.append(f"<summary><h4>⚪ No Action Needed ({len(na_items)})</h4></summary>\n")
    md.append("Internal Skia changes, automatic engine improvements, or correctly absent APIs.\n")
    for f in sorted(na_items, key=lambda x: x['name']):
        bs = '🚫' if f['bindingStatus'] == 'correctly_absent' else 'N/A'
        ms = f" (m{f['milestone']})" if f.get('milestone') else ""
        md.append(f"- {bs} **{f['name']}**{ms} - {f['description'][:100]}")
    md.append("\n</details>\n")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write('\n'.join(md))

    size_kb = os.path.getsize(output_path) / 1024
    transformative = sum(1 for f in actionable if f.get('impact') == 'transformative')
    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   {subtitle} · {len(actionable)} actionable · {missing_count} missing · {transformative} transformative")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
