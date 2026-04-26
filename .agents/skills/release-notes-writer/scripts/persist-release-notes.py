#!/usr/bin/env python3
"""Persist a validated release notes JSON to output/ai/ and render reports.

Copies the JSON to output/ai/repos/mono-SkiaSharp/ai-release-notes/{refFrom}..{refTo}.json
and generates sidecar .md and .html files.

Usage:
    python3 persist-release-notes.py <path-to-json>
"""

import json
import re
import shutil
import subprocess
import sys
from pathlib import Path


def sanitize_ref(ref: str) -> str:
    """Sanitize a git ref for use in filenames."""
    return re.sub(r"[^a-zA-Z0-9._-]", "_", ref)


def main() -> None:
    if len(sys.argv) < 2:
        print("Usage: python3 persist-release-notes.py <path-to-json>")
        sys.exit(1)

    src = Path(sys.argv[1])
    if not src.exists():
        print(f"❌ File not found: {src}")
        sys.exit(2)

    # Run validation before persisting
    validate_script = Path(__file__).parent / "validate-release-notes.py"
    if not validate_script.exists():
        print(f"❌ Validation script not found: {validate_script}")
        sys.exit(2)

    result = subprocess.run([sys.executable, str(validate_script), str(src)])
    if result.returncode != 0:
        print("❌ Validation failed — refusing to persist invalid release notes JSON")
        sys.exit(result.returncode)

    with open(src) as f:
        data = json.load(f)

    meta = data.get("meta", {})
    ref_from = sanitize_ref(meta.get("refFrom", "unknown"))
    ref_to = sanitize_ref(meta.get("refTo", "unknown"))
    basename = f"{ref_from}..{ref_to}"

    dest_dir = Path("output/ai/repos/mono-SkiaSharp/ai-release-notes")
    dest_dir.mkdir(parents=True, exist_ok=True)
    dest = dest_dir / f"{basename}.json"
    shutil.copy2(src, dest)
    print(f"✅ Copied JSON to {dest}")

    # Write slides markdown
    slides_md = dest_dir / f"{basename}.slides.md"
    slides_content = data.get("slides", "")
    if slides_content:
        with open(slides_md, "w", encoding="utf-8") as f:
            f.write(f"# What's New in SkiaSharp ({meta.get('refTo', '?')})\n\n")
            f.write(slides_content)
        print(f"✅ Slides: {slides_md.name}")

    # Write changelog markdown
    changelog_md = dest_dir / f"{basename}.changelog.md"
    changelog_content = data.get("changelog", "")
    if changelog_content:
        with open(changelog_md, "w", encoding="utf-8") as f:
            f.write(f"# SkiaSharp Changelog: {meta.get('refFrom', '?')} → {meta.get('refTo', '?')}\n\n")
            f.write(changelog_content)
        print(f"✅ Changelog: {changelog_md.name}")

    # Render HTML
    render_script = Path(__file__).parent / "render-release-notes.py"
    if render_script.exists():
        html_path = dest_dir / f"{basename}.html"
        subprocess.run(
            [sys.executable, str(render_script), str(dest), str(html_path)],
            check=True
        )
    else:
        print("⚠️  render-release-notes.py not found — skipping HTML generation")


if __name__ == "__main__":
    main()
