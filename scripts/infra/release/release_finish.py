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
from datetime import date
from pathlib import Path
from typing import Callable

from release_common import ConflictError, DIGEST_FIELD, NotReadyError, PlanError, build_envelope, utcnow_iso
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
    deadline_seconds: float | None = None,
    clock: Callable[[], float] | None = None,
    sleep: Callable[[float], None] | None = None,
) -> dict:
    """Build the read-only finish plan (equivalent to ``finish plan``).

    ``deadline_seconds``/``clock``/``sleep`` are passed straight through to
    :func:`release_nuget.verify_public_receipt` when given, so tests can
    inject a fake clock/sleep pair (and a short deadline) instead of the
    real 20-minute wall-clock budget, without a CLI caller needing to know
    about them at all -- ``None`` (the default for each) leaves that
    function's own defaults untouched.
    """

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
    if deadline_seconds is not None:
        kwargs["deadline_seconds"] = deadline_seconds
    if clock is not None:
        kwargs["clock"] = clock
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
    draft_has_markers = bool(existing_release and gh.has_managed_markers(existing_release.body))
    if draft_published:
        next_action = "closeout"
    elif draft_exists and draft_has_markers:
        # An unpublished draft already carrying the managed marker body is
        # ready for a fresh publication review.
        next_action = "plan-publication"
    else:
        # No draft yet, or an unpublished draft that predates managed
        # markers (created by hand, or by an older tool version): either
        # way, only ``create_draft`` knows how to create it or safely
        # migrate it in place, so route there rather than sending the
        # workflow straight to ``plan-publication``, which cannot create
        # or migrate anything and would otherwise be stuck reporting
        # ``next_action="create-draft"`` from *inside* its own response
        # without the CLI ever routing back to it.
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
            "hasManagedMarkers": draft_has_markers,
        },
        "warnings": warnings,
    }
    return plan


FINISH_PENDING_SCHEMA = "finish-pending.schema.json"
FINISH_PENDING_SCHEMA_VERSION = 1


def build_pending_report(
    *, requested_version: str, tooling_sha: str, error: NotReadyError
) -> dict:
    """Build the machine-readable pending report for a ``finish plan`` run
    that could not complete because one or more packages are not yet
    listed on NuGet.org.

    Unlike :func:`build_finish_plan`'s successful output, this carries no
    ``release``/``receipt``/``tag``/``draft`` shape at all: a
    :class:`~release_common.NotReadyError` can be raised before the source
    commit (and therefore the release identity) is even known, so there is
    nothing for those fields to describe yet. ``nextAction`` is the literal
    string ``"pending"`` -- distinct from every value the successful
    finish-plan schema allows -- so a thin workflow can branch on it
    without first checking which schema the file matches.
    """

    return {
        "schemaVersion": FINISH_PENDING_SCHEMA_VERSION,
        "operation": "finish-plan-pending",
        "generatedAt": utcnow_iso(),
        "toolingSha": tooling_sha,
        "nextAction": "pending",
        "requestedVersion": requested_version,
        "missingPackages": list(error.missing),
        "elapsedSeconds": error.elapsed_seconds,
        "deadlineSeconds": error.deadline_seconds,
        "message": str(error),
    }


def create_draft(plan: dict, *, repo: GitRepository, github: GitHubClient) -> dict:
    """Apply ``finish create-draft``: push the tag and create/reconcile the draft.

    An existing unpublished draft that already carries the managed marker
    body (created by a prior run of this same function) is treated as
    already done. An existing unpublished draft that predates managed
    markers -- created by hand, or by an older tool version -- is safely
    migrated in place: its existing (GitHub-generated or hand-written)
    body is preserved and wrapped with the managed markers instead of
    being discarded, then re-read to verify the migration actually took
    effect. Without this, ``plan-publication`` would keep reporting
    ``next_action="create-draft"`` for a marker-less draft and this
    function would keep reporting it "already done", oscillating forever
    without ever converging on ``publish``. A published release is never
    rewritten here, marked or not -- see ``plan_publication``/``publish``
    for the only path that touches a published release's body.
    """

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

    existing = github.get_release(tag)
    gh.check_release_conflict(
        existing,
        expected_title=release["title"],
        expected_target=source_commit,
        expected_prerelease=not release["stable"],
    )
    if existing is not None:
        is_published = not existing.is_draft
        if is_published or gh.has_managed_markers(existing.body):
            return build_envelope(
                plan,
                next_action=("closeout" if is_published else "plan-publication"),
                tag=tag,
                tagStatus="done",
                draftStatus="done",
                url=existing.url,
                alreadyExists=True,
                isPublished=is_published,
                migrated=False,
            )
        # An unpublished draft exists but predates managed markers: migrate
        # it in place rather than oscillating between create-draft and
        # plan-publication forever.
        migrated_body = gh.build_initial_body(existing.body)
        github.update_release_body(tag=tag, body=migrated_body)
        reloaded = github.get_release(tag)
        if reloaded is None or not gh.has_managed_markers(reloaded.body):
            raise ConflictError(
                f"migrating draft {tag} to the managed-marker body did not verify"
            )
        return build_envelope(
            plan,
            next_action="plan-publication",
            tag=tag,
            tagStatus="done",
            draftStatus="done",
            url=reloaded.url,
            bodySha256=gh.body_sha256(reloaded.body),
            alreadyExists=True,
            isPublished=False,
            migrated=True,
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
        tag=tag,
        tagStatus="done",
        draftStatus="done",
        bodySha256=gh.body_sha256(body),
        alreadyExists=False,
        isPublished=False,
        migrated=False,
    )


def plan_publication(plan: dict, *, github: GitHubClient) -> dict:
    """Build ``finish plan-publication``: reads and validates the actual draft.

    ``existing`` may already be published (e.g. re-running plan-publication
    after ``publish`` to reconcile ``next_action``), so the target check
    reuses :func:`release_github.target_commitish_conflicts` -- the same
    strict-for-drafts, tolerant-for-published-legacy-branch-targets rule
    :func:`release_github.check_release_conflict` applies in create-draft --
    rather than a stricter, open-coded equality check.
    """

    tag = plan["tag"]["name"]
    release = plan["release"]
    existing = github.get_release(tag)
    if existing is None:
        raise PlanError(f"no draft or release exists for {tag}; run create-draft first")
    if gh.target_commitish_conflicts(existing, plan["receipt"]["sourceCommit"]):
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
    """Apply ``finish publish``: publish the existing draft unchanged.

    ``publication`` must be the exact, persisted result of a prior
    ``finish plan-publication`` run for this same plan -- never recomputed
    in this process. This is what binds the human/environment approval
    gate (which reviews that persisted plan-publication report before the
    protected job running ``publish`` is allowed to proceed) to the
    specific draft body it approved: :func:`_validate_publication_binding`
    rejects a publication report generated from a different plan, for a
    different tag, or that was never actually ready to publish, and the
    body-hash comparison below still detects a draft edited after the
    approved report was generated. Recomputing plan-publication here
    instead of consuming a persisted report would let a single process
    approve its own fresh recomputation, defeating the approval gate
    entirely -- so this function only ever reads ``publication``, it never
    calls :func:`plan_publication` itself.
    """

    tag = plan["tag"]["name"]
    release = plan["release"]
    existing = github.get_release(tag)
    if existing is None:
        raise PlanError(f"no draft exists for {tag}")
    if not existing.is_draft:
        if gh.target_commitish_conflicts(existing, plan["receipt"]["sourceCommit"]):
            raise ConflictError(
                f"release {tag} is already published but targets "
                f"{existing.target_commitish}, not the package source commit"
            )
        return build_envelope(
            plan, next_action="closeout", tag=tag, status="already-published", url=existing.url
        )
    _validate_publication_binding(plan, publication, tag=tag)
    if gh.body_sha256(existing.body) != publication["bodySha256"]:
        raise ConflictError(f"draft {tag} body changed since plan-publication was generated")
    github.publish_release(tag=tag, title=release["title"], body=existing.body)
    return build_envelope(plan, next_action="closeout", tag=tag, status="published", url=existing.url)


def _validate_publication_binding(plan: dict, publication: dict, *, tag: str) -> None:
    """Ensure ``publication`` is genuinely the matching, approved
    plan-publication result for ``plan`` -- not a stale, mismatched, or
    hand-built stand-in that happens to satisfy the body-hash check below."""

    if publication.get(DIGEST_FIELD) != plan.get(DIGEST_FIELD):
        raise ConflictError(
            f"publication report for {tag} was generated from a different plan "
            "(planDigest mismatch); rerun 'finish plan-publication' against this "
            "exact plan and use its output"
        )
    if publication.get("tag") != tag:
        raise ConflictError(
            f"publication report is for tag {publication.get('tag')!r}, expected {tag!r}"
        )
    if not publication.get("hasManagedMarkers"):
        raise ConflictError(
            f"publication report for {tag} does not have the managed marker body; "
            "run 'finish create-draft' to migrate it, then 'finish plan-publication' again"
        )
    if not publication.get("readyToPublish"):
        raise ConflictError(
            f"publication report for {tag} is not ready to publish "
            f"(isPublished={publication.get('isPublished')}, isDraft={publication.get('isDraft')})"
        )
    if "bodySha256" not in publication:
        raise ConflictError(f"publication report for {tag} is missing bodySha256")


UPDATE_RELEASE_NOTES_WORKFLOW = "update-release-notes.lock.yml"
ISSUE_TEMPLATE_REFRESH_WORKFLOW = "auto-update-issue-template-versions.yml"


def _schedule_operation_summary(op: milestones.ScheduleOperation) -> dict:
    return {
        "title": op.title,
        "number": op.number,
        "status": op.status,
        "action": op.action,
        "dueOn": op.due_on,
        "description": op.description,
        "changes": list(op.changes),
    }


def _closeout_operation_summary(op: milestones.ClosureOperation) -> dict:
    return {
        "milestone": op.milestone_title,
        "tag": op.tag,
        "status": op.status,
        "openItemCount": len(op.open_items),
        "moveTo": op.move_to_title,
        "detail": op.detail,
    }


def _reconcile_operation_summary(op: milestones.ReconcileOperation) -> dict:
    return {
        "kind": op.kind,
        "number": op.number,
        "viaPullRequest": op.via_pull_request,
        "fromMilestone": op.from_milestone,
        "toMilestone": op.to_milestone,
        "status": op.status,
    }


def _closeout_next_action(operations: list[milestones.ClosureOperation], *, applied: bool) -> str:
    statuses = {op.status for op in operations}
    if "blocked" in statuses:
        return "blocked"
    if applied:
        return "done"
    return "closeout" if "pending" in statuses else "done"


def resolve_reconciliation_range(
    repo: GitRepository, *, previous_tag: str | None, source_commit: str
) -> str | None:
    """Return the exclusive lower-bound commit for ranging
    ``git log <lower>..<source_commit>`` when reconciling merged PRs/issues
    for one exact release, or ``None`` when there is no previous release
    (the very first tag ever, ranging from the beginning of history).

    Explicitly fails (:class:`~release_common.ConflictError`) rather than
    guessing when the boundary is ambiguous: the previous tag has no
    resolvable commit, or that commit is not an ancestor of the shipped
    commit (a diverged or rewritten history).
    """

    if previous_tag is None:
        return None
    previous_sha = repo.remote_tags().get(previous_tag)
    if previous_sha is None:
        raise ConflictError(
            f"cannot resolve previous tag {previous_tag!r} to a commit; the "
            "release boundary for PR/issue reconciliation is ambiguous"
        )
    if not repo.is_ancestor(previous_sha, source_commit):
        raise ConflictError(
            f"previous tag {previous_tag!r} ({previous_sha}) is not an ancestor "
            f"of the shipped commit {source_commit}; the release boundary for "
            "PR/issue reconciliation is ambiguous"
        )
    return previous_sha


def _reconcile_operations_for_release(
    plan: dict,
    *,
    repo: GitRepository,
    milestone_client: milestones.MilestoneClient,
    all_milestones: list[milestones.Milestone],
) -> tuple[list[milestones.ReconcileOperation], list[str]]:
    """Plan reassigning merged PRs (and the issues they close) for the exact
    shipped release to its own milestone, before that milestone is advanced
    or closed. A missing target milestone is reported as a warning (nothing
    to reconcile against) rather than a hard failure; an ambiguous commit
    boundary always fails explicitly via :func:`resolve_reconciliation_range`.
    """

    release = plan["release"]
    warnings: list[str] = []
    target = next((m for m in all_milestones if m.title == release["identity"]), None)
    if target is None:
        warnings.append(
            f"no milestone titled {release['identity']!r} exists; skipping "
            "PR/issue reconciliation for this release"
        )
        return [], warnings

    source_commit = plan["receipt"]["sourceCommit"]
    lower_bound = resolve_reconciliation_range(
        repo, previous_tag=plan.get("previousTag"), source_commit=source_commit
    )
    range_spec = f"{lower_bound}..{source_commit}" if lower_bound else source_commit
    subjects = repo.commit_subjects_first_parent(range_spec)
    pr_numbers = milestones.extract_merged_pull_requests(subjects)

    operations = milestones.plan_reconcile(
        pull_request_numbers=pr_numbers,
        target_milestone=target,
        get_pull_request_milestone=milestone_client.pull_request_milestone,
        get_closing_issues=milestone_client.closing_issues,
        get_issue_milestone=milestone_client.issue_milestone,
    )
    return operations, warnings


def _release_notes_dispatch_inputs(plan: dict) -> dict[str, str]:
    numeric = plan["receipt"]["base"]
    return {"source_branch": "main", "min_version": numeric, "max_version": numeric, "force": "false"}


def _require_release_is_shipped(plan: dict, *, repo: GitRepository, github: GitHubClient) -> None:
    """Hard gate before any closeout milestone read/write: reverify live
    state, never trust the plan document alone.

    ``finish closeout`` is a public CLI; a caller must not be able to
    trigger milestone reconciliation/advancement merely by supplying a
    schema-valid, digest-verified finish plan whose tag/publish state was
    true when the plan was generated (or was never true at all -- e.g. a
    plan generated before ``create-draft``/``publish`` ever ran) but is
    not true right now. This requires, freshly, that: the exact tag still
    exists on the remote and still resolves to the receipt's exact source
    commit (not moved, not missing); and a matching GitHub Release exists
    for that tag and is published, not still a draft. Anything else is a
    :class:`~release_common.ConflictError` raised before
    ``milestone_client``/``github`` is touched for anything else.
    """

    tag = plan["tag"]["name"]
    source_commit = plan["receipt"]["sourceCommit"]

    tag_sha = repo.remote_tags().get(tag)
    if tag_sha is None:
        raise ConflictError(
            f"cannot close out {tag}: it does not exist on the remote; "
            "finish create-draft/publish must run before closeout"
        )
    if tag_sha != source_commit:
        raise ConflictError(
            f"cannot close out {tag}: it points to {tag_sha}, expected the "
            f"package source commit {source_commit}"
        )

    release = github.get_release(tag)
    if release is None:
        raise ConflictError(
            f"cannot close out {tag}: no GitHub release exists for it yet; "
            "finish create-draft/publish must run before closeout"
        )
    if release.is_draft:
        raise ConflictError(
            f"cannot close out {tag}: its GitHub release is still an "
            "unpublished draft; finish publish must run before closeout"
        )


DEFAULT_SCHEDULE_MILESTONE_COUNT = 3
_VERSIONS_PATH_ON_MAIN = "scripts/VERSIONS.txt"


def _read_current_major_and_milestone(repo: GitRepository) -> tuple[int, int]:
    """Read the current SkiaSharp major version and Skia/Chromium milestone
    from ``scripts/VERSIONS.txt`` on ``main`` -- never the release branch
    being closed out, since schedule maintenance for the *upcoming*
    milestones is a main-line concern independent of any specific release.
    """

    text = repo.read_ref_file("refs/remotes/origin/main", _VERSIONS_PATH_ON_MAIN)
    return milestones.parse_current_major_and_milestone(text)


def _plan_schedule_operations_for_release(
    repo: GitRepository,
    *,
    schedule_client: milestones.ScheduleClient,
    schedule_count: int,
    existing: dict[str, milestones.Milestone],
) -> tuple[list[milestones.ScheduleOperation], list[str]]:
    """Compute the create/update schedule operations for the next
    ``schedule_count`` Chromium/Skia milestones' preview/RC/stable
    SkiaSharp milestones.

    A Chromium schedule fetch failure for one milestone number (a network
    error, an incomplete/unpublished schedule) is recorded as a warning and
    that milestone number is simply skipped -- it never raises and never
    blocks the rest of closeout (PR/issue reconciliation, milestone
    rollover/closure, and the release-notes/issue-template dispatches all
    proceed independently of schedule-fetch success).
    """

    warnings: list[str] = []
    desired: list[milestones.DesiredMilestone] = []
    major, current_milestone = _read_current_major_and_milestone(repo)
    for number in range(current_milestone, current_milestone + schedule_count):
        try:
            schedule = schedule_client.fetch_schedule(number)
        except milestones.MilestoneError as exc:
            warnings.append(f"could not fetch Chromium schedule for m{number}: {exc}")
            continue
        desired.extend(milestones.desired_milestones(schedule, milestone=number, major=major))
    operations = milestones.plan_schedule_operations(desired, existing, today=date.today())
    return operations, warnings


def plan_closeout(
    plan: dict,
    *,
    repo: GitRepository,
    milestone_client: milestones.MilestoneClient,
    github: GitHubClient,
    schedule_client: milestones.ScheduleClient,
    tags: list[str],
    schedule_count: int = DEFAULT_SCHEDULE_MILESTONE_COUNT,
) -> dict:
    """Build the read-only closeout plan for the release described by ``plan``.

    ``plan`` is the finish-plan.json produced earlier in the pipeline; it
    supplies the standardized envelope context (tooling SHA, release
    identity/version/branch, plan digest). The actual schedule/PR/issue
    reconciliation and milestone-advancement operations are always
    recomputed from live Git/milestone/tag/Chromium-schedule state, since
    closeout may run long after the plan was generated. Before any of
    that, :func:`_require_release_is_shipped` reverifies the release has
    actually shipped (exact tag, published release) -- this is a
    read-only preview, but it must never present a misleadingly "safe"
    plan for a release that has not actually shipped.

    The schedule (create/update the upcoming preview/RC/stable
    milestones) is planned *before* rollover/closure, exactly as it would
    be applied, so a milestone about to be created shows up as a valid
    move-to target in this same preview (see ``creatable_titles`` on
    :func:`release_milestones.plan_closeout`).
    """

    _require_release_is_shipped(plan, repo=repo, github=github)

    all_milestones = milestone_client.milestones()
    existing_map = milestones.milestone_map(all_milestones)
    schedule_ops, schedule_warnings = _plan_schedule_operations_for_release(
        repo, schedule_client=schedule_client, schedule_count=schedule_count, existing=existing_map
    )
    creatable_titles = frozenset(
        op.title for op in schedule_ops if op.action == "create" and op.status == "pending"
    )

    reconcile_ops, reconcile_warnings = _reconcile_operations_for_release(
        plan, repo=repo, milestone_client=milestone_client, all_milestones=all_milestones
    )
    advance_ops, advance_warnings = milestones.plan_closeout(
        milestones=all_milestones,
        tags=tags,
        open_items_for=milestone_client.open_milestone_items,
        creatable_titles=creatable_titles,
    )
    next_action = _closeout_next_action(advance_ops, applied=False)
    if next_action == "done" and (reconcile_ops or any(op.status == "pending" for op in schedule_ops)):
        next_action = "closeout"
    return build_envelope(
        plan,
        next_action=next_action,
        scheduleOperations=[_schedule_operation_summary(op) for op in schedule_ops],
        reconcileOperations=[_reconcile_operation_summary(op) for op in reconcile_ops],
        operations=[_closeout_operation_summary(op) for op in advance_ops],
        releaseNotesDispatch=_release_notes_dispatch_inputs(plan),
        issueTemplateRefreshNeeded=bool(plan["release"]["stable"]),
        warnings=[*schedule_warnings, *reconcile_warnings, *advance_warnings],
    )


def apply_closeout(
    plan: dict,
    *,
    repo: GitRepository,
    milestone_client: milestones.MilestoneClient,
    github: GitHubClient,
    schedule_client: milestones.ScheduleClient,
    tags: list[str],
    schedule_count: int = DEFAULT_SCHEDULE_MILESTONE_COUNT,
) -> dict:
    """Apply closeout: create/update the upcoming preview/RC/stable
    milestones from the Chromium/Skia schedule, reconcile merged PRs/issues
    for the exact shipped release to its own milestone, move any remaining
    open items off shipped milestones and close them, then dispatch the
    release-notes sync and (for a stable release) the issue-template
    refresh workflows.

    The schedule step runs *before* rollover/closure and is applied first:
    a milestone it creates becomes a real, numbered milestone immediately,
    so it is available as a genuine move-to target for the closure step
    that follows in this same invocation (milestones are re-fetched after
    the schedule writes, specifically so this works). A Chromium schedule
    fetch failure for one milestone number never raises and never blocks
    anything else here -- it is recorded as a warning and that milestone
    number is simply skipped (see
    :func:`_plan_schedule_operations_for_release`); PR/issue reconciliation,
    milestone rollover/closure, and both workflow dispatches all still run.

    The two workflow dispatches are unconditional on every successful
    invocation of this function -- never gated on there being new
    milestone reconcile/advance/schedule work, and never gated on the
    milestone advancement or schedule maintenance having fully succeeded
    (a "blocked" milestone -- no eligible target for its open items --
    or a schedule-fetch warning does not withhold the dispatch). A first
    closeout for a release with no milestone activity at all must still
    generate its release notes; and if a prior invocation completed the
    milestone writes but then failed before reaching the dispatch (a
    crash, a network blip, an expired token), a rerun that finds no new
    milestone work left to do must not silently skip the dispatch too --
    that would leave a published release with no notes and no way to
    recover except by hand. Both dispatch targets
    (``update-release-notes.lock.yml``, ``auto-update-issue-template-
    versions.yml``) are themselves convergent/idempotent, so redispatching
    them on every rerun is always safer than silently missing one.
    Milestone reconciliation/advancement/schedule maintenance itself is
    still only-write-what's-needed idempotent: rerunning after everything
    is already reconciled/advanced/up to date performs no further writes.

    Before any of that, :func:`_require_release_is_shipped` reverifies
    live state -- the exact tag still resolves to the receipt's source
    commit, and a matching GitHub Release exists and is published, not a
    draft. ``finish closeout`` is a public CLI; it must never close
    milestones just because it was handed a schema-valid,
    digest-verified finish plan whose publish state is not (or is not
    yet) actually true.
    """

    _require_release_is_shipped(plan, repo=repo, github=github)

    all_milestones = milestone_client.milestones()
    existing_map = milestones.milestone_map(all_milestones)
    schedule_ops, schedule_warnings = _plan_schedule_operations_for_release(
        repo, schedule_client=schedule_client, schedule_count=schedule_count, existing=existing_map
    )
    schedule_results = milestones.apply_schedule_operations(schedule_ops, milestone_client)

    # Re-fetch: any milestone just created/updated above must be visible --
    # with a real number -- to the reconcile/advance steps below.
    all_milestones = milestone_client.milestones()

    reconcile_ops, reconcile_warnings = _reconcile_operations_for_release(
        plan, repo=repo, milestone_client=milestone_client, all_milestones=all_milestones
    )
    milestones.apply_reconcile(reconcile_ops, milestone_client)

    advance_ops, advance_warnings = milestones.plan_closeout(
        milestones=all_milestones,
        tags=tags,
        open_items_for=milestone_client.open_milestone_items,
    )
    pending = [op for op in advance_ops if op.status == "pending"]
    blocked = [op for op in advance_ops if op.status == "blocked"]
    results = milestones.apply_closeout(pending, milestone_client)
    for op in blocked:
        results.append({"milestone": op.milestone_title, "status": "blocked", "detail": op.detail})

    dispatches: list[dict] = []
    notes_inputs = _release_notes_dispatch_inputs(plan)
    github.dispatch_workflow(workflow=UPDATE_RELEASE_NOTES_WORKFLOW, ref="main", inputs=notes_inputs)
    dispatches.append(
        {"workflow": UPDATE_RELEASE_NOTES_WORKFLOW, "inputs": notes_inputs, "status": "dispatched"}
    )
    if plan["release"]["stable"]:
        github.dispatch_workflow(workflow=ISSUE_TEMPLATE_REFRESH_WORKFLOW, ref="main", inputs={})
        dispatches.append(
            {"workflow": ISSUE_TEMPLATE_REFRESH_WORKFLOW, "inputs": {}, "status": "dispatched"}
        )

    return build_envelope(
        plan,
        next_action=_closeout_next_action(advance_ops, applied=True),
        scheduleResults=schedule_results,
        reconcileResults=[_reconcile_operation_summary(op) for op in reconcile_ops],
        results=results,
        dispatches=dispatches,
        warnings=[*schedule_warnings, *reconcile_warnings, *advance_warnings],
    )
