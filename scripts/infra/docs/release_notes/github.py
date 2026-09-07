"""GitHub Release body access owned by the exact-summary package."""

from __future__ import annotations

from dataclasses import dataclass
import json
import os
import re
from urllib import error, parse, request

SUMMARY_START_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:START -->"
SUMMARY_END_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:END -->"
# Read-only compatibility markers. Older updater versions wrapped the original
# release body in this pair; current convergence validates and removes them.
GENERATED_START_MARKER = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->"
GENERATED_END_MARKER = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->"

_REPOSITORY_RE = re.compile(r"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$")


class GitHubError(RuntimeError):
    """A GitHub Release query or update failed."""


@dataclass(frozen=True)
class ReleaseInfo:
    tag_name: str
    name: str
    is_draft: bool
    is_prerelease: bool
    target_commitish: str
    body: str
    url: str


def _marker_state(body: str) -> str:
    summary_counts = [
        body.count(SUMMARY_START_MARKER),
        body.count(SUMMARY_END_MARKER),
    ]
    generated_counts = [
        body.count(GENERATED_START_MARKER),
        body.count(GENERATED_END_MARKER),
    ]
    if summary_counts == [0, 0] and generated_counts == [0, 0]:
        return "unmanaged"
    if summary_counts != [1, 1]:
        raise GitHubError("release body has incomplete or duplicate managed markers")
    summary_start = body.index(SUMMARY_START_MARKER)
    summary_end = body.index(SUMMARY_END_MARKER)
    if summary_start >= summary_end:
        raise GitHubError("release body managed markers are out of order")
    if generated_counts == [0, 0]:
        return "managed"
    if generated_counts != [1, 1]:
        raise GitHubError("release body has incomplete or duplicate managed markers")
    generated_start = body.index(GENERATED_START_MARKER)
    generated_end = body.index(GENERATED_END_MARKER)
    if not summary_start < summary_end < generated_start < generated_end:
        raise GitHubError("release body managed markers are out of order")
    return "legacy"


def has_managed_markers(body: str) -> bool:
    return _marker_state(body) != "unmanaged"


def build_managed_body(summary: str) -> str:
    return "{}\n{}\n{}\n".format(
        SUMMARY_START_MARKER,
        summary.strip(),
        SUMMARY_END_MARKER,
    )


def replace_managed_summary(body: str, summary: str) -> str:
    """Return the one canonical reviewed body.

    GitHub-generated notes are useful only until reviewed prose is available.
    Once a summary is reviewed, this package owns the complete body so legacy
    summaries, duplicate links, and incorrectly bounded generated changelogs
    cannot survive beside it. ``_marker_state`` still rejects malformed marker
    contracts before convergence.
    """

    _marker_state(body)
    return build_managed_body(summary)


class RestGitHubClient:
    """Minimal GitHub REST client for release-summary reads and body updates."""

    def __init__(
        self,
        repository: str,
        *,
        token: str | None = None,
        api_url: str | None = None,
    ):
        if (
            not _REPOSITORY_RE.fullmatch(repository)
            or any(part in (".", "..") for part in repository.split("/"))
        ):
            raise GitHubError("repository must be in owner/name form")
        self.repository = repository
        self.token = token or os.environ.get("GH_TOKEN") or os.environ.get("GITHUB_TOKEN")
        if not self.token:
            raise GitHubError("GH_TOKEN or GITHUB_TOKEN is required")
        self.api_url = (api_url or os.environ.get("GITHUB_API_URL") or "https://api.github.com").rstrip("/")
        self._release_ids: dict[str, int] = {}

    def get_release(self, tag: str) -> ReleaseInfo | None:
        encoded_tag = parse.quote(tag, safe="")
        payload = self._request(
            "GET",
            "/repos/{}/releases/tags/{}".format(self.repository, encoded_tag),
            allow_not_found=True,
        )
        if payload is None:
            return None
        if not isinstance(payload, dict):
            raise GitHubError("GitHub returned a non-object release response")
        try:
            release_id = payload["id"]
            result = ReleaseInfo(
                tag_name=payload["tag_name"],
                name=payload["name"] or "",
                is_draft=payload["draft"],
                is_prerelease=payload["prerelease"],
                target_commitish=payload["target_commitish"],
                body=payload["body"] or "",
                url=payload["html_url"],
            )
        except (KeyError, TypeError) as exc:
            raise GitHubError("GitHub returned an incomplete release response") from exc
        if not isinstance(release_id, int) or release_id <= 0:
            raise GitHubError("GitHub returned an invalid release id")
        self._release_ids[tag] = release_id
        return result

    def update_release_body(self, *, tag: str, body: str) -> None:
        release_id = self._release_ids.get(tag)
        if release_id is None:
            if self.get_release(tag) is None:
                raise GitHubError("GitHub Release {} does not exist".format(tag))
            release_id = self._release_ids[tag]
        self._request(
            "PATCH",
            "/repos/{}/releases/{}".format(self.repository, release_id),
            {"body": body},
        )

    def _request(
        self,
        method: str,
        path: str,
        body: dict | None = None,
        *,
        allow_not_found: bool = False,
    ) -> object | None:
        data = None if body is None else json.dumps(body).encode("utf-8")
        operation = request.Request(
            self.api_url + path,
            data=data,
            method=method,
            headers={
                "Accept": "application/vnd.github+json",
                "Authorization": "Bearer {}".format(self.token),
                "X-GitHub-Api-Version": "2022-11-28",
                "User-Agent": "SkiaSharp-release-summary",
            },
        )
        try:
            with request.urlopen(operation, timeout=30) as response:
                payload = response.read()
        except error.HTTPError as exc:
            if allow_not_found and exc.code == 404:
                return None
            raise GitHubError("GitHub API {} {} failed with HTTP {}".format(method, path, exc.code)) from exc
        except error.URLError as exc:
            raise GitHubError("GitHub API {} {} failed: {}".format(method, path, exc.reason)) from exc
        if not payload:
            return None
        try:
            return json.loads(payload)
        except ValueError as exc:
            raise GitHubError("GitHub returned invalid JSON for {} {}".format(method, path)) from exc
