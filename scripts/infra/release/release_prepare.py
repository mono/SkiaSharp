"""Prepare planning and apply: release version detection and paired branches.

Ported and refactored from ``.agents/skills/release-branch/scripts/
create-release-branches.py`` and ``detect-release-version.py`` on
``main`` (6386960c2a4fddf0e68a8815856cbb7470deefce). The dynamic
``executionCommand``/``nextCommand`` string handoffs from those scripts are
deliberately not reproduced: apply consumes a schema-validated plan file
directly instead of a shell command.
"""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

from release_common import PlanError, build_envelope, utcnow_iso
from release_git import GitRepository
from release_github import GitHubClient, GitHubError
import release_model as model

VARIABLES_PATH = "scripts/azure-templates-variables.yml"
VERSIONS_PATH = "scripts/VERSIONS.txt"
SKIA_SUBMODULE_PATH = "externals/skia"
VERSION_PATHS = (VARIABLES_PATH, VERSIONS_PATH)

_SKIASHARP_VERSION_RE = re.compile(r"^\s*SKIASHARP_VERSION:\s*['\"]?([^'\"\s]+)", re.MULTILINE)
_PREVIEW_LABEL_RE = re.compile(r"^\s*PREVIEW_LABEL:\s*['\"]?([^'\"\r\n]+)", re.MULTILINE)
_SKIA_NUGET_RE = re.compile(r"^SkiaSharp\s+nuget\s+(\S+)", re.MULTILINE)
_HARFBUZZ_NUGET_RE = re.compile(r"^HarfBuzzSharp\s+nuget\s+(\S+)", re.MULTILINE)

PREPARE_SCHEMA = "prepare-plan.schema.json"
PREPARE_SCHEMA_VERSION = 1


@dataclass(frozen=True)
class VersionState:
    skia: str
    harfbuzz: str
    label: str


def _parse_state(variables_text: str, versions_text: str) -> VersionState:
    version_match = _SKIASHARP_VERSION_RE.search(variables_text)
    label_match = _PREVIEW_LABEL_RE.search(variables_text)
    skia_match = _SKIA_NUGET_RE.search(versions_text)
    harfbuzz_match = _HARFBUZZ_NUGET_RE.search(versions_text)
    if not (version_match and label_match and skia_match and harfbuzz_match):
        raise PlanError(
            "could not parse SKIASHARP_VERSION/PREVIEW_LABEL/nuget versions"
        )
    return VersionState(
        skia=skia_match.group(1),
        harfbuzz=harfbuzz_match.group(1),
        label=label_match.group(1).strip(),
    )


def read_version_state(repo: GitRepository, ref: str) -> VersionState:
    variables = repo.read_ref_file(ref, VARIABLES_PATH)
    versions = repo.read_ref_file(ref, VERSIONS_PATH)
    return _parse_state(variables, versions)


def read_worktree_version_state(root: Path) -> VersionState:
    variables = (root / VARIABLES_PATH).read_text(encoding="utf-8")
    versions = (root / VERSIONS_PATH).read_text(encoding="utf-8")
    return _parse_state(variables, versions)


def update_version_files(
    root: Path,
    *,
    preview_label: str,
    skia_version: str | None = None,
    harfbuzz_version: str | None = None,
    dry_run: bool = False,
) -> list[str]:
    """Rewrite VARIABLES_PATH/VERSIONS_PATH in place; return changed paths.

    Faithful port of ``update_version_files`` from create-release-branches.py:
    only ``PREVIEW_LABEL`` changes for a prerelease bump; ``SKIASHARP_VERSION``
    and every ``*nuget*`` line change together for a version bump.
    """

    variables_path = root / VARIABLES_PATH
    versions_path = root / VERSIONS_PATH
    variables_text = variables_path.read_text(encoding="utf-8")
    versions_text = versions_path.read_text(encoding="utf-8")
    changed: set[str] = set()

    new_variables, count = _PREVIEW_LABEL_LINE_RE.subn(
        lambda m: f"{m.group(1)}'{preview_label}'{m.group(2)}", variables_text, count=1
    )
    if count != 1:
        raise PlanError(f"could not update PREVIEW_LABEL in {VARIABLES_PATH}")
    if new_variables != variables_text:
        changed.add(VARIABLES_PATH)
    variables_text = new_variables

    if skia_version is not None or harfbuzz_version is not None:
        if skia_version is None or harfbuzz_version is None:
            raise PlanError("skia_version and harfbuzz_version must be set together")
        parts = skia_version.split(".")
        skia_file = f"{skia_version}.0" if len(parts) == 3 else skia_version

        new_versions, count = _SKIA_FILE_RE.subn(
            lambda m: f"{m.group(1)}{skia_file}{m.group(2)}{m.group(3)}", versions_text, count=1
        )
        if count != 1:
            raise PlanError(f"could not update 'SkiaSharp file' in {VERSIONS_PATH}")
        versions_text = new_versions

        versions_text, _ = _SKIA_NUGET_LINE_RE.subn(
            lambda m: f"{m.group(1)}{skia_version}{m.group(2)}{m.group(3)}", versions_text
        )
        new_versions, count = _HARFBUZZ_FILE_RE.subn(
            lambda m: f"{m.group(1)}{harfbuzz_version}{m.group(2)}{m.group(3)}", versions_text, count=1
        )
        if count != 1:
            raise PlanError(f"could not update 'HarfBuzzSharp file' in {VERSIONS_PATH}")
        versions_text = new_versions
        versions_text, _ = _HARFBUZZ_NUGET_LINE_RE.subn(
            lambda m: f"{m.group(1)}{harfbuzz_version}{m.group(2)}{m.group(3)}", versions_text
        )

        new_variables, count = _SKIASHARP_VERSION_LINE_RE.subn(
            lambda m: f"{m.group(1)}{skia_version}{m.group(2)}{m.group(3)}", variables_text, count=1
        )
        if count != 1:
            raise PlanError(f"could not update SKIASHARP_VERSION in {VARIABLES_PATH}")
        variables_text = new_variables
        changed.add(VARIABLES_PATH)

    if versions_text != (root / VERSIONS_PATH).read_text(encoding="utf-8"):
        changed.add(VERSIONS_PATH)

    if not changed:
        raise PlanError("update_version_files made no changes")

    if not dry_run:
        variables_path.write_text(variables_text, encoding="utf-8")
        versions_path.write_text(versions_text, encoding="utf-8")

    return sorted(changed)


_PREVIEW_LABEL_LINE_RE = re.compile(r"^(\s*PREVIEW_LABEL:\s*)[^\r\n]*(\r?)$", re.MULTILINE)
_SKIASHARP_VERSION_LINE_RE = re.compile(
    r"^(\s*SKIASHARP_VERSION:\s*)\S+([^\S\r\n]*)(\r?)$", re.MULTILINE
)
_SKIA_FILE_RE = re.compile(r"^(SkiaSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$", re.MULTILINE)
_SKIA_NUGET_LINE_RE = re.compile(r"^(SkiaSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$", re.MULTILINE)
_HARFBUZZ_FILE_RE = re.compile(r"^(HarfBuzzSharp\s+file\s+)\S+([^\S\r\n]*)(\r?)$", re.MULTILINE)
_HARFBUZZ_NUGET_LINE_RE = re.compile(
    r"^(HarfBuzzSharp\S*\s+nuget\s+)\S+([^\S\r\n]*)(\r?)$", re.MULTILINE
)


def latest_prerelease_branch(branches: list[str], numeric_version: str) -> str | None:
    pattern = re.compile(rf"^release/{re.escape(numeric_version)}-(preview|rc)\.(\d+)$")
    candidates = []
    for branch in branches:
        match = pattern.fullmatch(f"release/{branch}" if not branch.startswith("release/") else branch)
        if match:
            channel_rank = 0 if match.group(1) == "preview" else 1
            candidates.append((channel_rank, int(match.group(2)), match.group(0)))
    if not candidates:
        return None
    candidates.sort()
    return candidates[-1][2]


@dataclass(frozen=True)
class BaseSelection:
    ref: str
    sha: str
    maintenance_branch_exists: bool
    maintenance_branch_action: str  # "none" | "create"
    maintenance_branch_base_sha: str | None


def select_base(
    repo: GitRepository,
    version: model.ReleaseVersion,
    release_branches: list[str],
    *,
    approved_base: str | None,
) -> BaseSelection:
    integration = version.integration_branch
    integration_ref = f"refs/remotes/origin/{integration}"
    integration_exists = repo.ref_exists(integration_ref)
    full_branches = [f"release/{b}" if not b.startswith("release/") else b for b in release_branches]

    if version.is_hotfix:
        if version.channel:
            # The first prerelease of a hotfix line starts from the stable
            # tag of the release it patches, e.g. 3.119.0.1-preview.1 bases
            # on v3.119.0, not on any 4-part tag.
            parent_numeric = ".".join(str(part) for part in version.parts[:3])
            tag_ref = f"refs/tags/v{parent_numeric}"
            if not repo.ref_exists(tag_ref):
                raise PlanError(f"hotfix base tag {tag_ref} does not exist")
            return BaseSelection(tag_ref, repo.resolve(tag_ref), integration_exists, "none", None)
        candidate = latest_prerelease_branch(full_branches, version.numeric)
        if candidate is None:
            raise PlanError(
                f"no prerelease branch found to base hotfix {version.raw} on"
            )
        ref = f"refs/remotes/origin/{candidate}"
        return BaseSelection(ref, repo.resolve(ref), integration_exists, "none", None)

    if integration_exists:
        return BaseSelection(
            integration_ref, repo.resolve(integration_ref), True, "none", None
        )

    # No maintenance branch yet.
    if version.channel == "preview" and version.iteration == 1:
        # First prerelease for a brand-new line: base on the audited main SHA
        # and create the maintenance branch from that same commit.
        main_ref = "refs/remotes/origin/main"
        sha = repo.resolve(main_ref)
        return BaseSelection(main_ref, sha, False, "create", sha)

    # Any later prerelease or a stable/rc cut with no maintenance branch:
    # only recover from a validated matching prerelease branch for the same
    # numeric version, or require an explicit approved base.
    candidate = latest_prerelease_branch(full_branches, version.numeric)
    if candidate is not None:
        ref = f"refs/remotes/origin/{candidate}"
        sha = repo.resolve(ref)
        return BaseSelection(ref, sha, False, "create", sha)
    if approved_base:
        if not repo.ref_exists(approved_base):
            raise PlanError(f"approved base {approved_base!r} does not exist")
        sha = repo.resolve(approved_base)
        return BaseSelection(approved_base, sha, False, "create", sha)
    raise PlanError(
        f"maintenance branch {integration} does not exist and no matching "
        f"prerelease branch for {version.numeric} was found; pass an "
        "explicitly approved base to recover"
    )


@dataclass(frozen=True)
class StableBumpPlan:
    integration_branch: str
    bump_branch: str
    skia_version: str
    harfbuzz_version: str
    status: str  # "done" | "pending" | "awaiting-user"
    pull_request_url: str | None
    title: str


def plan_stable_bump(
    repo: GitRepository,
    version: model.ReleaseVersion,
    released_harfbuzz: str,
    *,
    github: GitHubClient | None,
    state_ref: str | None = None,
) -> StableBumpPlan:
    integration = version.integration_branch
    integration_ref = f"refs/remotes/origin/{integration}"
    # When the maintenance branch does not exist remotely yet (a stable cut
    # recovering from missing maintenance state, see select_base), its
    # post-apply content is identical to the recovery base, so read state
    # from there instead of a ref that will only start existing after apply.
    state = read_version_state(repo, state_ref or integration_ref)
    next_skia, next_harfbuzz = model.calculate_next_versions(version.numeric, released_harfbuzz)
    bump_branch = f"bump-version-{next_skia}"
    title = f"Bump to the next version ({next_skia}) after release"

    pull_request_url = None
    if github is not None:
        found = github.find_open_pull_request(head=bump_branch, base=integration)
        pull_request_url = found.url if found else None

    if state.label != "preview.0":
        raise PlanError(
            f"integration branch {integration} PREVIEW_LABEL is {state.label!r}, "
            "expected 'preview.0'"
        )
    if tuple(int(p) for p in state.skia.split(".")) >= tuple(int(p) for p in next_skia.split(".")):
        status = "done"
    elif state.skia == version.numeric:
        status = "awaiting-user" if pull_request_url else "pending"
    else:
        raise PlanError(
            f"integration branch {integration} SkiaSharp version {state.skia} is "
            f"neither the released version {version.numeric} nor the next "
            f"version {next_skia}"
        )
    return StableBumpPlan(
        integration_branch=integration,
        bump_branch=bump_branch,
        skia_version=next_skia,
        harfbuzz_version=next_harfbuzz,
        status=status,
        pull_request_url=pull_request_url,
        title=title,
    )


def stable_pr_body(released: str, plan: StableBumpPlan) -> str:
    return (
        f"Advance `{plan.integration_branch}` after cutting {released}.\n\n"
        "## Description\n\n"
        f"Bumps the integration branch back to `preview.0` at "
        f"SkiaSharp {plan.skia_version} / HarfBuzzSharp {plan.harfbuzz_version} "
        "so the next preview starts from a clean baseline.\n\n"
        "Related issues: N/A\n\n"
        "Required skia PR: None.\n\n"
        "## Changes\n\n"
        "None - version metadata only (`scripts/azure-templates-variables.yml`, "
        "`scripts/VERSIONS.txt`).\n\n"
        "## Testing\n\n"
        "The release automation validated this exact version transform before "
        "opening this pull request.\n\n"
        "## Areas Affected\n\n"
        "- [x] Build/infra\n"
    )


def build_prepare_plan(
    repo: GitRepository,
    *,
    integration_target: str,
    requested_version: str | None,
    tooling_sha: str,
    github: GitHubClient | None = None,
    approved_base: str | None = None,
) -> dict:
    """Build the read-only prepare plan (equivalent to ``prepare plan``)."""

    normalized_target = model.normalize_integration_branch(integration_target)
    target_ref = f"refs/remotes/origin/{normalized_target}"
    if not repo.ref_exists(target_ref):
        raise PlanError(f"integration target {normalized_target} does not exist on origin")

    warnings: list[str] = []
    release_branch_names = repo.release_branches()

    if requested_version:
        version = model.parse_release_version(requested_version)
    else:
        state = read_version_state(repo, target_ref)
        if state.label != "preview.0":
            raise PlanError(
                f"{normalized_target} PREVIEW_LABEL is {state.label!r}; cannot "
                "calculate the next preview automatically"
            )
        version = _next_preview_version(state.skia, release_branch_names)

    base = select_base(repo, version, release_branch_names, approved_base=approved_base)

    release_branch = version.release_branch
    release_ref = f"refs/remotes/origin/{release_branch}"
    skia_sha = repo.read_gitlink(base.ref, SKIA_SUBMODULE_PATH)

    remote_release_sha = repo.remote_sha(release_branch)
    skiasharp_state = "matching" if remote_release_sha else "missing"
    if remote_release_sha and remote_release_sha != base.sha:
        # It may already carry the version-bump commit; only a hard mismatch
        # against something outside this base's history is a conflict.
        if not repo.is_ancestor(base.sha, remote_release_sha):
            raise GitHubError(
                f"existing branch {release_branch} ({remote_release_sha}) is not "
                f"a descendant of the planned base {base.sha}"
            )

    skia_remote_sha = None
    if github is not None:
        skia_remote_sha = github.ref_sha(
            repository="mono/skia", ref=f"refs/heads/{release_branch}"
        )
    skia_state = "matching" if skia_remote_sha == skia_sha else (
        "missing" if skia_remote_sha is None else "conflict"
    )
    if skia_state == "conflict":
        raise GitHubError(
            f"mono/skia branch {release_branch} already exists at "
            f"{skia_remote_sha}, expected {skia_sha}"
        )

    if base.maintenance_branch_action == "create" and not base.maintenance_branch_exists:
        warnings.append(
            f"maintenance branch {version.integration_branch} does not exist "
            f"and will be created from {base.maintenance_branch_base_sha}"
        )

    requires_bump = _requires_package_bump(repo, base.ref, version)

    label = version.label
    operations = [
        {
            "id": "create-maintenance-branch",
            "kind": "git-ref",
            "status": "done" if base.maintenance_branch_exists else (
                "pending" if base.maintenance_branch_action == "create" else "skipped"
            ),
            "detail": version.integration_branch,
        },
        {
            "id": "create-skia-ref",
            "kind": "github-ref",
            "status": "done" if skia_state == "matching" else "pending",
            "detail": f"mono/skia:{release_branch}@{skia_sha}",
        },
        {
            "id": "create-release-branch",
            "kind": "git-ref",
            "status": "done" if skiasharp_state == "matching" else "pending",
            "detail": f"mono/SkiaSharp:{release_branch}",
        },
    ]

    stable_bump = None
    if version.release_type == "stable":
        # A hotfix's stable cut (release_type == "hotfix stable") never
        # advances the main line's next version, so no bump PR is planned.
        integration_ref = f"refs/remotes/origin/{version.integration_branch}"
        state_ref = integration_ref if repo.ref_exists(integration_ref) else base.ref
        stable_bump = plan_stable_bump(
            repo,
            version,
            _harfbuzz_from_ref(repo, base.ref),
            github=github,
            state_ref=state_ref,
        )
        operations.append(
            {
                "id": "open-stable-bump-pr",
                "kind": "github-pull-request",
                "status": stable_bump.status,
                "detail": stable_bump.bump_branch,
            }
        )

    plan = {
        "schemaVersion": PREPARE_SCHEMA_VERSION,
        "operation": "prepare",
        "generatedAt": utcnow_iso(),
        "toolingSha": tooling_sha,
        "nextAction": _prepare_next_action(operations),
        "input": {
            "integrationTarget": normalized_target,
            "requestedVersion": requested_version,
        },
        "release": {
            "identity": version.raw,
            "version": version.raw,
            "numeric": version.numeric,
            "label": label,
            "releaseType": version.release_type,
            "branch": release_branch,
            "integrationBranch": version.integration_branch,
            "isHotfix": version.is_hotfix,
            "stable": version.stable,
        },
        "base": {"ref": base.ref, "sha": base.sha},
        "maintenanceBranch": {
            "name": version.integration_branch,
            "exists": base.maintenance_branch_exists,
            "action": base.maintenance_branch_action,
            "baseSha": base.maintenance_branch_base_sha,
        },
        "skia": {
            "sha": skia_sha,
            "releaseBranch": release_branch,
            "remoteState": skia_state,
        },
        "skiaSharpRemoteState": skiasharp_state,
        "versions": {
            "skiaSharp": version.numeric,
            "requiresPackageBump": requires_bump,
        },
        "operations": operations,
        "stableBump": (
            {
                "integrationBranch": stable_bump.integration_branch,
                "bumpBranch": stable_bump.bump_branch,
                "skiaSharpVersion": stable_bump.skia_version,
                "harfBuzzSharpVersion": stable_bump.harfbuzz_version,
                "status": stable_bump.status,
                "pullRequestUrl": stable_bump.pull_request_url,
                "title": stable_bump.title,
            }
            if stable_bump
            else None
        ),
        "warnings": warnings,
    }
    return plan


def _prepare_next_action(operations: list[dict]) -> str:
    """Derive the standardized top-level ``nextAction`` from operation statuses.

    Precedence: an immutable-state conflict always wins ("blocked"); then a
    still-open write ("apply"); then a bump PR merely awaiting a human merge
    ("await-merge"); otherwise everything already matches the plan ("done").
    """

    statuses = {op["status"] for op in operations}
    if "blocked" in statuses:
        return "blocked"
    if "pending" in statuses:
        return "apply"
    if "awaiting-user" in statuses:
        return "await-merge"
    return "done"


def _requires_package_bump(repo: GitRepository, base_ref: str, version: model.ReleaseVersion) -> bool:
    state = read_version_state(repo, base_ref)
    return state.skia != version.numeric


def _harfbuzz_from_ref(repo: GitRepository, ref: str) -> str:
    return read_version_state(repo, ref).harfbuzz


def _next_preview_version(current_skia: str, release_branch_names: list[str]) -> model.ReleaseVersion:
    # release_branch_names comes from GitRepository.release_branches(), whose
    # elements already carry the "release/" prefix (confirmed by
    # tests/test_release_git.py), so every comparison below is prefixed too.
    if f"release/{current_skia}" in set(release_branch_names):
        raise PlanError(f"stable branch release/{current_skia} already exists")
    if any(b.startswith(f"release/{current_skia}-rc.") for b in release_branch_names):
        raise PlanError(f"an RC branch for {current_skia} already exists")
    pattern = re.compile(rf"^release/{re.escape(current_skia)}-preview\.(\d+)$")
    iterations = [
        int(match.group(1))
        for branch in release_branch_names
        if (match := pattern.fullmatch(branch))
    ]
    next_iteration = max(iterations, default=0) + 1
    return model.parse_release_version(f"{current_skia}-preview.{next_iteration}")


def apply_prepare_plan(
    plan: dict,
    *,
    repo: GitRepository,
    skia_repo: GitRepository,
    github: GitHubClient,
) -> dict:
    """Apply an already schema-validated, digest-verified prepare plan.

    Revalidates every expected SHA against live state immediately before any
    write, then performs the writes in the order the plan describes: the
    mono/skia ref, then the SkiaSharp release branch/commit, then (only for
    a stable release) the bump PR. Existing matching state is treated as
    already done; nothing is force-updated.
    """

    release = plan["release"]
    base = plan["base"]
    skia = plan["skia"]
    release_branch = release["branch"]
    integration_branch = release["integrationBranch"]

    repo.fetch()
    live_base_sha = repo.resolve(base["ref"]) if repo.ref_exists(base["ref"]) else None
    if live_base_sha is not None and live_base_sha != base["sha"]:
        raise GitHubError(
            f"base ref {base['ref']} moved from {base['sha']} to {live_base_sha} "
            "since the plan was generated; regenerate the plan"
        )

    report: dict = {"operations": []}

    # 1. Maintenance branch (release/X.Y.x), local-only ref update; the
    #    branch is pushed alongside the release branch below.
    maintenance = plan["maintenanceBranch"]
    if maintenance["action"] == "create":
        maint_ref = f"refs/remotes/origin/{maintenance['name']}"
        existing = repo.resolve(maint_ref) if repo.ref_exists(maint_ref) else None
        if existing is not None and existing != maintenance["baseSha"]:
            raise GitHubError(
                f"maintenance branch {maintenance['name']} already exists at "
                f"{existing}, expected {maintenance['baseSha']}"
            )
        if existing is None:
            repo.git("update-ref", f"refs/heads/{maintenance['name']}", maintenance["baseSha"])
            repo.push_branch(maintenance["name"])
            report["operations"].append({"id": "create-maintenance-branch", "status": "done"})
        else:
            report["operations"].append({"id": "create-maintenance-branch", "status": "done"})

    # 2. mono/skia ref, created directly through the GitHub refs API.
    existing_skia_sha = github.ref_sha(repository="mono/skia", ref=f"refs/heads/{release_branch}")
    if existing_skia_sha is None:
        github.create_ref(
            repository="mono/skia", ref=f"refs/heads/{release_branch}", sha=skia["sha"]
        )
        report["operations"].append({"id": "create-skia-ref", "status": "done"})
    elif existing_skia_sha == skia["sha"]:
        report["operations"].append({"id": "create-skia-ref", "status": "done"})
    else:
        raise GitHubError(
            f"mono/skia branch {release_branch} already exists at "
            f"{existing_skia_sha}, expected {skia['sha']}"
        )

    # 3. SkiaSharp release commit/ref.
    remote_sha = repo.remote_sha(release_branch)
    if remote_sha is None:
        _prepare_and_push_release_commit(repo, plan)
        report["operations"].append({"id": "create-release-branch", "status": "done"})
    else:
        report["operations"].append({"id": "create-release-branch", "status": "done"})

    # 4. Stable bump PR.
    stable_bump = plan.get("stableBump")
    if stable_bump and stable_bump["status"] != "done":
        pr_url = stable_bump["pullRequestUrl"]
        if pr_url is None:
            pr_url = _create_stable_bump_pr(repo, github, release["version"], stable_bump)
        report["operations"].append(
            {"id": "open-stable-bump-pr", "status": "done", "pullRequestUrl": pr_url}
        )
        report["stableBumpPullRequestUrl"] = pr_url
    elif stable_bump:
        report["stableBumpPullRequestUrl"] = stable_bump["pullRequestUrl"]

    return build_envelope(plan, next_action="done", **report)


def _prepare_and_push_release_commit(repo: GitRepository, plan: dict) -> None:
    release = plan["release"]
    base = plan["base"]
    release_branch = release["branch"]
    repo.switch_create(release_branch, base["sha"])
    changed = update_version_files(
        repo.root,
        preview_label=release["label"],
        skia_version=(release["numeric"] if plan["versions"]["requiresPackageBump"] else None),
        harfbuzz_version=(
            _harfbuzz_from_ref(repo, base["ref"]) if plan["versions"]["requiresPackageBump"] else None
        ),
    )
    if changed:
        message = (
            f"Bump the version to {release['version']}\n\n"
            f"Release-Base: {base['sha']}\n"
            f"Release-Skia: {plan['skia']['sha']}\n"
        )
        repo.commit(message, paths=changed)
    repo.push_branch(release_branch)


def _create_stable_bump_pr(
    repo: GitRepository, github: GitHubClient, released_version: str, stable_bump: dict
) -> str:
    bump_branch = stable_bump["bumpBranch"]
    integration_branch = stable_bump["integrationBranch"]
    remote_sha = repo.remote_sha(bump_branch)
    if remote_sha is None:
        integration_ref = f"refs/remotes/origin/{integration_branch}"
        repo.switch_create(bump_branch, integration_ref)
        changed = update_version_files(
            repo.root,
            preview_label="preview.0",
            skia_version=stable_bump["skiaSharpVersion"],
            harfbuzz_version=stable_bump["harfBuzzSharpVersion"],
        )
        repo.commit(stable_bump["title"], paths=changed)
        repo.push_branch(bump_branch)
    plan_obj = StableBumpPlan(
        integration_branch=integration_branch,
        bump_branch=bump_branch,
        skia_version=stable_bump["skiaSharpVersion"],
        harfbuzz_version=stable_bump["harfBuzzSharpVersion"],
        status=stable_bump["status"],
        pull_request_url=None,
        title=stable_bump["title"],
    )
    created = github.create_pull_request(
        head=bump_branch,
        base=integration_branch,
        title=stable_bump["title"],
        body=stable_pr_body(released_version, plan_obj),
    )
    return created.url
