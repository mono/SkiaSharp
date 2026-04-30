#!/usr/bin/env python3
"""Copy a validated skia-review JSON to output/ai/ for collection and render HTML report.

Usage: python3 persist-skia-review.py /tmp/skiasharp/skia-review/20260320-164500/170.json
"""
import os
import shutil
import subprocess
import sys
from pathlib import Path


def main():
    if len(sys.argv) < 2:
        print("Usage: python3 persist-skia-review.py <path-to-json>")
        sys.exit(2)

    path = Path(sys.argv[1])
    if not path.exists():
        print(f"❌ File not found: {path}")
        sys.exit(2)

    number = path.stem
    if not number.isdigit():
        print(f"❌ Cannot extract PR number from filename: {path.name}")
        sys.exit(2)

    dest_dir = Path("output/ai/repos/mono-skia/ai-review")
    dest_dir.mkdir(parents=True, exist_ok=True)

    # Validate before persisting
    validate_script = Path(__file__).parent / "validate-skia-review.py"
    if not validate_script.exists():
        print(f"❌ validate-skia-review.py not found: {validate_script}")
        sys.exit(2)
    result = subprocess.run(["python3", str(validate_script), str(path)])
    if result.returncode != 0:
        sys.exit(result.returncode)

    dest = dest_dir / f"{number}.json"
    shutil.copy2(str(path), str(dest))
    print(f"✅ Copied to {dest}")

    # Render HTML report alongside the JSON
    render_script = Path(__file__).parent / "render-skia-review.py"
    if render_script.exists():
        subprocess.run(["python3", str(render_script), str(dest)])
    else:
        print("⚠️  render-skia-review.py not found — skipping HTML generation")


if __name__ == "__main__":
    main()
