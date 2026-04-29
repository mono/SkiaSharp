#!/usr/bin/env python3
"""Render a Skia Analyst JSON report as a self-contained HTML file.

Usage: python3 render-skia-analyst.py <path-to-json> [output.html]
"""
import json
import os
import sys
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-skia-analyst.py <path-to-json> [output.html]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    output_path = Path(sys.argv[2]) if len(sys.argv) >= 3 else json_path.with_suffix(".html")

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

    template_path = Path(__file__).parent / "viewer.html"
    if not template_path.exists():
        print(f"❌ Template not found: {template_path}")
        sys.exit(1)

    with open(template_path, encoding="utf-8") as f:
        template = f.read()

    json_str = json.dumps(data, ensure_ascii=False)
    json_str = json_str.replace("</script>", "<\\/script>")

    marker = "/*INJECT_DATA_HERE*/null"
    if marker not in template:
        print(f"❌ Injection marker not found in template")
        sys.exit(1)

    html = template.replace(marker, json_str)

    meta = data.get("meta", {})
    ms = meta.get("currentMilestone", "?")
    date = meta.get("date", "?")
    mode = meta.get("scanMode", "full")
    html = html.replace("<title>Skia Analyst</title>",
                        f"<title>Skia Analyst — m{ms} ({mode}, {date})</title>")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_kb = os.path.getsize(output_path) / 1024
    summary = data.get("summary", {})
    total = summary.get("totalFindings", "?")
    missing = summary.get("byBindingStatus", {}).get("missing", 0)
    transformative = summary.get("byImpact", {}).get("transformative", 0)

    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   m{ms} • {date} • {total} findings • {missing} missing • {transformative} transformative")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
