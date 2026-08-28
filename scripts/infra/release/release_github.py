"""GitHub interactions: refs, pull requests, tags, drafts, and releases.

All GitHub access goes through the :class:`GitHubClient` protocol so that
planning and validation logic can be unit tested with an in-memory fake
while :class:`GhCliGitHubClient` performs the real ``gh`` CLI calls in
production. Nothing in this module accepts a command string from a plan
file; every write takes typed, already-validated arguments.
"""

from __future__ import annotations

import hashlib
import json
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Protocol

from release_common import CommandRunner, DEFAULT_RUNNER, ReleaseToolError

GITHUB_REPOSITORY = "mono/SkiaSharp"

# Managed markers bound the exact bytes later reviewed release-note prose is
# allowed to replace, and the exact bytes GitHub's own generated notes occupy.
# Ported from the reviewed-summary prototype (5bb3346795e711cf6c6d2572445080b6c908e55a)
# so the two converge on the same body without either overwriting the other.
SUMMARY_START_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:START -->"
SUMMARY_END_MARKER = "<!-- SKIASHARP:RELEASE-SUMMARY:END -->"
GENERATED_START_MARKER = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->"
GENERATED_END_MARKER = "<!-- SKIASHARP:GITHUB-GENERATED-NOTES:END -->"

_CHANNEL_RANK = {"preview": 0, "rc": 1, None: 2}


class GitHubError(ReleaseToolError):
    """A GitHub query or write could not be completed as requested."""


@dataclass(frozen=True)
class TagVersion:
    """A parsed ``vX.Y.Z[-preview.N|-rc.N]`` tag used for previous-tag ordering."""

    name: str
    numeric: tuple[int, ...]
    channel: str | None
    prerelease: tuple[int, ...]

    @classmethod
    def parse(cls, value: str) -> "TagVersion | None":
        from release_model import RELEASE_TAG_RE

        match = RELEASE_TAG_RE.fullmatch(value)
        if not match:
            return None
        numeric = tuple(int(part) for part in match.group("numeric").split("."))
        channel = match.group("channel")
        iteration = (int(match.group("iteration")),) if channel else ()
        return cls(name=value, numeric=numeric, channel=channel, prerelease=iteration)

    @property
    def sort_key(self) -> tuple:
        return (self.numeric, _CHANNEL_RANK[self.channel], self.prerelease)


def previous_release_tag(current_tag: str, tags: list[str]) -> str | None:
    """Return the immediately preceding NuGet-compatible tag, or ``None``.

    Ordering spans channels: every preview/rc of a numeric version sorts
    before its stable release, and stable releases sort by numeric version.
    """

    current = TagVersion.parse(current_tag)
    if current is None:
        raise GitHubError(f"cannot order previous tag for invalid tag {current_tag!r}")
    candidates = [
        parsed
        for tag in tags
        if tag != current_tag and (parsed := TagVersion.parse(tag)) is not None
        and parsed.sort_key < current.sort_key
    ]
    if not candidates:
        return None
    return max(candidates, key=lambda item: item.sort_key).name


class Release(Protocol):
    """Structural typing helper describing the JSON shape returned for a release."""


@dataclass(frozen=True)
class ReleaseInfo:
    tag_name: str
    name: str
    is_draft: bool
    is_prerelease: bool
    target_commitish: str
    body: str
    url: str


@dataclass(frozen=True)
class PullRequestRef:
    number: int
    url: str


class GitHubClient(Protocol):
    """Everything the release CLI needs from GitHub, as typed operations.

    Every method is a request/response call; none of them accept or return
    shell command strings, and none of them silently retry across
    conflicting state.
    """

    def find_open_pull_request(self, *, head: str, base: str) -> PullRequestRef | None: ...

    def create_pull_request(
        self, *, head: str, base: str, title: str, body: str
    ) -> PullRequestRef: ...

    def create_ref(self, *, repository: str, ref: str, sha: str) -> None: ...

    def ref_sha(self, *, repository: str, ref: str) -> str | None: ...

    def get_release(self, tag: str) -> ReleaseInfo | None: ...

    def generate_notes(self, *, tag: str, target_commitish: str, previous_tag: str | None) -> dict: ...

    def create_draft(
        self,
        *,
        tag: str,
        title: str,
        target_commitish: str,
        body: str,
        prerelease: bool,
    ) -> None: ...

    def update_release_body(self, *, tag: str, body: str) -> None: ...

    def publish_release(self, *, tag: str, title: str, body: str) -> None: ...

    def dispatch_workflow(self, *, workflow: str, ref: str, inputs: dict[str, str]) -> None: ...


class GhCliGitHubClient:
    """The real client, backed by the ``gh`` CLI (never a raw shell string)."""

    def __init__(self, repository: str = GITHUB_REPOSITORY, *, runner: CommandRunner = DEFAULT_RUNNER):
        self.repository = repository
        self.runner = runner

    def _run_json(self, args: list[str]) -> object:
        result = self.runner.run(args, cwd=Path.cwd())
        text = result.stdout.strip()
        if not text:
            return None
        return json.loads(text)

    def find_open_pull_request(self, *, head: str, base: str) -> PullRequestRef | None:
        result = self.runner.run(
            [
                "gh", "pr", "list",
                "--repo", self.repository,
                "--head", head,
                "--base", base,
                "--state", "open",
                "--json", "number,url",
            ],
            cwd=Path.cwd(),
        )
        items = json.loads(result.stdout or "[]")
        if not items:
            return None
        return PullRequestRef(number=items[0]["number"], url=items[0]["url"])

    def create_pull_request(self, *, head: str, base: str, title: str, body: str) -> PullRequestRef:
        result = self.runner.run(
            [
                "gh", "pr", "create",
                "--repo", self.repository,
                "--head", head,
                "--base", base,
                "--title", title,
                "--body", body,
            ],
            cwd=Path.cwd(),
        )
        url = result.stdout.strip().splitlines()[-1]
        found = self.find_open_pull_request(head=head, base=base)
        if found is None:
            raise GitHubError(f"created pull request {url} but could not re-read it")
        return found

    def create_ref(self, *, repository: str, ref: str, sha: str) -> None:
        self.runner.run(
            [
                "gh", "api", f"repos/{repository}/git/refs",
                "-X", "POST",
                "-f", f"ref={ref}",
                "-f", f"sha={sha}",
            ],
            cwd=Path.cwd(),
        )

    def ref_sha(self, *, repository: str, ref: str) -> str | None:
        """Return the SHA a ref currently points to, or ``None`` if absent.

        Uses GitHub's singular "get a reference" endpoint
        (``git/ref/{ref}``, with the leading ``refs/`` stripped) rather
        than the plural "list matching references" endpoint
        (``git/refs/{ref}``). The plural endpoint performs a *string*
        prefix match on ref names -- not a git-tree-hierarchy match -- so
        a query for ``refs/heads/release/6.0.x`` can also match a longer,
        unrelated branch such as ``release/6.0.x-preview`` or
        ``release/6.0.x-rc.1`` (created for a later channel of the same
        release) and return a JSON *array* instead of a single object,
        crashing a caller that calls ``.get(...)`` on it as a dict. The
        singular endpoint always resolves exactly one ref (404 if
        absent), so this cannot happen there; the list handling below is
        kept only as a defensive fallback that exact-matches the
        requested ref by name, in case a response ever still comes back
        as a list.
        """

        stripped = ref[len("refs/"):] if ref.startswith("refs/") else ref
        result = self.runner.run(
            ["gh", "api", f"repos/{repository}/git/ref/{stripped}"],
            cwd=Path.cwd(),
            check=False,
        )
        if not result.ok:
            return None
        payload = json.loads(result.stdout)
        if isinstance(payload, list):
            exact = [item for item in payload if item.get("ref") == ref]
            if len(exact) != 1:
                return None
            payload = exact[0]
        return payload.get("object", {}).get("sha")

    def get_release(self, tag: str) -> ReleaseInfo | None:
        result = self.runner.run(
            [
                "gh", "release", "view", tag,
                "--repo", self.repository,
                "--json", "tagName,name,isDraft,isPrerelease,targetCommitish,body,url",
            ],
            cwd=Path.cwd(),
            check=False,
        )
        if not result.ok:
            if "release not found" in (result.stderr + result.stdout).lower():
                return None
            raise GitHubError(f"could not query release {tag}: {result.stderr}")
        payload = json.loads(result.stdout)
        return ReleaseInfo(
            tag_name=payload["tagName"],
            name=payload["name"],
            is_draft=payload["isDraft"],
            is_prerelease=payload["isPrerelease"],
            target_commitish=payload["targetCommitish"],
            body=payload.get("body") or "",
            url=payload["url"],
        )

    def generate_notes(self, *, tag: str, target_commitish: str, previous_tag: str | None) -> dict:
        args = [
            "gh", "api", f"repos/{self.repository}/releases/generate-notes",
            "-X", "POST",
            "-f", f"tag_name={tag}",
            "-f", f"target_commitish={target_commitish}",
        ]
        if previous_tag:
            args.extend(["-f", f"previous_tag_name={previous_tag}"])
        result = self.runner.run(args, cwd=Path.cwd())
        return json.loads(result.stdout)

    def create_draft(
        self,
        *,
        tag: str,
        title: str,
        target_commitish: str,
        body: str,
        prerelease: bool,
    ) -> None:
        args = [
            "gh", "release", "create", tag,
            "--repo", self.repository,
            "--title", title,
            "--notes", body,
            "--target", target_commitish,
            "--verify-tag",
            "--draft",
        ]
        if prerelease:
            args.extend(["--prerelease", "--latest=false"])
        self.runner.run(args, cwd=Path.cwd())

    def update_release_body(self, *, tag: str, body: str) -> None:
        self.runner.run(
            [
                "gh", "release", "edit", tag,
                "--repo", self.repository,
                "--notes", body,
            ],
            cwd=Path.cwd(),
        )

    def publish_release(self, *, tag: str, title: str, body: str) -> None:
        self.runner.run(
            [
                "gh", "release", "edit", tag,
                "--repo", self.repository,
                "--title", title,
                "--notes", body,
                "--verify-tag",
                "--draft=false",
            ],
            cwd=Path.cwd(),
        )

    def dispatch_workflow(self, *, workflow: str, ref: str, inputs: dict[str, str]) -> None:
        args = ["gh", "workflow", "run", workflow, "--repo", self.repository, "--ref", ref]
        for key, value in inputs.items():
            args.extend(["-f", f"{key}={value}"])
        self.runner.run(args, cwd=Path.cwd())


def body_sha256(body: str) -> str:
    return hashlib.sha256(body.encode("utf-8")).hexdigest()


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
        raise GitHubError("release body has incomplete or duplicate managed markers")
    positions = tuple(body.index(marker) for marker in markers)
    if not positions[0] < positions[1] < positions[2] < positions[3]:
        raise GitHubError("release body managed markers are out of order")
    return positions


def has_managed_markers(body: str) -> bool:
    return _marker_positions(body) is not None


def build_initial_body(generated_notes_body: str) -> str:
    """Compose the initial release body: managed markers around an empty
    summary region, followed by GitHub's own generated notes.

    Finish publishes with GitHub-generated notes immediately; reviewed
    summary prose converges later by replacing only the summary region.
    """

    return (
        f"{SUMMARY_START_MARKER}\n\n{SUMMARY_END_MARKER}\n\n"
        f"{GENERATED_START_MARKER}\n{generated_notes_body.strip()}\n{GENERATED_END_MARKER}\n"
    )


def replace_managed_summary(body: str, summary: str) -> str | None:
    """Replace only the managed summary bytes; ``None`` for an unmarked legacy body."""

    positions = _marker_positions(body)
    if positions is None:
        return None
    summary_start, summary_end, _, _ = positions
    owned_start = summary_start + len(SUMMARY_START_MARKER)
    return body[:owned_start] + "\n" + summary.strip() + "\n" + body[summary_end:]


@dataclass(frozen=True)
class TagDraftConflict:
    detail: str


def check_tag_conflict(existing_sha: str | None, expected_sha: str) -> None:
    """Raise :class:`GitHubError` (blocked) if an existing tag points elsewhere."""

    if existing_sha is not None and existing_sha != expected_sha:
        raise GitHubError(
            f"tag already exists and points to {existing_sha}, not the "
            f"expected package source commit {expected_sha}; tags are never "
            "moved automatically"
        )


_FULL_SHA_RE = re.compile(r"^[0-9a-f]{40}$")


def target_commitish_conflicts(existing: ReleaseInfo, expected_target: str) -> bool:
    """Return whether ``existing.target_commitish`` disagrees with ``expected_target``.

    ``target_commitish`` is only compared strictly when it is itself a full
    40-hex commit SHA -- what every release this tool creates or manages
    always sets. An already-*published* legacy release may instead carry a
    branch name there (e.g. ``"main"``): observed live on real, older
    mono/SkiaSharp releases (e.g. ``v4.151.1``), because GitHub's
    ``target_commitish`` is only a fallback target used if the named tag
    doesn't exist yet. Once the tag exists -- independently verified
    elsewhere via :func:`check_tag_conflict` against the exact package
    source commit -- the tag itself is authoritative and
    ``target_commitish`` becomes purely informational, no longer
    trustworthy as a second opinion. So a non-SHA ``target_commitish`` on
    an already-published release is never treated as a conflict; a genuine
    SHA-vs-SHA disagreement, or *any* disagreement while the release is
    still an unpublished draft (which this tool always creates with an
    exact SHA), still is.

    This is the single rule shared by :func:`check_release_conflict` and
    the ``finish plan-publication`` / ``finish publish`` binding checks in
    :mod:`release_finish`, so a published legacy release is never treated
    more strictly in one call site than the other.
    """

    if existing.target_commitish == expected_target:
        return False
    target_is_exact_sha = bool(_FULL_SHA_RE.fullmatch(existing.target_commitish))
    legacy_published_target = (not existing.is_draft) and not target_is_exact_sha
    return not legacy_published_target


def check_release_conflict(
    existing: ReleaseInfo | None,
    *,
    expected_title: str,
    expected_target: str,
    expected_prerelease: bool,
) -> None:
    """Raise :class:`GitHubError` if an existing release mismatches the plan.

    See :func:`target_commitish_conflicts` for the ``target_commitish``
    comparison rule applied below.
    """

    if existing is None:
        return
    mismatches = []
    if existing.name != expected_title:
        mismatches.append(f"title {existing.name!r} != {expected_title!r}")
    if target_commitish_conflicts(existing, expected_target):
        mismatches.append(
            f"target {existing.target_commitish!r} != {expected_target!r}"
        )
    if existing.is_prerelease != expected_prerelease:
        mismatches.append(
            f"prerelease {existing.is_prerelease} != {expected_prerelease}"
        )
    if mismatches:
        raise GitHubError(
            "existing GitHub release conflicts with the plan: " + "; ".join(mismatches)
        )
