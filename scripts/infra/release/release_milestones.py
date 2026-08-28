"""Milestone reconciliation and advance/closeout logic.

Ported and refactored from ``.agents/skills/release-milestones/scripts/
milestone_common.py``, ``advance-release-milestones.py``, and
``reconcile-release-assignments.py`` on ``main``
(6386960c2a4fddf0e68a8815856cbb7470deefce). The generated
``reconcile-release-assignments.py --dry-run`` command string produced by the
old ``release_github.py`` is intentionally not reproduced.
"""

from __future__ import annotations

import re
from dataclasses import dataclass
from typing import Protocol

from release_common import ReleaseToolError
import release_model as model

_CHANNEL_RANK = {"preview": 0, "rc": 1, None: 2}


class MilestoneError(ReleaseToolError):
    """Milestone state could not be read or changed safely."""


@dataclass(frozen=True)
class ReleaseMilestone:
    title: str
    numeric: tuple[int, ...]
    channel: str | None
    iteration: int | None

    @classmethod
    def parse(cls, title: str) -> "ReleaseMilestone | None":
        match = model.RELEASE_VERSION_RE.fullmatch(title)
        if not match:
            return None
        numeric = tuple(int(part) for part in match.group("numeric").split("."))
        channel = match.group("channel")
        iteration = int(match.group("iteration")) if channel else None
        return cls(title=title, numeric=numeric, channel=channel, iteration=iteration)

    @property
    def sort_key(self) -> tuple:
        return (self.numeric, _CHANNEL_RANK[self.channel], self.iteration or 0)


def shipped_tag(title: str, tags: list[str]) -> str | None:
    """Return the exact tag for ``title`` if the release shipped, else ``None``."""

    exact = f"v{title}"
    return exact if exact in tags else None


@dataclass(frozen=True)
class Milestone:
    number: int
    title: str
    state: str  # "open" | "closed"


@dataclass(frozen=True)
class MilestoneItem:
    number: int
    title: str
    url: str
    kind: str  # "pull-request" | "issue"


class MilestoneClient(Protocol):
    def milestones(self) -> list[Milestone]: ...

    def create_milestone(self, title: str, *, due_on: str | None, description: str | None) -> Milestone: ...

    def open_milestone_items(self, milestone_number: int) -> list[MilestoneItem]: ...

    def update_item_milestone(self, item_number: int, milestone_number: int) -> None: ...

    def close_milestone(self, milestone_number: int) -> None: ...

    def closing_issues(self, pull_request_number: int) -> list[int]: ...

    def pull_request_milestone(self, pull_request_number: int) -> str | None: ...

    def issue_milestone(self, issue_number: int) -> str | None: ...


def milestone_map(milestones: list[Milestone]) -> dict[str, Milestone]:
    result: dict[str, Milestone] = {}
    duplicates = []
    for milestone in milestones:
        if milestone.title in result:
            duplicates.append(milestone.title)
        result[milestone.title] = milestone
    if duplicates:
        raise MilestoneError(f"duplicate milestone titles: {sorted(set(duplicates))}")
    return result


@dataclass(frozen=True)
class ClosureOperation:
    milestone_title: str
    milestone_number: int
    tag: str
    status: str  # "done" | "pending" | "blocked"
    open_items: tuple[MilestoneItem, ...]
    move_to_title: str | None
    move_to_number: int | None
    detail: str | None = None


def plan_closeout(
    *,
    milestones: list[Milestone],
    tags: list[str],
    open_items_for: "callable[[int], list[MilestoneItem]]",
) -> tuple[list[ClosureOperation], list[str]]:
    """Plan moving open items off every shipped-but-open milestone and closing it.

    A milestone only needs a closure operation once: after it closes it no
    longer has ``state == "open"`` and will not be replanned, which is what
    makes rerunning closeout idempotent.
    """

    warnings: list[str] = []
    operations: list[ClosureOperation] = []
    parsed = [
        (milestone, ReleaseMilestone.parse(milestone.title))
        for milestone in milestones
    ]
    parsed = [(m, r) for m, r in parsed if r is not None]
    parsed.sort(key=lambda pair: pair[1].sort_key)

    for milestone, release in parsed:
        if milestone.state != "open":
            continue
        tag = shipped_tag(milestone.title, tags)
        if tag is None:
            continue
        open_items = tuple(open_items_for(milestone.number))
        target = None
        for candidate, candidate_release in parsed:
            if candidate_release.sort_key <= release.sort_key:
                continue
            if candidate.state != "open":
                continue
            if shipped_tag(candidate.title, tags) is not None:
                continue
            target = candidate
            break
        if open_items and target is None:
            warnings.append(
                f"milestone {milestone.title!r} shipped as {tag} but has open "
                "items and no unshipped milestone to move them to"
            )
            operations.append(
                ClosureOperation(
                    milestone_title=milestone.title,
                    milestone_number=milestone.number,
                    tag=tag,
                    status="blocked",
                    open_items=open_items,
                    move_to_title=None,
                    move_to_number=None,
                    detail="no eligible target milestone",
                )
            )
            continue
        operations.append(
            ClosureOperation(
                milestone_title=milestone.title,
                milestone_number=milestone.number,
                tag=tag,
                status="pending",
                open_items=open_items,
                move_to_title=target.title if target else None,
                move_to_number=target.number if target else None,
            )
        )
    return operations, warnings


def apply_closeout(operations: list[ClosureOperation], client: MilestoneClient) -> list[dict]:
    results = []
    for operation in operations:
        if operation.status == "blocked":
            results.append(
                {
                    "milestone": operation.milestone_title,
                    "status": "blocked",
                    "detail": operation.detail,
                }
            )
            continue
        for item in operation.open_items:
            client.update_item_milestone(item.number, operation.move_to_number)
        remaining = client.open_milestone_items(operation.milestone_number)
        if remaining:
            raise MilestoneError(
                f"milestone {operation.milestone_title!r} still has open items "
                f"after moving: {[item.number for item in remaining]}"
            )
        client.close_milestone(operation.milestone_number)
        results.append(
            {
                "milestone": operation.milestone_title,
                "status": "done",
                "movedTo": operation.move_to_title,
            }
        )
    return results


_PR_NUMBER_RE = re.compile(r"\(#(\d+)\)")


def extract_merged_pull_requests(commit_subjects: list[str]) -> list[int]:
    """Extract PR numbers referenced by ``(#123)`` in first-parent commit subjects."""

    numbers = []
    for subject in commit_subjects:
        match = _PR_NUMBER_RE.search(subject)
        if match:
            numbers.append(int(match.group(1)))
    return numbers


@dataclass(frozen=True)
class ReconcileOperation:
    kind: str  # "pull-request" | "issue"
    number: int
    via_pull_request: int | None
    from_milestone: str | None
    to_milestone: str
    to_milestone_number: int
    status: str = "pending"


def plan_reconcile(
    *,
    pull_request_numbers: list[int],
    target_milestone: Milestone,
    get_pull_request_milestone: "callable[[int], str | None]",
    get_closing_issues: "callable[[int], list[int]]",
    get_issue_milestone: "callable[[int], str | None]",
) -> list[ReconcileOperation]:
    """Plan assigning merged PRs (and issues they close) to ``target_milestone``."""

    operations: list[ReconcileOperation] = []
    for pr_number in pull_request_numbers:
        current = get_pull_request_milestone(pr_number)
        if current != target_milestone.title:
            operations.append(
                ReconcileOperation(
                    kind="pull-request",
                    number=pr_number,
                    via_pull_request=None,
                    from_milestone=current,
                    to_milestone=target_milestone.title,
                    to_milestone_number=target_milestone.number,
                )
            )
        for issue_number in get_closing_issues(pr_number):
            issue_current = get_issue_milestone(issue_number)
            if issue_current != target_milestone.title:
                operations.append(
                    ReconcileOperation(
                        kind="issue",
                        number=issue_number,
                        via_pull_request=pr_number,
                        from_milestone=issue_current,
                        to_milestone=target_milestone.title,
                        to_milestone_number=target_milestone.number,
                    )
                )
    return operations


def apply_reconcile(operations: list[ReconcileOperation], client: MilestoneClient) -> None:
    for operation in operations:
        client.update_item_milestone(operation.number, operation.to_milestone_number)
