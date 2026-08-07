#!/usr/bin/env python3
"""Synchronize and close SkiaSharp release milestones."""

from __future__ import annotations

import argparse
from dataclasses import dataclass
from datetime import date, datetime, timezone
import json
import re
import sys
import urllib.error
import urllib.request

import milestone_common as common


SCHEDULE_URL = (
    "https://chromiumdash.appspot.com/"
    "fetch_milestone_schedule?mstone={milestone}"
)
REQUIRED_SCHEDULE_FIELDS = (
    "branch_point",
    "earliest_beta",
    "early_stable_cut",
    "early_stable",
    "stable_cut",
    "stable_date",
)
RELEASE_MILESTONE_RE = re.compile(
    r"^(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)


@dataclass(frozen=True)
class DesiredMilestone:
    title: str
    due: date
    description: str

    @property
    def due_on(self) -> str:
        return f"{self.due.isoformat()}T00:00:00Z"


@dataclass(frozen=True)
class ReleaseMilestone:
    title: str
    numeric: tuple[int, ...]
    channel: str | None
    iteration: int

    @classmethod
    def parse(cls, value: str) -> ReleaseMilestone | None:
        match = RELEASE_MILESTONE_RE.fullmatch(value)
        if not match:
            return None
        return cls(
            title=value,
            numeric=tuple(
                int(part) for part in match.group("numeric").split(".")
            ),
            channel=match.group("channel"),
            iteration=int(match.group("iteration") or 0),
        )

    @property
    def sort_key(self) -> tuple:
        base = self.numeric[:3]
        hotfix = self.numeric[3] if len(self.numeric) == 4 else 0
        channel = {"preview": 0, "rc": 1, None: 2}[self.channel]
        return base, hotfix, channel, self.iteration


def remote_tags(root) -> list[str]:
    lines = common.run(
        ["git", "ls-remote", "--tags", "origin", "refs/tags/v*"],
        cwd=root,
    ).stdout.splitlines()
    return sorted(
        {
            ref.removeprefix("refs/tags/")
            for line in lines
            if line
            for _, ref in [line.split(maxsplit=1)]
            if not ref.endswith("^{}")
        }
    )


def parse_date(value: str) -> date:
    return date.fromisoformat(value.split("T", 1)[0])


def display_date(value: date) -> str:
    return value.strftime("%a, %b %d, %Y")


def fetch_schedule(milestone: int) -> dict:
    url = SCHEDULE_URL.format(milestone=milestone)
    request = urllib.request.Request(
        url,
        headers={"User-Agent": "SkiaSharp-release-milestones"},
    )
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            data = json.load(response)
    except (
        urllib.error.HTTPError,
        urllib.error.URLError,
        TimeoutError,
        json.JSONDecodeError,
    ) as error:
        raise common.MilestoneError(
            f"failed to fetch Chromium schedule m{milestone}: {error}"
        ) from error
    entries = data.get("mstones") or []
    if not entries:
        raise common.MilestoneError(
            f"Chromium returned no schedule for m{milestone}"
        )
    schedule = entries[0]
    missing = [
        field for field in REQUIRED_SCHEDULE_FIELDS if not schedule.get(field)
    ]
    if missing:
        raise common.MilestoneError(
            f"Chromium m{milestone} schedule is missing {missing}"
        )
    return schedule


def desired_milestones(
    schedule: dict,
    *,
    milestone: int,
    major: int,
) -> list[DesiredMilestone]:
    branch = parse_date(schedule["branch_point"])
    beta = parse_date(schedule["earliest_beta"])
    early_cut = parse_date(schedule["early_stable_cut"])
    early_stable = parse_date(schedule["early_stable"])
    stable_cut = parse_date(schedule["stable_cut"])
    stable = parse_date(schedule["stable_date"])
    base = f"{major}.{milestone}.0"
    separator = "\u00b7"
    return [
        DesiredMilestone(
            f"{base}-preview.1",
            beta,
            (
                f"Skia m{milestone} preview.1 {separator} Start "
                f"{display_date(branch)} {separator} Merge Skia sync PR and "
                "ship preview."
            ),
        ),
        DesiredMilestone(
            f"{base}-preview.2",
            early_stable,
            (
                f"Skia m{milestone} preview.2 {separator} Start "
                f"{display_date(early_cut)} {separator} Bug fixes and API "
                "additions from preview.1 feedback."
            ),
        ),
        DesiredMilestone(
            f"{base}-rc.1",
            stable_cut,
            (
                f"Skia m{milestone} RC {separator} Start "
                f"{display_date(early_stable)} {separator} Critical bug fixes "
                "only, no new features."
            ),
        ),
        DesiredMilestone(
            base,
            stable,
            (
                f"Skia m{milestone} stable {separator} Start "
                f"{display_date(stable_cut)} {separator} Ship to NuGet.org, "
                "tag and create GitHub Release."
            ),
        ),
    ]


def execution_command(args) -> str:
    command = [
        sys.executable,
        ".agents/skills/release-milestones/scripts/sync-milestones.py",
        "--count",
        str(args.count),
        "--repo",
        args.repo,
    ]
    return common.shell_command(command)


def plan_closures(
    existing: dict[str, dict],
    milestones: list[ReleaseMilestone],
    tags: list[str],
    open_issues_for,
    *,
    creatable_titles: set[str] | None = None,
) -> tuple[list[dict], list[str]]:
    creatable_titles = creatable_titles or set()
    ordered = sorted(milestones, key=lambda item: item.sort_key)
    operations = []
    warnings = []
    for current_item in ordered:
        found = existing.get(current_item.title)
        tag = common.shipped_tag(current_item.title, tags)
        if not found or found.get("state") != "open" or not tag:
            continue
        open_issues = open_issues_for(current_item.title)
        target = next(
            (
                candidate
                for candidate in ordered
                if candidate.sort_key > current_item.sort_key
                and not common.shipped_tag(candidate.title, tags)
                and (
                    existing.get(candidate.title, {}).get("state") == "open"
                    or candidate.title in creatable_titles
                )
            ),
            None,
        )
        if open_issues and target is None:
            warnings.append(
                f"{current_item.title} shipped as {tag} but has "
                f"{len(open_issues)} open issue(s) and no future milestone"
            )
            status = "blocked"
        else:
            status = "pending"
        operations.append(
            {
                "title": current_item.title,
                "number": int(found["number"]),
                "tag": tag,
                "status": status,
                "openIssues": open_issues,
                "moveTo": target.title if target else None,
            }
        )
    return operations, warnings


def build_plan(args) -> tuple[dict, list[dict]]:
    root = common.repository_root()
    major, current = common.read_current_version(root)
    github = common.GitHub(args.repo)
    existing = github.milestone_map()
    cutoff = datetime.now(timezone.utc).date().toordinal() - 30
    operations = []
    desired = []
    for milestone in range(current, current + args.count):
        schedule = fetch_schedule(milestone)
        desired.extend(
            desired_milestones(
                schedule,
                milestone=milestone,
                major=major,
            )
        )
    for item in desired:
        found = existing.get(item.title)
        expected_due = item.due.isoformat()
        if found:
            actual_due = (
                str(found.get("due_on") or "")[:10]
            )
            actual_description = found.get("description") or ""
            changes = []
            if actual_due != expected_due:
                changes.append(
                    {
                        "field": "dueOn",
                        "from": actual_due or None,
                        "to": expected_due,
                    }
                )
            if actual_description != item.description:
                changes.append(
                    {
                        "field": "description",
                        "from": actual_description or None,
                        "to": item.description,
                    }
                )
            status = "pending" if changes else "done"
            action = "update" if changes else "none"
            number = int(found["number"])
        elif item.due.toordinal() >= cutoff:
            status = "pending"
            action = "create"
            changes = []
            number = None
        else:
            status = "skipped"
            action = "none"
            changes = []
            number = None
        operations.append(
            {
                "title": item.title,
                "number": number,
                "status": status,
                "action": action,
                "dueOn": item.due_on,
                "description": item.description,
                "changes": changes,
            }
        )
    tags = remote_tags(root)
    known = {
        title: ReleaseMilestone.parse(title)
        for title in {*existing, *(item.title for item in desired)}
    }
    closure_operations, warnings = plan_closures(
        existing,
        [item for item in known.values() if item is not None],
        tags,
        github.open_milestone_issues,
        creatable_titles={
            item["title"]
            for item in operations
            if item["status"] == "pending" and item["action"] == "create"
        },
    )
    pending = [item for item in operations if item["status"] == "pending"]
    pending_closures = [
        item for item in closure_operations if item["status"] == "pending"
    ]
    if warnings:
        next_action = "resolve-sync-warnings"
    elif pending or pending_closures:
        next_action = "confirm-sync"
    else:
        next_action = "complete"
    report = {
        "schemaVersion": 1,
        "dryRun": bool(args.dry_run),
        "repository": args.repo,
        "source": {
            "majorVersion": major,
            "currentSkiaMilestone": current,
            "scheduleCount": args.count,
        },
        "operations": operations,
        "closureOperations": closure_operations,
        "summary": {
            "create": sum(item["action"] == "create" for item in operations),
            "update": sum(item["action"] == "update" for item in operations),
            "unchanged": sum(item["status"] == "done" for item in operations),
            "skipped": sum(
                item["status"] == "skipped" for item in operations
            ),
            "moveIssues": sum(
                len(item["openIssues"]) for item in pending_closures
            ),
            "close": len(pending_closures),
        },
        "warnings": warnings,
        "nextAction": next_action,
        "executionCommand": (
            execution_command(args)
            if next_action == "confirm-sync"
            else None
        ),
    }
    return report, {
        "schedule": operations,
        "closures": closure_operations,
    }


def execute(args, plan: dict[str, list[dict]]) -> None:
    github = common.GitHub(args.repo)
    for item in plan["schedule"]:
        if item["action"] == "create":
            github.create_milestone(
                title=item["title"],
                due_on=item["dueOn"],
                description=item["description"],
            )
        elif item["action"] == "update":
            github.update_milestone(
                item["number"],
                due_on=item["dueOn"],
                description=item["description"],
            )
    milestones = github.milestone_map()
    for item in plan["closures"]:
        if item["status"] != "pending":
            continue
        target_title = item["moveTo"]
        if item["openIssues"]:
            target = milestones.get(target_title)
            if not target:
                raise common.MilestoneError(
                    f"future milestone {target_title} does not exist"
                )
            for issue in item["openIssues"]:
                github.update_issue_milestone(
                    int(issue["number"]),
                    int(target["number"]),
                )
        remaining = github.open_milestone_issues(item["title"])
        if remaining:
            raise common.MilestoneError(
                f"milestone {item['title']} still has open issues"
            )
        github.close_milestone(item["number"])


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--count", type=int, default=5)
    parser.add_argument("--repo", default=common.GITHUB_REPOSITORY)
    parser.add_argument("--dry-run", action="store_true")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        if args.count <= 0:
            raise common.MilestoneError("count must be positive")
        report, plan = build_plan(args)
        if not args.dry_run:
            execute(args, plan)
            report, _ = build_plan(args)
            report["dryRun"] = False
        print(json.dumps(report, indent=2))
    except (common.MilestoneError, OSError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
