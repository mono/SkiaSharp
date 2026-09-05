"""Shared exact-release tag grammar and parsing.

Every function here is a pure computation over already-fetched strings, so
the package stays independently unit testable without a real repository.
"""

from __future__ import annotations

import re
import sys
from dataclasses import dataclass
from pathlib import Path

_INFRA_DIR = Path(__file__).resolve().parents[2]
if str(_INFRA_DIR) not in sys.path:
    sys.path.insert(0, str(_INFRA_DIR))
from repository_identity import (  # noqa: E402
    IdentityError,
    normalize_github_repository,
    resolve_identity,
)

def configure_identity(
    repository: str | None = None,
    *,
    root: Path | None = None,
    environ=None,
    config_path: Path | None = None,
    identity: dict | None = None,
) -> dict:
    """Refresh shared release-note identity for a generator or updater run."""

    global IDENTITY, PUBLIC_SITE_BASE_URL, REPO
    IDENTITY = identity or resolve_identity(
        root=root,
        repository=repository,
        environ=environ,
        config_path=config_path,
    )
    REPO = IDENTITY["repository"]
    PUBLIC_SITE_BASE_URL = IDENTITY["publicSiteBaseUrl"]
    return IDENTITY


configure_identity()
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
    except IdentityError:
        return False
    allowed = {
        candidate.casefold()
        for candidate in (REPO,) + _HISTORICAL_REPOSITORIES
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
