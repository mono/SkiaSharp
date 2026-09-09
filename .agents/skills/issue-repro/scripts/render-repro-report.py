#!/usr/bin/env python3
"""Render an issue-repro JSON report to sidecar Markdown and HTML files.

Usage:
    python3 render-repro-report.py <path-to-json> [output.html]

If output path is omitted, writes to the same directory as the JSON using the
same basename (e.g., 3400.json -> 3400.md and 3400.html).

Markdown is rendered via a Jinja2 template (repro-report.md.jinja2).
HTML is rendered by injecting JSON into the viewer.html template.
"""

from __future__ import annotations

import json
import os
import sys
from pathlib import Path
from typing import Any

try:
    from jinja2 import Environment, FileSystemLoader
except ImportError:
    print("❌ jinja2 is required: pip3 install jinja2")
    sys.exit(1)

SCRIPTS_DIR = Path(__file__).parent


# ── Jinja2 filters ──────────────────────────────────────────────────────────

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


# ── Markdown rendering (Jinja2 template) ────────────────────────────────────

def render_markdown(data: dict[str, Any]) -> str:
    env = Environment(
        loader=FileSystemLoader(str(SCRIPTS_DIR)),
        trim_blocks=True,
        lstrip_blocks=True,
        keep_trailing_newline=True,
    )
    env.filters["fmt_confidence"] = fmt_confidence
    env.filters["fmt_list"] = fmt_list

    template = env.get_template("repro-report.md.jinja2")
    return template.render(
        meta=data.get("meta", {}),
        conclusion=data.get("conclusion", "—"),
        notes=data.get("notes", "—"),
        reproductionSteps=data.get("reproductionSteps", []),
        environment=data.get("environment", {}),
        reproProject=data.get("reproProject"),
        versionResults=data.get("versionResults", []),
        errorMessages=data.get("errorMessages"),
        scope=data.get("scope"),
        feedback=data.get("feedback"),
        blockers=data.get("blockers"),
        output=data.get("output"),
        raw_json=json.dumps(data, indent=2, ensure_ascii=False),
    )


# ── HTML rendering (viewer.html injection) ──────────────────────────────────

def render_html(data: dict[str, Any]) -> str:
    template_path = SCRIPTS_DIR / "viewer.html"
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
    conclusion = data.get("conclusion", "?")
    html_doc = html_doc.replace(
        "<title>Issue Reproduction Report</title>",
        f"<title>Issue Repro — #{number} ({conclusion})</title>",
    )
    return html_doc


# ── Main ─────────────────────────────────────────────────────────────────────

def main() -> None:
    if len(sys.argv) < 2:
        print("Usage: python3 render-repro-report.py <path-to-json> [output.html]")
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

    for key in ("meta", "conclusion", "notes", "reproductionSteps", "environment"):
        if key not in data:
            print(f"❌ Missing required key: {key}")
            sys.exit(1)

    markdown = render_markdown(data)
    html_doc = render_html(data)

    md_path.parent.mkdir(parents=True, exist_ok=True)
    with open(md_path, "w", encoding="utf-8") as f:
        f.write(markdown)

    html_path.parent.mkdir(parents=True, exist_ok=True)
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html_doc)

    meta = data.get("meta", {})
    number = meta.get("number", "?")
    conclusion = data.get("conclusion", "?")
    print(f"✅ {md_path.name} ({os.path.getsize(md_path) / 1024:.0f} KB)")
    print(f"✅ {html_path.name} ({os.path.getsize(html_path) / 1024:.0f} KB)")
    print(f"   Issue #{number} • {conclusion}")
    print(f"   Output: {json_path.with_suffix('').as_posix()}.[json|md|html]")


if __name__ == "__main__":
    main()
