#!/usr/bin/env python3
"""Restore pinned tools and reset release integration-test output."""

import argparse
import json
from pathlib import Path
import shutil
import sys

import release_test_common as common


OUTPUT_PATH = Path("output/logs/testlogs/integration")


PreparationError = common.ReleaseTestError
run = common.run_checked


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
        root = common.repository_root(cwd=Path.cwd())
        run(["dotnet", "tool", "restore"], cwd=root)
        output = reset_output(root)
        print(
            json.dumps(
                {
                    "toolsRestored": True,
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
