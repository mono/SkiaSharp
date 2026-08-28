"""Shared grammar, tag parsing, and release_github import glue.

This module has no side effects other than the deliberate ``sys.path``
insertion needed to reach ``scripts/infra/release/release_github.py`` (see
:func:`import_release_github`). It never shells out to ``git`` itself — every
function here is a pure computation over already-fetched strings, so the
whole package stays independently unit testable without a real repository.
"""

from __future__ import annotations

import re
import sys
from dataclasses import dataclass
from pathlib import Path

# The public GitHub repository these exact shipments and their GitHub Release
# summaries belong to. Matches ``scripts/infra/release/release_github.py``'s
# ``GITHUB_REPOSITORY`` and ``scripts/infra/docs/release-notes-data.py``'s
# ``REPO`` -- kept as a separate literal here (rather than importing either)
# so this package has no required import-time dependency on either sibling.
REPO = "mono/SkiaSharp"

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
    r"(?:-(?P<channel>preview|rc)\.(?P<milestone>\d+)(?:\.(?P<build>\d+))?)?$"
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
    build: int | None
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
            label += " (Build {})".format(self.build)
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
    build_i = int(build) if build is not None else None
    hotfix_i = int(hotfix) if hotfix is not None else None
    sort_key = (
        core_tuple(core),
        _CHANNEL_RANK[channel],
        milestone_i or 0,
        build_i or 0,
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


_RELEASE_DIR = Path(__file__).resolve().parents[2] / "release"


def import_release_github():
    """Import ``scripts/infra/release/release_github.py`` by path.

    The exact-summary updater must use exactly the managed markers and body
    helpers ``release_github.py`` defines (``SUMMARY_START_MARKER`` etc.,
    ``has_managed_markers``, ``replace_managed_summary``,
    ``GhCliGitHubClient``) -- the same module Finish uses to compose a new
    release's initial body -- so the two paths can never diverge on marker
    bytes. This mirrors the sys.path convention ``scripts/infra/release``'s
    own tests use for importing sibling modules, and the ``importlib``
    convention ``release-notes-render.py`` uses for importing
    ``release-notes-data.py``: insert the directory once, then import
    normally so ``release_github``'s own bare ``import release_common`` and
    ``from release_model import RELEASE_TAG_RE`` resolve.
    """

    if str(_RELEASE_DIR) not in sys.path:
        sys.path.insert(0, str(_RELEASE_DIR))
    import release_github  # noqa: PLC0415 -- see docstring for why this is late.

    return release_github
