#!/usr/bin/env python3
"""Persist a validated Sample Scout JSON to output/ai/ and render HTML.

Usage:
    python3 persist-sample-scout.py <path-to-json>
"""
import shutil
import subprocess
import sys
from pathlib import Path


def main() -> None:
    if len(sys.argv) < 2:
        print("Usage: python3 persist-sample-scout.py <path-to-json>")
        sys.exit(1)

    src = Path(sys.argv[1])
    if not src.exists():
        print(f"❌ File not found: {src}")
        sys.exit(2)

    # Validate first
    validate_script = Path(__file__).parent / "validate-sample-scout.py"
    if validate_script.exists():
        result = subprocess.run([sys.executable, str(validate_script), str(src)])
        if result.returncode != 0:
            print("❌ Validation failed — refusing to persist")
            sys.exit(result.returncode)

    dest_dir = Path("output/ai/repos/mono-SkiaSharp/ai-sample-scout")
    dest_dir.mkdir(parents=True, exist_ok=True)
    dest = dest_dir / src.name
    shutil.copy2(src, dest)
    print(f"✅ Copied JSON to {dest}")

    # Render HTML
    render_script = Path(__file__).parent / "render-sample-scout.py"
    if render_script.exists():
        html_path = dest.with_suffix(".html")
        subprocess.run(
            [sys.executable, str(render_script), str(dest), str(html_path)],
            check=True
        )
    else:
        print("⚠️  render-sample-scout.py not found — skipping HTML generation")


if __name__ == "__main__":
    main()
