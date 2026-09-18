#!/usr/bin/env python3
"""Resolve and freeze a reviewable reciprocal Skia sync pull-request pair."""

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
    """The two PRs do not form a safe Skia sync review pair."""


def _field(pr: dict[str, Any], path: str) -> Any:
    value: Any = pr
    for part in path.split("."):
        if not isinstance(value, dict) or part not in value:
            raise PairValidationError(f"PR JSON is missing {path}.")
        value = value[part]
    return value


def _string(pr: dict[str, Any], path: str) -> str:
    value = _field(pr, path)
    if not isinstance(value, str) or not value:
        raise PairValidationError(f"PR JSON field {path} must be a nonempty string.")
    return value


def _sha(pr: dict[str, Any], path: str) -> str:
    value = _string(pr, path)
    if not re.fullmatch(r"[0-9a-f]{40}", value):
        raise PairValidationError(f"PR JSON field {path} must be a lowercase full SHA.")
    return value


def _link(body: object, repository: str) -> int:
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


def _contract(pr: dict[str, Any]) -> dict[str, Any]:
    number = _field(pr, "number")
    if not isinstance(number, int) or number < 1:
        raise PairValidationError("PR number must be a positive integer.")
    if _string(pr, "state").lower() != "open":
        raise PairValidationError("Review mode requires both PRs to be open.")
    return {
        "number": number,
        "url": _string(pr, "html_url"),
        "title": _string(pr, "title"),
        "author": _string(pr, "user.login"),
        "head_branch": _string(pr, "head.ref"),
        "head_sha": _sha(pr, "head.sha"),
        "base_branch": _string(pr, "base.ref"),
        "base_sha": _sha(pr, "base.sha"),
        "state": "open",
        "draft": bool(_field(pr, "draft")),
    }


def _milestone(parent: dict[str, Any], native: dict[str, Any], branch: str) -> int:
    branch_match = MILESTONE_BRANCH_RE.fullmatch(branch)
    text = "\n".join(
        (_string(parent, "title"), _string(parent, "body"),
         _string(native, "title"), _string(native, "body"))
    )
    text_milestones = {int(value) for value in MILESTONE_TEXT_RE.findall(text)}
    if branch_match:
        milestone = int(branch_match.group(1))
        if text_milestones and text_milestones != {milestone}:
            raise PairValidationError(
                "chrome/mNNN text must agree with the skia-sync/mNNN branch milestone."
            )
        return milestone
    if len(text_milestones) != 1:
        raise PairValidationError(
            "The sync must identify exactly one chrome/mNNN milestone in its PR metadata."
        )
    return text_milestones.pop()


def resolve_pair(parent: dict[str, Any], native: dict[str, Any]) -> dict[str, Any]:
    """Validate both REST PR payloads and return an immutable shell-safe contract."""

    for pr, repository, label in (
        (parent, SKIASHARP_REPOSITORY, "Parent"),
        (native, SKIA_REPOSITORY, "Native"),
    ):
        if _string(pr, "base.repo.full_name") != repository:
            raise PairValidationError(f"{label} PR base must be in {repository}.")
        if _string(pr, "head.repo.full_name") != repository:
            raise PairValidationError(f"{label} PR head must be in {repository}.")
        if not _string(pr, "title").startswith("[skia-sync]"):
            raise PairValidationError(f"{label} PR title must start with [skia-sync].")

    parent_contract = _contract(parent)
    native_contract = _contract(native)
    if _link(_field(parent, "body"), SKIA_REPOSITORY) != native_contract["number"]:
        raise PairValidationError("Parent PR does not link to this native PR.")
    if _link(_field(native, "body"), SKIASHARP_REPOSITORY) != parent_contract["number"]:
        raise PairValidationError("Native PR does not link back to this parent PR.")

    branch = parent_contract["head_branch"]
    if branch != native_contract["head_branch"]:
        raise PairValidationError("Parent and native PRs must use the same head branch.")
    if not SKIA_SYNC_BRANCH_RE.fullmatch(branch) or branch == "skia-sync/main":
        raise PairValidationError(
            "Only milestone skia-sync branches are supported; skia-sync/main is rejected."
        )

    parent_base = parent_contract["base_branch"]
    if parent_base != "main" and not RELEASE_BRANCH_RE.fullmatch(parent_base):
        raise PairValidationError("Parent base must be main or release/A.B.x.")
    expected_native_base = "skiasharp" if parent_base == "main" else parent_base
    if native_contract["base_branch"] != expected_native_base:
        raise PairValidationError(
            f"Native base must be {expected_native_base!r} for parent base {parent_base!r}."
        )

    return {
        "schema_version": 1,
        "mode": "review",
        "milestone": _milestone(parent, native, branch),
        "head_branch": branch,
        "repositories": {"parent": SKIASHARP_REPOSITORY, "native": SKIA_REPOSITORY},
        "links": {
            "parent_to_native": native_contract["number"],
            "native_to_parent": parent_contract["number"],
        },
        "parent": parent_contract,
        "native": native_contract,
    }


def _get_pr(repository: str, number: int) -> dict[str, Any]:
    completed = subprocess.run(
        ["gh", "api", f"repos/{repository}/pulls/{number}"],
        check=True,
        capture_output=True,
        text=True,
    )
    value = json.loads(completed.stdout)
    if not isinstance(value, dict):
        raise PairValidationError("GitHub returned a non-object PR response.")
    return value


def write_outputs(contract: dict[str, Any], output_path: Path) -> None:
    """Export only frozen scalar metadata for Actions consumers."""

    parent = contract["parent"]
    native = contract["native"]
    values = {
        "milestone": contract["milestone"],
        "head_branch": contract["head_branch"],
        "parent_pr": parent["number"],
        "parent_url": parent["url"],
        "parent_head": parent["head_sha"],
        "parent_base": parent["base_branch"],
        "parent_base_sha": parent["base_sha"],
        "native_pr": native["number"],
        "native_url": native["url"],
        "native_head": native["head_sha"],
        "native_base": native["base_branch"],
        "native_base_sha": native["base_sha"],
    }
    with output_path.open("a", encoding="utf-8") as stream:
        for key, value in values.items():
            stream.write(f"{key}={value}\n")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--skiasharp-pr", type=int, required=True)
    parser.add_argument("--fixture", type=Path, help="Offline JSON with parent and native PR payloads.")
    args = parser.parse_args()
    try:
        if args.fixture:
            fixture = json.loads(args.fixture.read_text(encoding="utf-8"))
            parent, native = fixture["parent"], fixture["native"]
        else:
            parent = _get_pr(SKIASHARP_REPOSITORY, args.skiasharp_pr)
            native = _get_pr(SKIA_REPOSITORY, _link(_field(parent, "body"), SKIA_REPOSITORY))
        contract = resolve_pair(parent, native)
        print(json.dumps(contract, sort_keys=True))
        if output := os.environ.get("GITHUB_OUTPUT"):
            write_outputs(contract, Path(output))
    except (PairValidationError, KeyError, OSError, json.JSONDecodeError, subprocess.CalledProcessError) as error:
        print(f"resolve-skia-sync-pair: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
