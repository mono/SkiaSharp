#!/usr/bin/env python3
"""Resolve and validate the two pull requests that make up a Skia sync."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Any


SKIASHARP_REPOSITORY = "mono/SkiaSharp"
SKIA_REPOSITORY = "mono/skia"
MILESTONE_BRANCH_RE = re.compile(r"^skia-sync/m([1-9][0-9]*)$")
SKIA_SYNC_BRANCH_RE = re.compile(r"^skia-sync/[A-Za-z0-9._-]+$")
MILESTONE_TEXT_RE = re.compile(r"\bchrome/m([1-9][0-9]*)\b", re.IGNORECASE)
RELEASE_BRANCH_RE = re.compile(r"^release/[0-9]+\.[0-9]+\.x$")


class PairValidationError(ValueError):
    """The two pull requests do not form a safe Skia sync pair."""


def _field(pr: dict[str, Any], path: str) -> Any:
    value: Any = pr
    for part in path.split("."):
        if not isinstance(value, dict) or part not in value:
            raise PairValidationError(f"PR JSON is missing {path}.")
        value = value[part]
    return value


def _require_string(pr: dict[str, Any], path: str) -> str:
    value = _field(pr, path)
    if not isinstance(value, str) or not value:
        raise PairValidationError(f"PR JSON field {path} must be a nonempty string.")
    return value


def _require_sha(pr: dict[str, Any], path: str, *, nullable: bool = False) -> str | None:
    value = _field(pr, path)
    if nullable and value is None:
        return None
    if not isinstance(value, str) or not re.fullmatch(r"[0-9a-f]{40}", value):
        raise PairValidationError(f"PR JSON field {path} must be a lowercase full SHA.")
    return value


def _required_link(body: object, repository: str) -> int:
    if not isinstance(body, str):
        raise PairValidationError("PR body must be text.")
    owner = repository.rsplit("/", 1)[1]
    exact_url = re.compile(
        rf"(?<![A-Za-z0-9_/-])https://github\.com/mono/{re.escape(owner)}/pull/"
        r"([1-9][0-9]*)(?![A-Za-z0-9_/-])"
    )
    matches = [int(match.group(1)) for match in exact_url.finditer(body)]
    if len(matches) != 1:
        raise PairValidationError(
            f"Expected exactly one HTTPS pull-request link to {repository}; found {len(matches)}."
        )
    return matches[0]


def _state(pr: dict[str, Any]) -> str:
    value = _require_string(pr, "state").lower()
    if value not in {"open", "closed"}:
        raise PairValidationError(f"Unsupported PR state {value!r}.")
    return value


def _is_merged(pr: dict[str, Any]) -> bool:
    return _field(pr, "merged_at") is not None


def _pr_contract(pr: dict[str, Any]) -> dict[str, Any]:
    return {
        "number": _field(pr, "number"),
        "url": _require_string(pr, "html_url"),
        "title": _require_string(pr, "title"),
        "author": _require_string(pr, "user.login"),
        "head_branch": _require_string(pr, "head.ref"),
        "head_sha": _require_sha(pr, "head.sha"),
        "base_branch": _require_string(pr, "base.ref"),
        "base_sha": _require_sha(pr, "base.sha"),
        "state": _state(pr),
        "draft": bool(_field(pr, "draft")),
        "merged": _is_merged(pr),
        "merge_sha": _require_sha(pr, "merge_commit_sha", nullable=True),
    }


def resolve_pair(
    parent: dict[str, Any], native: dict[str, Any], mode: str
) -> dict[str, Any]:
    """Validate REST PR JSON and return a small, shell-safe contract."""

    if mode not in {"review", "merge"}:
        raise PairValidationError("Mode must be review or merge.")
    if _require_string(parent, "base.repo.full_name") != SKIASHARP_REPOSITORY:
        raise PairValidationError("Control PR must be a mono/SkiaSharp pull request.")
    if _require_string(parent, "head.repo.full_name") != SKIASHARP_REPOSITORY:
        raise PairValidationError("Parent PR head must be in mono/SkiaSharp.")
    if _require_string(native, "base.repo.full_name") != SKIA_REPOSITORY:
        raise PairValidationError("Native PR must be a mono/skia pull request.")
    if _require_string(native, "head.repo.full_name") != SKIA_REPOSITORY:
        raise PairValidationError("Native PR head must be in mono/skia.")

    parent_number = _field(parent, "number")
    native_number = _field(native, "number")
    if not isinstance(parent_number, int) or not isinstance(native_number, int):
        raise PairValidationError("PR numbers must be integers.")
    if not _require_string(parent, "title").startswith("[skia-sync]"):
        raise PairValidationError("Parent PR title must start with [skia-sync].")
    if not _require_string(native, "title").startswith("[skia-sync]"):
        raise PairValidationError("Native PR title must start with [skia-sync].")
    if _required_link(_field(parent, "body"), SKIA_REPOSITORY) != native_number:
        raise PairValidationError("Parent PR does not link to this native PR.")
    if _required_link(_field(native, "body"), SKIASHARP_REPOSITORY) != parent_number:
        raise PairValidationError("Native PR does not link back to this parent PR.")

    parent_head = _require_string(parent, "head.ref")
    native_head = _require_string(native, "head.ref")
    if parent_head != native_head:
        raise PairValidationError("Parent and native PRs must use the same head branch.")
    if not SKIA_SYNC_BRANCH_RE.fullmatch(parent_head) or parent_head == "skia-sync/main":
        raise PairValidationError(
            "Only milestone skia-sync branches are supported; "
            "skia-sync/main is intentionally rejected."
        )
    milestone_match = MILESTONE_BRANCH_RE.fullmatch(parent_head)
    milestones = (
        {int(milestone_match.group(1))}
        if milestone_match
        else {int(value) for value in MILESTONE_TEXT_RE.findall(
            f"{_require_string(parent, 'title')}\n{_field(parent, 'body')}"
        )}
    )
    if len(milestones) != 1:
        raise PairValidationError(
            "The sync must identify exactly one chrome/mNNN milestone in its branch or parent PR."
        )
    if milestone_match:
        text_milestones = {
            int(value)
            for value in MILESTONE_TEXT_RE.findall(
                f"{_require_string(parent, 'title')}\n{_field(parent, 'body')}"
            )
        }
        if text_milestones and text_milestones != milestones:
            raise PairValidationError(
                "chrome/mNNN text must agree with the skia-sync/mNNN branch milestone."
            )

    parent_base = _require_string(parent, "base.ref")
    native_base = _require_string(native, "base.ref")
    if parent_base != "main" and not RELEASE_BRANCH_RE.fullmatch(parent_base):
        raise PairValidationError("Parent base must be main or release/A.B.x.")
    expected_native_base = "skiasharp" if parent_base == "main" else parent_base
    if native_base != expected_native_base:
        raise PairValidationError(
            f"Native base must be {expected_native_base!r} for parent base {parent_base!r}."
        )

    parent_state, native_state = _state(parent), _state(native)
    parent_merged, native_merged = _is_merged(parent), _is_merged(native)
    if mode == "review" and (parent_state != "open" or native_state != "open"):
        raise PairValidationError("Review mode requires both PRs to be open.")
    if mode == "merge":
        if native_state == "closed" and not native_merged:
            raise PairValidationError("Native PR is closed without being merged.")
        if parent_state == "closed" and not parent_merged:
            raise PairValidationError("Parent PR is closed without being merged.")
        if parent_merged and not native_merged:
            raise PairValidationError("A merged parent requires an already merged native PR.")
        if native_merged and _require_sha(native, "merge_commit_sha", nullable=True) is None:
            raise PairValidationError("Merged native PR must provide a merge commit SHA.")

    return {
        "schema_version": 1,
        "mode": mode,
        "milestone": milestones.pop(),
        "head_branch": parent_head,
        "parent": _pr_contract(parent),
        "native": _pr_contract(native),
    }


def _get_pr(repository: str, number: int) -> dict[str, Any]:
    command = ["gh", "api", f"repos/{repository}/pulls/{number}"]
    completed = subprocess.run(command, check=True, capture_output=True, text=True)
    value = json.loads(completed.stdout)
    if not isinstance(value, dict):
        raise PairValidationError("GitHub returned a non-object PR response.")
    return value


def write_outputs(contract: dict[str, Any], output_path: Path) -> None:
    """Export scalar values for Actions without exposing untrusted PR bodies."""

    parent = contract["parent"]
    native = contract["native"]
    values = {
        "milestone": contract["milestone"],
        "head_branch": contract["head_branch"],
        "parent_pr": parent["number"],
        "parent_url": parent["url"],
        "parent_head_sha": parent["head_sha"],
        "parent_base": parent["base_branch"],
        "parent_base_sha": parent["base_sha"],
        "parent_state": parent["state"],
        "parent_draft": str(parent["draft"]).lower(),
        "parent_merged": str(parent["merged"]).lower(),
        "native_pr": native["number"],
        "native_url": native["url"],
        "native_head_sha": native["head_sha"],
        "native_base": native["base_branch"],
        "native_base_sha": native["base_sha"],
        "native_state": native["state"],
        "native_draft": str(native["draft"]).lower(),
        "native_merged": str(native["merged"]).lower(),
        "native_merge_sha": native["merge_sha"] or "",
    }
    with output_path.open("a", encoding="utf-8") as stream:
        for key, value in values.items():
            stream.write(f"{key}={value}\n")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--skiasharp-pr", type=int, required=True)
    parser.add_argument("--mode", choices=("review", "merge"), required=True)
    parser.add_argument(
        "--fixture",
        type=Path,
        help="Offline JSON object with parent and native REST PR payloads.",
    )
    args = parser.parse_args()
    try:
        if args.fixture:
            fixture = json.loads(args.fixture.read_text(encoding="utf-8"))
            parent = fixture["parent"]
            native = fixture["native"]
        else:
            parent = _get_pr(SKIASHARP_REPOSITORY, args.skiasharp_pr)
            native_number = _required_link(_field(parent, "body"), SKIA_REPOSITORY)
            native = _get_pr(SKIA_REPOSITORY, native_number)
        contract = resolve_pair(parent, native, args.mode)
        print(json.dumps(contract, sort_keys=True))
        output = os.environ.get("GITHUB_OUTPUT")
        if output:
            write_outputs(contract, Path(output))
    except (PairValidationError, KeyError, OSError, json.JSONDecodeError, subprocess.CalledProcessError) as error:
        print(f"resolve-skia-sync-pair: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
