#!/usr/bin/env python3
"""Render a skia-review JSON report as a self-contained HTML file.

Usage:
    python3 render-skia-review.py <path-to-json> [output.html]

If output path is omitted, writes to the same directory as the JSON
with a .html extension (e.g., 171.json → 171.html).

Exit codes: 0=success, 1=error.
"""
import json
import os
import sys
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 render-skia-review.py <path-to-json> [output.html]")
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

    # Load HTML template
    template_path = Path(__file__).parent / "viewer.html"
    if not template_path.exists():
        print(f"❌ Template not found: {template_path}")
        sys.exit(1)

    with open(template_path, encoding="utf-8") as f:
        template = f.read()

    # Serialize JSON for injection — ensure it's safe for embedding in HTML
    # We need to escape </script> tags that might appear in diff content
    json_str = json.dumps(data, ensure_ascii=False)
    json_str = json_str.replace("</script>", "<\\/script>")
    json_str = json_str.replace("</Script>", "<\\/Script>")
    json_str = json_str.replace("</SCRIPT>", "<\\/SCRIPT>")

    # Build the data injection script tag
    data_script = f"<script>const DATA = {json_str};</script>"

    # Inject into template
    marker = "<!--INJECT_DATA_HERE-->"
    if marker not in template:
        print(f"❌ Injection marker '{marker}' not found in template")
        sys.exit(1)

    html = template.replace(marker, data_script)

    # Update the page title with PR info
    meta = data.get("meta", {})
    pr_num = meta.get("skiaPrNumber", "?")
    branch = meta.get("upstreamBranch", "")
    milestone = branch.replace("chrome/", "") if branch else ""
    title = f"Skia Review — PR #{pr_num}"
    if milestone:
        title += f" ({milestone})"
    html = html.replace("<title>Skia Review Report</title>", f"<title>{title}</title>")

    # Write output
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_kb = os.path.getsize(output_path) / 1024
    print(f"✅ {output_path.name} ({size_kb:.0f} KB)")
    print(f"   PR #{pr_num} • {meta.get('oldUpstreamBranch', '?')} → {meta.get('upstreamBranch', '?')} • Risk: {data.get('riskAssessment', '?')}")
    print(f"   Output: {output_path}")


if __name__ == "__main__":
    main()
