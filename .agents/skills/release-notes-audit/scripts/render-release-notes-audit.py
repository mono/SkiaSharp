#!/usr/bin/env python3
"""Render a release-notes-audit JSON report as a self-contained HTML file.

Usage:
    python3 render-release-notes-audit.py <path-to-json> [output.html]

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
        print("Usage: python3 render-release-notes-audit.py <path-to-json> [output.html]")
        sys.exit(1)

    json_path = Path(sys.argv[1])
    if not json_path.exists():
        print(f"❌ JSON file not found: {json_path}")
        sys.exit(1)

    # Determine output path
    if len(sys.argv) >= 3:
        output_path = Path(sys.argv[2])
    else:
        output_path = json_path.with_suffix(".html")

    # Load JSON data
    try:
        with open(json_path, encoding="utf-8") as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"❌ Invalid JSON: {e}")
        sys.exit(1)

    # Validate required fields
    for key in ("meta", "summary", "items"):
        if key not in data:
            print(f"❌ Missing required key: {key}")
            sys.exit(1)

    # Ensure optional keys have defaults
    data.setdefault("deprecations", [])
    data.setdefault("nextSteps", [])

    # Load HTML template
    template_path = Path(__file__).parent / "viewer.html"
    if not template_path.exists():
        print(f"❌ Template not found: {template_path}")
        sys.exit(1)

    with open(template_path, encoding="utf-8") as f:
        template = f.read()

    # Serialize JSON for injection (escape </script> tags)
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

    # Update title
    meta = data.get("meta", {})
    prev_ms = meta.get("previousMilestone", "?")
    curr_ms = meta.get("currentMilestone", "?")
    date = meta.get("date", "?")
    html = html.replace(
        "<title>Release Notes Audit</title>",
        f"<title>Release Notes Audit — m{prev_ms}→m{curr_ms} ({date})</title>"
    )

    # Write output
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_kb = os.path.getsize(output_path) / 1024
    summary = data.get("summary", {})
    total = summary.get("totalItems", "?")
    missing = summary.get("missing", 0)
    action = summary.get("actionNeeded", 0)
    full = summary.get("full", 0)

    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   m{prev_ms} → m{curr_ms} • {date} • {total} items")
    print(f"   ❌ {missing} missing · ⚠️ {action} action needed · ✅ {full} full")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
