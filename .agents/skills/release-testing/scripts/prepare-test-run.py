#!/usr/bin/env python3
"""Restore pinned tools and reset release integration-test output."""

import argparse
import json
from pathlib import Path
import shutil
import subprocess
import sys


OUTPUT_PATH = Path("output/logs/testlogs/integration")


class PreparationError(RuntimeError):
    pass


def run(args, *, cwd):
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
        )
    except FileNotFoundError as error:
        raise PreparationError(
            f"{args[0]} was not found on PATH"
        ) from error
    if result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise PreparationError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
        )
    return result


def reset_output(root: Path) -> Path:
    output = (root / OUTPUT_PATH).resolve()
    expected_parent = (root / "output/logs/testlogs").resolve()
    if output.parent != expected_parent:
        raise PreparationError(f"unexpected output path: {output}")
    output.mkdir(parents=True, exist_ok=True)
    for child in output.iterdir():
        if child.is_dir() and not child.is_symlink():
            shutil.rmtree(child)
        else:
            child.unlink()
    return output


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.parse_args()
    try:
        root = Path(
            run(
                ["git", "rev-parse", "--show-toplevel"],
                cwd=Path.cwd(),
            ).stdout.strip()
        )
        run(["dotnet", "tool", "restore"], cwd=root)
        run(
            ["dotnet", "tool", "run", "android", "--", "--help"],
            cwd=root,
        )
        tools = ["android"]
        if sys.platform == "darwin":
            run(
                ["dotnet", "tool", "run", "apple", "--", "--help"],
                cwd=root,
            )
            tools.append("apple")
        output = reset_output(root)
        print(
            json.dumps(
                {
                    "toolsRestored": True,
                    "toolsVerified": tools,
                    "outputDirectory": str(output),
                    "outputReset": True,
                },
                indent=2,
            )
        )
    except PreparationError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
