#!/usr/bin/env python3

"""Fetch curated API documentation issue context deterministically."""

import argparse
import json
import os
import re
import subprocess
import sys
import tempfile
from pathlib import Path
from typing import Any, Callable, TextIO
from urllib.parse import quote


SCHEMA_VERSION = 1
DEFAULT_MAX_ISSUES = 50
DEFAULT_MAX_BYTES = 1024 * 1024


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
    repository: str,
    label: str,
    *,
    max_issues: int,
) -> dict[str, Any]:
    if max_issues < 0:
        raise ContextFetchError("Issue limit must not be negative.")
    if repository.count("/") != 1 or any(
        not segment for segment in repository.split("/")
    ):
        raise ContextFetchError("Repository must use owner/name form.")
    if not label:
        raise ContextFetchError("Label must not be empty.")

    issue_endpoint = (
        f"repos/{repository}/issues?state=all&labels={quote(label, safe='')}"
        "&per_page=100"
    )
    raw_issues = [
        issue
        for issue in client.get_pages(issue_endpoint)
        if "pull_request" not in issue and label in _labels(issue)
    ]
    if len(raw_issues) > max_issues:
        raise ContextFetchError(
            f"Approved issue count {len(raw_issues)} exceeds limit {max_issues}."
        )

    issues = []
    for raw_issue in raw_issues:
        number = _required(raw_issue, "number", int)
        expected_comments = _required(raw_issue, "comments", int)
        comment_endpoint = f"repos/{repository}/issues/{number}/comments?per_page=100"
        raw_comments = client.get_pages(comment_endpoint)
        if len(raw_comments) != expected_comments:
            raise ContextFetchError(
                f"Issue #{number} expected {expected_comments} comments but "
                f"retrieved {len(raw_comments)}; pagination may be incomplete."
            )

        comments = [
            {
                "id": _required(comment, "id", int),
                "author": _author(comment),
                "url": _required(comment, "html_url", str),
                "createdAt": _required(comment, "created_at", str),
                "updatedAt": _required(comment, "updated_at", str),
                "body": comment.get("body") or "",
            }
            for comment in raw_comments
        ]
        comments.sort(key=lambda comment: (comment["createdAt"], comment["id"]))

        issues.append(
            {
                "number": number,
                "title": _required(raw_issue, "title", str),
                "url": _required(raw_issue, "html_url", str),
                "state": _required(raw_issue, "state", str),
                "author": _author(raw_issue),
                "createdAt": _required(raw_issue, "created_at", str),
                "updatedAt": _required(raw_issue, "updated_at", str),
                "labels": _labels(raw_issue),
                "body": raw_issue.get("body") or "",
                "comments": comments,
            }
        )

    issues.sort(key=lambda issue: issue["number"])
    return {
        "schemaVersion": SCHEMA_VERSION,
        "repository": repository,
        "label": label,
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
    max_bytes: int,
) -> int:
    if max_bytes < 1:
        raise ContextFetchError("Byte limit must be positive.")

    payload = canonical_json(context)
    if len(payload) > max_bytes:
        raise ContextFetchError(
            f"Context size {len(payload)} bytes exceeds limit {max_bytes} bytes."
        )

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
    except Exception as error:
        if temporary_path is not None:
            temporary_path.unlink(missing_ok=True)
        raise ContextFetchError(f"Could not write output: {error}") from error
    return len(payload)


def _single_line(value: str) -> str:
    value = re.sub(r"[\x00-\x1f\x7f-\x9f]+", " ", value)
    return " ".join(value.split())


def emit_manifest(
    context: dict[str, Any],
    byte_count: int,
    output: Path,
    stream: TextIO,
) -> None:
    issues = context["issues"]
    print(
        "CONTEXT | "
        f"{context['repository']} | {context['label']} | {len(issues)} | "
        f"{byte_count} | {output}",
        file=stream,
    )
    for issue in issues:
        print(
            "ISSUE | "
            f"{issue['number']} | {issue['state']} | {issue['updatedAt']} | "
            f"{issue['url']} | {_single_line(issue['title'])}",
            file=stream,
        )


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Fetch approved API documentation issue context as canonical JSON."
        )
    )
    parser.add_argument("--repository", required=True)
    parser.add_argument("--label", required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--max-issues", type=int, default=DEFAULT_MAX_ISSUES)
    parser.add_argument("--max-bytes", type=int, default=DEFAULT_MAX_BYTES)
    return parser.parse_args(argv)


def run(
    args: argparse.Namespace,
    *,
    client: GhApiClient,
    stdout: TextIO,
    stderr: TextIO,
) -> int:
    output = args.output.resolve()
    try:
        output.unlink(missing_ok=True)
        context = fetch_context(
            client,
            args.repository,
            args.label,
            max_issues=args.max_issues,
        )
        size = write_context(context, output, max_bytes=args.max_bytes)
    except (ContextFetchError, OSError) as error:
        output.unlink(missing_ok=True)
        print(f"error: {error}", file=stderr)
        return 1

    emit_manifest(context, size, output, stdout)
    return 0


def main(argv: list[str] | None = None) -> int:
    return run(
        parse_args(argv),
        client=GhApiClient(),
        stdout=sys.stdout,
        stderr=sys.stderr,
    )


if __name__ == "__main__":
    raise SystemExit(main())
