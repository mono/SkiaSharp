#!/usr/bin/env python3
"""Render a Sample Scout JSON report as GitHub-flavored Markdown.

Usage: python3 render-sample-scout.py <path-to-json> [output.md]

Headers start at ### so the output can be embedded directly in a GitHub issue.
"""
import json
import os
import sys
from pathlib import Path

INTEREST_EMOJI = {'high': '🔥', 'medium': '🔶', 'low': '🔹'}
STATUS_EMOJI = {'none': '🆕', 'similar': '🔶', 'existing': '✅'}


def render_item(f):
    interest = INTEREST_EMOJI.get(f.get('interesting', 'low'), '🔹')
    avail = '✅' if f.get('apis_available') else '❌'
    status = STATUS_EMOJI.get(f.get('sampleStatus', 'none'), '🆕')

    lines = ["<details>", f"<summary>{interest} {avail} {status} <b>{f['name']}</b></summary>\n"]
    lines.append(f"> {f['description']}  ")

    detail = []
    if f.get('visualGoal'):
        detail.append(f"**Visual goal:** {f['visualGoal']}")
    if f.get('category'):
        detail.append(f"**Category:** {f['category']}")
    if f.get('key_apis'):
        detail.append(f"**Key APIs:** {', '.join(f'`{a}`' for a in f['key_apis'][:5])}")
    if f.get('skiaSharpApis'):
        detail.append(f"**SkiaSharp APIs:** {', '.join(f'`{a}`' for a in f['skiaSharpApis'][:5])}")
    if f.get('missing_apis') and len(f['missing_apis']) > 0:
        detail.append(f"**Missing APIs:** {', '.join(f'`{a}`' for a in f['missing_apis'][:5])}")
    if f.get('suggestedControls') and len(f['suggestedControls']) > 0:
        detail.append(f"**Controls:** {', '.join(f['suggestedControls'][:5])}")
    if f.get('matchedSample'):
        detail.append(f"**Matched sample:** `{f['matchedSample']}`")
    if f.get('notes'):
        detail.append(f"**Notes:** {f['notes']}")
    if f.get('file'):
        detail.append(f"**Source:** `gm/{f['file']}`")

    if detail:
        lines.append(">  \n> " + "  \n> ".join(detail))
    lines.append("\n</details>\n")
    return '\n'.join(lines)


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-sample-scout.py <path-to-json> [output.md]")
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

    findings = data.get("findings", [])
    meta = data.get("meta", {})

    # Categorize
    opportunities = [f for f in findings
                     if f.get('interesting') == 'high'
                     and f.get('apis_available')
                     and f.get('sampleStatus') == 'none']
    needs_apis = [f for f in findings
                  if f.get('interesting') == 'high'
                  and not f.get('apis_available')
                  and f.get('sampleStatus') == 'none']
    high = [f for f in findings if f.get('interesting') == 'high']
    medium = [f for f in findings if f.get('interesting') == 'medium']
    low = [f for f in findings if f.get('interesting') == 'low']
    no_sample = [f for f in findings if f.get('sampleStatus') == 'none']
    similar = [f for f in findings if f.get('sampleStatus') == 'similar']
    existing = [f for f in findings if f.get('sampleStatus') == 'existing']

    md = []
    md.append("### Sample Scout Report\n")
    md.append(f"> **{len(findings)} GM samples analyzed** · "
              f"{len(opportunities)} ready to build · "
              f"{len(needs_apis)} need new APIs  ")
    md.append(f"> Generated {meta.get('date', '?')}\n")

    # Legend
    md.append("<details>")
    md.append("<summary><b>Legend</b></summary>\n")
    md.append("| Symbol | Meaning |")
    md.append("|--------|---------|")
    md.append("| 🔥 | High interest |")
    md.append("| 🔶 | Medium interest |")
    md.append("| 🔹 | Low interest |")
    md.append("| ✅ | APIs available / already covered |")
    md.append("| ❌ | Missing APIs |")
    md.append("| 🆕 | No existing Gallery sample |")
    md.append("")
    md.append("</details>\n")

    # Summary table
    md.append("| Category | Count |")
    md.append("|----------|-------|")
    md.append(f"| 🔥 High interest | {len(high)} |")
    md.append(f"| 🔶 Medium interest | {len(medium)} |")
    md.append(f"| 🔹 Low interest | {len(low)} |")
    md.append(f"| 🆕 No existing sample | {len(no_sample)} |")
    md.append(f"| 🔶 Similar sample exists | {len(similar)} |")
    md.append(f"| ✅ Already covered | {len(existing)} |")
    md.append(f"| 🎯 **Ready to build** | **{len(opportunities)}** |")
    md.append(f"| 🔧 Needs new APIs | {len(needs_apis)} |")
    md.append("")

    # Top opportunities
    if opportunities:
        md.append("#### 🎯 Ready to Build\n")
        md.append("High interest, APIs available, no existing Gallery sample.\n")
        for f in sorted(opportunities, key=lambda x: x['name']):
            md.append(render_item(f))

    # Needs APIs
    if needs_apis:
        md.append("#### 🔧 Needs New APIs First\n")
        md.append("High interest but requires APIs not yet in SkiaSharp.\n")
        for f in sorted(needs_apis, key=lambda x: x['name']):
            md.append(render_item(f))

    # Medium interest opportunities
    medium_opps = [f for f in medium if f.get('apis_available') and f.get('sampleStatus') == 'none']
    if medium_opps:
        md.append("<details>")
        md.append(f"<summary><h4>🔶 Medium Interest Opportunities ({len(medium_opps)})</h4></summary>\n")
        for f in sorted(medium_opps, key=lambda x: x['name']):
            md.append(render_item(f))
        md.append("\n</details>\n")

    # Already covered
    if existing:
        md.append("<details>")
        md.append(f"<summary><h4>✅ Already Covered ({len(existing)})</h4></summary>\n")
        for f in sorted(existing, key=lambda x: x['name']):
            matched = f" → `{f['matchedSample']}`" if f.get('matchedSample') else ""
            md.append(f"- **{f['name']}**{matched} — {f['description'][:80]}")
        md.append("\n</details>\n")

    # Low interest
    if low:
        md.append("<details>")
        md.append(f"<summary><h4>🔹 Low Interest ({len(low)})</h4></summary>\n")
        for f in sorted(low, key=lambda x: x['name']):
            md.append(f"- **{f['name']}** — {f['description'][:80]}")
        md.append("\n</details>\n")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write('\n'.join(md))

    size_kb = os.path.getsize(output_path) / 1024
    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   {len(findings)} samples · {len(opportunities)} ready to build · {len(needs_apis)} need APIs")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
