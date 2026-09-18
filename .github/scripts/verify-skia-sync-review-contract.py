#!/usr/bin/env python3
"""Verify frozen and live Skia sync contracts against trusted scalar inputs."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


class ContractError(ValueError):
    """A contract is stale or has been changed."""


def load(path: Path):
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ContractError(f"{path.name} must be a JSON object.")
    return value


def verify(contract, expected):
    if contract.get("schema_version") != 1 or contract.get("mode") != "review":
        raise ContractError("Contract is not a schema-v1 review contract.")
    for key in ("milestone", "head_branch", "repositories", "links"):
        if contract.get(key) != expected.get(key):
            raise ContractError(f"Contract {key} differs from frozen input.")
    for side in ("parent", "native"):
        actual = contract.get(side)
        frozen = expected.get(side)
        if not isinstance(actual, dict) or not isinstance(frozen, dict):
            raise ContractError(f"Contract {side} is missing.")
        for key in ("number", "head_branch", "head_sha", "base_branch", "base_sha"):
            if actual.get(key) != frozen.get(key):
                raise ContractError(f"Contract {side}.{key} differs from frozen input.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--expected", type=Path, required=True)
    parser.add_argument("--frozen", type=Path, required=True)
    parser.add_argument("--live", type=Path, required=True)
    args = parser.parse_args()
    try:
        expected = load(args.expected)
        verify(load(args.frozen), expected)
        verify(load(args.live), expected)
    except (ContractError, OSError, json.JSONDecodeError) as error:
        print(f"verify-skia-sync-review-contract: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
