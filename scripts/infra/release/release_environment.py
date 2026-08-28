"""GitHub Actions deployment-environment protection verification.

Read-only, on purpose: this module never creates or modifies a GitHub
environment. Its only job is to let a workflow verify -- as the very first
step inside a protected job, before any write or auth operation -- that a
required protected environment (``release-branching``, ``release-tag``,
``release-publish``) actually exists with the exact protection GitHub
Actions is relying on to gate that job.

If a workflow's ``environment:`` names an environment that does not exist
yet, GitHub silently auto-creates an *unprotected* one on first use and
runs the job anyway -- no required reviewers, no branch restriction, no
self-review prevention. That is exactly the bypass this check exists to
turn into an explicit, loud failure instead of a silent no-op approval
gate. See :func:`check_environment` for the exact policy enforced.
"""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Protocol
from urllib.parse import quote

from release_common import CommandRunner, DEFAULT_RUNNER, ReleaseToolError

GITHUB_REPOSITORY = "mono/SkiaSharp"


class EnvironmentError(ReleaseToolError):
    """An environment's protection configuration could not be read.

    Reserved for a genuine query failure (auth, network, an unexpected API
    shape) -- a *misconfigured* environment is not an error, it is the
    expected "fail" outcome reported via :class:`EnvironmentCheckResult`.
    """


@dataclass(frozen=True)
class RequiredReviewersRule:
    reviewer_count: int
    prevent_self_review: bool


@dataclass(frozen=True)
class BranchPolicy:
    name: str
    kind: str  # "branch" | "tag", exactly GitHub's deployment-branch-policy "type"


@dataclass(frozen=True)
class EnvironmentSnapshot:
    """Everything :func:`check_environment` needs, already parsed from the
    GitHub API's ``environment`` and ``deployment-branch-policies``
    response shapes (see ``GhCliEnvironmentClient``)."""

    name: str
    protection_rule_types: tuple[str, ...]
    required_reviewers: RequiredReviewersRule | None
    protected_branches: bool
    custom_branch_policies: bool
    branch_policies: tuple[BranchPolicy, ...]


class EnvironmentClient(Protocol):
    """Everything the release CLI needs to inspect (never write) a GitHub
    Actions environment's protection configuration."""

    def get_environment(self, name: str) -> EnvironmentSnapshot | None: ...


def _parse_environment(
    name: str, payload: dict, branch_policies: tuple[BranchPolicy, ...]
) -> EnvironmentSnapshot:
    rules = payload.get("protection_rules") or []
    rule_types = tuple(rule["type"] for rule in rules)
    required_reviewers = None
    for rule in rules:
        if rule["type"] == "required_reviewers":
            required_reviewers = RequiredReviewersRule(
                reviewer_count=len(rule.get("reviewers") or []),
                prevent_self_review=bool(rule.get("prevent_self_review", False)),
            )
            break
    # deployment_branch_policy is null when every branch/tag may deploy (no
    # restriction at all), which must be treated as custom_branch_policies
    # being disabled -- not as "no opinion".
    policy_settings = payload.get("deployment_branch_policy") or {}
    return EnvironmentSnapshot(
        name=name,
        protection_rule_types=rule_types,
        required_reviewers=required_reviewers,
        protected_branches=bool(policy_settings.get("protected_branches", False)),
        custom_branch_policies=bool(policy_settings.get("custom_branch_policies", False)),
        branch_policies=branch_policies,
    )


class GhCliEnvironmentClient:
    """The real client, backed by the ``gh`` CLI (never a raw shell string).

    Both reads pass ``-X GET`` explicitly: ``gh api`` silently switches its
    default HTTP method from GET to POST as soon as any ``-f``/``-F`` flag
    is present (used here for the deployment-branch-policies ``per_page``
    pagination parameter).
    """

    def __init__(self, repository: str = GITHUB_REPOSITORY, *, runner: CommandRunner = DEFAULT_RUNNER):
        self.repository = repository
        self.runner = runner

    def get_environment(self, name: str) -> EnvironmentSnapshot | None:
        # Per GitHub's docs: "any slashes in the name must be replaced with
        # %2F" -- environment names here never contain one, but this stays
        # correct regardless.
        encoded = quote(name, safe="")
        result = self.runner.run(
            ["gh", "api", f"repos/{self.repository}/environments/{encoded}", "-X", "GET"],
            cwd=Path.cwd(),
            check=False,
        )
        if not result.ok:
            if "HTTP 404" in result.stderr:
                return None
            raise EnvironmentError(
                f"could not query environment {name!r}: {result.stderr.strip() or result.stdout.strip()}"
            )
        payload = json.loads(result.stdout)
        return _parse_environment(name, payload, self._branch_policies(encoded))

    def _branch_policies(self, encoded_name: str) -> tuple[BranchPolicy, ...]:
        result = self.runner.run(
            [
                "gh", "api",
                f"repos/{self.repository}/environments/{encoded_name}/deployment-branch-policies",
                "-X", "GET", "--paginate", "--slurp", "-f", "per_page=100",
            ],
            cwd=Path.cwd(),
        )
        text = result.stdout.strip()
        pages = json.loads(text) if text else []
        policies: list[BranchPolicy] = []
        for page in pages:
            for item in page.get("branch_policies", []):
                policies.append(BranchPolicy(name=item["name"], kind=item["type"]))
        return tuple(policies)


@dataclass(frozen=True)
class EnvironmentCheckResult:
    """The report ``release.py check-environment`` emits and writes to
    ``--output``. ``ok`` is the single field a thin workflow needs to gate
    on; ``reasons`` explains exactly why when it is ``False``."""

    name: str
    exists: bool
    ok: bool
    reasons: tuple[str, ...]
    default_branch: str
    protection_rule_types: tuple[str, ...]
    allowed_branches: tuple[str, ...]
    reviewer_count: int
    prevent_self_review: bool
    custom_branch_policies: bool


def check_environment(
    snapshot: EnvironmentSnapshot | None, *, name: str, default_branch: str
) -> EnvironmentCheckResult:
    """Validate ``snapshot`` against the fixed policy every protected
    release job requires:

    - the environment exists (GitHub did not silently auto-create it);
    - it has a ``required_reviewers`` protection rule with at least one
      reviewer configured;
    - that rule's ``prevent_self_review`` is enabled;
    - it has custom deployment branch policies enabled (not "protected
      branches" mode, and not "any branch may deploy"); and
    - the exact set of allowed deployment branches is exactly the
      repository's default branch -- no more, no fewer, and no tag
      policies at all.

    Never raises for a misconfigured environment: that is the expected
    "fail" outcome, reported via ``ok``/``reasons`` so a workflow can gate
    on the former and log the latter. Only a genuine usage error (an empty
    ``default_branch``) raises :class:`EnvironmentError`.
    """

    if not default_branch:
        raise EnvironmentError("default_branch must not be empty")

    if snapshot is None:
        return EnvironmentCheckResult(
            name=name,
            exists=False,
            ok=False,
            reasons=(
                "environment does not exist: GitHub would auto-create an "
                "unprotected environment on first use and run the job "
                "without any required reviewers or branch restriction",
            ),
            default_branch=default_branch,
            protection_rule_types=(),
            allowed_branches=(),
            reviewer_count=0,
            prevent_self_review=False,
            custom_branch_policies=False,
        )

    reasons: list[str] = []
    reviewers = snapshot.required_reviewers
    if reviewers is None:
        reasons.append("no 'required_reviewers' protection rule is configured")
    elif reviewers.reviewer_count < 1:
        reasons.append("the 'required_reviewers' protection rule has no reviewers configured")
    if reviewers is not None and not reviewers.prevent_self_review:
        reasons.append("'prevent_self_review' is not enabled on the required_reviewers rule")

    if not snapshot.custom_branch_policies:
        reasons.append(
            "custom deployment branch policies are not enabled "
            f"(protected_branches={snapshot.protected_branches}, "
            f"custom_branch_policies={snapshot.custom_branch_policies})"
        )

    branch_names = tuple(
        sorted(policy.name for policy in snapshot.branch_policies if policy.kind == "branch")
    )
    tag_names = tuple(sorted(policy.name for policy in snapshot.branch_policies if policy.kind == "tag"))
    if tag_names:
        reasons.append(f"tag deployment policies are configured and not allowed: {list(tag_names)}")
    if branch_names != (default_branch,):
        reasons.append(
            f"allowed deployment branches are {list(branch_names)}, expected exactly [{default_branch!r}]"
        )

    return EnvironmentCheckResult(
        name=name,
        exists=True,
        ok=not reasons,
        reasons=tuple(reasons),
        default_branch=default_branch,
        protection_rule_types=snapshot.protection_rule_types,
        allowed_branches=branch_names,
        reviewer_count=reviewers.reviewer_count if reviewers else 0,
        prevent_self_review=reviewers.prevent_self_review if reviewers else False,
        custom_branch_policies=snapshot.custom_branch_policies,
    )


def check_result_to_dict(result: EnvironmentCheckResult) -> dict:
    """The JSON-serializable shape ``release.py check-environment`` emits."""

    return {
        "name": result.name,
        "exists": result.exists,
        "ok": result.ok,
        "reasons": list(result.reasons),
        "defaultBranch": result.default_branch,
        "protectionRuleTypes": list(result.protection_rule_types),
        "allowedBranches": list(result.allowed_branches),
        "reviewerCount": result.reviewer_count,
        "preventSelfReview": result.prevent_self_review,
        "customBranchPolicies": result.custom_branch_policies,
    }
