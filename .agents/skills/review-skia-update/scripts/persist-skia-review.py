#!/usr/bin/env python3
"""Copy a validated skia-review JSON to output/ai/ for collection and render HTML report.

Usage: python3 persist-skia-review.py /tmp/skiasharp/skia-review/20260320-164500/170.json
"""
import argparse
import shutil
import subprocess
import sys
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description="Validate and persist a Skia review with HTML.")
    parser.add_argument("path", type=Path)
    parser.add_argument("--output-dir", type=Path,
                        default=Path("output/ai/repos/mono-skia/ai-review"))
    args = parser.parse_args()
    path = args.path
    if not path.exists():
        print(f"❌ File not found: {path}")
        sys.exit(2)

    number = path.stem
    if not number.isdigit():
        print(f"❌ Cannot extract PR number from filename: {path.name}")
        sys.exit(2)

    # Validate before persisting
    validate_script = Path(__file__).parent / "validate-skia-review.py"
    if not validate_script.exists():
        print(f"❌ validate-skia-review.py not found: {validate_script}")
        sys.exit(2)
    result = subprocess.run(["python3", str(validate_script), str(path)])
    if result.returncode != 0:
        sys.exit(result.returncode)

    args.output_dir.mkdir(parents=True, exist_ok=True)
    dest = args.output_dir / f"{number}.json"
    if path.resolve() != dest.resolve():
        shutil.copy2(str(path), str(dest))
    print(f"✅ Copied to {dest}")

    # Render HTML report alongside the JSON
    render_script = Path(__file__).parent / "render-skia-review.py"
    if not render_script.exists():
        raise FileNotFoundError(f"render-skia-review.py not found: {render_script}")
    subprocess.run([sys.executable, str(render_script), str(dest)], check=True)


if __name__ == "__main__":
    main()
