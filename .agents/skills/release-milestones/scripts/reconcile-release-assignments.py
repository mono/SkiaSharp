#!/usr/bin/env python3
"""Reconcile shipped release milestone assignments."""

from __future__ import annotations

import argparse
from dataclasses import dataclass
import json
from pathlib import Path
import re
import sys

import milestone_common as common


BRANCH_RE = re.compile(
    r"^release/(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)
PR_RE = re.compile(r"\(#(\d+)\)")
CLOSING_RE = re.compile(
    r"(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\s*:?\s+#(\d+)",
    re.IGNORECASE,
)


@dataclass(frozen=True)
class ReleaseBranch:
    name: str
    title: str
    numeric: tuple[int, ...]
    channel: str | None
    iteration: int

    @classmethod
    def parse(cls, name: str) -> ReleaseBranch | None:
        match = BRANCH_RE.fullmatch(name)
        if not match:
            return None
        numeric_text = match.group("numeric")
        title = numeric_text
        channel = match.group("channel")
        iteration = int(match.group("iteration") or 0)
        if channel:
            title += f"-{channel}.{iteration}"
        return cls(
            name=name,
            title=title,
            numeric=tuple(int(part) for part in numeric_text.split(".")),
            channel=channel,
            iteration=iteration,
        )

    @property
    def sort_key(self) -> tuple:
        base = self.numeric[:3]
        hotfix = self.numeric[3] if len(self.numeric) == 4 else 0
        channel = {"preview": 0, "rc": 1, None: 2}[self.channel]
        return base, hotfix, channel, self.iteration


def git(
    root: Path,
    *args: str,
    check: bool = True,
) -> str:
    return common.run(
        ["git", *args],
        cwd=root,
        check=check,
    ).stdout.strip()


def normalize_version(value: str | None, root: Path) -> str:
    if value:
        if not re.fullmatch(r"\d+\.\d+\.\d+(?:\.\d+)?", value):
            raise common.MilestoneError(
                "version must be numeric X.Y.Z or X.Y.Z.F"
            )
        return value
    major, milestone = common.read_current_version(root)
    return f"{major}.{milestone}.0"


def remote_tags(root: Path, version: str) -> list[str]:
    lines = git(
        root,
        "ls-remote",
        "--tags",
        "origin",
        f"refs/tags/v{version}*",
    ).splitlines()
    return sorted(
        {
            ref.removeprefix("refs/tags/")
            for line in lines
            if line
            for _, ref in [line.split(maxsplit=1)]
            if not ref.endswith("^{}")
        }
    )


def release_branches(
    root: Path,
    version: str,
) -> tuple[list[ReleaseBranch], list[ReleaseBranch]]:
    lines = git(
        root,
        "for-each-ref",
        "--format=%(refname:strip=3)",
        "refs/remotes/origin/release/",
    ).splitlines()
    parsed = [
        branch
        for line in lines
        if (branch := ReleaseBranch.parse(line))
    ]
    selected = [
        branch
        for branch in parsed
        if branch.title == version
        or branch.title.startswith(f"{version}-")
        or branch.title.startswith(f"{version}.")
    ]
    if not selected:
        raise common.MilestoneError(
            f"no release branches match {version}"
        )
    return sorted(selected, key=lambda item: item.sort_key), parsed


def previous_stable_branch(
    branches: list[ReleaseBranch],
    version: str,
) -> ReleaseBranch | None:
    target = tuple(int(part) for part in version.split("."))
    candidates = [
        branch
        for branch in branches
        if branch.channel is None and branch.numeric < target
    ]
    return (
        max(candidates, key=lambda item: item.numeric)
        if candidates
        else None
    )


def effective_titles(
    branches: list[ReleaseBranch],
    tags: list[str],
) -> list[str | None]:
    result = []
    for index in range(len(branches)):
        title = next(
            (
                branch.title
                for branch in branches[index:]
                if common.shipped_tag(branch.title, tags)
            ),
            None,
        )
        result.append(title)
    return result


def pr_numbers(root: Path, start: str, end: str) -> list[int]:
    subjects = git(
        root,
        "log",
        "--format=%s",
        "--first-parent",
        f"{start}..{end}",
    ).splitlines()
    return sorted(
        {
            int(match.group(1))
            for subject in subjects
            if (match := PR_RE.search(subject))
        }
    )


def linked_issues(github: common.GitHub, pull_request: int) -> list[int]:
    linked = set(github.closing_issues(pull_request))
    body = github.pull_request(pull_request).get("body") or ""
    linked.update(int(match.group(1)) for match in CLOSING_RE.finditer(body))
    return sorted(linked)


def execution_command(args, version: str) -> str:
    command = [
        sys.executable,
        (
            ".agents/skills/release-milestones/scripts/"
            "reconcile-release-assignments.py"
        ),
        "--version",
        version,
        "--repo",
        args.repo,
    ]
    return common.shell_command(command)


def build_plan(args) -> tuple[dict, list[dict]]:
    root = common.repository_root()
    version = normalize_version(args.version, root)
    tags = remote_tags(root, version)

    git(root, "fetch", "origin", "--prune")
    branches, all_branches = release_branches(root, version)
    previous = previous_stable_branch(all_branches, version)
    merge_bases = {
        branch.name: git(
            root,
            "merge-base",
            "origin/main",
            f"origin/{branch.name}",
        )
        for branch in branches
    }
    previous_base = (
        git(
            root,
            "merge-base",
            "origin/main",
            f"origin/{previous.name}",
        )
        if previous
        else None
    )
    effective = effective_titles(branches, tags)
    github = common.GitHub(args.repo)
    milestones = github.milestone_map()
    operations = []
    warnings = []
    correct = 0

    for index, branch in enumerate(branches):
        target_title = effective[index]
        if target_title is None:
            continue
        target = milestones.get(target_title)
        if target is None:
            warnings.append(
                f"milestone {target_title} does not exist"
            )
            continue
        start = (
            previous_base
            if index == 0
            else merge_bases[branches[index - 1].name]
        )
        if not start:
            warnings.append(
                f"no previous release boundary for {target_title}"
            )
            continue
        end = merge_bases[branch.name]
        for pull_request in pr_numbers(root, start, end):
            issue = github.issue(pull_request)
            current = (issue.get("milestone") or {}).get("title")
            if current == target_title:
                correct += 1
            else:
                operations.append(
                    {
                        "kind": "pull-request",
                        "number": pull_request,
                        "viaPullRequest": None,
                        "fromMilestone": current,
                        "toMilestone": target_title,
                        "toMilestoneNumber": int(target["number"]),
                        "status": "pending",
                    }
                )
            for linked in linked_issues(github, pull_request):
                linked_data = github.issue(linked)
                linked_current = (
                    linked_data.get("milestone") or {}
                ).get("title")
                if linked_current == target_title:
                    correct += 1
                else:
                    operations.append(
                        {
                            "kind": "issue",
                            "number": linked,
                            "viaPullRequest": pull_request,
                            "fromMilestone": linked_current,
                            "toMilestone": target_title,
                            "toMilestoneNumber": int(target["number"]),
                            "status": "pending",
                        }
                    )

    if warnings:
        next_action = "resolve-reconciliation-warnings"
    elif operations:
        next_action = "confirm-reconcile-assignments"
    else:
        next_action = "complete"
    report = {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "repository": args.repo,
        "version": version,
        "tags": tags,
        "branches": [
            {
                "name": branch.name,
                "title": branch.title,
                "shippedTag": common.shipped_tag(branch.title, tags),
                "effectiveMilestone": effective[index],
                "mergeBase": merge_bases[branch.name],
            }
            for index, branch in enumerate(branches)
        ],
        "previousBoundary": previous.name if previous else None,
        "operations": operations,
        "summary": {
            "pending": len(operations),
            "correct": correct,
            "warnings": len(warnings),
        },
        "warnings": warnings,
        "nextAction": next_action,
        "executionCommand": (
            execution_command(args, version)
            if next_action == "confirm-reconcile-assignments"
            else None
        ),
    }
    return report, operations


def execute(
    args,
    operations: list[dict],
) -> None:
    github = common.GitHub(args.repo)
    for item in operations:
        github.update_issue_milestone(
            item["number"],
            item["toMilestoneNumber"],
        )


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version")
    parser.add_argument("--repo", default=common.GITHUB_REPOSITORY)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        report, operations = build_plan(args)
        if (
            not args.dry_run
            and report["nextAction"] == "confirm-reconcile-assignments"
        ):
            execute(args, operations)
            report, _ = build_plan(args)
            report["dryRun"] = False
        print(json.dumps(report, indent=2))
    except (common.MilestoneError, OSError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
