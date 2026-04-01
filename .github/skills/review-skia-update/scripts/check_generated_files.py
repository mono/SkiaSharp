#!/usr/bin/env python3
"""Verify that generated P/Invoke files match independent regeneration.

Assumes the working tree is already checked out to the correct state
(SkiaSharp companion PR + skia submodule at PR head — done by the orchestrator).
Runs utils/generate.ps1, then uses git diff to check if the regenerated
files match what's checked in. Any diff = FAIL.
HarfBuzzSharp is excluded — harfbuzz updates are never part of a Skia update,
so the generated file is reverted to HEAD before comparison.
"""
import argparse
import os
import subprocess
import sys

GENERATED_FILES = [
    "binding/SkiaSharp/SkiaApi.generated.cs",
    "binding/SkiaSharp.Skottie/SkottieApi.generated.cs",
    "binding/SkiaSharp.SceneGraph/SceneGraphApi.generated.cs",
    "binding/SkiaSharp.Resources/ResourcesApi.generated.cs",
]

HARFBUZZ_GENERATED_FILE = "binding/HarfBuzzSharp/HarfBuzzApi.generated.cs"


def eprint(*args, **kwargs):
    """Print to stderr (status messages)."""
    print(*args, file=sys.stderr, **kwargs)


def run_check(repo_root: str, output_dir: str) -> dict:
    """Run generated file verification. Returns result dict."""
    os.makedirs(output_dir, exist_ok=True)
    generator_log = os.path.join(output_dir, "generator-output.log")

    # --- Step 1: Run the generator and capture raw output as proof of execution ---
    eprint(f"🔄 Running utils/generate.ps1 (capturing output to {generator_log})...")
    try:
        proc = subprocess.Popen(
            ["pwsh", "./utils/generate.ps1"],
            cwd=repo_root,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
        )
        try:
            with open(generator_log, "w") as log_file:
                for line in proc.stdout:
                    decoded = line.decode("utf-8", errors="replace")
                    sys.stderr.write(decoded)
                    log_file.write(decoded)
            proc.wait(timeout=600)
        except subprocess.TimeoutExpired:
            proc.kill()
            proc.wait()
            eprint("❌ Generator timed out after 600 seconds")
            return {
                "status": "FAIL",
                "checked": [],
                "mismatches": [],
                "generatorError": "Generator timed out after 600 seconds",
                "generatorLog": generator_log,
            }
        except Exception:
            proc.kill()
            proc.wait()
            raise
        if proc.returncode != 0:
            eprint(f"❌ Generator exited with code {proc.returncode}")
            eprint(f"   See {generator_log} for full output")
            return {
                "status": "FAIL",
                "checked": [],
                "mismatches": [],
                "generatorError": f"Generator exited with code {proc.returncode}",
                "generatorLog": generator_log,
            }
    except Exception as e:
        eprint(f"❌ Generator threw exception: {e}")
        with open(generator_log, "a") as log_file:
            log_file.write(f"\nException: {e}\n")
        return {
            "status": "FAIL",
            "checked": [],
            "mismatches": [],
            "generatorError": f"Generator threw exception: {e}",
            "generatorLog": generator_log,
        }

    # --- Step 2: Revert HarfBuzz to HEAD (not part of Skia updates) ---
    eprint(f"🔄 Reverting {HARFBUZZ_GENERATED_FILE} to HEAD (harfbuzz excluded from Skia updates)")
    revert_result = subprocess.run(
        ["git", "checkout", "HEAD", "--", HARFBUZZ_GENERATED_FILE],
        cwd=repo_root,
        capture_output=True,
        text=True,
    )
    if revert_result.returncode != 0:
        eprint(f"❌ Failed to revert {HARFBUZZ_GENERATED_FILE}: {revert_result.stderr.strip()}")
        return {
            "status": "FAIL",
            "checked": [],
            "mismatches": [],
            "generatorError": f"Failed to revert HarfBuzz generated file: {revert_result.stderr.strip()}",
            "generatorLog": generator_log,
        }

    # --- Step 3: git diff each generated file — any diff = FAIL ---
    checked = []
    mismatches = []

    for file in GENERATED_FILES:
        checked.append(file)
        result = subprocess.run(
            ["git", "diff", "--no-ext-diff", "--", file],
            cwd=repo_root,
            capture_output=True,
            text=True,
        )
        diff_str = result.stdout.strip()
        if diff_str:
            summary = diff_str[:2000] if len(diff_str) > 2000 else diff_str
            mismatches.append({"file": file, "diffSummary": summary})

    # Revert generated files to HEAD so the check is non-destructive
    for file in GENERATED_FILES:
        revert = subprocess.run(
            ["git", "checkout", "HEAD", "--", file],
            cwd=repo_root,
            capture_output=True,
            text=True,
        )
        if revert.returncode != 0:
            eprint(f"⚠️ Failed to revert {file}: {revert.stderr.strip()}")

    status = "PASS" if len(mismatches) == 0 else "FAIL"

    eprint()
    if status == "PASS":
        eprint(f"✅ Generated files: PASS — all {len(checked)} files match after regeneration")
    else:
        eprint(f"❌ Generated files: FAIL — {len(mismatches)} file(s) differ after regeneration")
        for m in mismatches:
            eprint(f"   {m['file']}")

    return {
        "status": status,
        "checked": checked,
        "mismatches": mismatches,
        "generatorLog": generator_log,
    }


def main():
    parser = argparse.ArgumentParser(description="Verify generated P/Invoke files match regeneration.")
    parser.add_argument("--repo-root", required=True, help="Path to the SkiaSharp repo root.")
    parser.add_argument("--output-dir", default="/tmp/skiasharp/skia-review", help="Directory for results.")
    args = parser.parse_args()

    import json
    result = run_check(args.repo_root, args.output_dir)
    json.dump(result, sys.stdout, indent=2)
    print()


if __name__ == "__main__":
    main()
