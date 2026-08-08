#!/usr/bin/env python3
"""Shared helpers for SkiaSharp release milestones."""

from __future__ import annotations

import json
from pathlib import Path
import re
import shlex
import shutil
import subprocess
import sys
import time


GITHUB_REPOSITORY = "mono/SkiaSharp"
VERSIONS_PATH = Path("scripts/VERSIONS.txt")


class MilestoneError(RuntimeError):
    """Milestone state could not be read or changed safely."""


def display(args: list[str]) -> str:
    return (
        subprocess.list2cmdline(args)
        if sys.platform == "win32"
        else shlex.join(args)
    )


def shell_command(args: list[str]) -> str:
    if sys.platform != "win32":
        return shlex.join(args)

    def quote(argument: str) -> str:
        if re.fullmatch(r"[A-Za-z0-9_./:\\=+-]+", argument):
            return argument
        return "'" + argument.replace("'", "''") + "'"

    formatted = " ".join(quote(argument) for argument in args)
    return f"& {formatted}" if formatted.startswith("'") else formatted


def resolve_command(args: list[str]) -> list[str]:
    executable = shutil.which(args[0])
    if not executable:
        return args
    resolved = [executable, *args[1:]]
    if sys.platform == "win32" and Path(executable).suffix.lower() in {
        ".bat",
        ".cmd",
    }:
        return [
            shutil.which("cmd.exe") or "cmd.exe",
            "/d",
            "/s",
            "/c",
            subprocess.list2cmdline(resolved),
        ]
    return resolved


def run(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 120,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    args = resolve_command(args)
    try:
        result = subprocess.run(
            args,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=timeout,
        )
    except FileNotFoundError as error:
        raise MilestoneError(f"{args[0]} was not found on PATH") from error
    except subprocess.TimeoutExpired as error:
        raise MilestoneError(
            f"command timed out after {timeout}s: {display(args)}"
        ) from error
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise MilestoneError(
            f"command failed ({result.returncode}): {display(args)}\n{detail}"
        )
    return result


def parse_json_output(text: str):
    decoder = json.JSONDecoder()
    for index, character in enumerate(text):
        if character not in "[{":
            continue
        try:
            value, _ = decoder.raw_decode(text[index:])
            return value
        except json.JSONDecodeError:
            pass
    raise MilestoneError("command returned no valid JSON")


def run_json(
    args: list[str],
    *,
    cwd: Path | None = None,
    timeout: int = 120,
):
    return parse_json_output(run(args, cwd=cwd, timeout=timeout).stdout)


def repository_root() -> Path:
    return Path(
        run(["git", "rev-parse", "--show-toplevel"]).stdout.strip()
    )


def read_current_version(root: Path) -> tuple[int, int]:
    path = root / VERSIONS_PATH
    if not path.is_file():
        raise MilestoneError(f"{VERSIONS_PATH} does not exist")
    major = None
    milestone = None
    for line in path.read_text(encoding="utf-8").splitlines():
        parts = line.strip().split()
        if len(parts) < 3:
            continue
        if parts[:2] == ["SkiaSharp", "nuget"]:
            major = int(parts[2].split(".", 1)[0])
        elif parts[:2] == ["libSkiaSharp", "milestone"]:
            milestone = int(parts[2])
    if major is None or milestone is None:
        raise MilestoneError(
            f"could not read SkiaSharp/libSkiaSharp versions from {path}"
        )
    return major, milestone


def shipped_tag(title: str, tags: list[str]) -> str | None:
    if "-preview." in title or "-rc." in title:
        pattern = re.compile(rf"^v{re.escape(title)}\.(?P<build>\d+)$")
        matches = [
            (int(match.group("build")), tag)
            for tag in tags
            if (match := pattern.fullmatch(tag))
        ]
        return max(matches)[1] if matches else None
    exact = f"v{title}"
    return exact if exact in tags else None


class GitHub:
    def __init__(self, repository: str = GITHUB_REPOSITORY) -> None:
        self.repository = repository
        self.gh_path = shutil.which("gh")
        if not self.gh_path:
            raise MilestoneError("GitHub CLI 'gh' was not found on PATH")

    def json(self, args: list[str], *, timeout: int = 120):
        for attempt in range(1, 4):
            try:
                return run_json(
                    [self.gh_path, *args],
                    timeout=timeout,
                )
            except MilestoneError as error:
                transient = any(
                    status in str(error)
                    for status in ("HTTP 502", "HTTP 503", "HTTP 504")
                )
                if not transient or attempt == 3:
                    raise
                time.sleep(attempt * 2)
        raise AssertionError("unreachable")

    def milestones(self) -> list[dict]:
        pages = self.json(
            [
                "api",
                "--paginate",
                "--slurp",
                f"repos/{self.repository}/milestones?state=all&per_page=100",
            ]
        )
        return [item for page in pages for item in page]

    def milestone_map(self) -> dict[str, dict]:
        milestones = self.milestones()
        result = {}
        for milestone in milestones:
            title = milestone.get("title")
            if title in result:
                raise MilestoneError(
                    f"multiple milestones are named {title}"
                )
            result[title] = milestone
        return result

    def create_milestone(
        self,
        *,
        title: str,
        due_on: str,
        description: str,
    ) -> dict:
        return self.json(
            [
                "api",
                f"repos/{self.repository}/milestones",
                "-X",
                "POST",
                "-f",
                f"title={title}",
                "-f",
                f"due_on={due_on}",
                "-f",
                f"description={description}",
            ]
        )

    def update_milestone(
        self,
        number: int,
        *,
        due_on: str,
        description: str,
    ) -> dict:
        return self.json(
            [
                "api",
                f"repos/{self.repository}/milestones/{number}",
                "-X",
                "PATCH",
                "-f",
                f"due_on={due_on}",
                "-f",
                f"description={description}",
            ]
        )

    def close_milestone(self, number: int) -> dict:
        return self.json(
            [
                "api",
                f"repos/{self.repository}/milestones/{number}",
                "-X",
                "PATCH",
                "-f",
                "state=closed",
            ]
        )

    def issue(self, number: int) -> dict:
        return self.json(
            ["api", f"repos/{self.repository}/issues/{number}"]
        )

    def pull_request(self, number: int) -> dict:
        return self.json(
            ["api", f"repos/{self.repository}/pulls/{number}"]
        )

    def update_issue_milestone(
        self,
        number: int,
        milestone_number: int,
    ) -> dict:
        return self.json(
            [
                "api",
                f"repos/{self.repository}/issues/{number}",
                "-X",
                "PATCH",
                "-F",
                f"milestone={milestone_number}",
            ]
        )

    def closing_issues(self, pull_request: int) -> list[int]:
        owner, name = self.repository.split("/", 1)
        query = """
query($owner: String!, $name: String!, $number: Int!) {
  repository(owner: $owner, name: $name) {
    pullRequest(number: $number) {
      closingIssuesReferences(first: 50) {
        nodes { number }
      }
    }
  }
}
"""
        data = self.json(
            [
                "api",
                "graphql",
                "-f",
                f"query={query}",
                "-F",
                f"owner={owner}",
                "-F",
                f"name={name}",
                "-F",
                f"number={pull_request}",
            ]
        )
        repository = (data.get("data") or {}).get("repository") or {}
        pull_request_data = repository.get("pullRequest") or {}
        references = (
            pull_request_data.get("closingIssuesReferences") or {}
        )
        nodes = references.get("nodes") or []
        return [int(node["number"]) for node in nodes]

    def open_milestone_items(self, number: int) -> list[dict]:
        pages = self.json(
            [
                "api",
                "--paginate",
                "--slurp",
                (
                    f"repos/{self.repository}/issues"
                    f"?milestone={number}&state=open&per_page=100"
                ),
            ]
        )
        return [
            {
                "number": int(item["number"]),
                "title": item.get("title") or "",
                "url": item.get("html_url"),
                "kind": (
                    "pull-request" if "pull_request" in item else "issue"
                ),
            }
            for page in pages
            for item in page
        ]
