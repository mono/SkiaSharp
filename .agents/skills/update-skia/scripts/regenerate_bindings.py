#!/usr/bin/env python3

"""Regenerate and review the maintained native bindings during Phase 08.

This helper runs every generator configuration and reports new P/Invoke
functions that may need a hand-written wrapper decision. The generator itself
owns deterministic ordering across hosts.
"""

import argparse
import shutil
import subprocess
from pathlib import Path


PROJECTS = (
    (
        "libSkiaSharp.json",
        "externals/skia",
        "SkiaSharp/SkiaApi.generated.cs",
    ),
    (
        "libSkiaSharp.Skottie.json",
        "externals/skia",
        "SkiaSharp.Skottie/SkottieApi.generated.cs",
    ),
    (
        "libSkiaSharp.SceneGraph.json",
        "externals/skia",
        "SkiaSharp.SceneGraph/SceneGraphApi.generated.cs",
    ),
    (
        "libSkiaSharp.Resources.json",
        "externals/skia",
        "SkiaSharp.Resources/ResourcesApi.generated.cs",
    ),
    (
        "libHarfBuzzSharp.json",
        "externals/skia/third_party/externals/harfbuzz",
        "HarfBuzzSharp/HarfBuzzApi.generated.cs",
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


def function_regions(text: str) -> tuple[str, list[str], dict[str, str], str]:
    lines = text.splitlines(keepends=True)
    starts = [
        index for index, line in enumerate(lines) if line.startswith("\t\t#region ")
    ]
    if not starts:
        return text, [], {}, ""

    blocks = {}
    order = []
    last_end = None
    for start in starts:
        name = lines[start].strip().removeprefix("#region ")
        if name in blocks:
            raise ValueError(f"Duplicate generated function region: {name}")
        end = next(
            (
                index + 1
                for index in range(start + 1, len(lines))
                if lines[index].startswith("\t\t#endregion")
            ),
            None,
        )
        if end is None:
            raise ValueError(f"Unterminated generated function region: {name}")
        blocks[name] = "".join(lines[start:end])
        order.append(name)
        last_end = end

    return "".join(lines[: starts[0]]), order, blocks, "".join(lines[last_end:])


def preserve_function_region_order(original: str, generated: str) -> str:
    _, original_order, _, _ = function_regions(original)
    prefix, generated_order, generated_blocks, suffix = function_regions(generated)
    if not original_order or not generated_order:
        return generated

    order = [name for name in original_order if name in generated_blocks]
    order.extend(sorted(name for name in generated_blocks if name not in original_order))
    return prefix + "\n".join(generated_blocks[name] for name in order) + suffix


def regenerate(repo_root: Path, config: str | None = None) -> None:
    """Run every selected generator and summarize wrapper work."""
    generator_project = (
        repo_root / "utils" / "SkiaSharpGenerator" / "SkiaSharpGenerator.csproj"
    )
    generated_directory = repo_root / "output" / "generated"
    generated_directory.mkdir(parents=True, exist_ok=True)

    run(repo_root, "dotnet", "build", str(generator_project))
    for config_name, source_root, output in select_projects(config):
        output_path = repo_root / "binding" / output
        original = output_path.read_text(encoding="utf-8") if output_path.exists() else None
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
        if original is not None:
            generated = output_path.read_text(encoding="utf-8")
            output_path.write_text(
                preserve_function_region_order(original, generated),
                encoding="utf-8",
            )
        shutil.copy2(output_path, generated_directory / output_path.name)

    harfbuzz = "binding/HarfBuzzSharp/HarfBuzzApi.generated.cs"
    harfbuzz_status = subprocess.run(
        ["git", "diff", "--quiet", "--", harfbuzz],
        cwd=repo_root,
        check=False,
    ).returncode
    if harfbuzz_status == 1:
        run(repo_root, "git", "restore", "--source=HEAD", "--", harfbuzz)
        print(f"Reverted {harfbuzz}; HarfBuzz updates are separate.")
    elif harfbuzz_status != 0:
        raise RuntimeError("Could not inspect the HarfBuzz binding diff.")

    binding_stat = run(repo_root, "git", "diff", "--stat", "--", "binding/", capture=True)
    print("Binding diff summary:")
    print(binding_stat.rstrip() or "  No binding changes.")

    skia_diff = run(
        repo_root,
        "git",
        "diff",
        "--",
        "binding/SkiaSharp/SkiaApi.generated.cs",
        capture=True,
    )
    functions = added_internal_functions(skia_diff)
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
