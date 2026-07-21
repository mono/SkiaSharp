#!/usr/bin/env python3
"""Persist a validated repro JSON to output/ai/ and render report wrappers.

Copies the repro JSON to output/ai/repos/mono-SkiaSharp/ai-repro/{number}.json
and generates sidecar {number}.md and {number}.html files.

Usage:
    python3 persist-repro.py /tmp/{number}.json
"""

import re
import shutil
import subprocess
import sys
from pathlib import Path


def main() -> None:
    if len(sys.argv) < 2:
        print("Usage: python3 persist-repro.py <path-to-json>")
        sys.exit(1)

    src = Path(sys.argv[1])
    if not src.exists():
        print(f"❌ File not found: {src}")
        sys.exit(2)

    number = src.stem.removeprefix("repro-")
    if not re.fullmatch(r"\d+", number):
        print(f"❌ Cannot extract issue number from filename: {src.name}")
        print("   Expected format: {number}.json or repro-{number}.json")
        sys.exit(2)

    # Run validation before persisting — fail fast if the JSON is invalid
    validate_script = Path(__file__).parent / "validate-repro.py"
    if not validate_script.exists():
        print(f"❌ Validation script not found: {validate_script}")
        sys.exit(2)

    result = subprocess.run([sys.executable, str(validate_script), str(src)])
    if result.returncode != 0:
        print("❌ Validation failed — refusing to persist invalid repro JSON")
        sys.exit(result.returncode)

    dest_dir = Path("output/ai/repos/mono-SkiaSharp/ai-repro")
    dest_dir.mkdir(parents=True, exist_ok=True)
    dest = dest_dir / f"{number}.json"
    shutil.copy2(src, dest)
    print(f"✅ Copied JSON to {dest}")

    render_script = Path(__file__).parent / "render-repro-report.py"
    if render_script.exists():
        subprocess.run([sys.executable, str(render_script), str(dest)], check=True)
    else:
        print("⚠️  render-repro-report.py not found — skipping Markdown/HTML generation")


if __name__ == "__main__":
    main()
