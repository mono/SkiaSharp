#!/usr/bin/env python3
"""Verify checked-in generated binding trees without changing the worktree."""
from __future__ import annotations

import argparse
import filecmp
import json
import shutil
import subprocess
import sys
from pathlib import Path


PROJECTS = (
    ("libSkiaSharp.json", "externals/skia", "SkiaSharp", "SkiaApi.generated.cs"),
    ("libSkiaSharp.Skottie.json", "externals/skia", "SkiaSharp.Skottie", "SkottieApi.generated.cs"),
    ("libSkiaSharp.SceneGraph.json", "externals/skia", "SkiaSharp.SceneGraph", "SceneGraphApi.generated.cs"),
    ("libSkiaSharp.Resources.json", "externals/skia", "SkiaSharp.Resources", "ResourcesApi.generated.cs"),
    ("libHarfBuzzSharp.json", "externals/skia/third_party/externals/harfbuzz", "HarfBuzzSharp", "HarfBuzzApi.generated.cs"),
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


def run_check(
    repo_root: Path,
    output_dir: Path,
    prepared_generated_root: Path | None = None,
) -> dict:
    repo_root = Path(repo_root)
    output_dir = Path(output_dir)
    generated_root = output_dir / "generated"
    if generated_root.exists():
        shutil.rmtree(generated_root)
    if prepared_generated_root:
        if not prepared_generated_root.is_dir():
            raise RuntimeError(
                f"Prepared generated root does not exist: {prepared_generated_root}"
            )
        for path in prepared_generated_root.rglob("*"):
            if path.is_symlink():
                raise RuntimeError(
                    f"Prepared generated root contains a symlink: {path}"
                )
        shutil.copytree(prepared_generated_root, generated_root)
    else:
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
            if not prepared_generated_root:
                run("dotnet", "build", str(generator))
                for config, source_root, project, legacy_file in PROJECTS:
                    project_root = repo_root / "binding" / project
                    expected_directory = project_root / "Generated"
                    if expected_directory.is_dir():
                        generated_output = generated_root / project
                        shutil.copytree(expected_directory, generated_output)
                    else:
                        expected_file = project_root / legacy_file
                        generated_output = generated_root / project / legacy_file
                        generated_output.parent.mkdir(parents=True, exist_ok=True)
                        if expected_file.is_file():
                            shutil.copy2(expected_file, generated_output)
                    run(
                        "dotnet", "run", "--no-build", "--no-launch-profile",
                        f"--project={generator}", "--", "generate",
                        "--config", str(repo_root / "binding" / config),
                        "--root", str(repo_root / source_root),
                        "--output", str(generated_output),
                    )
        except Exception as error:
            return {
                "status": "FAIL", "checked": [], "mismatches": [],
                "generatorError": str(error), "generatorLog": str(log_path),
            }

    checked, mismatches = [], []
    for _, _, project, legacy_file in PROJECTS:
        project_root = repo_root / "binding" / project
        expected = project_root / "Generated"
        actual = generated_root / project
        if expected.is_dir():
            checked.append(str(expected.relative_to(repo_root)))
            differences = compare_trees(expected, actual)
        else:
            expected = project_root / legacy_file
            actual = actual / legacy_file
            checked.append(str(expected.relative_to(repo_root)))
            differences = (
                []
                if expected.is_file()
                and actual.is_file()
                and filecmp.cmp(expected, actual, shallow=False)
                else [f"modified: {legacy_file}"]
            )
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
