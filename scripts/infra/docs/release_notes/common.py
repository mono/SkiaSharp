"""Shared exact-release tag grammar and parsing.

Every function here is a pure computation over already-fetched strings, so
the package stays independently unit testable without a real repository.
"""

from __future__ import annotations

import configparser
import os
import re
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Mapping
from urllib.parse import urlsplit

DEFAULT_ROOT = Path(__file__).resolve().parents[4]
PUBLIC_SITE_BASE_URL = "https://mono.github.io/SkiaSharp"
_SLUG_RE = re.compile(r"[^/]+/[^/]+")
_OWNER_RE = re.compile(
    r"[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?"
)
_REPOSITORY_RE = re.compile(r"[A-Za-z0-9._-]{1,100}")
_URL_PATTERNS = (
    re.compile(
        r"https://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
    re.compile(
        r"git://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
    re.compile(
        r"git@github\.com:(?P<slug>[^/]+/[^/]+?)(?:\.git)?",
        re.IGNORECASE,
    ),
    re.compile(
        r"ssh://git@github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
)


class RepositoryIdentityError(RuntimeError):
    """Release-note repository identity could not be resolved safely."""


def normalize_github_repository(value: str) -> str:
    """Return a validated owner/repository slug for a GitHub URL or slug."""

    if not isinstance(value, str):
        raise RepositoryIdentityError("Unsupported GitHub repository identity.")
    candidate = value
    if candidate != candidate.strip() or any(
        character.isspace()
        or ord(character) < 32
        or ord(character) == 127
        for character in candidate
    ):
        raise RepositoryIdentityError("Unsupported GitHub repository identity.")
    parsed = urlsplit(candidate)
    if parsed.scheme.lower() == "https" and parsed.hostname == "github.com":
        if parsed.query or parsed.fragment:
            raise RepositoryIdentityError(
                "Unsupported GitHub repository identity."
            )
        candidate = parsed.path.removeprefix("/")
    else:
        for pattern in _URL_PATTERNS:
            match = pattern.fullmatch(candidate)
            if match:
                candidate = match.group("slug")
                break
    candidate = candidate.removesuffix(".git")
    if not _SLUG_RE.fullmatch(candidate):
        raise RepositoryIdentityError("Unsupported GitHub repository identity.")
    owner, repository = candidate.split("/", 1)
    if (
        not _OWNER_RE.fullmatch(owner)
        or "--" in owner
        or not _REPOSITORY_RE.fullmatch(repository)
        or repository in {".", ".."}
    ):
        raise RepositoryIdentityError("Unsupported GitHub repository identity.")
    return candidate


def github_url(repository: str, *, git: bool = False) -> str:
    suffix = ".git" if git else ""
    return "https://github.com/{}{}".format(
        normalize_github_repository(repository),
        suffix,
    )


def normalize_public_site_base_url(value: str) -> str:
    """Validate and normalize the release-notes public-site base URL."""

    if (
        not isinstance(value, str)
        or value != value.strip()
        or any(
            character.isspace()
            or ord(character) < 32
            or ord(character) == 127
            for character in value
        )
    ):
        raise RepositoryIdentityError(
            "Public site base URL must be an absolute HTTPS URL."
        )
    parsed = urlsplit(value)
    if (
        parsed.scheme != "https"
        or not parsed.netloc
        or parsed.username
        or parsed.password
        or parsed.query
        or parsed.fragment
    ):
        raise RepositoryIdentityError(
            "Public site base URL must be an absolute HTTPS URL "
            "without credentials, query, or fragment."
        )
    return value.rstrip("/")


def _origin_remote(root: Path) -> str:
    result = subprocess.run(
        ["git", "-C", str(root), "remote", "get-url", "--all", "origin"],
        check=False,
        capture_output=True,
        text=True,
    )
    values = [
        line.strip()
        for line in result.stdout.splitlines()
        if line.strip()
    ]
    if result.returncode != 0 or len(values) != 1:
        raise RepositoryIdentityError(
            "Unable to resolve one unambiguous current repository from "
            "git remote 'origin'."
        )
    return values[0]


def resolve_current_repository(
    explicit: str | None = None,
    *,
    environ: Mapping[str, str] | None = None,
    root: Path | None = None,
    remote_url: str | None = None,
) -> str:
    """Resolve explicit CLI, GitHub Actions, then validated origin identity."""

    if explicit is not None:
        return normalize_github_repository(explicit)
    values = environ if environ is not None else os.environ
    runtime = values.get("GITHUB_REPOSITORY")
    if runtime:
        return normalize_github_repository(runtime)
    root = (root or DEFAULT_ROOT).resolve()
    value = remote_url if remote_url is not None else _origin_remote(root)
    return normalize_github_repository(value)


def read_submodule_repository(root: Path, path: str) -> str:
    gitmodules = root / ".gitmodules"
    parser = configparser.ConfigParser(interpolation=None)
    try:
        with gitmodules.open(encoding="utf-8") as stream:
            parser.read_file(stream)
    except (OSError, configparser.Error) as exc:
        raise RepositoryIdentityError(
            "Unable to read {}: {}".format(gitmodules, exc)
        ) from exc
    section = 'submodule "{}"'.format(path)
    if not parser.has_option(section, "url"):
        raise RepositoryIdentityError(
            "{} has no URL for submodule {!r}.".format(gitmodules, path)
        )
    return normalize_github_repository(parser.get(section, "url"))


def configure_repository(
    repository: str | None = None,
    *,
    root: Path | None = None,
    environ=None,
    remote_url: str | None = None,
) -> str:
    """Refresh the current release-note repository for a generator run."""

    global REPO
    REPO = resolve_current_repository(
        repository,
        environ=environ,
        root=root,
        remote_url=remote_url,
    )
    return REPO


REPO: str | None = None


def get_repository() -> str:
    """Return the configured repository, resolving the checkout lazily."""

    return REPO or configure_repository()


_COMPARE_URL_RE = re.compile(
    r"https://github\.com/(?P<owner>[^/\s]+)/(?P<repository>[^/\s]+)/compare/"
    r"(?P<previous>[^/\s]+)\.\.\.(?P<tag>[^/\s]+)"
)
_HISTORICAL_REPOSITORIES = ("mono/SkiaSharp",)


def is_skiasharp_compare_url(
    value: object,
    previous_tag: str | None = None,
    tag: str | None = None,
) -> bool:
    """Return whether value is a safe SkiaSharp GitHub compare URL."""

    if not isinstance(value, str):
        return False
    match = _COMPARE_URL_RE.fullmatch(value)
    if match is None:
        return False
    try:
        repository = normalize_github_repository(
            "{}/{}".format(
                match.group("owner"),
                match.group("repository"),
            )
        )
    except RepositoryIdentityError:
        return False
    allowed = {
        candidate.casefold()
        for candidate in (get_repository(),) + _HISTORICAL_REPOSITORIES
    }
    return (
        repository.casefold() in allowed
        and (previous_tag is None or match.group("previous") == previous_tag)
        and (tag is None or match.group("tag") == tag)
    )


# Bump together with ``scripts/infra/docs/release-notes-data.py``'s
# ``_DATA_JSON_FORMAT_VERSION`` -- a test in this package's ``tests/`` folder
# asserts the two stay equal. This is the smallest compatible format bump
# needed to add ``shipments`` (facts) to data.json; it intentionally does NOT
# revisit the rest of the v3 shape. A data.json whose ``format`` is below this
# value has no ``shipments`` and is safely skipped by the updater rather than
# rewritten.
DATA_FORMAT = 4

# An exact release tag: ``vMAJOR.MINOR.PATCH[.HOTFIX][-{preview,rc}.N[.BUILD]]``.
# Deliberately narrower than release-notes-data.py's lenient ``_parse_tag``
# (which also matches historical/decorative labels like ``-beta`` or
# ``-gpu1``): the exact-summary path must never associate a GitHub Release
# summary with a tag it cannot confidently classify as stable, hotfix,
# preview, or rc. Any tag this regex rejects is simply not a "shipment" --
# it is neither generated nor consumed by this package.
EXACT_RELEASE_TAG_RE = re.compile(
    r"^v(?P<numeric>\d+\.\d+\.\d+)(?:\.(?P<hotfix>\d+))?"
    r"(?:-(?P<channel>preview|rc)\.(?P<milestone>\d+)"
    r"(?:\.(?P<build>\d+(?:\.\d+)?))?)?$"
)

FRIENDLY_CHANNEL = {"preview": "Preview", "rc": "Release Candidate"}
_CHANNEL_RANK = {"preview": 0, "rc": 1, None: 2}


def core_tuple(core: str) -> tuple[int, int, int, int]:
    """(major, minor, patch, hotfix) ints from a dotted core like ``4.151.0``."""

    parts = (core.split(".") + ["0", "0", "0", "0"])[:4]
    return tuple(int(part) if part.isdigit() else 0 for part in parts)


@dataclass(frozen=True)
class ParsedTag:
    """One parsed ``vX.Y.Z[.H][-{preview,rc}.N[.B]]`` exact release tag."""

    tag: str
    core: str
    core_tuple: tuple[int, int, int, int]
    channel: str | None
    milestone: int | None
    build: tuple[int, ...] | None
    hotfix: int | None
    sort_key: tuple

    @property
    def public_version(self) -> str:
        return self.tag[1:]

    @property
    def channel_name(self) -> str:
        """``"stable"`` when this tag carries no preview/rc channel."""

        return self.channel or "stable"

    @property
    def label(self) -> str:
        if self.channel is None:
            return "Hotfix" if self.hotfix else "Stable"
        label = "{} {}".format(FRIENDLY_CHANNEL[self.channel], self.milestone)
        if self.build:
            label += " (Build {})".format(
                ".".join(str(part) for part in self.build)
            )
        return label


def parse_tag(tag: str) -> ParsedTag | None:
    """Parse an exact release tag, or ``None`` when it does not match.

    Deliberately returns ``None`` (rather than raising) for anything outside
    the narrow exact-release grammar -- callers treat that as "not a
    shipment", never as an error, so old/decorative tags are silently
    excluded instead of aborting a run.
    """

    match = EXACT_RELEASE_TAG_RE.fullmatch(tag)
    if not match:
        return None
    numeric = match.group("numeric")
    hotfix = match.group("hotfix")
    channel = match.group("channel")
    milestone = match.group("milestone")
    build = match.group("build")
    core = numeric + (".{}".format(hotfix) if hotfix else "")
    milestone_i = int(milestone) if milestone is not None else None
    build_i = (
        tuple(int(part) for part in build.split("."))
        if build is not None
        else None
    )
    hotfix_i = int(hotfix) if hotfix is not None else None
    sort_key = (
        core_tuple(core),
        _CHANNEL_RANK[channel],
        milestone_i or 0,
        build_i or (0,),
    )
    return ParsedTag(
        tag=tag,
        core=core,
        core_tuple=core_tuple(core),
        channel=channel,
        milestone=milestone_i,
        build=build_i,
        hotfix=hotfix_i,
        sort_key=sort_key,
    )
