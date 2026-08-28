"""Milestone reconciliation and advance/closeout logic.

Ported and refactored from ``.agents/skills/release-milestones/scripts/
milestone_common.py``, ``advance-release-milestones.py``, and
``reconcile-release-assignments.py`` on ``main``
(6386960c2a4fddf0e68a8815856cbb7470deefce). The generated
``reconcile-release-assignments.py --dry-run`` command string produced by the
old ``release_github.py`` is intentionally not reproduced. The schedule/
desired-milestone calculation and injectable schedule client (this module's
``desired_milestones``/``plan_schedule_operations``/``ScheduleClient``/
``HttpChromiumScheduleClient``) are ported from the same
``advance-release-milestones.py`` -- the one capability the initial
consolidation onto ``release_milestones.py`` dropped (it only reconciled/
closed *existing* milestones, never created or updated the upcoming
preview/RC/stable schedule from Chromium/Skia).
"""

from __future__ import annotations

import json
import re
from dataclasses import dataclass
from datetime import date
from typing import Protocol
from urllib import request as urllib_request
from urllib.error import HTTPError, URLError

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
    due_on: str | None = None
    description: str | None = None


@dataclass(frozen=True)
class MilestoneItem:
    number: int
    title: str
    url: str
    kind: str  # "pull-request" | "issue"


class MilestoneClient(Protocol):
    def milestones(self) -> list[Milestone]: ...

    def create_milestone(self, title: str, *, due_on: str | None, description: str | None) -> Milestone: ...

    def update_milestone(self, number: int, *, due_on: str, description: str) -> None: ...

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
    creatable_titles: frozenset[str] = frozenset(),
) -> tuple[list[ClosureOperation], list[str]]:
    """Plan moving open items off every shipped-but-open milestone and closing it.

    A milestone only needs a closure operation once: after it closes it no
    longer has ``state == "open"`` and will not be replanned, which is what
    makes rerunning closeout idempotent.

    ``creatable_titles`` lets a read-only preview (``finish closeout
    --dry-run``) recognize an upcoming schedule milestone that does not
    exist yet, but has a pending "create" schedule operation that will be
    applied before this closure logic runs for real, as a valid move
    target -- so the preview does not misreport "blocked" for a release
    whose actual apply would succeed. A real ``apply_closeout`` always
    creates/updates schedule milestones and re-reads live milestone state
    before calling this, so it never needs to pass anything here.
    """

    warnings: list[str] = []
    operations: list[ClosureOperation] = []
    parsed = [
        (milestone, ReleaseMilestone.parse(milestone.title))
        for milestone in milestones
    ]
    parsed = [(m, r) for m, r in parsed if r is not None]
    parsed.sort(key=lambda pair: pair[1].sort_key)

    creatable = sorted(
        ((title, release) for title in creatable_titles if (release := ReleaseMilestone.parse(title)) is not None),
        key=lambda pair: pair[1].sort_key,
    )

    for milestone, release in parsed:
        if milestone.state != "open":
            continue
        tag = shipped_tag(milestone.title, tags)
        if tag is None:
            continue
        open_items = tuple(open_items_for(milestone.number))
        target_title: str | None = None
        target_number: int | None = None
        for candidate, candidate_release in parsed:
            if candidate_release.sort_key <= release.sort_key:
                continue
            if candidate.state != "open":
                continue
            if shipped_tag(candidate.title, tags) is not None:
                continue
            target_title, target_number = candidate.title, candidate.number
            break
        if target_title is None:
            for candidate_title, candidate_release in creatable:
                if candidate_release.sort_key <= release.sort_key:
                    continue
                if shipped_tag(candidate_title, tags) is not None:
                    continue
                target_title, target_number = candidate_title, None
                break
        if open_items and target_title is None:
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
                move_to_title=target_title,
                move_to_number=target_number,
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


# --- Schedule maintenance: the upcoming preview/RC/stable milestones ------
#
# Ported from the retired ``advance-release-milestones.py``: for each of the
# next few Chromium/Skia milestones, fetch its public release schedule and
# derive the four SkiaSharp milestones (preview.1, preview.2, rc.1, stable)
# that should exist with specific due dates, then create/update them to
# match. This never touches milestones that already match, and never
# deletes/renames anything.

CHROMIUM_SCHEDULE_URL = "https://chromiumdash.appspot.com/fetch_milestone_schedule?mstone={milestone}"

_REQUIRED_SCHEDULE_FIELDS = (
    "branch_point",
    "earliest_beta",
    "early_stable_cut",
    "early_stable",
    "stable_cut",
    "stable_date",
)

# scripts/VERSIONS.txt lines this reads, e.g. "SkiaSharp    nuget    4.152.0"
# (major=4) and "libSkiaSharp    milestone    152" (current Skia milestone).
_SKIASHARP_NUGET_MAJOR_RE = re.compile(r"^SkiaSharp\s+nuget\s+(\d+)\.", re.MULTILINE)
_LIBSKIASHARP_MILESTONE_RE = re.compile(r"^libSkiaSharp\s+milestone\s+(\d+)\s*$", re.MULTILINE)


def parse_current_major_and_milestone(versions_text: str) -> tuple[int, int]:
    """Return ``(major, current_skia_milestone)`` from ``scripts/VERSIONS.txt``.

    e.g. major ``4`` from ``SkiaSharp nuget 4.152.0`` and milestone ``152``
    from ``libSkiaSharp milestone 152``.
    """

    major_match = _SKIASHARP_NUGET_MAJOR_RE.search(versions_text)
    milestone_match = _LIBSKIASHARP_MILESTONE_RE.search(versions_text)
    if not major_match or not milestone_match:
        raise MilestoneError(
            "scripts/VERSIONS.txt has no 'SkiaSharp nuget X.Y.Z' or "
            "'libSkiaSharp milestone N' line"
        )
    return int(major_match.group(1)), int(milestone_match.group(1))


class ScheduleClient(Protocol):
    """A read-only source for one Chromium milestone's public release schedule."""

    def fetch_schedule(self, milestone: int) -> dict: ...


class HttpChromiumScheduleClient:
    """The real client, standard-library only: reads Chromium's own public
    milestone-schedule JSON endpoint. Never writes anything."""

    def __init__(self, *, timeout: int = 30):
        self.timeout = timeout

    def fetch_schedule(self, milestone: int) -> dict:
        url = CHROMIUM_SCHEDULE_URL.format(milestone=milestone)
        request = urllib_request.Request(url, headers={"User-Agent": "SkiaSharp-release-automation"})
        try:
            with urllib_request.urlopen(request, timeout=self.timeout) as response:  # noqa: S310
                raw = response.read()
        except HTTPError as exc:
            raise MilestoneError(
                f"failed to fetch Chromium schedule for m{milestone}: HTTP {exc.code}"
            ) from exc
        except URLError as exc:
            raise MilestoneError(f"failed to fetch Chromium schedule for m{milestone}: {exc}") from exc
        try:
            data = json.loads(raw.decode("utf-8"))
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise MilestoneError(
                f"Chromium schedule response for m{milestone} is not valid JSON: {exc}"
            ) from exc
        entries = data.get("mstones") or []
        if not entries:
            raise MilestoneError(f"Chromium returned no schedule for m{milestone}")
        schedule = entries[0]
        missing = [field for field in _REQUIRED_SCHEDULE_FIELDS if not schedule.get(field)]
        if missing:
            raise MilestoneError(f"Chromium m{milestone} schedule is missing {missing}")
        return schedule


@dataclass(frozen=True)
class DesiredMilestone:
    title: str
    due: date
    description: str

    @property
    def due_on(self) -> str:
        return f"{self.due.isoformat()}T00:00:00Z"


def _parse_schedule_date(value: str) -> date:
    return date.fromisoformat(value.split("T", 1)[0])


def _display_date(value: date) -> str:
    return value.strftime("%a, %b %d, %Y")


def desired_milestones(schedule: dict, *, milestone: int, major: int) -> list[DesiredMilestone]:
    """Compute the 4 desired preview/RC/stable milestones -- title, due
    date, description -- for one Chromium milestone's schedule.

    Wording/dates ported verbatim from the retired ``advance-release-
    milestones.py``'s ``desired_milestones``.
    """

    branch = _parse_schedule_date(schedule["branch_point"])
    beta = _parse_schedule_date(schedule["earliest_beta"])
    early_cut = _parse_schedule_date(schedule["early_stable_cut"])
    early_stable = _parse_schedule_date(schedule["early_stable"])
    stable_cut = _parse_schedule_date(schedule["stable_cut"])
    stable = _parse_schedule_date(schedule["stable_date"])
    base = f"{major}.{milestone}.0"
    separator = "\u00b7"
    return [
        DesiredMilestone(
            f"{base}-preview.1",
            beta,
            (
                f"Skia m{milestone} preview.1 {separator} Start "
                f"{_display_date(branch)} {separator} Merge Skia sync PR and "
                "ship preview."
            ),
        ),
        DesiredMilestone(
            f"{base}-preview.2",
            early_stable,
            (
                f"Skia m{milestone} preview.2 {separator} Start "
                f"{_display_date(early_cut)} {separator} Bug fixes and API "
                "additions from preview.1 feedback."
            ),
        ),
        DesiredMilestone(
            f"{base}-rc.1",
            stable_cut,
            (
                f"Skia m{milestone} RC {separator} Start "
                f"{_display_date(early_stable)} {separator} Critical bug fixes "
                "only, no new features."
            ),
        ),
        DesiredMilestone(
            base,
            stable,
            (
                f"Skia m{milestone} stable {separator} Start "
                f"{_display_date(stable_cut)} {separator} Ship to NuGet.org, "
                "tag and create GitHub Release."
            ),
        ),
    ]


# A desired milestone whose due date is more than this many days in the past
# is reported as "skipped" (never created): ported from the retired script's
# cutoff, which avoided creating a milestone for a schedule window that had
# already fully elapsed by the time this ever ran for a given Chromium
# milestone (e.g. the very first run against an old, already-past m-number).
_SCHEDULE_CREATE_CUTOFF_DAYS = 30


@dataclass(frozen=True)
class ScheduleOperation:
    title: str
    number: int | None
    status: str  # "done" | "pending" | "skipped"
    action: str  # "create" | "update" | "none"
    due_on: str
    description: str
    changes: tuple[dict, ...]


def plan_schedule_operations(
    desired: list[DesiredMilestone],
    existing: dict[str, Milestone],
    *,
    today: date,
) -> list[ScheduleOperation]:
    """Compare ``desired`` milestones against ``existing`` ones (by exact
    title) and produce a create/update/none operation for each.

    A milestone that already matches (same due date and description) is
    ``action="none"``/``status="done"``; one that already exists but
    disagrees is ``action="update"``/``status="pending"`` with the exact
    field-level ``changes``; one that does not exist yet and is not more
    than :data:`_SCHEDULE_CREATE_CUTOFF_DAYS` in the past is
    ``action="create"``/``status="pending"``; one that does not exist and
    is that far in the past is ``action="none"``/``status="skipped"``
    (never created after the fact).
    """

    cutoff = today.toordinal() - _SCHEDULE_CREATE_CUTOFF_DAYS
    operations: list[ScheduleOperation] = []
    for item in desired:
        found = existing.get(item.title)
        expected_due = item.due.isoformat()
        if found is not None:
            actual_due = (found.due_on or "")[:10]
            actual_description = found.description or ""
            changes: list[dict] = []
            if actual_due != expected_due:
                changes.append({"field": "dueOn", "from": actual_due or None, "to": expected_due})
            if actual_description != item.description:
                changes.append(
                    {"field": "description", "from": actual_description or None, "to": item.description}
                )
            status = "pending" if changes else "done"
            action = "update" if changes else "none"
            number = found.number
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
            ScheduleOperation(
                title=item.title,
                number=number,
                status=status,
                action=action,
                due_on=item.due_on,
                description=item.description,
                changes=tuple(changes),
            )
        )
    return operations


def apply_schedule_operations(operations: list[ScheduleOperation], client: MilestoneClient) -> list[dict]:
    """Apply every ``"create"``/``"update"`` schedule operation.

    Idempotent: an operation already ``action="none"`` (nothing disagrees)
    or ``status="skipped"`` (too far in the past to create) performs no
    write and is reported as such, matching its planned status exactly.
    """

    results: list[dict] = []
    for operation in operations:
        if operation.action == "create":
            created = client.create_milestone(
                operation.title, due_on=operation.due_on, description=operation.description
            )
            results.append(
                {"title": operation.title, "action": "create", "status": "done", "number": created.number}
            )
        elif operation.action == "update":
            client.update_milestone(operation.number, due_on=operation.due_on, description=operation.description)
            results.append(
                {"title": operation.title, "action": "update", "status": "done", "number": operation.number}
            )
        else:
            results.append(
                {"title": operation.title, "action": "none", "status": operation.status, "number": operation.number}
            )
    return results
