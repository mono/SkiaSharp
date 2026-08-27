"""Deterministic plan/result-summary rendering for thin GitHub Actions workflows.

A prepare/finish plan -- and every command result derived from one (apply,
create-draft, plan-publication, publish, closeout) -- carries the same
standardized workflow-facing envelope: top-level ``toolingSha``,
``planDigest``, ``nextAction``, and a nested ``release`` object with
``identity``/``version``/``branch`` (see ``release_common.build_envelope``
and ``schemas/result-envelope.schema.json``). A workflow step that only
needs to set a handful of job outputs should not have to know the full
plan/result schema or scrape stdout with ad hoc ``jq`` paths scattered
across YAML. :func:`summarize_document` derives one small, flat,
schema-versioned surface from *either* shape; ``release.py render-plan`` is
its CLI.
"""

from __future__ import annotations

from release_common import PlanError

SUMMARY_SCHEMA = "plan-summary.schema.json"
SUMMARY_SCHEMA_VERSION = 1

_REQUIRED_TOP_LEVEL = ("toolingSha", "planDigest", "nextAction")
_REQUIRED_RELEASE = ("identity", "version", "branch")


def summarize_document(document: dict) -> dict:
    """Derive the deterministic, flat summary surface from a standardized
    plan or command-result document.

    ``document`` must already carry the standardized envelope (schema- and
    digest-verified for a plan via :func:`release_common.read_plan`, or
    schema-validated for a result via
    :func:`release_common.validate_result_envelope`); this function only
    projects fields that are already present, it never re-derives them from
    untrusted input.
    """

    release = document.get("release") or {}
    missing = [field for field in _REQUIRED_TOP_LEVEL if field not in document]
    missing += [f"release.{field}" for field in _REQUIRED_RELEASE if field not in release]
    if missing:
        raise PlanError(
            "document is missing required standardized field(s): " + ", ".join(missing)
        )
    summary = {
        "schemaVersion": SUMMARY_SCHEMA_VERSION,
        "toolingSha": document["toolingSha"],
        "planDigest": document["planDigest"],
        "nextAction": document["nextAction"],
        "releaseIdentity": release["identity"],
        "releaseVersion": release["version"],
        "releaseBranch": release["branch"],
        "warnings": list(document.get("warnings", [])),
    }
    operation = document.get("operation")
    if operation is not None:
        summary["operation"] = operation
    return summary
