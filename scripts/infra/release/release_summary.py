"""Deterministic plan-summary rendering for thin GitHub Actions workflows.

A prepare/finish plan file is the full audit record, but a workflow step
that only needs to set a handful of job outputs (tooling SHA, concurrency
key, exact release branch/version, plan digest) should not have to know the
internal plan schema or scrape stdout with ad hoc ``jq`` paths scattered
across YAML. :func:`summarize_plan` derives one small, flat, schema-versioned
surface for exactly that purpose; ``release.py render-plan`` is its CLI.
"""

from __future__ import annotations

from release_common import PlanError

SUMMARY_SCHEMA = "plan-summary.schema.json"
SUMMARY_SCHEMA_VERSION = 1


def _summarize_prepare(plan: dict) -> dict:
    release = plan["release"]
    return {
        # The exact release version doubles as the normalized identity for
        # prepare: it never carries a CI build revision.
        "releaseIdentity": release["version"],
        "releaseBranch": release["releaseBranch"],
        "releaseVersion": release["version"],
    }


def _summarize_finish(plan: dict) -> dict:
    release = plan["release"]
    receipt = plan["receipt"]
    return {
        # release["raw"] is the release-branch identity (no CI build
        # revision); receipt["sourceBranch"] is the branch actually embedded
        # in and verified against the published package, which is the exact
        # branch a workflow should report/act on.
        "releaseIdentity": release["raw"],
        "releaseBranch": receipt["sourceBranch"],
        "releaseVersion": plan["input"]["requestedVersion"],
    }


_SUMMARIZERS = {
    "prepare": _summarize_prepare,
    "finish": _summarize_finish,
}


def summarize_plan(plan: dict) -> dict:
    """Derive the deterministic, flat plan-summary surface from a plan dict.

    ``plan`` must already be schema-validated and digest-verified (for
    example via :func:`release_common.read_plan`); this function only
    projects fields that are already present, it never re-derives them from
    untrusted input.
    """

    operation = plan.get("operation")
    summarizer = _SUMMARIZERS.get(operation)
    if summarizer is None:
        raise PlanError(
            f"cannot render a plan summary for unknown operation {operation!r}; "
            f"expected one of {sorted(_SUMMARIZERS)}"
        )
    fields = summarizer(plan)
    return {
        "schemaVersion": SUMMARY_SCHEMA_VERSION,
        "operation": operation,
        "toolingSha": plan["toolingSha"],
        "digest": plan["digest"],
        "warnings": list(plan.get("warnings", [])),
        **fields,
    }
