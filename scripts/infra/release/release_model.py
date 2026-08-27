"""Release, tag, and public-version grammars shared by every release command.

Every regex here is deliberately narrow and is validated by
``tests/test_release_model.py``. Keeping the grammars centralised means the
prepare and finish paths can never silently drift apart.
"""

from __future__ import annotations

import re
from dataclasses import dataclass

from release_common import PlanError

# ``main`` or an existing maintenance line. Never an arbitrary ref or a PR ref.
INTEGRATION_BRANCH_RE = re.compile(r"^(?:main|release/\d+\.\d+\.x)$")

# An exact release version: X.Y.Z, the hotfix form X.Y.Z.F, and the optional
# preview/rc channel suffix. Never a range, wildcard, or partial version.
_NUMERIC = r"\d+\.\d+\.\d+(?:\.\d+)?"
RELEASE_VERSION_RE = re.compile(
    rf"^(?P<numeric>{_NUMERIC})(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)

# The exact release branch grammar (``release/`` + the version grammar above).
RELEASE_BRANCH_RE = re.compile(
    rf"^release/(?P<numeric>{_NUMERIC})"
    rf"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)

# The exact tag grammar used by GitHub releases: ``v`` + the version grammar.
RELEASE_TAG_RE = re.compile(
    rf"^v(?P<numeric>{_NUMERIC})(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)

# The CI build-revision grammar already accepted by set-build-variables.ps1:
# either a bare build number, or a 5- or 8-digit date-ish prefix + '.' + number.
BUILD_REVISION_RE = re.compile(r"^(?:(?:\d{5}|\d{8})\.)?\d+$")

_CHANNEL_RANK = {"preview": 0, "rc": 1, None: 2}


def _parts(numeric: str) -> tuple[int, ...]:
    return tuple(int(part) for part in numeric.split("."))


@dataclass(frozen=True)
class ReleaseVersion:
    """An exact release version such as ``3.119.0-preview.1`` or ``3.119.0.1``."""

    raw: str
    numeric: str
    parts: tuple[int, ...]
    channel: str | None
    iteration: int | None

    @property
    def is_hotfix(self) -> bool:
        return len(self.parts) == 4

    @property
    def stable(self) -> bool:
        return self.channel is None

    @property
    def label(self) -> str:
        return "stable" if self.channel is None else f"{self.channel}.{self.iteration}"

    @property
    def release_type(self) -> str:
        prefix = "hotfix " if self.is_hotfix else ""
        return prefix + (self.channel or "stable")

    @property
    def line(self) -> str:
        return f"{self.parts[0]}.{self.parts[1]}"

    @property
    def integration_branch(self) -> str:
        return f"release/{self.line}.x"

    @property
    def release_branch(self) -> str:
        return f"release/{self.raw}"

    @property
    def tag(self) -> str:
        return f"v{self.raw}"

    @property
    def title(self) -> str:
        if self.channel == "preview":
            return f"Version {self.numeric} (Preview {self.iteration})"
        if self.channel == "rc":
            return f"Version {self.numeric} (RC {self.iteration})"
        return f"Version {self.numeric}"

    @property
    def sort_key(self) -> tuple:
        return (self.parts, _CHANNEL_RANK[self.channel], self.iteration or 0)

    def validate_public_version(self, version: str) -> tuple[str, str | None]:
        """Validate ``version`` was composed from this release, base + label + build.

        Returns ``(base, build_revision)`` where ``base`` is always the bare
        numeric version (matching ``SKIASHARP_VERSION``/``VERSIONS.txt``,
        never including the ``-preview.N``/``-rc.N`` channel), and
        ``build_revision`` is ``None`` for a stable release (a bare public
        version) and the exact matched build-revision string otherwise.
        Composition, not equality, is used for preview/rc so that the CI
        build-revision suffix is accepted.
        """

        if self.stable:
            if version != self.numeric:
                raise PlanError(
                    f"public version {version!r} does not equal the stable "
                    f"base {self.numeric!r}"
                )
            return self.numeric, None
        prefix = f"{self.raw}."
        if not version.startswith(prefix):
            raise PlanError(
                f"public version {version!r} does not start with {prefix!r}"
            )
        build_revision = version[len(prefix):]
        if not BUILD_REVISION_RE.fullmatch(build_revision):
            raise PlanError(
                f"public version {version!r} has an invalid build revision "
                f"{build_revision!r}"
            )
        return self.numeric, build_revision


def parse_release_version(value: str) -> ReleaseVersion:
    match = RELEASE_VERSION_RE.fullmatch(value)
    if not match:
        raise PlanError(
            f"invalid release version {value!r}: expected X.Y.Z[.F]"
            "[-preview.N|-rc.N]"
        )
    channel = match.group("channel")
    iteration = int(match.group("iteration")) if channel else None
    if channel and iteration == 0:
        raise PlanError(f"invalid release version {value!r}: iteration must be >= 1")
    return ReleaseVersion(
        raw=value,
        numeric=match.group("numeric"),
        parts=_parts(match.group("numeric")),
        channel=channel,
        iteration=iteration,
    )


def parse_release_branch(value: str) -> ReleaseVersion:
    match = RELEASE_BRANCH_RE.fullmatch(value)
    if not match:
        raise PlanError(
            f"invalid release branch {value!r}: expected release/X.Y.Z[.F]"
            "[-preview.N|-rc.N]"
        )
    return parse_release_version(value[len("release/"):])


def parse_release_tag(value: str) -> ReleaseVersion:
    match = RELEASE_TAG_RE.fullmatch(value)
    if not match:
        raise PlanError(
            f"invalid release tag {value!r}: expected vX.Y.Z[.F]"
            "[-preview.N|-rc.N]"
        )
    return parse_release_version(value[1:])


def normalize_integration_branch(value: str) -> str:
    for prefix in ("refs/remotes/origin/", "refs/heads/", "origin/"):
        if value.startswith(prefix):
            value = value[len(prefix):]
            break
    if not INTEGRATION_BRANCH_RE.fullmatch(value):
        raise PlanError(
            f"invalid integration target {value!r}: expected 'main' or "
            "'release/X.Y.x'"
        )
    return value


def increment_harfbuzz(value: str) -> str:
    """Bump a HarfBuzzSharp version the same way create-release-branches.py did."""

    parts = value.split(".")
    if len(parts) == 3 and all(part.isdigit() for part in parts):
        return f"{value}.1"
    if len(parts) == 4 and all(part.isdigit() for part in parts):
        return ".".join([*parts[:3], str(int(parts[3]) + 1)])
    raise PlanError(f"cannot increment HarfBuzzSharp version {value!r}")


def calculate_next_versions(released_numeric: str, current_harfbuzz: str) -> tuple[str, str]:
    """Compute the next preview.0 SkiaSharp/HarfBuzzSharp versions after a stable cut."""

    parts = _parts(released_numeric)
    if len(parts) != 3:
        raise PlanError(
            f"cannot calculate next version from hotfix release {released_numeric!r}"
        )
    next_skia = f"{parts[0]}.{parts[1]}.{parts[2] + 1}"
    return next_skia, increment_harfbuzz(current_harfbuzz)


def compose_public_version(base: str, label: str, build_revision: str) -> str:
    if label == "stable":
        raise PlanError("a stable public version has no build revision")
    if not BUILD_REVISION_RE.fullmatch(build_revision):
        raise PlanError(f"invalid build revision {build_revision!r}")
    return f"{base}-{label}.{build_revision}"
