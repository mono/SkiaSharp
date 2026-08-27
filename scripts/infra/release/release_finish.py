"""Finish planning and execution: public receipt, tag/draft, publish, closeout.

Ported and refactored from ``.agents/skills/release-publish/scripts/
release_github.py``, ``create-release-draft.py``, and ``publish-release.py``
on ``main`` (6386960c2a4fddf0e68a8815856cbb7470deefce), replacing the private
AzDO/BAR handoff those scripts required with the public NuGet.org receipt in
``release_nuget.py``. Managed summary markers are ported from the
reviewed-summary prototype (5bb3346795e711cf6c6d2572445080b6c908e55a).
"""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Callable

from release_common import ConflictError, PlanError, build_envelope, utcnow_iso
from release_git import GitRepository
import release_github as gh
from release_github import GitHubClient
import release_milestones as milestones
import release_model as model
import release_nuget as nuget

FINISH_SCHEMA = "finish-plan.schema.json"
FINISH_SCHEMA_VERSION = 1


@dataclass
class GitVersionsFileReader:
    """Adapts :class:`GitRepository` to ``release_nuget.VersionsFileReader``."""

    repo: GitRepository

    def read_file(self, commit: str, path: str) -> str:
        return self.repo.read_ref_file(commit, path)

    def commit_exists(self, commit: str) -> bool:
        return self.repo.git("cat-file", "-e", f"{commit}^{{commit}}", check=False).ok

    def branch_contains(self, branch: str, commit: str) -> bool:
        ref = f"refs/remotes/origin/{branch}"
        if not self.repo.ref_exists(ref):
            return False
        return self.repo.contains_commit(ref, commit)


def build_finish_plan(
    *,
    requested_version: str,
    nuget_client: nuget.NuGetClient,
    repo: GitRepository,
    github: GitHubClient,
    manifest: dict,
    fingerprints: tuple[str, ...],
    signature_verifier: nuget.SignatureVerifier,
    download_dir: Path,
    tooling_sha: str,
    sleep: Callable[[float], None] | None = None,
) -> dict:
    """Build the read-only finish plan (equivalent to ``finish plan``)."""

    repo.fetch()
    reader = GitVersionsFileReader(repo)
    kwargs = dict(
        nuget=nuget_client,
        versions_reader=reader,
        requested_version=requested_version,
        manifest=manifest,
        download_dir=download_dir,
        signature_verifier=signature_verifier,
        fingerprints=fingerprints,
    )
    if sleep is not None:
        kwargs["sleep"] = sleep
    receipt = nuget.verify_public_receipt(**kwargs)

    release = model.parse_release_branch(receipt.source_branch)
    tag = release.tag

    existing_tag_sha = repo.remote_tags().get(tag)
    gh.check_tag_conflict(existing_tag_sha, receipt.source_commit)
    tag_status = "done" if existing_tag_sha == receipt.source_commit else "pending"

    all_tags = list(repo.remote_tags().keys())
    if existing_tag_sha is None:
        all_tags = [*all_tags, tag]
    previous_tag = gh.previous_release_tag(tag, all_tags)

    existing_release = github.get_release(tag)
    gh.check_release_conflict(
        existing_release,
        expected_title=release.title,
        expected_target=receipt.source_commit,
        expected_prerelease=not release.stable,
    )
    draft_status = "done" if existing_release is not None else "pending"

    warnings = list(receipt.warnings)
    if release.release_branch != receipt.source_branch:
        warnings.append(
            f"embedded commit's own branch name {receipt.source_branch!r} differs "
            f"from the parsed release branch {release.release_branch!r}"
        )

    draft_exists = existing_release is not None
    draft_published = bool(existing_release and not existing_release.is_draft)
    if draft_published:
        next_action = "closeout"
    elif draft_exists:
        next_action = "plan-publication"
    else:
        next_action = "create-draft"

    plan = {
        "schemaVersion": FINISH_SCHEMA_VERSION,
        "operation": "finish",
        "generatedAt": utcnow_iso(),
        "toolingSha": tooling_sha,
        "nextAction": next_action,
        "input": {"requestedVersion": requested_version},
        "receipt": {
            "skiaSharpVersion": receipt.skiasharp_version,
            "base": receipt.base,
            "label": receipt.label,
            "buildRevision": receipt.build_revision,
            "sourceCommit": receipt.source_commit,
            "sourceBranch": receipt.source_branch,
            "harfBuzzSharpVersion": receipt.harfbuzzsharp_version,
            "packages": [
                {
                    "id": package.id,
                    "version": package.version,
                    "sourceCommit": package.source_commit,
                    "sourceBranch": package.source_branch,
                }
                for package in receipt.packages
            ],
        },
        "release": {
            "identity": release.raw,
            "version": requested_version,
            "branch": receipt.source_branch,
            "raw": release.raw,
            "numeric": release.numeric,
            "label": release.label,
            "releaseType": release.release_type,
            "stable": release.stable,
            "title": release.title,
            "tag": tag,
        },
        "tag": {
            "name": tag,
            "targetCommit": receipt.source_commit,
            "existingSha": existing_tag_sha,
            "status": tag_status,
        },
        "previousTag": previous_tag,
        "draft": {
            "exists": draft_exists,
            "isPublished": draft_published,
            "status": draft_status,
        },
        "warnings": warnings,
    }
    return plan


def create_draft(plan: dict, *, repo: GitRepository, github: GitHubClient) -> dict:
    """Apply ``finish create-draft``: push the tag and create/reconcile the draft."""

    tag_info = plan["tag"]
    receipt = plan["receipt"]
    release = plan["release"]
    tag = tag_info["name"]
    source_commit = receipt["sourceCommit"]

    live_tag_sha = repo.remote_tags().get(tag)
    gh.check_tag_conflict(live_tag_sha, source_commit)
    if live_tag_sha is None:
        if not repo.git("cat-file", "-e", f"{source_commit}^{{commit}}", check=False).ok:
            repo.fetch()
        repo.push_tag(tag, source_commit)
        tag_result = "done"
    else:
        tag_result = "done"

    existing = github.get_release(tag)
    gh.check_release_conflict(
        existing,
        expected_title=release["title"],
        expected_target=source_commit,
        expected_prerelease=not release["stable"],
    )
    if existing is not None:
        is_published = not existing.is_draft
        return build_envelope(
            plan,
            next_action=("closeout" if is_published else "plan-publication"),
            tag=tag_result,
            draft="done",
            url=existing.url,
            alreadyExists=True,
            isPublished=is_published,
        )

    generated = github.generate_notes(
        tag=tag, target_commitish=source_commit, previous_tag=plan.get("previousTag")
    )
    body = gh.build_initial_body(generated.get("body", ""))
    github.create_draft(
        tag=tag,
        title=release["title"],
        target_commitish=source_commit,
        body=body,
        prerelease=not release["stable"],
    )
    return build_envelope(
        plan,
        next_action="plan-publication",
        tag=tag_result,
        draft="done",
        bodySha256=gh.body_sha256(body),
        alreadyExists=False,
        isPublished=False,
    )


def plan_publication(plan: dict, *, github: GitHubClient) -> dict:
    """Build ``finish plan-publication``: reads and validates the actual draft."""

    tag = plan["tag"]["name"]
    release = plan["release"]
    existing = github.get_release(tag)
    if existing is None:
        raise PlanError(f"no draft or release exists for {tag}; run create-draft first")
    if existing.target_commitish != plan["receipt"]["sourceCommit"]:
        raise ConflictError(
            f"draft {tag} targets {existing.target_commitish}, expected "
            f"{plan['receipt']['sourceCommit']}"
        )
    if existing.is_prerelease != (not release["stable"]):
        raise ConflictError(f"draft {tag} prerelease flag does not match the release type")
    markers_present = gh.has_managed_markers(existing.body)
    is_published = not existing.is_draft
    ready_to_publish = existing.is_draft and markers_present
    if is_published:
        next_action = "closeout"
    elif ready_to_publish:
        next_action = "publish"
    else:
        next_action = "create-draft"
    return build_envelope(
        plan,
        next_action=next_action,
        tag=tag,
        draftUrl=existing.url,
        isDraft=existing.is_draft,
        isPublished=is_published,
        bodySha256=gh.body_sha256(existing.body),
        hasManagedMarkers=markers_present,
        readyToPublish=ready_to_publish,
    )


def publish(plan: dict, publication: dict, *, github: GitHubClient) -> dict:
    """Apply ``finish publish``: publish the existing draft unchanged."""

    tag = plan["tag"]["name"]
    release = plan["release"]
    existing = github.get_release(tag)
    if existing is None:
        raise PlanError(f"no draft exists for {tag}")
    if not existing.is_draft:
        if existing.target_commitish != plan["receipt"]["sourceCommit"]:
            raise ConflictError(
                f"release {tag} is already published but targets "
                f"{existing.target_commitish}, not the package source commit"
            )
        return build_envelope(
            plan, next_action="closeout", tag=tag, status="already-published", url=existing.url
        )
    if gh.body_sha256(existing.body) != publication["bodySha256"]:
        raise ConflictError(f"draft {tag} body changed since plan-publication was generated")
    github.publish_release(tag=tag, title=release["title"], body=existing.body)
    return build_envelope(plan, next_action="closeout", tag=tag, status="published", url=existing.url)


def _closeout_operation_summary(op: milestones.ClosureOperation) -> dict:
    return {
        "milestone": op.milestone_title,
        "tag": op.tag,
        "status": op.status,
        "openItemCount": len(op.open_items),
        "moveTo": op.move_to_title,
        "detail": op.detail,
    }


def _closeout_next_action(operations: list[milestones.ClosureOperation], *, applied: bool) -> str:
    statuses = {op.status for op in operations}
    if "blocked" in statuses:
        return "blocked"
    if applied:
        return "done"
    return "closeout" if "pending" in statuses else "done"


def plan_closeout(plan: dict, *, milestone_client: milestones.MilestoneClient, tags: list[str]) -> dict:
    """Build the read-only closeout plan for the release described by ``plan``.

    ``plan`` is the finish-plan.json produced earlier in the pipeline; it
    supplies the standardized envelope context (tooling SHA, release
    identity/version/branch, plan digest). The actual closure operations are
    always recomputed from live milestone/tag state, since closeout may run
    long after the plan was generated.
    """

    all_milestones = milestone_client.milestones()
    operations, warnings = milestones.plan_closeout(
        milestones=all_milestones,
        tags=tags,
        open_items_for=milestone_client.open_milestone_items,
    )
    return build_envelope(
        plan,
        next_action=_closeout_next_action(operations, applied=False),
        operations=[_closeout_operation_summary(op) for op in operations],
        warnings=warnings,
    )


def apply_closeout(plan: dict, *, milestone_client: milestones.MilestoneClient, tags: list[str]) -> dict:
    """Apply closeout: move open items off shipped milestones and close them."""

    all_milestones = milestone_client.milestones()
    operations, warnings = milestones.plan_closeout(
        milestones=all_milestones,
        tags=tags,
        open_items_for=milestone_client.open_milestone_items,
    )
    pending = [op for op in operations if op.status == "pending"]
    blocked = [op for op in operations if op.status == "blocked"]
    results = milestones.apply_closeout(pending, milestone_client)
    for op in blocked:
        results.append({"milestone": op.milestone_title, "status": "blocked", "detail": op.detail})
    return build_envelope(
        plan,
        next_action=_closeout_next_action(operations, applied=True),
        results=results,
        warnings=warnings,
    )
