#!/usr/bin/env python3

"""Regenerate and review the maintained native bindings during Phase 08.

This helper runs every generator configuration and reports new P/Invoke
functions that may need a hand-written wrapper decision. The generator itself
owns deterministic ordering across hosts.
"""

from __future__ import annotations

import argparse
import shutil
import subprocess
from pathlib import Path


PROJECTS = (
    (
        "libSkiaSharp.json",
        "externals/skia",
        "SkiaSharp/Generated",
    ),
    (
        "libSkiaSharp.Skottie.json",
        "externals/skia",
        "SkiaSharp.Skottie/Generated",
    ),
    (
        "libSkiaSharp.SceneGraph.json",
        "externals/skia",
        "SkiaSharp.SceneGraph/Generated",
    ),
    (
        "libSkiaSharp.Resources.json",
        "externals/skia",
        "SkiaSharp.Resources/Generated",
    ),
    (
        "libHarfBuzzSharp.json",
        "externals/skia/third_party/externals/harfbuzz",
        "HarfBuzzSharp/Generated",
    ),
)


def run(repo_root: Path, *args: str, capture: bool = False) -> str:
    result = subprocess.run(
        [*args],
        cwd=repo_root,
        check=True,
        capture_output=capture,
        text=True,
    )
    return result.stdout if capture else ""


def select_projects(config: str | None):
    """Select one generator config or the complete maintained binding set."""
    if config is None:
        return PROJECTS
    name = Path(config).name
    selected = tuple(project for project in PROJECTS if project[0] == name)
    if not selected:
        valid = ", ".join(project[0] for project in PROJECTS)
        raise ValueError(f"Unknown config '{config}'. Valid configs: {valid}")
    return selected


def added_internal_functions(diff: str) -> list[str]:
    """Extract newly generated P/Invoke declarations from a Git diff."""
    return [
        line[1:].strip()
        for line in diff.splitlines()
        if line.startswith("+") and not line.startswith("+++") and "internal static" in line
    ]


def added_internal_functions_from_diffs(diffs: list[str]) -> list[str]:
    """Collect newly generated P/Invokes from every selected binding output."""
    return [
        function
        for diff in diffs
        for function in added_internal_functions(diff)
    ]


def generated_file_changes(repo_root: Path, projects) -> list[str]:
    """List added, modified, and deleted generated files across every output tree."""
    return [
        line
        for _, _, output in projects
        for line in run(
            repo_root,
            "git",
            "diff",
            "--name-status",
            "--",
            f"binding/{output}",
            capture=True,
        ).splitlines()
    ]


def regenerate(repo_root: Path, config: str | None = None) -> None:
    """Run every selected generator and summarize wrapper work."""
    generator_project = (
        repo_root / "utils" / "SkiaSharpGenerator" / "SkiaSharpGenerator.csproj"
    )
    generated_directory = repo_root / "output" / "generated"
    generated_directory.mkdir(parents=True, exist_ok=True)
    projects = select_projects(config)

    run(repo_root, "dotnet", "build", str(generator_project))
    for config_name, source_root, output in projects:
        output_path = repo_root / "binding" / output
        command = (
            "dotnet",
            "run",
            "--no-build",
            "--no-launch-profile",
            f"--project={generator_project}",
            "--",
            "generate",
            "--config",
            str(repo_root / "binding" / config_name),
            "--root",
            str(repo_root / source_root),
            "--output",
            str(output_path),
        )
        print(" ".join(str(part) for part in command))
        run(repo_root, *command)
        destination = generated_directory / output_path.parent.name
        if destination.exists():
            shutil.rmtree(destination)
        shutil.copytree(output_path, destination)

    changes = generated_file_changes(repo_root, projects)
    print("Generated binding file changes:")
    print("\n".join(f"  {change}" for change in changes) or "  No generated binding changes.")

    diffs = [
        run(repo_root, "git", "diff", "--", f"binding/{output}", capture=True)
        for _, _, output in projects
    ]
    functions = added_internal_functions_from_diffs(diffs)
    if functions:
        print("New generated functions requiring wrapper review:")
        for function in functions:
            print(f"  {function}")
    else:
        print("No new generated functions.")

    print("GATE PASSED: binding regeneration completed.")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Regenerate SkiaSharp bindings and report wrapper work."
    )
    parser.add_argument("--config")
    parser.add_argument("--repo-root", type=Path)
    args = parser.parse_args()
    repo_root = (
        args.repo_root.resolve()
        if args.repo_root
        else Path(__file__).resolve().parents[4]
    )
    regenerate(repo_root, args.config)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
