#!/usr/bin/env python3
"""Apply reviewed release summaries to managed GitHub Release bodies.

The release-notes workflow owns summary prose and Markdown rendering. This script
only selects exact tags, expands deterministic links, and replaces the managed
summary bytes in an already-marked GitHub Release body.
"""

from __future__ import annotations

import argparse
from dataclasses import dataclass, field
import hashlib
import importlib.util
import json
import os
from pathlib import Path, PurePosixPath
import re
import subprocess
import sys
from typing import Callable
from urllib import error, parse, request

PACKAGE_PARENT = Path(__file__).resolve().parent.parent
if str(PACKAGE_PARENT) not in sys.path:
    sys.path.insert(0, str(PACKAGE_PARENT))

from release_notes import common, model


REPOSITORY_DEFAULT = "mono/SkiaSharp"
SOURCES_DIR = PurePosixPath("documentation/docfx/releases/_sources")
PROSE_NAME_RE = re.compile(
    r"^documentation/docfx/releases/_sources/"
    r"(?P<version>\d+(?:\.\d+){2,3}(?:-unreleased)?)\.prose\.json$"
)
SHA_RE = re.compile(r"^[0-9a-f]{40}$")
REPOSITORY_RE = re.compile(
    r"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$"
)
TAG_RE = common.EXACT_RELEASE_TAG_RE
CORE_VERSION_RE = re.compile(r"^\d+(?:\.\d+){2,3}$")

SUMMARY_START_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:START -->"
SUMMARY_END_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:END -->"
GENERATED_START_MARKER = (
    "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->"
)
GENERATED_END_MARKER = (
    "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->"
)
RELEASE_LINKS_MARKER = "<!-- RELEASE_LINKS -->"


class UpdateError(RuntimeError):
    pass


@dataclass(frozen=True)
class Candidate:
    tag: str
    prose_path: str
    data_path: str
    prose: dict
    data: dict
    shipment: dict


@dataclass(frozen=True)
class ReleaseSnapshot:
    release_id: int
    tag: str
    body: str
    etag: str
    url: str

    @property
    def body_sha256(self) -> str:
        return hashlib.sha256(self.body.encode("utf-8")).hexdigest()


@dataclass(frozen=True)
class PlannedUpdate:
    candidate: Candidate
    snapshot: ReleaseSnapshot
    new_body: str


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
        lines.extend([
            "| Tag | Result | Detail |",
            "|---|---|---|",
        ])
        for entry in self.entries:
            detail = entry.detail.replace("|", "\\|").replace("\n", " ")
            lines.append("| `{}` | {} | {} |".format(
                entry.tag, entry.status, detail))
        return "\n".join(lines) + "\n"


def _run_git(args: list[str], *, allow_missing: bool = False) -> str | None:
    completed = subprocess.run(
        ["git", *args],
        check=False,
        capture_output=True,
        text=True,
    )
    if completed.returncode == 0:
        return completed.stdout
    if allow_missing and completed.returncode == 128:
        return None
    raise UpdateError(
        "git {} failed: {}".format(
            " ".join(args), completed.stderr.strip() or completed.stdout.strip())
    )


class RepositoryView:
    """Read current or historical JSON without evaluating repository content."""

    def __init__(self, root: Path):
        self.root = root

    @staticmethod
    def _validate_ref(ref: str) -> None:
        if not SHA_RE.fullmatch(ref):
            raise UpdateError("expected a full lowercase commit SHA, got {!r}".format(ref))

    @staticmethod
    def _validate_prose_path(path: str) -> re.Match:
        match = PROSE_NAME_RE.fullmatch(path)
        if not match:
            raise UpdateError("unexpected release prose path: {!r}".format(path))
        return match

    def changed_prose_paths(self, before: str, after: str) -> list[str]:
        self._validate_ref(after)
        if before == "0" * 40:
            output = _run_git([
                "-C", str(self.root), "ls-tree", "-r", "--name-only", after,
                "--", str(SOURCES_DIR),
            ]) or ""
        else:
            self._validate_ref(before)
            output = _run_git([
                "-C", str(self.root), "diff", "--name-only", "--diff-filter=AMDR",
                before, after, "--", str(SOURCES_DIR),
            ]) or ""
        paths = []
        for path in output.splitlines():
            if path.endswith(".data.json"):
                path = path[:-len(".data.json")] + ".prose.json"
            elif not path.endswith(".prose.json"):
                continue
            self._validate_prose_path(path)
            paths.append(path)
        return sorted(set(paths))

    def json_at(self, ref: str, path: str) -> dict | None:
        self._validate_ref(ref)
        self._validate_prose_path(
            path if path.endswith(".prose.json")
            else path.replace(".data.json", ".prose.json")
        )
        text = _run_git(
            ["-C", str(self.root), "show", "{}:{}".format(ref, path)],
            allow_missing=True,
        )
        if text is None:
            return None
        return _parse_object(text, "{} at {}".format(path, ref))

    def current_prose_paths(self) -> list[str]:
        directory = self.root / Path(SOURCES_DIR)
        paths = []
        for path in directory.glob("*.prose.json"):
            relative = path.relative_to(self.root).as_posix()
            self._validate_prose_path(relative)
            paths.append(relative)
        return sorted(paths)

    def current_json(self, path: str) -> dict | None:
        self._validate_prose_path(
            path if path.endswith(".prose.json")
            else path.replace(".data.json", ".prose.json")
        )
        full_path = self.root / path
        if not full_path.exists():
            return None
        return _parse_object(
            full_path.read_text(encoding="utf-8"),
            path,
        )


def _parse_object(text: str, source: str) -> dict:
    try:
        value = json.loads(text)
    except ValueError as exc:
        raise UpdateError("{} is not valid JSON: {}".format(source, exc)) from exc
    if not isinstance(value, dict):
        raise UpdateError("{} must contain a JSON object".format(source))
    return value


def _summary_map(prose: dict | None) -> dict:
    summaries = (prose or {}).get("release_summaries")
    if summaries is None:
        return {}
    if not isinstance(summaries, dict):
        raise UpdateError("release_summaries must be a JSON object")
    return summaries


def _shipment_map(data: dict | None, source: str) -> dict[str, dict]:
    if data is not None and data.get("format") != model.DATA_FORMAT:
        raise UpdateError(
            "{} uses unsupported release data format {}; expected {}"
            .format(source, data.get("format"), model.DATA_FORMAT)
        )
    shipments = (data or {}).get("shipments") or []
    if not isinstance(shipments, list):
        raise UpdateError("{} shipments must be an array".format(source))
    result = {}
    duplicates = []
    for shipment in shipments:
        if not isinstance(shipment, dict) or not isinstance(shipment.get("tag"), str):
            raise UpdateError("{} has a shipment without an exact tag".format(source))
        tag = shipment["tag"]
        if tag in result:
            duplicates.append(tag)
        result[tag] = shipment
    if duplicates:
        raise UpdateError(
            "{} has duplicate exact shipment tags: {}".format(
                source, ", ".join(sorted(set(duplicates))))
        )
    return result


def _is_supported_data(data: dict | None) -> bool:
    return bool(data and data.get("format") == model.DATA_FORMAT)


def _data_path(prose_path: str) -> str:
    return prose_path[:-len(".prose.json")] + ".data.json"


def _is_unreleased(prose_path: str) -> bool:
    match = PROSE_NAME_RE.fullmatch(prose_path)
    return bool(match and match.group("version").endswith("-unreleased"))


def _candidate(
    tag: str,
    prose_path: str,
    prose: dict,
    data: dict,
) -> Candidate:
    if not TAG_RE.fullmatch(tag):
        raise UpdateError("invalid exact release tag {!r}".format(tag))
    shipments = _shipment_map(data, _data_path(prose_path))
    shipment = shipments.get(tag)
    if shipment is None:
        raise UpdateError(
            "{} summary has no exact shipment facts in {}".format(
                tag, _data_path(prose_path))
        )
    public_version = shipment.get("public_version")
    core_version = shipment.get("core_version")
    if public_version != tag[1:]:
        raise UpdateError(
            "{} shipment public_version is {!r}, expected {!r}".format(
                tag, public_version, tag[1:])
        )
    if (
        not isinstance(core_version, str)
        or not CORE_VERSION_RE.fullmatch(core_version)
    ):
        raise UpdateError(
            "{} shipment has invalid core_version {!r}".format(tag, core_version)
        )
    source_version = PROSE_NAME_RE.fullmatch(prose_path).group("version")
    if source_version != core_version:
        raise UpdateError(
            "{} shipment core_version {!r} does not match {}".format(
                tag, core_version, prose_path)
        )
    return Candidate(
        tag=tag,
        prose_path=prose_path,
        data_path=_data_path(prose_path),
        prose=prose,
        data=data,
        shipment=shipment,
    )


def select_push_candidates(
    repository: RepositoryView,
    before: str,
    after: str,
) -> list[Candidate]:
    """Select only semantic exact-tag changes across a possibly batched push."""
    selected = []
    seen_tags = set()
    for prose_path in repository.changed_prose_paths(before, after):
        if _is_unreleased(prose_path):
            continue
        old_prose = repository.json_at(before, prose_path) if before != "0" * 40 else None
        new_prose = repository.json_at(after, prose_path)
        if new_prose is None:
            continue
        data_path = _data_path(prose_path)
        new_data = repository.json_at(after, data_path)
        if new_data is None:
            raise UpdateError("{} is missing".format(data_path))
        if not _is_supported_data(new_data):
            continue
        old_data = (
            repository.json_at(before, data_path)
            if before != "0" * 40 else None
        )
        if not _is_supported_data(old_data):
            old_data = None
            old_prose = None
        new_shipments = _shipment_map(new_data, data_path)
        old_shipments = _shipment_map(old_data, data_path)
        new_eligible = set(_summary_map(new_prose)) & set(new_shipments)
        new_eligible.update(
            tag for tag, shipment in new_shipments.items()
            if shipment.get("channel") == "stable"
        )
        old_eligible = set(_summary_map(old_prose)) & set(old_shipments)
        old_eligible.update(
            tag for tag, shipment in old_shipments.items()
            if shipment.get("channel") == "stable"
        )

        for tag in sorted(new_eligible):
            new_candidate = _candidate(tag, prose_path, new_prose, new_data)
            old_rendered = None
            if tag in old_eligible and old_prose is not None and old_data is not None:
                try:
                    old_rendered = render_managed_summary(
                        _candidate(tag, prose_path, old_prose, old_data)
                    )
                except UpdateError:
                    old_rendered = None
            new_rendered = render_managed_summary(new_candidate)
            if old_rendered == new_rendered:
                continue
            if tag in seen_tags:
                raise UpdateError(
                    "exact release tag {} is selected by multiple prose files".format(tag)
                )
            seen_tags.add(tag)
            selected.append(new_candidate)
    return selected


def select_current_candidates(
    repository: RepositoryView,
    *,
    tag: str | None = None,
) -> list[Candidate]:
    """Select reviewed current-main summaries for release/dispatch convergence."""
    selected = []
    seen_tags = set()
    summary_tags = set()
    shipment_tags = set()
    for prose_path in repository.current_prose_paths():
        if _is_unreleased(prose_path):
            continue
        prose = repository.current_json(prose_path)
        data_path = _data_path(prose_path)
        data = repository.current_json(data_path)
        if prose is None or data is None:
            continue
        if not _is_supported_data(data):
            continue
        summaries = _summary_map(prose)
        shipments = _shipment_map(data, data_path)
        summary_tags.update(summaries)
        shipment_tags.update(shipments)
        eligible = set(summaries) & set(shipments)
        eligible.update(
            shipment_tag
            for shipment_tag, shipment in shipments.items()
            if shipment.get("channel") == "stable"
        )
        if tag is not None:
            eligible.intersection_update({tag})
        for exact_tag in sorted(eligible):
            if exact_tag in seen_tags:
                raise UpdateError(
                    "exact release tag {} appears in multiple prose files".format(
                        exact_tag)
                )
            seen_tags.add(exact_tag)
            selected.append(_candidate(exact_tag, prose_path, prose, data))
    if tag is not None and tag in summary_tags and tag not in shipment_tags:
        raise UpdateError(
            "{} summary has no exact shipment facts in current data".format(tag)
        )
    return selected


def _load_renderer() -> Callable[[dict, dict, str], str]:
    path = Path(__file__).with_name("render.py")
    spec = importlib.util.spec_from_file_location(
        "_release_notes_summary_renderer", str(path))
    if spec is None or spec.loader is None:
        raise UpdateError("could not load release-notes summary renderer")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    renderer = getattr(module, "render_github_release_summary", None)
    if not callable(renderer):
        raise UpdateError(
            "release-notes renderer has no render_github_release_summary function"
        )
    return renderer


def render_managed_summary(
    candidate: Candidate,
    renderer: Callable[[dict, dict, str], str] | None = None,
) -> str:
    renderer = renderer or _load_renderer()
    try:
        rendered = renderer(candidate.data, candidate.prose, candidate.tag)
    except (KeyError, TypeError, ValueError) as exc:
        raise UpdateError(
            "{} summary failed validation: {}".format(candidate.tag, exc)
        ) from exc
    if rendered.count(RELEASE_LINKS_MARKER) != 1:
        raise UpdateError(
            "{} rendered summary must contain exactly one {}".format(
                candidate.tag, RELEASE_LINKS_MARKER)
        )
    shipment = candidate.shipment
    public_version = shipment["public_version"]
    core_version = shipment["core_version"]
    links = [
        "\U0001f4e6 [NuGet](https://www.nuget.org/packages/SkiaSharp/{})".format(
            public_version),
        "\U0001f4d6 [Release notes]"
        "(https://mono.github.io/SkiaSharp/docs/releases/{}.html)".format(
            core_version),
    ]
    changelog_url = shipment.get("changelog_url")
    base_version = (candidate.data.get("range") or {}).get("base_version")
    if shipment.get("channel") == "stable" and base_version:
        changelog_url = (
            "https://github.com/{}/compare/v{}...{}".format(
                candidate.data.get("repository") or REPOSITORY_DEFAULT,
                base_version,
                candidate.tag,
            )
        )
    if changelog_url:
        if not isinstance(changelog_url, str) or not changelog_url.startswith(
            "https://github.com/"
        ):
            raise UpdateError(
                "{} shipment has invalid changelog_url".format(candidate.tag)
            )
        links.append("\U0001f500 [Full changelog]({})".format(changelog_url))
    return rendered.replace(RELEASE_LINKS_MARKER, " \u00b7 ".join(links)).strip()


def _marker_positions(body: str) -> tuple[int, int, int, int] | None:
    markers = (
        SUMMARY_START_MARKER,
        SUMMARY_END_MARKER,
        GENERATED_START_MARKER,
        GENERATED_END_MARKER,
    )
    counts = [body.count(marker) for marker in markers]
    if counts == [0, 0, 0, 0]:
        return None
    if counts != [1, 1, 1, 1]:
        raise UpdateError("release body has incomplete or duplicate managed markers")
    positions = tuple(body.index(marker) for marker in markers)
    if not positions[0] < positions[1] < positions[2] < positions[3]:
        raise UpdateError("release body managed markers are out of order")
    return positions


def replace_managed_summary(body: str, summary: str) -> str | None:
    """Replace only managed summary bytes; return None for an unmarked legacy body."""
    positions = _marker_positions(body)
    if positions is None:
        return None
    summary_start, summary_end, _, _ = positions
    owned_start = summary_start + len(SUMMARY_START_MARKER)
    return (
        body[:owned_start]
        + "\n"
        + summary.strip()
        + "\n"
        + body[summary_end:]
    )


class GitHubClient:
    def __init__(
        self,
        repository: str,
        token: str,
        *,
        api_url: str = "https://api.github.com",
        opener: Callable = request.urlopen,
    ):
        if not REPOSITORY_RE.fullmatch(repository):
            raise UpdateError("repository must be owner/name")
        if not token:
            raise UpdateError("GITHUB_TOKEN is required")
        self.repository = repository
        self.token = token
        self.api_url = api_url.rstrip("/")
        self.opener = opener

    def _request(
        self,
        method: str,
        path: str,
        *,
        payload: dict | None = None,
        etag: str | None = None,
        allow_not_found: bool = False,
    ) -> tuple[dict, str] | None:
        data = (
            json.dumps(payload, separators=(",", ":")).encode("utf-8")
            if payload is not None else None
        )
        headers = {
            "Accept": "application/vnd.github+json",
            "Authorization": "Bearer {}".format(self.token),
            "User-Agent": "SkiaSharp-release-summary-updater",
            "X-GitHub-Api-Version": "2022-11-28",
        }
        if payload is not None:
            headers["Content-Type"] = "application/json"
        if etag:
            headers["If-Match"] = etag
        req = request.Request(
            self.api_url + path,
            data=data,
            headers=headers,
            method=method,
        )
        try:
            with self.opener(req, timeout=30) as response:
                response_body = response.read().decode("utf-8")
                result = _parse_object(response_body, "GitHub API response")
                return result, response.headers.get("ETag", "")
        except error.HTTPError as exc:
            if allow_not_found and exc.code == 404:
                return None
            if exc.code in (409, 412):
                raise UpdateError(
                    "GitHub rejected a stale release body (HTTP {})".format(exc.code)
                ) from exc
            detail = exc.read().decode("utf-8", errors="replace")
            raise UpdateError(
                "GitHub API {} {} failed (HTTP {}): {}".format(
                    method, path, exc.code, detail[:500])
            ) from exc
        except error.URLError as exc:
            raise UpdateError("GitHub API request failed: {}".format(exc.reason)) from exc

    def get_release(self, tag: str) -> ReleaseSnapshot | None:
        response = self._request(
            "GET",
            "/repos/{}/releases/tags/{}".format(
                self.repository, parse.quote(tag, safe="")),
            allow_not_found=True,
        )
        if response is None:
            return None
        data, etag = response
        if not etag:
            raise UpdateError("{} release response has no ETag".format(tag))
        if data.get("tag_name") != tag:
            raise UpdateError(
                "GitHub returned tag {!r} while querying {!r}".format(
                    data.get("tag_name"), tag)
            )
        release_id = data.get("id")
        if not isinstance(release_id, int):
            raise UpdateError("{} release response has no numeric id".format(tag))
        body = data.get("body")
        if not isinstance(body, str):
            raise UpdateError("{} release body is not text".format(tag))
        return ReleaseSnapshot(
            release_id=release_id,
            tag=tag,
            body=body,
            etag=etag,
            url=data.get("html_url") or "",
        )

    def patch_release(
        self,
        release_id: int,
        body: str,
        *,
        expected_etag: str,
    ) -> None:
        self._request(
            "PATCH",
            "/repos/{}/releases/{}".format(self.repository, release_id),
            payload={"body": body},
            etag=expected_etag,
        )


def update_releases(
    candidates: list[Candidate],
    github,
    *,
    renderer: Callable[[dict, dict, str], str] | None = None,
) -> UpdateResult:
    """Preflight every target and race check all bodies before the first PATCH."""
    result = UpdateResult()
    plans = []
    errors = []
    for candidate in candidates:
        try:
            snapshot = github.get_release(candidate.tag)
            if snapshot is None:
                result.add(candidate.tag, "skipped", "GitHub Release does not exist")
                continue
            rendered = render_managed_summary(candidate, renderer)
            new_body = replace_managed_summary(snapshot.body, rendered)
            if new_body is None:
                result.add(
                    candidate.tag,
                    "skipped",
                    "legacy release body has no managed markers",
                )
                continue
            if new_body == snapshot.body:
                result.add(candidate.tag, "unchanged", "managed summary is current")
                continue
            plans.append(PlannedUpdate(candidate, snapshot, new_body))
        except UpdateError as exc:
            errors.append("{}: {}".format(candidate.tag, exc))

    if errors:
        raise UpdateError(
            "preflight failed before any release update: " + "; ".join(errors))

    # Refresh every body before any mutation. Body SHA catches content changes;
    # ETag also catches representation changes. PATCH then repeats the ETag guard.
    for plan in plans:
        current = github.get_release(plan.candidate.tag)
        if current is None:
            errors.append("{}: release disappeared after preflight".format(
                plan.candidate.tag))
            continue
        if (
            current.release_id != plan.snapshot.release_id
            or current.body_sha256 != plan.snapshot.body_sha256
            or current.etag != plan.snapshot.etag
        ):
            errors.append(
                "{}: expected current body sha256 {} and ETag {}, "
                "found sha256 {} and ETag {}".format(
                    plan.candidate.tag,
                    plan.snapshot.body_sha256,
                    plan.snapshot.etag,
                    current.body_sha256,
                    current.etag,
                )
            )
    if errors:
        raise UpdateError(
            "release body changed after preflight; no PATCH was sent: "
            + "; ".join(errors)
        )

    for plan in plans:
        github.patch_release(
            plan.snapshot.release_id,
            plan.new_body,
            expected_etag=plan.snapshot.etag,
        )
        result.add(
            plan.candidate.tag,
            "updated",
            "managed summary replaced (expected body sha256 {})".format(
                plan.snapshot.body_sha256),
        )
    return result


def _write_summary(result: UpdateResult, error_message: str | None = None) -> None:
    summary = result.markdown(error_message=error_message)
    sys.stdout.write(summary)
    path = os.environ.get("GITHUB_STEP_SUMMARY")
    if path:
        with open(path, "a", encoding="utf-8") as stream:
            stream.write(summary)


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--event",
        required=True,
        choices=("push", "release", "workflow_dispatch"),
    )
    parser.add_argument("--repository", default=REPOSITORY_DEFAULT)
    parser.add_argument("--before")
    parser.add_argument("--after")
    parser.add_argument("--tag")
    parser.add_argument("--root", type=Path, default=Path.cwd())
    return parser


def main(argv: list[str] | None = None) -> int:
    args = create_parser().parse_args(argv)
    result = UpdateResult()
    try:
        if (
            args.event == "release"
            and (not args.tag or not TAG_RE.fullmatch(args.tag))
        ):
            result.add(
                args.tag or "(missing tag)",
                "skipped",
                "release tag is outside the managed SkiaSharp version format",
            )
            _write_summary(result)
            return 0
        repository = RepositoryView(args.root.resolve())
        if args.event == "push":
            if not args.before or not args.after:
                raise UpdateError("push mode requires --before and --after")
            candidates = select_push_candidates(
                repository, args.before, args.after)
        else:
            if args.tag and not TAG_RE.fullmatch(args.tag):
                raise UpdateError("invalid exact release tag {!r}".format(args.tag))
            candidates = select_current_candidates(repository, tag=args.tag)
        github = GitHubClient(
            args.repository,
            os.environ.get("GITHUB_TOKEN", ""),
        )
        result = update_releases(candidates, github)
        _write_summary(result)
        return 0
    except (OSError, UpdateError) as exc:
        _write_summary(result, str(exc))
        print("ERROR: {}".format(exc), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
