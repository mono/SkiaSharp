#!/usr/bin/env python3

"""Fetch curated SkiaSharp API documentation issue context deterministically."""

import argparse
import json
import os
import subprocess
import sys
import tempfile
from pathlib import Path
from typing import Any, Callable
from urllib.parse import quote


SCHEMA_VERSION = 1
REPOSITORY = "mono/SkiaSharp-API-docs"
LABEL = "approved-for-context"
DEFAULT_MAX_ISSUES = 100
DEFAULT_MAX_COMMENTS_PER_ISSUE = 500
DEFAULT_MAX_BYTES = 4 * 1024 * 1024


class ContextFetchError(RuntimeError):
    pass


class GhApiClient:
    def __init__(
        self,
        runner: Callable[..., subprocess.CompletedProcess[str]] = subprocess.run,
    ) -> None:
        self._runner = runner

    def get_pages(self, endpoint: str) -> list[dict[str, Any]]:
        result = self._runner(
            [
                "gh",
                "api",
                "--method",
                "GET",
                "--paginate",
                "--slurp",
                "-H",
                "Accept: application/vnd.github+json",
                "-H",
                "X-GitHub-Api-Version: 2022-11-28",
                endpoint,
            ],
            capture_output=True,
            text=True,
        )
        if result.returncode != 0:
            detail = result.stderr.strip() or "gh api returned a nonzero exit code"
            raise ContextFetchError(
                f"GitHub API request failed for {endpoint}: {detail}"
            )

        try:
            payload = json.loads(result.stdout)
        except json.JSONDecodeError as error:
            raise ContextFetchError(
                f"GitHub API returned invalid JSON for {endpoint}."
            ) from error

        if not isinstance(payload, list):
            raise ContextFetchError(
                f"GitHub API returned an unexpected payload for {endpoint}."
            )

        if all(isinstance(page, list) for page in payload):
            pages = payload
        elif all(isinstance(item, dict) for item in payload):
            pages = [payload]
        else:
            raise ContextFetchError(
                f"GitHub API returned an unexpected paginated payload for {endpoint}."
            )

        items: list[dict[str, Any]] = []
        for page in pages:
            for item in page:
                if not isinstance(item, dict):
                    raise ContextFetchError(
                        f"GitHub API returned a non-object item for {endpoint}."
                    )
                items.append(item)
        return items


def _author(item: dict[str, Any]) -> str | None:
    user = item.get("user")
    return user.get("login") if isinstance(user, dict) else None


def _labels(issue: dict[str, Any]) -> list[str]:
    labels = issue.get("labels")
    if not isinstance(labels, list):
        raise ContextFetchError("An issue returned labels in an unexpected shape.")

    names = []
    for label in labels:
        if not isinstance(label, dict) or not isinstance(label.get("name"), str):
            raise ContextFetchError("An issue returned an invalid label entry.")
        names.append(label["name"])
    return sorted(set(names))


def _required(item: dict[str, Any], key: str, kind: type) -> Any:
    value = item.get(key)
    if not isinstance(value, kind):
        raise ContextFetchError(f"GitHub API item is missing a valid {key}.")
    return value


def fetch_context(
    client: GhApiClient,
    *,
    max_issues: int = DEFAULT_MAX_ISSUES,
    max_comments_per_issue: int = DEFAULT_MAX_COMMENTS_PER_ISSUE,
) -> dict[str, Any]:
    if max_issues < 1 or max_comments_per_issue < 0:
        raise ContextFetchError("Issue and comment bounds must be positive.")

    label = quote(LABEL, safe="")
    issue_endpoint = (
        f"repos/{REPOSITORY}/issues?state=all&labels={label}&per_page=100"
    )
    raw_issues = [
        issue
        for issue in client.get_pages(issue_endpoint)
        if "pull_request" not in issue and LABEL in _labels(issue)
    ]
    if len(raw_issues) > max_issues:
        raise ContextFetchError(
            f"Approved issue count {len(raw_issues)} exceeds limit {max_issues}."
        )

    issues = []
    for raw_issue in raw_issues:
        number = _required(raw_issue, "number", int)
        expected_comments = _required(raw_issue, "comments", int)
        if expected_comments > max_comments_per_issue:
            raise ContextFetchError(
                f"Issue #{number} comment count {expected_comments} exceeds "
                f"limit {max_comments_per_issue}."
            )

        comment_endpoint = (
            f"repos/{REPOSITORY}/issues/{number}/comments?per_page=100"
        )
        raw_comments = client.get_pages(comment_endpoint)
        if len(raw_comments) != expected_comments:
            raise ContextFetchError(
                f"Issue #{number} expected {expected_comments} comments but "
                f"retrieved {len(raw_comments)}; pagination may be incomplete."
            )

        comments = [
            {
                "author": _author(comment),
                "body": comment.get("body") or "",
                "created_at": _required(comment, "created_at", str),
                "updated_at": _required(comment, "updated_at", str),
                "url": _required(comment, "html_url", str),
            }
            for comment in raw_comments
        ]
        comments.sort(key=lambda comment: (comment["created_at"], comment["url"]))

        issues.append(
            {
                "author": _author(raw_issue),
                "body": raw_issue.get("body") or "",
                "closed_at": raw_issue.get("closed_at"),
                "comments": comments,
                "created_at": _required(raw_issue, "created_at", str),
                "labels": _labels(raw_issue),
                "number": number,
                "state": _required(raw_issue, "state", str),
                "title": _required(raw_issue, "title", str),
                "updated_at": _required(raw_issue, "updated_at", str),
                "url": _required(raw_issue, "html_url", str),
            }
        )

    issues.sort(key=lambda issue: issue["number"])
    return {
        "schema_version": SCHEMA_VERSION,
        "repository": REPOSITORY,
        "label": LABEL,
        "issues": issues,
    }


def canonical_json(context: dict[str, Any]) -> bytes:
    return (
        json.dumps(
            context,
            ensure_ascii=False,
            indent=2,
            sort_keys=True,
        )
        + "\n"
    ).encode("utf-8")


def write_context(
    context: dict[str, Any],
    output: Path,
    *,
    max_bytes: int = DEFAULT_MAX_BYTES,
) -> int:
    if max_bytes < 1:
        raise ContextFetchError("Byte limit must be positive.")

    payload = canonical_json(context)
    if len(payload) > max_bytes:
        raise ContextFetchError(
            f"Context size {len(payload)} bytes exceeds limit {max_bytes} bytes."
        )

    output = output.resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    temporary_path: Path | None = None
    try:
        with tempfile.NamedTemporaryFile(
            mode="wb",
            dir=output.parent,
            prefix=f".{output.name}.",
            suffix=".tmp",
            delete=False,
        ) as temporary:
            temporary_path = Path(temporary.name)
            temporary.write(payload)
            temporary.flush()
            os.fsync(temporary.fileno())
        os.replace(temporary_path, output)
    except Exception:
        if temporary_path is not None:
            temporary_path.unlink(missing_ok=True)
        raise
    return len(payload)


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Fetch approved SkiaSharp API documentation issue context as "
            "canonical JSON."
        )
    )
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--max-issues", type=int, default=DEFAULT_MAX_ISSUES)
    parser.add_argument(
        "--max-comments-per-issue",
        type=int,
        default=DEFAULT_MAX_COMMENTS_PER_ISSUE,
    )
    parser.add_argument("--max-bytes", type=int, default=DEFAULT_MAX_BYTES)
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    try:
        context = fetch_context(
            GhApiClient(),
            max_issues=args.max_issues,
            max_comments_per_issue=args.max_comments_per_issue,
        )
        size = write_context(context, args.output, max_bytes=args.max_bytes)
    except ContextFetchError as error:
        print(f"error: {error}", file=sys.stderr)
        return 1

    print(
        f"Wrote {len(context['issues'])} approved issues ({size} bytes) "
        f"to {args.output}."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
