#!/usr/bin/env python3
"""Dry-run or execute the complete SkiaSharp release-branch workflow."""

from __future__ import annotations

import argparse
from dataclasses import dataclass
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile


# ---------------------------------------------------------------------------
# Constants and data
# ---------------------------------------------------------------------------

SKIA_PATH = "externals/skia"
VARIABLES_PATH = "scripts/azure-templates-variables.yml"
VERSIONS_PATH = "scripts/VERSIONS.txt"
VERSION_PATHS = (VERSIONS_PATH, VARIABLES_PATH)

RELEASE_RE = re.compile(
    r"^(?P<numeric>\d+\.\d+\.\d+(?:\.\d+)?)"
    r"(?:-(?P<channel>preview|rc)\.(?P<iteration>\d+))?$"
)
LABEL_RE = re.compile(r"^(?:stable|preview\.\d+|rc\.\d+)$")


class ReleaseError(RuntimeError):
    """A release precondition or command failed."""


@dataclass(frozen=True)
class ReleaseVersion:
    raw: str
    numeric: str
    parts: tuple[int, ...]
    channel: str | None
    iteration: int | None

    @property
    def is_hotfix(self) -> bool:
        return len(self.parts) == 4

    @property
    def label(self) -> str:
        if self.channel is None:
            return "stable"
        return f"{self.channel}.{self.iteration}"

    @property
    def release_type(self) -> str:
        prefix = "hotfix " if self.is_hotfix else ""
        return prefix + (self.channel or "stable")

    @property
    def line(self) -> str:
        return f"{self.parts[0]}.{self.parts[1]}"


@dataclass(frozen=True)
class VersionState:
    skia: str
    harfbuzz: str
    label: str


@dataclass(frozen=True)
class NextVersionPlan:
    integration_branch: str
    bump_branch: str
    skia_version: str
    harfbuzz_version: str
    status: str
    pull_request: str | None

    def as_output(self) -> dict:
        return {
            "integrationBranch": self.integration_branch,
            "bumpBranch": self.bump_branch,
            "skiaSharpVersion": self.skia_version,
            "harfBuzzSharpVersion": self.harfbuzz_version,
            "status": self.status,
            "pullRequest": self.pull_request,
        }


@dataclass(frozen=True)
class ReleasePlan:
    version: ReleaseVersion
    release_branch: str
    base_ref: str
    base_sha: str
    skia_version: str
    harfbuzz_version: str
    requires_package_bump: bool
    skia_sha: str
    release_sha: str | None
    release_prepared: bool
    worktree_prepared: bool
    local_release_exists: bool
    skiasharp_remote_state: str
    skia_remote_state: str
    post_stable: NextVersionPlan | None
    warnings: tuple[str, ...]

    def as_output(self) -> dict:
        operations = [
            {
                "id": "skiasharp-release-commit",
                "status": "done" if self.release_prepared else "pending",
                "detail": (
                    f"{self.release_branch} is {self.release_sha}"
                    if self.release_prepared
                    else (
                        (
                            "version files are prepared; commit them"
                            if self.worktree_prepared
                            else (
                                f"create {self.release_branch} from "
                                f"{self.base_sha}, update version files, "
                                "and commit"
                            )
                        )
                    )
                ),
            },
            {
                "id": "skia-release-branch",
                "status": (
                    "done"
                    if self.skia_remote_state == "matching"
                    else "pending"
                ),
                "detail": (
                    f"mono/skia {self.release_branch} -> {self.skia_sha}"
                ),
            },
            {
                "id": "skiasharp-release-push",
                "status": (
                    "done"
                    if self.skiasharp_remote_state == "matching"
                    else "pending"
                ),
                "detail": (
                    f"mono/SkiaSharp {self.release_branch}"
                    + (
                        f" -> {self.release_sha}"
                        if self.release_sha
                        else " (push starts CI)"
                    )
                ),
            },
        ]
        if self.post_stable:
            if self.post_stable.status == "done":
                detail = (
                    f"{self.post_stable.integration_branch} has advanced to "
                    f"SkiaSharp {self.post_stable.skia_version} or later"
                )
            elif self.post_stable.pull_request:
                detail = (
                    f"PR {self.post_stable.pull_request} awaits "
                    "maintainer merge"
                )
            else:
                detail = (
                    f"create {self.post_stable.bump_branch}, push it, "
                    f"and open a PR to "
                    f"{self.post_stable.integration_branch}"
                )
            operations.append(
                {
                    "id": "post-stable-version-pr",
                    "status": self.post_stable.status,
                    "detail": detail,
                }
            )
        return {
            "version": self.version.raw,
            "type": self.version.release_type,
            "releaseBranch": self.release_branch,
            "baseRef": self.base_ref,
            "baseSha": self.base_sha,
            "previewLabel": self.version.label,
            "skiaSharpVersion": self.skia_version,
            "harfBuzzSharpVersion": self.harfbuzz_version,
            "requiresPackageBump": self.requires_package_bump,
            "skiaSha": self.skia_sha,
            "remoteState": {
                "SkiaSharp": self.skiasharp_remote_state,
                "mono/skia": self.skia_remote_state,
            },
            "postStableBump": (
                self.post_stable.as_output() if self.post_stable else None
            ),
            "warnings": list(self.warnings),
            "operations": operations,
            "executionCommand": (
                "python3 .agents/skills/release-branch/scripts/"
                f"create-release-branches.py {self.version.raw} "
                f"--expect-base-sha {self.base_sha} "
                f"--expect-skia-sha {self.skia_sha}"
            ),
        }


# ---------------------------------------------------------------------------
# Git and process helpers
# ---------------------------------------------------------------------------

class Repository:
    def __init__(self, root: Path):
        self.root = root

    @classmethod
    def discover(cls) -> Repository:
        result = run(
            ["git", "rev-parse", "--show-toplevel"],
            cwd=Path.cwd(),
        )
        return cls(Path(result.stdout.strip()))

    def git(
        self,
        *args: str,
        repository: str | Path | None = None,
        check: bool = True,
    ) -> subprocess.CompletedProcess[str]:
        command = ["git"]
        if repository is not None:
            command.extend(["-C", str(repository)])
        command.extend(args)
        return run(command, cwd=self.root, check=check)

    def fetch(self) -> None:
        self.git("fetch", "origin", "--prune", "--tags")

    def ref_exists(
        self,
        ref: str,
        repository: str | Path | None = None,
    ) -> bool:
        return self.git(
            "show-ref",
            "--verify",
            "--quiet",
            ref,
            repository=repository,
            check=False,
        ).returncode == 0

    def read_ref_file(self, ref: str, path: str) -> str:
        return self.git("show", f"{ref}:{path}").stdout

    def remote_sha(
        self,
        remote: str,
        branch: str,
        repository: str | Path | None = None,
    ) -> str | None:
        output = self.git(
            "ls-remote",
            "--heads",
            remote,
            f"refs/heads/{branch}",
            repository=repository,
        ).stdout.strip()
        return output.split()[0] if output else None

    def require_clean(self) -> None:
        staged = self.git(
            "diff",
            "--cached",
            "--name-only",
        ).stdout.splitlines()
        if staged:
            raise ReleaseError(
                f"working tree has staged changes: {sorted(staged)}"
            )
        if self.git(
            "status",
            "--porcelain",
            "--ignore-submodules=all",
        ).stdout.strip():
            raise ReleaseError(
                "working tree is not clean; commit or stash existing changes first"
            )


def run(
    args: list[str],
    *,
    cwd: Path,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True)
    if check and result.returncode != 0:
        detail = result.stderr.strip() or result.stdout.strip() or "no output"
        raise ReleaseError(
            f"command failed ({result.returncode}): {' '.join(args)}\n{detail}"
        )
    return result


def require_github_cli(repo: Repository) -> str:
    gh = shutil.which("gh")
    if not gh:
        raise ReleaseError("GitHub CLI 'gh' was not found on PATH")
    run(
        [gh, "auth", "status", "--hostname", "github.com"],
        cwd=repo.root,
    )
    return gh


# ---------------------------------------------------------------------------
# Release planning
# ---------------------------------------------------------------------------

def parse_release_version(value: str) -> ReleaseVersion:
    match = RELEASE_RE.fullmatch(value)
    if not match:
        raise ReleaseError(
            "version must be X.Y.Z[-preview.N|-rc.N] or "
            "X.Y.Z.F[-preview.N|-rc.N]"
        )
    numeric = match.group("numeric")
    channel = match.group("channel")
    iteration = (
        int(match.group("iteration"))
        if match.group("iteration") is not None
        else None
    )
    if channel and iteration == 0:
        raise ReleaseError("preview and rc iterations must start at 1")
    return ReleaseVersion(
        raw=value,
        numeric=numeric,
        parts=tuple(int(part) for part in numeric.split(".")),
        channel=channel,
        iteration=iteration,
    )


def read_version_state(repo: Repository, ref: str) -> VersionState:
    variables = repo.read_ref_file(ref, VARIABLES_PATH)
    versions = repo.read_ref_file(ref, VERSIONS_PATH)
    skia = re.search(
        r"^\s*SKIASHARP_VERSION:\s*['\"]?([^'\"\s]+)",
        variables,
        re.MULTILINE,
    )
    label = re.search(
        r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)",
        variables,
        re.MULTILINE,
    )
    skia_nuget = re.search(
        r"^SkiaSharp\s+nuget\s+(\S+)",
        versions,
        re.MULTILINE,
    )
    harfbuzz = re.search(
        r"^HarfBuzzSharp\s+nuget\s+(\S+)",
        versions,
        re.MULTILINE,
    )
    if not skia or not label or not skia_nuget or not harfbuzz:
        raise ReleaseError(f"could not parse release versions from {ref}")
    skia_value = skia.group(1)
    if skia_nuget.group(1) != skia_value:
        raise ReleaseError(f"{ref} disagrees on the SkiaSharp version")
    return VersionState(
        skia=skia_value,
        harfbuzz=harfbuzz.group(1),
        label=label.group(1).strip(),
    )


def read_worktree_version_state(root: Path) -> VersionState:
    variables = read_text(root / VARIABLES_PATH)
    versions = read_text(root / VERSIONS_PATH)
    skia = re.search(
        r"^\s*SKIASHARP_VERSION:\s*['\"]?([^'\"\s]+)",
        variables,
        re.MULTILINE,
    )
    label = re.search(
        r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)",
        variables,
        re.MULTILINE,
    )
    harfbuzz = re.search(
        r"^HarfBuzzSharp\s+nuget\s+(\S+)",
        versions,
        re.MULTILINE,
    )
    if not skia or not label or not harfbuzz:
        raise ReleaseError("could not parse worktree release versions")
    return VersionState(
        skia=skia.group(1),
        harfbuzz=harfbuzz.group(1),
        label=label.group(1).strip(),
    )


def release_branches(repo: Repository) -> list[str]:
    output = repo.git(
        "for-each-ref",
        "--format=%(refname:strip=3)",
        "refs/remotes/origin/release/",
    ).stdout
    return [line for line in output.splitlines() if line]


def increment_harfbuzz(value: str) -> str:
    parts = value.split(".")
    if len(parts) == 3 and all(part.isdigit() for part in parts):
        return f"{value}.1"
    if len(parts) == 4 and all(part.isdigit() for part in parts):
        return ".".join([*parts[:3], str(int(parts[3]) + 1)])
    raise ReleaseError(f"cannot increment HarfBuzzSharp version '{value}'")


def latest_prerelease_branch(
    branches: list[str],
    numeric_version: str,
) -> str:
    pattern = re.compile(
        rf"^release/{re.escape(numeric_version)}-(preview|rc)\.(\d+)$"
    )
    channel_order = {"preview": 0, "rc": 1}
    candidates = [
        (
            channel_order[match.group(1)],
            int(match.group(2)),
            branch,
        )
        for branch in branches
        if (match := pattern.fullmatch(branch))
    ]
    if not candidates:
        raise ReleaseError(
            f"no prerelease branch exists for hotfix {numeric_version}"
        )
    return max(candidates)[2]


def select_base_ref(
    repo: Repository,
    version: ReleaseVersion,
    branches: list[str],
) -> str:
    if version.is_hotfix and version.channel:
        ref = (
            "refs/tags/v"
            + ".".join(str(part) for part in version.parts[:3])
        )
    elif version.is_hotfix:
        ref = (
            "refs/remotes/origin/"
            + latest_prerelease_branch(branches, version.numeric)
        )
    else:
        maintenance = f"refs/remotes/origin/release/{version.line}.x"
        if repo.ref_exists(maintenance):
            ref = maintenance
        elif version.channel:
            ref = "refs/remotes/origin/main"
        else:
            raise ReleaseError(
                f"stable {version.numeric} requires release/{version.line}.x"
            )
    if not repo.ref_exists(ref):
        raise ReleaseError(f"base ref {ref} does not exist")
    return ref


def calculate_next_versions(
    released_version: str,
    current_harfbuzz: str,
) -> tuple[str, str]:
    match = re.fullmatch(r"(\d+)\.(\d+)\.(\d+)", released_version)
    if not match:
        raise ReleaseError("released version must be a stable X.Y.Z")
    major, minor, patch = (int(value) for value in match.groups())
    return (
        f"{major}.{minor}.{patch + 1}",
        increment_harfbuzz(current_harfbuzz),
    )


def find_open_pull_request(
    repo: Repository,
    *,
    head: str,
    base: str,
) -> str | None:
    gh = require_github_cli(repo)
    url = run(
        [
            gh,
            "pr",
            "list",
            "--head",
            head,
            "--base",
            base,
            "--state",
            "open",
            "--json",
            "url",
            "--jq",
            ".[0].url // empty",
        ],
        cwd=repo.root,
    ).stdout.strip()
    return url or None


def plan_next_version(
    repo: Repository,
    version: ReleaseVersion,
    released_harfbuzz: str,
) -> NextVersionPlan:
    integration = f"release/{version.line}.x"
    ref = f"refs/remotes/origin/{integration}"
    state = read_version_state(repo, ref)
    next_skia, next_harfbuzz = calculate_next_versions(
        version.numeric,
        released_harfbuzz,
    )
    bump_branch = f"bump-version-{next_skia}"
    pull_request = find_open_pull_request(
        repo,
        head=bump_branch,
        base=integration,
    )
    remote_bump = repo.remote_sha("origin", bump_branch)
    candidate = NextVersionPlan(
        integration_branch=integration,
        bump_branch=bump_branch,
        skia_version=next_skia,
        harfbuzz_version=next_harfbuzz,
        status="pending",
        pull_request=pull_request,
    )

    if state.label != "preview.0":
        raise ReleaseError(
            f"{integration} is at {state.skia} / {state.label}, "
            "expected preview.0"
        )
    if version_key(state.skia) >= version_key(next_skia):
        status = "done"
    elif state.skia == version.numeric:
        if remote_bump:
            bump_ref = f"refs/remotes/origin/{bump_branch}"
            validate_bump_ref(repo, bump_ref, candidate)
        status = (
            "awaiting-user"
            if pull_request
            else "pending"
        )
    else:
        raise ReleaseError(
            f"{integration} is at unexpected version {state.skia}"
        )

    return NextVersionPlan(
        integration_branch=integration,
        bump_branch=bump_branch,
        skia_version=next_skia,
        harfbuzz_version=next_harfbuzz,
        status=status,
        pull_request=pull_request,
    )


def validate_version_transform(
    repo: Repository,
    ref: str,
    *,
    preview_label: str,
    skia_version: str | None,
    harfbuzz_version: str | None,
    expected_paths: set[str],
) -> None:
    with tempfile.TemporaryDirectory() as temporary:
        root = Path(temporary)
        for path in (VARIABLES_PATH, VERSIONS_PATH):
            target = root / path
            target.parent.mkdir(parents=True, exist_ok=True)
            write_text(target, repo.read_ref_file(ref, path))
        changed = update_version_files(
            root,
            preview_label=preview_label,
            skia_version=skia_version,
            harfbuzz_version=harfbuzz_version,
            dry_run=True,
        )
        if set(changed) != expected_paths:
            raise ReleaseError(
                f"dry-run expected {sorted(expected_paths)}, "
                f"found {sorted(changed)}"
            )


def validate_existing_release_ref(
    repo: Repository,
    ref: str,
    *,
    version: ReleaseVersion,
    harfbuzz_version: str,
    skia_sha: str,
) -> None:
    state = read_version_state(repo, ref)
    if (
        state.skia != version.numeric
        or state.harfbuzz != harfbuzz_version
        or state.label != version.label
    ):
        raise ReleaseError(
            f"{ref} has {state.skia} / {state.harfbuzz} / {state.label}; "
            f"expected {version.numeric} / {harfbuzz_version} / "
            f"{version.label}"
        )
    versions = repo.read_ref_file(ref, VERSIONS_PATH)
    skia_file = (
        f"{version.numeric}.0"
        if len(version.numeric.split(".")) == 3
        else version.numeric
    )
    file_versions = {
        "SkiaSharp": skia_file,
        "HarfBuzzSharp": harfbuzz_version,
    }
    for package, expected in file_versions.items():
        match = re.search(
            rf"^{package}\s+file\s+(\S+)\s*$",
            versions,
            re.MULTILINE,
        )
        if not match or match.group(1) != expected:
            actual = match.group(1) if match else "missing"
            raise ReleaseError(
                f"{ref} has {package} file version {actual}, "
                f"expected {expected}"
            )
    for prefix, expected in (
        ("SkiaSharp", version.numeric),
        ("HarfBuzzSharp", harfbuzz_version),
    ):
        package_versions = re.findall(
            rf"^{prefix}\S*\s+nuget\s+(\S+)\s*$",
            versions,
            re.MULTILINE,
        )
        if not package_versions or any(
            actual != expected for actual in package_versions
        ):
            raise ReleaseError(
                f"{ref} has inconsistent {prefix} NuGet versions"
            )
    parent = repo.git("rev-parse", f"{ref}^").stdout.strip()
    parent_state = read_version_state(repo, parent)
    expected_paths = {VARIABLES_PATH}
    if parent_state.skia != version.numeric:
        expected_paths.add(VERSIONS_PATH)
    changed_paths = set(
        repo.git(
            "diff",
            "--name-only",
            f"{parent}..{ref}",
        ).stdout.splitlines()
    )
    if changed_paths != expected_paths:
        raise ReleaseError(
            f"{ref} changes {sorted(changed_paths)}, "
            f"expected {sorted(expected_paths)}"
        )
    actual_skia = repo.git(
        "rev-parse",
        f"{ref}:{SKIA_PATH}",
    ).stdout.strip()
    if actual_skia != skia_sha:
        raise ReleaseError(
            f"{ref} pins Skia {actual_skia}, expected {skia_sha}"
        )


def validate_local_release_commit(
    repo: Repository,
    ref: str,
    *,
    version: ReleaseVersion,
    harfbuzz_version: str,
) -> tuple[str, str]:
    message = repo.git("show", "-s", "--format=%B", ref).stdout
    base_match = re.search(
        r"^Release-Base:\s*([0-9a-f]{40})\s*$",
        message,
        re.MULTILINE,
    )
    skia_match = re.search(
        r"^Release-Skia:\s*([0-9a-f]{40})\s*$",
        message,
        re.MULTILINE,
    )
    if not base_match or not skia_match:
        raise ReleaseError(
            f"local {ref} lacks release automation provenance; "
            "delete or rename it before continuing"
        )
    base_sha = base_match.group(1)
    skia_sha = skia_match.group(1)
    parent = repo.git("rev-parse", f"{ref}^").stdout.strip()
    if parent != base_sha:
        raise ReleaseError(
            f"local {ref} parent is {parent}, trailer says {base_sha}"
        )
    commit_count = int(
        repo.git(
            "rev-list",
            "--count",
            f"{base_sha}..{ref}",
        ).stdout.strip()
    )
    if commit_count != 1:
        raise ReleaseError(
            f"local {ref} contains {commit_count} commits after its base"
        )
    parent_state = read_version_state(repo, base_sha)
    expected_paths = {VARIABLES_PATH}
    if parent_state.skia != version.numeric:
        expected_paths.add(VERSIONS_PATH)
    changed_paths = set(
        repo.git(
            "diff",
            "--name-only",
            f"{base_sha}..{ref}",
        ).stdout.splitlines()
    )
    if changed_paths != expected_paths:
        raise ReleaseError(
            f"local {ref} changes {sorted(changed_paths)}, "
            f"expected {sorted(expected_paths)}"
        )
    validate_existing_release_ref(
        repo,
        ref,
        version=version,
        harfbuzz_version=harfbuzz_version,
        skia_sha=skia_sha,
    )
    return base_sha, skia_sha


def build_release_plan(
    repo: Repository,
    version_value: str,
) -> ReleasePlan:
    version = parse_release_version(version_value)
    branch = f"release/{version.raw}"
    branches = release_branches(repo)
    local_ref = f"refs/heads/{branch}"
    remote_ref = f"refs/remotes/origin/{branch}"
    local_exists = repo.ref_exists(local_ref)
    parent_remote = repo.remote_sha("origin", branch)
    local_sha = (
        repo.git("rev-parse", local_ref).stdout.strip()
        if local_exists
        else None
    )

    release_sha = None
    release_prepared = False
    warnings = []
    if parent_remote:
        if not repo.ref_exists(remote_ref):
            raise ReleaseError(f"could not resolve fetched {remote_ref}")
        release_ref = remote_ref
        release_sha = parent_remote
        release_prepared = True
        release_state = read_version_state(repo, release_ref)
        if (
            release_state.skia != version.numeric
            or release_state.label != version.label
        ):
            raise ReleaseError(
                f"{release_ref} has {release_state.skia} / "
                f"{release_state.label}, expected {version.numeric} / "
                f"{version.label}"
            )
        target_harfbuzz = release_state.harfbuzz
        base_ref = f"{release_ref}^"
        base_sha = repo.git("rev-parse", base_ref).stdout.strip()
        skia_sha = repo.git(
            "rev-parse",
            f"{release_ref}:{SKIA_PATH}",
        ).stdout.strip()
        validate_existing_release_ref(
            repo,
            release_ref,
            version=version,
            harfbuzz_version=target_harfbuzz,
            skia_sha=skia_sha,
        )
        if local_sha and local_sha != parent_remote:
            raise ReleaseError(
                f"local {branch} is {local_sha}, origin is {parent_remote}"
            )
        requires_bump = False
    elif local_exists:
        local_state = read_version_state(repo, local_ref)
        if (
            local_state.skia == version.numeric
            and local_state.label == version.label
        ):
            release_sha = local_sha
            release_prepared = True
            target_harfbuzz = local_state.harfbuzz
            base_ref = f"{local_ref}^"
            base_sha, skia_sha = validate_local_release_commit(
                repo,
                local_ref,
                version=version,
                harfbuzz_version=target_harfbuzz,
            )
            selected_base_ref = select_base_ref(repo, version, branches)
            selected_base_sha = repo.git(
                "rev-parse",
                f"{selected_base_ref}^{{commit}}",
            ).stdout.strip()
            if selected_base_sha != base_sha:
                warnings.append(
                    f"local {branch} was approved from {base_sha}; "
                    f"the integration tip is now {selected_base_sha}"
                )
            requires_bump = False
        else:
            base_ref = select_base_ref(repo, version, branches)
            base_sha = repo.git(
                "rev-parse",
                f"{base_ref}^{{commit}}",
            ).stdout.strip()
            if local_sha != base_sha:
                raise ReleaseError(
                    f"unprepared local {branch} is at {local_sha}, "
                    f"but the selected base is {base_sha}"
                )
            state = local_state
            requires_bump = state.skia != version.numeric
            target_harfbuzz = (
                increment_harfbuzz(state.harfbuzz)
                if requires_bump
                else state.harfbuzz
            )
            skia_sha = repo.git(
                "rev-parse",
                f"{base_ref}:{SKIA_PATH}",
            ).stdout.strip()
    else:
        base_ref = select_base_ref(repo, version, branches)
        base_sha = repo.git(
            "rev-parse",
            f"{base_ref}^{{commit}}",
        ).stdout.strip()
        state = read_version_state(repo, base_ref)
        if not version.is_hotfix and state.label != "preview.0":
            raise ReleaseError(
                f"{base_ref} has PREVIEW_LABEL '{state.label}'"
            )
        if version.is_hotfix and version.channel and state.label != "stable":
            raise ReleaseError(
                f"{base_ref} has PREVIEW_LABEL '{state.label}'"
            )
        if version.is_hotfix and not version.channel and not re.fullmatch(
            r"(?:preview|rc)\.\d+",
            state.label,
        ):
            raise ReleaseError(
                f"{base_ref} has PREVIEW_LABEL '{state.label}'"
            )
        requires_bump = state.skia != version.numeric
        if requires_bump:
            stable_base = ".".join(
                str(part) for part in version.parts[:3]
            )
            if (
                not version.is_hotfix
                or not version.channel
                or state.skia != stable_base
            ):
                raise ReleaseError(
                    f"{base_ref} contains SkiaSharp {state.skia}, "
                    f"expected {version.numeric}"
                )
            target_harfbuzz = increment_harfbuzz(state.harfbuzz)
        else:
            target_harfbuzz = state.harfbuzz
        skia_sha = repo.git(
            "rev-parse",
            f"{base_ref}:{SKIA_PATH}",
        ).stdout.strip()

    if not release_prepared:
        expected_paths = {VARIABLES_PATH}
        if requires_bump:
            expected_paths.add(VERSIONS_PATH)
        validate_version_transform(
            repo,
            base_ref,
            preview_label=version.label,
            skia_version=version.numeric if requires_bump else None,
            harfbuzz_version=(
                target_harfbuzz if requires_bump else None
            ),
            expected_paths=expected_paths,
        )

    skia_url = repo.git(
        "config",
        "-f",
        ".gitmodules",
        "--get",
        "submodule.externals/skia.url",
    ).stdout.strip()
    skia_remote = repo.remote_sha(skia_url, branch)
    if skia_remote and skia_remote != skia_sha:
        raise ReleaseError(
            f"mono/skia {branch} is {skia_remote}, expected {skia_sha}"
        )

    worktree_prepared = False
    current_branch = repo.git("branch", "--show-current").stdout.strip()
    worktree_status = repo.git(
        "status",
        "--porcelain",
        "--ignore-submodules=all",
    ).stdout.splitlines()
    if current_branch == branch and worktree_status:
        changed_paths = {
            line[3:] for line in worktree_status if len(line) > 3
        }
        if release_prepared:
            raise ReleaseError(
                f"{branch} is already prepared but has uncommitted changes: "
                f"{sorted(changed_paths)}"
            )
        expected_paths = {VARIABLES_PATH}
        if requires_bump:
            expected_paths.add(VERSIONS_PATH)
        if changed_paths != expected_paths:
            raise ReleaseError(
                f"{branch} has unexpected worktree changes: "
                f"{sorted(changed_paths)}"
            )
        worktree_state = read_worktree_version_state(repo.root)
        if (
            worktree_state.skia != version.numeric
            or worktree_state.harfbuzz != target_harfbuzz
            or worktree_state.label != version.label
        ):
            raise ReleaseError(
                f"{branch} has incomplete version-file changes"
            )
        worktree_prepared = True

    post_stable = None
    if version.release_type == "stable":
        post_stable = plan_next_version(
            repo,
            version,
            target_harfbuzz,
        )
        if post_stable.status == "pending" and not repo.remote_sha(
            "origin",
            post_stable.bump_branch,
        ):
            validate_version_transform(
                repo,
                f"refs/remotes/origin/{post_stable.integration_branch}",
                preview_label="preview.0",
                skia_version=post_stable.skia_version,
                harfbuzz_version=post_stable.harfbuzz_version,
                expected_paths={VARIABLES_PATH, VERSIONS_PATH},
            )

    return ReleasePlan(
        version=version,
        release_branch=branch,
        base_ref=base_ref,
        base_sha=base_sha,
        skia_version=version.numeric,
        harfbuzz_version=target_harfbuzz,
        requires_package_bump=requires_bump,
        skia_sha=skia_sha,
        release_sha=release_sha,
        release_prepared=release_prepared,
        worktree_prepared=worktree_prepared,
        local_release_exists=local_exists,
        skiasharp_remote_state="matching" if parent_remote else "missing",
        skia_remote_state="matching" if skia_remote else "missing",
        post_stable=post_stable,
        warnings=tuple(warnings),
    )


# ---------------------------------------------------------------------------
# Version-file updates
# ---------------------------------------------------------------------------

def read_text(path: Path) -> str:
    with path.open("r", encoding="utf-8", newline="") as stream:
        return stream.read()


def write_text(path: Path, text: str) -> None:
    with path.open("w", encoding="utf-8", newline="") as stream:
        stream.write(text)


def replace_checked(
    text: str,
    pattern: str,
    replacement,
    expected: int,
    description: str,
) -> str:
    updated, count = re.subn(
        pattern,
        replacement,
        text,
        flags=re.MULTILINE,
    )
    if count != expected:
        raise ReleaseError(
            f"expected {expected} {description} line(s), found {count}"
        )
    return updated


def version_key(value: str) -> tuple[int, ...]:
    parts = [int(part) for part in value.split(".")]
    return tuple(parts + [0] * (4 - len(parts)))


def update_version_files(
    root: Path,
    *,
    preview_label: str,
    skia_version: str | None = None,
    harfbuzz_version: str | None = None,
    dry_run: bool = False,
) -> list[str]:
    if not LABEL_RE.fullmatch(preview_label):
        raise ReleaseError(
            "preview label must be stable, preview.N, or rc.N"
        )
    if bool(skia_version) != bool(harfbuzz_version):
        raise ReleaseError(
            "SkiaSharp and HarfBuzzSharp versions must be provided together"
        )
    for name, value in (
        ("SkiaSharp", skia_version),
        ("HarfBuzzSharp", harfbuzz_version),
    ):
        if value and not re.fullmatch(r"\d+(?:\.\d+){2,3}", value):
            raise ReleaseError(f"{name} version must have 3 or 4 parts")

    variables_path = root / VARIABLES_PATH
    versions_path = root / VERSIONS_PATH
    if not variables_path.is_file() or not versions_path.is_file():
        raise ReleaseError("expected release version files were not found")

    variables_before = read_text(variables_path)
    versions_before = read_text(versions_path)
    variables_after = replace_checked(
        variables_before,
        r"^(\s*PREVIEW_LABEL:\s*)[^\r\n]*(\r?)$",
        lambda match: (
            f"{match.group(1)}'{preview_label}'{match.group(2)}"
        ),
        1,
        "PREVIEW_LABEL",
    )
    versions_after = versions_before

    if skia_version and harfbuzz_version:
        assembly = re.search(
            r"^SkiaSharp\s+assembly\s+(\d+)\.(\d+)\.",
            versions_before,
            re.MULTILINE,
        )
        current_skia = re.search(
            r"^SkiaSharp\s+nuget\s+(\d+(?:\.\d+){2,3})\s*$",
            versions_before,
            re.MULTILINE,
        )
        current_harfbuzz = re.search(
            r"^HarfBuzzSharp\s+nuget\s+(\d+(?:\.\d+){2,3})\s*$",
            versions_before,
            re.MULTILINE,
        )
        if not assembly or not current_skia or not current_harfbuzz:
            raise ReleaseError("could not parse package version guard lines")
        if ".".join(assembly.groups()) != ".".join(
            skia_version.split(".")[:2]
        ):
            raise ReleaseError(
                "major.minor changes require a Skia milestone update"
            )
        if version_key(skia_version) <= version_key(current_skia.group(1)):
            raise ReleaseError(
                f"SkiaSharp version must increase from {current_skia.group(1)}"
            )
        if version_key(harfbuzz_version) <= version_key(
            current_harfbuzz.group(1)
        ):
            raise ReleaseError(
                "HarfBuzzSharp version must increase from "
                f"{current_harfbuzz.group(1)}"
            )

        skia_file = (
            f"{skia_version}.0"
            if len(skia_version.split(".")) == 3
            else skia_version
        )
        skia_count = len(
            re.findall(
                r"^SkiaSharp\S*\s+nuget\s+",
                versions_before,
                re.MULTILINE,
            )
        )
        harfbuzz_count = len(
            re.findall(
                r"^HarfBuzzSharp\S*\s+nuget\s+",
                versions_before,
                re.MULTILINE,
            )
        )
        if not skia_count or not harfbuzz_count:
            raise ReleaseError("could not find package version lines")

        versions_after = replace_checked(
            versions_after,
            r"^(SkiaSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$",
            lambda match: (
                f"{match.group(1)}{skia_file}"
                f"{match.group(2)}{match.group(3)}"
            ),
            1,
            "SkiaSharp file",
        )
        versions_after = replace_checked(
            versions_after,
            r"^(SkiaSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$",
            lambda match: (
                f"{match.group(1)}{skia_version}"
                f"{match.group(2)}{match.group(3)}"
            ),
            skia_count,
            "SkiaSharp nuget",
        )
        versions_after = replace_checked(
            versions_after,
            r"^(HarfBuzzSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$",
            lambda match: (
                f"{match.group(1)}{harfbuzz_version}"
                f"{match.group(2)}{match.group(3)}"
            ),
            1,
            "HarfBuzzSharp file",
        )
        versions_after = replace_checked(
            versions_after,
            r"^(HarfBuzzSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$",
            lambda match: (
                f"{match.group(1)}{harfbuzz_version}"
                f"{match.group(2)}{match.group(3)}"
            ),
            harfbuzz_count,
            "HarfBuzzSharp nuget",
        )
        variables_after = replace_checked(
            variables_after,
            r"^(\s*SKIASHARP_VERSION:\s*)\S+([^\S\r\n]*)(\r?)$",
            lambda match: (
                f"{match.group(1)}{skia_version}"
                f"{match.group(2)}{match.group(3)}"
            ),
            1,
            "SKIASHARP_VERSION",
        )
        if re.findall(
            r"^SkiaSharp\s+assembly\s+.*$",
            versions_after,
            re.MULTILINE,
        ) != re.findall(
            r"^SkiaSharp\s+assembly\s+.*$",
            versions_before,
            re.MULTILINE,
        ):
            raise ReleaseError("SkiaSharp assembly versions changed")

    changed = []
    if variables_after != variables_before:
        changed.append(str(VARIABLES_PATH))
    if versions_after != versions_before:
        changed.append(str(VERSIONS_PATH))
    if not changed:
        raise ReleaseError("requested update produced no changes")

    if not dry_run:
        if variables_after != variables_before:
            write_text(variables_path, variables_after)
        if versions_after != versions_before:
            write_text(versions_path, versions_after)
        if read_text(variables_path) != variables_after:
            raise ReleaseError(f"failed to verify {VARIABLES_PATH}")
        if read_text(versions_path) != versions_after:
            raise ReleaseError(f"failed to verify {VERSIONS_PATH}")
    return changed


# ---------------------------------------------------------------------------
# Paired release-branch preparation and validation
# ---------------------------------------------------------------------------

def initialize_skia(repo: Repository) -> None:
    repo.git(
        "submodule",
        "sync",
        "--recursive",
        "--",
        SKIA_PATH,
    )
    repo.git(
        "submodule",
        "update",
        "--init",
        "--recursive",
        "--checkout",
        "--",
        SKIA_PATH,
    )
    expected_url = repo.git(
        "config",
        "-f",
        ".gitmodules",
        "--get",
        "submodule.externals/skia.url",
    ).stdout.strip()
    actual_url = repo.git(
        "remote",
        "get-url",
        "origin",
        repository=SKIA_PATH,
    ).stdout.strip()
    if actual_url != expected_url:
        raise ReleaseError(
            f"externals/skia origin is {actual_url}, expected {expected_url}"
        )


def prepare_skia_branch(repo: Repository, plan: ReleasePlan) -> None:
    initialize_skia(repo)
    if repo.git(
        "status",
        "--porcelain",
        repository=SKIA_PATH,
    ).stdout.strip():
        raise ReleaseError("externals/skia has uncommitted changes")
    local_ref = f"refs/heads/{plan.release_branch}"
    if repo.ref_exists(local_ref, repository=SKIA_PATH):
        local_sha = repo.git(
            "rev-parse",
            local_ref,
            repository=SKIA_PATH,
        ).stdout.strip()
        if local_sha != plan.skia_sha:
            raise ReleaseError(
                f"local mono/skia {plan.release_branch} is {local_sha}, "
                f"expected {plan.skia_sha}"
            )
        repo.git(
            "switch",
            plan.release_branch,
            repository=SKIA_PATH,
        )
    else:
        repo.git(
            "switch",
            "-c",
            plan.release_branch,
            plan.skia_sha,
            repository=SKIA_PATH,
        )


def prepare_release_branches(
    repo: Repository,
    plan: ReleasePlan,
) -> list[str]:
    if plan.release_prepared:
        if plan.local_release_exists:
            repo.git("switch", plan.release_branch)
        else:
            repo.git(
                "switch",
                "-c",
                plan.release_branch,
                f"refs/remotes/origin/{plan.release_branch}",
            )
        prepare_skia_branch(repo, plan)
        return []

    if plan.worktree_prepared:
        prepare_skia_branch(repo, plan)
        changed = [VARIABLES_PATH]
        if plan.requires_package_bump:
            changed.append(VERSIONS_PATH)
        return changed

    repo.require_clean()
    if plan.local_release_exists:
        repo.git("switch", plan.release_branch)
    else:
        repo.git(
            "switch",
            "-c",
            plan.release_branch,
            plan.base_sha,
        )
    prepare_skia_branch(repo, plan)
    if plan.release_prepared:
        return []
    changed = update_version_files(
        repo.root,
        preview_label=plan.version.label,
        skia_version=(
            plan.skia_version if plan.requires_package_bump else None
        ),
        harfbuzz_version=(
            plan.harfbuzz_version if plan.requires_package_bump else None
        ),
    )
    repo.git("diff", "--check")
    expected = {str(VARIABLES_PATH)}
    if plan.requires_package_bump:
        expected.add(str(VERSIONS_PATH))
    if set(changed) != expected:
        raise ReleaseError(
            f"unexpected changed files: {sorted(changed)}"
        )
    return changed


def validate_release_branches(
    repo: Repository,
    plan: ReleasePlan,
) -> dict:
    branch = repo.git("branch", "--show-current").stdout.strip()
    if branch != plan.release_branch:
        raise ReleaseError(
            f"SkiaSharp is on {branch or 'detached HEAD'}, "
            f"expected {plan.release_branch}"
        )
    repo.require_clean()
    expected_skia = repo.git(
        "rev-parse",
        f"HEAD:{SKIA_PATH}",
    ).stdout.strip()
    skia_branch = repo.git(
        "branch",
        "--show-current",
        repository=SKIA_PATH,
    ).stdout.strip()
    skia_sha = repo.git(
        "rev-parse",
        "HEAD",
        repository=SKIA_PATH,
    ).stdout.strip()
    if skia_branch != plan.release_branch:
        raise ReleaseError(
            f"mono/skia is on {skia_branch or 'detached HEAD'}, "
            f"expected {plan.release_branch}"
        )
    if skia_sha != expected_skia:
        raise ReleaseError(
            f"mono/skia is at {skia_sha}, expected gitlink {expected_skia}"
        )
    if repo.git(
        "status",
        "--porcelain",
        repository=SKIA_PATH,
    ).stdout.strip():
        raise ReleaseError("mono/skia working tree is not clean")

    parent_remote = repo.remote_sha("origin", plan.release_branch)
    skia_remote = repo.remote_sha(
        "origin",
        plan.release_branch,
        repository=SKIA_PATH,
    )
    parent_head = repo.git("rev-parse", "HEAD").stdout.strip()
    if parent_remote and parent_remote != parent_head:
        raise ReleaseError(
            f"origin/{plan.release_branch} is {parent_remote}, "
            f"local SkiaSharp is {parent_head}"
        )
    if skia_remote and skia_remote != skia_sha:
        raise ReleaseError(
            f"mono/skia origin/{plan.release_branch} is {skia_remote}, "
            f"local is {skia_sha}"
        )
    return {
        "SkiaSharp": parent_head,
        "mono/skia": skia_sha,
        "remoteState": {
            "SkiaSharp": "matching" if parent_remote else "missing",
            "mono/skia": "matching" if skia_remote else "missing",
        },
    }


def commit_release(repo: Repository, plan: ReleasePlan, changed: list[str]) -> None:
    if not changed:
        return
    repo.git("add", "--", *changed)
    staged = set(
        repo.git(
            "diff",
            "--cached",
            "--name-only",
        ).stdout.splitlines()
    )
    if staged != set(changed):
        raise ReleaseError(
            f"staged paths are {sorted(staged)}, expected {sorted(changed)}"
        )
    repo.git("diff", "--cached", "--check")
    repo.git(
        "commit",
        "--only",
        "-m",
        (
            f"Bump the version to {plan.version.raw}\n\n"
            f"Release-Base: {plan.base_sha}\n"
            f"Release-Skia: {plan.skia_sha}"
        ),
        "--",
        *changed,
    )


def push_release_branches(repo: Repository, plan: ReleasePlan) -> None:
    if plan.skia_remote_state != "matching":
        repo.git(
            "push",
            "-u",
            "origin",
            plan.release_branch,
            repository=SKIA_PATH,
        )
    if plan.skiasharp_remote_state != "matching":
        repo.git("push", "-u", "origin", plan.release_branch)


# ---------------------------------------------------------------------------
# Stable post-cut bump PR
# ---------------------------------------------------------------------------

def stable_pr_body(
    released: str,
    plan: NextVersionPlan,
) -> str:
    return f"""## Description

Advance `{plan.integration_branch}` immediately after cutting `{released}` so new changes target `{plan.skia_version}`.

**Related issues**

N/A.

**Required skia PR**

None.

**Areas affected**

- [ ] Managed API (`binding/`)
- [ ] Native / C API (`externals/skia/src/c`, `include/c`)
- [ ] Generated P/Invoke bindings
- [ ] Native dependency or Skia update (libpng, HarfBuzz, FreeType, zlib, milestone bump, ...)
- [ ] Views & integrations (MAUI, Uno, WPF, WinUI, Blazor, ...)
- [ ] Rendering output / visual behavior
- [ ] Performance
- [ ] Tests
- [x] Build, packaging, or CI
- [ ] Documentation or samples

## Changes

None - version metadata only; no public API or behavioral changes.

## Testing

The release automation validated all SkiaSharp package versions at `{plan.skia_version}`, all HarfBuzzSharp package versions at `{plan.harfbuzz_version}`, unchanged assembly versions, and a clean staged diff.

## Checklist

- [x] Tests added or updated (not applicable: version metadata only)
- [x] `Changes` above lists all public API and behavioral changes (None)
- [x] New/changed public API? Not applicable
- [x] Native change? Not applicable
"""


def validate_bump_ref(
    repo: Repository,
    ref: str,
    plan: NextVersionPlan,
) -> None:
    state = read_version_state(repo, ref)
    if (
        state.skia != plan.skia_version
        or state.harfbuzz != plan.harfbuzz_version
        or state.label != "preview.0"
    ):
        raise ReleaseError(f"{ref} has unexpected version state")
    integration_ref = f"refs/remotes/origin/{plan.integration_branch}"
    merge_base = repo.git(
        "merge-base",
        integration_ref,
        ref,
    ).stdout.strip()
    commit_count = int(
        repo.git(
            "rev-list",
            "--count",
            f"{merge_base}..{ref}",
        ).stdout.strip()
    )
    if commit_count != 1:
        raise ReleaseError(
            f"{ref} contains {commit_count} commits after its integration base"
        )
    changed = set(
        repo.git(
            "diff",
            "--name-only",
            f"{merge_base}..{ref}",
        ).stdout.splitlines()
    )
    if changed != set(VERSION_PATHS):
        raise ReleaseError(
            f"{ref} changes {sorted(changed)}, "
            f"expected {sorted(VERSION_PATHS)}"
        )


def create_stable_bump_pr(
    repo: Repository,
    released: ReleaseVersion,
    plan: NextVersionPlan,
) -> str | None:
    if plan.status == "done":
        return plan.pull_request

    repo.require_clean()
    title = (
        f"Bump to the next version ({plan.skia_version}) after release"
    )
    gh = require_github_cli(repo)
    remote_bump = repo.remote_sha("origin", plan.bump_branch)
    if not remote_bump:
        local_ref = f"refs/heads/{plan.bump_branch}"
        if repo.ref_exists(local_ref):
            repo.git("switch", plan.bump_branch)
            validate_bump_ref(repo, local_ref, plan)
            prepared = True
        else:
            repo.git(
                "switch",
                "-c",
                plan.bump_branch,
                f"origin/{plan.integration_branch}",
            )
            prepared = False

        if not prepared:
            initialize_skia(repo)
            changed = update_version_files(
                repo.root,
                preview_label="preview.0",
                skia_version=plan.skia_version,
                harfbuzz_version=plan.harfbuzz_version,
            )
            if set(changed) != set(VERSION_PATHS):
                raise ReleaseError(
                    f"unexpected changed files: {sorted(changed)}"
                )
            repo.git("diff", "--check")
            repo.git("add", "--", *VERSION_PATHS)
            staged = set(
                repo.git(
                    "diff",
                    "--cached",
                    "--name-only",
                ).stdout.splitlines()
            )
            if staged != set(VERSION_PATHS):
                raise ReleaseError(
                    f"staged paths are {sorted(staged)}, "
                    f"expected {sorted(VERSION_PATHS)}"
                )
            repo.git("diff", "--cached", "--check")
            repo.git(
                "commit",
                "--only",
                "-m",
                title,
                "--",
                *VERSION_PATHS,
            )
        repo.git("push", "-u", "origin", plan.bump_branch)

    url = plan.pull_request or find_open_pull_request(
        repo,
        head=plan.bump_branch,
        base=plan.integration_branch,
    )
    if not url:
        url = run(
            [
                gh,
                "pr",
                "create",
                "--base",
                plan.integration_branch,
                "--head",
                plan.bump_branch,
                "--title",
                title,
                "--body",
                stable_pr_body(released.raw, plan),
            ],
            cwd=repo.root,
        ).stdout.strip()
    return url


# ---------------------------------------------------------------------------
# Workflow orchestration
# ---------------------------------------------------------------------------

def print_dry_run(repo: Repository, plan: ReleasePlan) -> None:
    print(json.dumps(plan.as_output(), indent=2))


def execute_release(repo: Repository, plan: ReleasePlan) -> dict:
    changed = prepare_release_branches(repo, plan)
    commit_release(repo, plan, changed)
    before = validate_release_branches(repo, plan)
    push_release_branches(repo, plan)
    after = validate_release_branches(repo, plan)

    pr_url = None
    if plan.post_stable:
        pr_url = create_stable_bump_pr(
            repo,
            plan.version,
            plan.post_stable,
        )
    return {
        "version": plan.version.raw,
        "releaseBranch": plan.release_branch,
        "beforePush": before,
        "afterPush": after,
        "postStablePullRequest": pr_url,
        "statusCommand": (
            "python3 .agents/skills/release-status/scripts/"
            f"pipeline-status.py {plan.release_branch}"
        ),
    }


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "version",
        help="Exact X.Y.Z[-preview.N|-rc.N] or X.Y.Z.F equivalent.",
    )
    parser.add_argument(
        "--dry-run",
        "--dryrun",
        action="store_true",
        help="Print the complete plan without changing either repository.",
    )
    parser.add_argument("--expect-base-sha")
    parser.add_argument("--expect-skia-sha")
    return parser


def main() -> int:
    args = create_parser().parse_args()
    try:
        repo = Repository.discover()
        if not args.dry_run:
            current_branch = repo.git(
                "branch",
                "--show-current",
            ).stdout.strip()
            if current_branch != f"release/{args.version}":
                repo.require_clean()
        repo.fetch()
        plan = build_release_plan(repo, args.version)
        if args.dry_run:
            print_dry_run(repo, plan)
        else:
            if not args.expect_base_sha or not args.expect_skia_sha:
                raise ReleaseError(
                    "execution requires --expect-base-sha and "
                    "--expect-skia-sha from the approved dry-run"
                )
            if args.expect_base_sha != plan.base_sha:
                raise ReleaseError(
                    f"base moved from {args.expect_base_sha} "
                    f"to {plan.base_sha}; run dry-run again"
                )
            if args.expect_skia_sha != plan.skia_sha:
                raise ReleaseError(
                    f"Skia moved from {args.expect_skia_sha} "
                    f"to {plan.skia_sha}; run dry-run again"
                )
            print(json.dumps(execute_release(repo, plan), indent=2))
    except ReleaseError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
