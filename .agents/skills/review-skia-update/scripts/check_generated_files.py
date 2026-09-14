#!/usr/bin/env python3
"""Verify checked-in generated binding trees without changing the worktree."""
import argparse
import filecmp
import json
import shutil
import subprocess
import sys
from pathlib import Path


PROJECTS = (
    ("libSkiaSharp.json", "externals/skia", "SkiaSharp"),
    ("libSkiaSharp.Skottie.json", "externals/skia", "SkiaSharp.Skottie"),
    ("libSkiaSharp.SceneGraph.json", "externals/skia", "SkiaSharp.SceneGraph"),
    ("libSkiaSharp.Resources.json", "externals/skia", "SkiaSharp.Resources"),
    ("libHarfBuzzSharp.json", "externals/skia/third_party/externals/harfbuzz", "HarfBuzzSharp"),
)


def compare_trees(expected: Path, actual: Path) -> list[str]:
    comparison = filecmp.dircmp(expected, actual)
    differences = [
        *(f"deleted: {path}" for path in comparison.left_only),
        *(f"added: {path}" for path in comparison.right_only),
        *(
            f"modified: {path}"
            for path in comparison.common_files
            if not filecmp.cmp(expected / path, actual / path, shallow=False)
        ),
    ]
    for directory in comparison.common_dirs:
        differences.extend(
            f"{directory}/{difference}"
            for difference in compare_trees(expected / directory, actual / directory)
        )
    return differences


def run_check(repo_root: Path, output_dir: Path) -> dict:
    generated_root = output_dir / "generated"
    if generated_root.exists():
        shutil.rmtree(generated_root)
    generated_root.mkdir(parents=True)
    generator = repo_root / "utils" / "SkiaSharpGenerator" / "SkiaSharpGenerator.csproj"
    log_path = output_dir / "generator-output.log"
    output_dir.mkdir(parents=True, exist_ok=True)

    with log_path.open("w") as log:
        def run(*args: str) -> None:
            process = subprocess.run(args, cwd=repo_root, stdout=log, stderr=subprocess.STDOUT, text=True)
            if process.returncode:
                raise RuntimeError(f"Generator command failed: {' '.join(args)}")

        try:
            run("dotnet", "build", str(generator))
            for config, source_root, project in PROJECTS:
                run(
                    "dotnet", "run", "--no-build", "--no-launch-profile",
                    f"--project={generator}", "--", "generate",
                    "--config", str(repo_root / "binding" / config),
                    "--root", str(repo_root / source_root),
                    "--output", str(generated_root / project),
                )
        except Exception as error:
            return {
                "status": "FAIL", "checked": [], "mismatches": [],
                "generatorError": str(error), "generatorLog": str(log_path),
            }

    checked, mismatches = [], []
    for _, _, project in PROJECTS:
        expected = repo_root / "binding" / project / "Generated"
        actual = generated_root / project
        checked.append(str(expected.relative_to(repo_root)))
        differences = compare_trees(expected, actual)
        if differences:
            mismatches.append({"file": str(expected.relative_to(repo_root)), "diffSummary": "\n".join(differences[:100])})

    return {
        "status": "PASS" if not mismatches else "FAIL",
        "checked": checked,
        "mismatches": mismatches,
        "generatorLog": str(log_path),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Verify generated P/Invoke binding trees.")
    parser.add_argument("--repo-root", required=True, type=Path, help="Path to the SkiaSharp repo root.")
    parser.add_argument("--output-dir", type=Path, default=Path("output/skia-review"), help="Directory for results.")
    args = parser.parse_args()
    result = run_check(args.repo_root.resolve(), args.output_dir.resolve())
    json.dump(result, sys.stdout, indent=2)
    print()
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
