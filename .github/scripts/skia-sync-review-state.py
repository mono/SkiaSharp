#!/usr/bin/env python3
"""Pure validation for immutable Skia-sync review evidence markers."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any


MARKER_RE = re.compile(
    r"<!-- skia-sync-review:v1 "
    r"native-pr=(?P<native_pr>[1-9][0-9]*) "
    r"native-head=(?P<native_head>[0-9a-f]{40}) "
    r"parent-pr=(?P<parent_pr>[1-9][0-9]*) "
    r"parent-head=(?P<parent_head>[0-9a-f]{40}) "
    r"run=(?P<run>[1-9][0-9]*) "
    r"artifact=(?P<artifact>[1-9][0-9]*) -->"
)
EXPECTED_ACTOR = "github-actions[bot]"


class ReviewEvidenceError(ValueError):
    """No trusted review evidence can authorize this immutable merge state."""


def find_review_marker(
    comments: list[dict[str, Any]],
    *,
    native_pr: int,
    native_head: str,
    parent_pr: int,
    parent_head: str,
    actor: str = EXPECTED_ACTOR,
) -> dict[str, str]:
    """Return the latest exact trusted marker for the frozen pair."""

    flattened = [comment for page in comments for comment in page] if comments and isinstance(comments[0], list) else comments
    matches = []
    for comment in flattened:
        user = comment.get("user")
        body = comment.get("body")
        if not isinstance(user, dict) or user.get("login") != actor or not isinstance(body, str):
            continue
        match = MARKER_RE.search(body)
        if not match:
            continue
        values = match.groupdict()
        if (
            values["native_pr"] == str(native_pr)
            and values["native_head"] == native_head
            and values["parent_pr"] == str(parent_pr)
        ):
            matches.append(values)
    if not matches:
        raise ReviewEvidenceError("No trusted skia-sync-review:v1 marker matches the native pair.")
    marker = matches[-1]
    if marker["parent_head"] != parent_head:
        marker["repin_parent_head"] = marker["parent_head"]
    return marker


def repin_is_safe(
    *,
    parent_head: str,
    marker_parent_head: str,
    parents: list[str],
    changed_paths: list[str],
    gitlink: str,
    component_repository_url: str,
    component_commit_hash: str,
    native_merge_sha: str,
) -> bool:
    """Accept only the direct two-file repin that follows frozen evidence."""

    return (
        bool(re.fullmatch(r"[0-9a-f]{40}", parent_head))
        and parents == [marker_parent_head]
        and sorted(changed_paths) == ["cgmanifest.json", "externals/skia"]
        and gitlink == native_merge_sha
        and component_repository_url == "https://github.com/mono/skia.git"
        and component_commit_hash == native_merge_sha
    )


def provenance_is_trusted(
    run: dict[str, Any],
    artifacts: dict[str, Any],
    artifact_id: str,
) -> bool:
    """Require that marker evidence came from the repository's successful workflow."""

    return (
        run.get("name") == "Review - Skia Sync"
        and run.get("path") == ".github/workflows/skia-sync-review.yml"
        and run.get("status") == "completed"
        and run.get("conclusion") == "success"
        and run.get("head_branch") == "main"
        and isinstance(run.get("head_repository"), dict)
        and run["head_repository"].get("full_name") == "mono/SkiaSharp"
        and sum(
            artifact.get("name") == "skia-review-evidence"
            and str(artifact.get("id")) == artifact_id
            and artifact.get("expired") is False
            for artifact in artifacts.get("artifacts", [])
            if isinstance(artifact, dict)
        ) == 1
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--comments", type=Path)
    parser.add_argument("--native-pr", type=int)
    parser.add_argument("--native-head")
    parser.add_argument("--parent-pr", type=int)
    parser.add_argument("--parent-head")
    parser.add_argument("--repin-json", type=Path)
    parser.add_argument("--provenance-json", type=Path)
    args = parser.parse_args()
    try:
        if args.comments:
            if None in (args.native_pr, args.native_head, args.parent_pr, args.parent_head):
                raise ReviewEvidenceError("Marker lookup requires every pair identity field.")
            comments = json.loads(args.comments.read_text(encoding="utf-8"))
            marker = find_review_marker(
                comments,
                native_pr=args.native_pr,
                native_head=args.native_head,
                parent_pr=args.parent_pr,
                parent_head=args.parent_head,
            )
            print(json.dumps(marker, sort_keys=True))
            return 0
        if args.repin_json:
            data = json.loads(args.repin_json.read_text(encoding="utf-8"))
            if not repin_is_safe(**data):
                raise ReviewEvidenceError("Review evidence does not permit this repin resume.")
            print(json.dumps({"safe": True}))
            return 0
        if args.provenance_json:
            data = json.loads(args.provenance_json.read_text(encoding="utf-8"))
            if not provenance_is_trusted(
                data["run"],
                data["artifacts"],
                str(data["artifact_id"]),
            ):
                raise ReviewEvidenceError("Marker run or evidence artifact is not trusted.")
            print(json.dumps({"trusted": True}))
            return 0
        raise ReviewEvidenceError("Choose --comments, --repin-json, or --provenance-json.")
    except (ReviewEvidenceError, OSError, TypeError, json.JSONDecodeError) as error:
        print(f"skia-sync-review-state: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
