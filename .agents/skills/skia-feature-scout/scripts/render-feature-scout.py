#!/usr/bin/env python3
"""Render a Skia Feature Scout JSON report (v3) as a self-contained HTML file.

Usage:
    python3 render-feature-scout.py <path-to-json> [output.html]

If output path is omitted, writes to the same directory as the JSON
with a .html extension.

Exit codes: 0=success, 1=error.
"""
import json
import os
import sys
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-feature-scout.py <path-to-json> [output.html]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    if len(sys.argv) >= 3:
        output_path = Path(sys.argv[2])
    else:
        output_path = json_path.with_suffix(".html")

    try:
        with open(json_path, encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"❌ Invalid JSON: {e}")
        sys.exit(1)

    for key in ("meta", "summary", "findings"):
        if key not in data:
            print(f"❌ Missing required key: {key}")
            sys.exit(1)

    data.setdefault("nextSteps", [])

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

    data_script = f"<script>const DATA = {json_str};</script>"

    marker = "<!--INJECT_DATA_HERE-->"
    if marker not in template:
        print(f"❌ Injection marker '{marker}' not found in template")
        sys.exit(1)

    html = template.replace(marker, data_script)

    meta = data.get("meta", {})
    curr_ms = meta.get("currentMilestone", "?")
    date = meta.get("date", "?")
    mode = meta.get("mode", "full")
    title_detail = f"m{curr_ms}" if mode == "full" else f"m{meta.get('previousMilestone', '?')}→m{curr_ms}"
    html = html.replace(
        "<title>Skia Feature Scout</title>",
        f"<title>Skia Feature Scout — {title_detail} ({date})</title>"
    )

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_kb = os.path.getsize(output_path) / 1024
    summary = data.get("summary", {})
    total = summary.get("totalFindings", "?")
    by_status = summary.get("byStatus", {})
    missing = by_status.get("missing", 0)
    full = by_status.get("full", 0)
    partial = by_status.get("partial", 0)
    by_source = summary.get("bySource", {})
    hidden = by_source.get("header-scan", 0)

    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   {title_detail} • {date} • {total} findings")
    print(f"   ❌ {missing} missing · 🟡 {partial} partial · ✅ {full} full · 🆕 {hidden} hidden")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
