#!/usr/bin/env python3
"""Apply reviewed release summaries to managed GitHub Release bodies.

The release-notes workflow and skill own summary prose (headline/body) and
this package owns Markdown structure; this script only selects exact tags,
expands deterministic links, and replaces the managed summary region of an
already-marked GitHub Release body byte-for-byte. It never touches the
``SKIASHARP:GITHUB-GENERATED-NOTES`` region GitHub itself generated, it skips
(never rewrites) a release whose body has no managed markers at all -- a
historical release published before this feature existed -- and it skips
(never rewrites) an unpublished draft: Finish persists a body-hash for the
exact draft it created while its own environment approval is pending, and
patching the draft here would invalidate that hash out from under Finish, a
genuine cross-workflow race. A draft converges once GitHub's "release"
(published) event fires, or on this workflow's next scheduled/dispatched run.

    update_github_summaries.py --event push --repository mono/SkiaSharp
    update_github_summaries.py --event release --repository mono/SkiaSharp --tag v4.151.0
    update_github_summaries.py --event workflow_dispatch --tag v4.151.0-preview.1
"""

from __future__ import annotations

import argparse
from dataclasses import dataclass, field
import json
import os
from pathlib import Path
import sys
from typing import Callable, Protocol

_PACKAGE_DIR = Path(__file__).resolve().parent
_DOCS_DIR = _PACKAGE_DIR.parent
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import common, github, render_summary, safety, shipments as shipments_module

SOURCES_DIR = "documentation/docfx/releases/_sources"


class UpdateError(RuntimeError):
    """A precondition failed; no GitHub write has been attempted (or, once
    raised past the race barrier, no *further* write will be attempted)."""


@dataclass(frozen=True)
class Candidate:
    tag: str
    prose_path: Path
    data_path: Path
    prose: dict
    data: dict
    shipment: dict


@dataclass(frozen=True)
class ResultEntry:
    tag: str
    status: str
    detail: str


@dataclass
class UpdateResult:
    entries: list[ResultEntry] = field(default_factory=list)

    def add(self, tag: str, status: str, detail: str) -> None:
        self.entries.append(ResultEntry(tag, status, detail))

    def markdown(self, *, error_message: str | None = None) -> str:
        lines = ["## GitHub Release summary updater", ""]
        if error_message:
            lines.extend(["**Failed:** {}".format(error_message), ""])
        if not self.entries:
            lines.append("No reviewed release summary changes were eligible.")
            return "\n".join(lines) + "\n"
        lines.extend(["| Tag | Result | Detail |", "|---|---|---|"])
        for entry in self.entries:
            detail = entry.detail.replace("|", "\\|").replace("\n", " ")
            lines.append("| `{}` | {} | {} |".format(entry.tag, entry.status, detail))
        return "\n".join(lines) + "\n"


class RepositoryView:
    """Read-only access to the committed ``_sources/*.data.json``/``.prose.json``."""

    def __init__(self, root: Path):
        self.root = root

    def data_paths(self) -> list[Path]:
        directory = self.root / SOURCES_DIR
        if not directory.is_dir():
            return []
        return sorted(directory.glob("*.data.json"))

    def read_json(self, path: Path) -> dict | None:
        if not path.exists():
            return None
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except ValueError as exc:
            raise UpdateError("{} is not valid JSON: {}".format(path, exc)) from exc
        if not isinstance(value, dict):
            raise UpdateError("{} must contain a JSON object".format(path))
        return value


def _version_from_data_path(path: Path) -> str:
    name = path.name
    assert name.endswith(".data.json")
    return name[: -len(".data.json")]


def _summary_map(prose: dict | None) -> dict:
    summaries = (prose or {}).get("release_summaries")
    if summaries is None:
        return {}
    if not isinstance(summaries, dict):
        raise UpdateError("release_summaries must be a JSON object")
    return summaries


def select_candidates(
    repository: RepositoryView,
    *,
    tag: str | None = None,
) -> list[Candidate]:
    """Select every exact shipment with both facts and reviewed prose.

    A ``push``/``release`` (no ``tag``) run converges everything on current
    main; a ``workflow_dispatch``/``release`` run with a ``tag`` narrows to
    that one exact tag. Old-format data.json (below
    :data:`common.DATA_FORMAT`) is silently skipped UNLESS the caller
    explicitly asked for a tag whose own page is that old file -- then it is
    a loud, actionable error rather than a silent no-op.
    """

    requested_core = None
    if tag is not None:
        parsed = common.parse_tag(tag)
        if parsed is None:
            raise UpdateError("invalid exact release tag {!r}".format(tag))
        requested_core = parsed.core

    candidates: list[Candidate] = []
    seen_tags: dict[str, Path] = {}
    for data_path in repository.data_paths():
        version = _version_from_data_path(data_path)
        if version.endswith("-unreleased"):
            continue  # An unreleased head page is never tagged; no shipments.
        data = repository.read_json(data_path)
        if data is None:
            continue
        if data.get("format") != common.DATA_FORMAT:
            if requested_core is not None and version == requested_core:
                raise UpdateError(
                    "{} uses unsupported release data format {}; expected {}. "
                    "Force-regenerate this version before updating its "
                    "release.".format(data_path, data.get("format"), common.DATA_FORMAT)
                )
            continue

        shipment_list = data.get("shipments") or []
        shipment_errors = shipments_module.validate_shipments(shipment_list)
        if shipment_errors:
            raise UpdateError("{}: {}".format(data_path, "; ".join(shipment_errors)))
        shipment_map = {item["tag"]: item for item in shipment_list}
        for shipment in shipment_map.values():
            if shipment["core_version"] != version:
                raise UpdateError(
                    "{}: shipment {} core_version {!r} does not match its own "
                    "page version {!r}".format(
                        data_path, shipment["tag"], shipment["core_version"], version
                    )
                )

        prose_path = data_path.with_name(version + ".prose.json")
        prose = repository.read_json(prose_path)
        summary_map = _summary_map(prose)

        eligible = sorted(set(shipment_map) & set(summary_map))
        if tag is not None:
            eligible = [item for item in eligible if item == tag]
        for exact_tag in eligible:
            if exact_tag in seen_tags:
                raise UpdateError(
                    "exact release tag {} appears in multiple data files "
                    "({} and {})".format(exact_tag, seen_tags[exact_tag], data_path)
                )
            seen_tags[exact_tag] = data_path
            candidates.append(
                Candidate(
                    tag=exact_tag,
                    prose_path=prose_path,
                    data_path=data_path,
                    prose=prose or {},
                    data=data,
                    shipment=shipment_map[exact_tag],
                )
            )
    return candidates


def render_managed_summary(
    candidate: Candidate,
    renderer: Callable[[dict, dict, str], str] = render_summary.render_github_release_summary,
) -> str:
    """Render ``candidate``'s summary body with its :data:`RELEASE_LINKS_MARKER`
    expanded into the exact deterministic links for this shipment."""

    try:
        rendered = renderer(candidate.data, candidate.prose, candidate.tag)
    except (KeyError, ValueError) as exc:
        raise UpdateError("{}: {}".format(candidate.tag, exc)) from exc

    if rendered.count(safety.RELEASE_LINKS_MARKER) != 1:
        raise UpdateError(
            "{}: rendered summary must contain exactly one {}".format(
                candidate.tag, safety.RELEASE_LINKS_MARKER
            )
        )
    shipment = candidate.shipment
    public_version = shipment["public_version"]
    core_version = shipment["core_version"]
    links = [
        "\U0001F4E6 [NuGet](https://www.nuget.org/packages/SkiaSharp/{})".format(
            public_version
        ),
        "\U0001F4D6 [Release notes]"
        "(https://mono.github.io/SkiaSharp/docs/releases/{}.html)".format(core_version),
    ]
    changelog_url = shipment.get("changelog_url")
    if changelog_url:
        expected_prefix = "https://github.com/{}/compare/".format(common.REPO)
        if not isinstance(changelog_url, str) or not changelog_url.startswith(expected_prefix):
            raise UpdateError(
                "{}: shipment has an invalid changelog_url".format(candidate.tag)
            )
        links.append("\U0001F500 [Full changelog]({})".format(changelog_url))
    return rendered.replace(
        safety.RELEASE_LINKS_MARKER, " \u00b7 ".join(links)
    ).strip()


class GitHubSummaryClient(Protocol):
    """The narrow surface the updater needs from a GitHub client.

    Production wires this package's minimal REST client; tests use an
    in-memory fake, so no test uses the network.
    """

    def get_release(self, tag: str):  # -> github.ReleaseInfo | None
        ...

    def update_release_body(self, *, tag: str, body: str) -> None:
        ...


@dataclass(frozen=True)
class PlannedUpdate:
    candidate: Candidate
    previous_body: str
    new_body: str


def update_releases(
    candidates: list[Candidate],
    client: GitHubSummaryClient,
    *,
    renderer: Callable[[dict, dict, str], str] = render_summary.render_github_release_summary,
) -> UpdateResult:
    """Preflight every candidate, race-check every body, then write.

    Three phases, matching the release-CLI's own preflight/apply/verify
    convention:

    1. **Preflight** -- fetch each release, skip it (never an error) when it
       does not exist, is still an unpublished draft (Finish holds a
       persisted body-hash for the exact draft it created and is waiting on
       environment approval to publish; patching the draft here would
       invalidate that hash out from under Finish -- a genuine
       cross-workflow race -- and force an unrelated reapproval), or has no
       managed markers (a historical, unmarked release), skip when the
       computed body is already current (idempotent), else render + validate
       and stage a plan. Any hard error here aborts the WHOLE batch before a
       single write is sent.
    2. **Race barrier** -- immediately before the first write, re-fetch every
       staged release and require its body to be byte-identical to what
       preflight read. The REST API has no conditional PATCH, so this
       is the last chance to detect a concurrent edit; any drift aborts the
       whole batch with none written.
    3. **Write + verify** -- PATCH the body, then re-fetch and require the
       stored body to equal the intended new body exactly.
    """

    result = UpdateResult()
    plans: list[PlannedUpdate] = []
    errors: list[str] = []

    for candidate in candidates:
        try:
            existing = client.get_release(candidate.tag)
            if existing is None:
                result.add(candidate.tag, "skipped", "GitHub Release does not exist")
                continue
            if existing.is_draft:
                # Finish holds an unpublished draft while its own environment
                # approval is pending, and separately persists the exact
                # draft body's hash to verify against before publishing. If
                # we patched the draft here, that persisted hash would go
                # stale out from under Finish -- a genuine cross-workflow
                # race -- and force an unrelated reapproval. Never write to a
                # draft; converge once GitHub's "release" (published) event
                # fires, or on this workflow's next scheduled/dispatched run
                # after that.
                result.add(
                    candidate.tag,
                    "skipped",
                    "release is an unpublished draft -- converges on publish "
                    "or the next run",
                )
                continue
            if not github.has_managed_markers(existing.body):
                result.add(
                    candidate.tag,
                    "skipped",
                    "release body has no managed markers (unmarked historical release)",
                )
                continue
            summary_text = render_managed_summary(candidate, renderer)
            new_body = github.replace_managed_summary(existing.body, summary_text)
            if new_body is None:
                result.add(
                    candidate.tag, "skipped", "release body markers disappeared before write"
                )
                continue
            if new_body == existing.body:
                result.add(
                    candidate.tag, "unchanged", "managed summary already matches reviewed prose"
                )
                continue
            plans.append(PlannedUpdate(candidate, existing.body, new_body))
        except UpdateError as exc:
            errors.append("{}: {}".format(candidate.tag, exc))
        except github.GitHubError as exc:
            errors.append("{}: {}".format(candidate.tag, exc))

    if errors:
        raise UpdateError(
            "preflight failed before any release update: " + "; ".join(errors)
        )

    # Race barrier: re-read every planned release immediately before the
    # first write.
    for plan in plans:
        current = client.get_release(plan.candidate.tag)
        if current is None:
            errors.append(
                "{}: release disappeared after preflight".format(plan.candidate.tag)
            )
        elif current.body != plan.previous_body:
            errors.append(
                "{}: release body changed after preflight; no write was sent".format(
                    plan.candidate.tag
                )
            )
    if errors:
        raise UpdateError(
            "release body changed after preflight; no write was sent: "
            + "; ".join(errors)
        )

    for plan in plans:
        client.update_release_body(tag=plan.candidate.tag, body=plan.new_body)
        stored = client.get_release(plan.candidate.tag)
        if stored is None or stored.body != plan.new_body:
            raise UpdateError(
                "{}: GitHub Release body did not match the requested managed "
                "summary after write".format(plan.candidate.tag)
            )
        result.add(
            plan.candidate.tag,
            "updated",
            "managed summary replaced and verified",
        )
    return result


def _write_summary(result: UpdateResult, *, error_message: str | None = None) -> None:
    text = result.markdown(error_message=error_message)
    sys.stdout.write(text)
    path = os.environ.get("GITHUB_STEP_SUMMARY")
    if path:
        with open(path, "a", encoding="utf-8") as stream:
            stream.write(text)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--event", required=True, choices=("push", "release", "workflow_dispatch")
    )
    parser.add_argument("--repository", default=common.REPO)
    parser.add_argument("--tag")
    parser.add_argument("--root", type=Path, default=Path.cwd())
    return parser


def main(argv: list[str] | None = None) -> int:
    args = create_parser().parse_args(argv)
    result = UpdateResult()
    try:
        repository = RepositoryView(args.root.resolve())
        tag = args.tag if args.event in ("release", "workflow_dispatch") else None
        candidates = select_candidates(repository, tag=tag)
        client = github.RestGitHubClient(args.repository)
        result = update_releases(candidates, client)
        _write_summary(result)
        return 0
    except (OSError, UpdateError) as exc:
        _write_summary(result, error_message=str(exc))
        print("ERROR: {}".format(exc), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
