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


_CONSUMED_TOP_LEVEL_KEYS = frozenset(
    {
        "schemaVersion",
        "operation",
        "generatedAt",
        "toolingSha",
        "planDigest",
        "nextAction",
        "input",
        "release",
        "warnings",
        "scheduleOperations",
        "scheduleResults",
        "operations",
        "results",
        "receipt",
        "tag",
        "previousTag",
        "draft",
        "stableBump",
        "base",
        "maintenanceBranch",
        "skia",
        "skiaSharpRemoteState",
        "versions",
    }
)

_SCHEDULE_COLUMNS = ("title", "number", "action", "status", "dueOn", "description", "changes")
_OPERATIONS_COLUMNS = (
    "id",
    "kind",
    "status",
    "milestone",
    "tag",
    "openItemCount",
    "moveTo",
    "movedTo",
    "detail",
    "pullRequestUrl",
)
_RESULTS_COLUMNS = ("milestone", "status", "movedTo", "detail")
_PACKAGES_COLUMNS = ("id", "version", "sourceCommit", "sourceBranch")


def _format_cell(value: object) -> str:
    if value is None:
        return ""
    if isinstance(value, bool):
        return "yes" if value else "no"
    if isinstance(value, (dict, list)):
        import json

        text = json.dumps(value, sort_keys=True, separators=(",", ": "))
    else:
        text = str(value)
    return text.replace("|", "\\|").replace("\n", " ")


def _markdown_table(rows: list, *, preferred_columns: tuple[str, ...]) -> str:
    if not rows:
        return "_none_\n"
    columns = list(preferred_columns)
    for row in rows:
        for key in sorted(row.keys()):
            if key not in columns:
                columns.append(key)
    columns = [column for column in columns if any(column in row for row in rows)]
    header = "| " + " | ".join(columns) + " |"
    divider = "| " + " | ".join("---" for _ in columns) + " |"
    lines = [header, divider]
    for row in rows:
        lines.append("| " + " | ".join(_format_cell(row.get(column)) for column in columns) + " |")
    return "\n".join(lines) + "\n"


def _markdown_fields(fields: dict) -> str:
    if not fields:
        return "_none_\n"
    lines = ["| Field | Value |", "| --- | --- |"]
    for key in sorted(fields):
        lines.append(f"| `{key}` | {_format_cell(fields[key])} |")
    return "\n".join(lines) + "\n"


def render_markdown(document: dict) -> str:
    """Render a deterministic, human-readable Markdown report for a
    standardized plan or command-result document.

    Section order and column layout are fixed regardless of the source
    document's own key order (plan/result files are written with sorted
    keys), so the same document always renders identically. Only sections
    whose underlying data is actually present in ``document`` are emitted;
    any top-level field not recognized by a dedicated section is still
    reported, under "Additional fields", so nothing is silently dropped.
    """

    summary = summarize_document(document)
    release = document.get("release") or {}
    lines: list[str] = []

    lines.append(f"# Release {summary['releaseIdentity']}")
    lines.append("")
    overview = {
        "Tooling SHA": summary["toolingSha"],
        "Plan digest": summary["planDigest"],
        "Next action": summary["nextAction"],
        "Release identity": release.get("identity"),
        "Release version": release.get("version"),
        "Release branch": release.get("branch"),
    }
    if "operation" in summary:
        overview["Operation"] = summary["operation"]
    lines.append("## Summary")
    lines.append("")
    lines.append(_markdown_fields(overview).rstrip("\n"))
    lines.append("")

    lines.append("## Warnings")
    lines.append("")
    if summary["warnings"]:
        lines.extend(f"- {warning}" for warning in summary["warnings"])
    else:
        lines.append("_none_")
    lines.append("")

    if isinstance(document.get("scheduleOperations"), list):
        lines.append("## Schedule")
        lines.append("")
        lines.append(
            _markdown_table(document["scheduleOperations"], preferred_columns=_SCHEDULE_COLUMNS).rstrip("\n")
        )
        lines.append("")

    if isinstance(document.get("scheduleResults"), list):
        lines.append("## Schedule results")
        lines.append("")
        lines.append(
            _markdown_table(document["scheduleResults"], preferred_columns=_SCHEDULE_COLUMNS).rstrip("\n")
        )
        lines.append("")

    if isinstance(document.get("operations"), list):
        lines.append("## Operations")
        lines.append("")
        lines.append(_markdown_table(document["operations"], preferred_columns=_OPERATIONS_COLUMNS).rstrip("\n"))
        lines.append("")

    if isinstance(document.get("results"), list):
        lines.append("## Results")
        lines.append("")
        lines.append(_markdown_table(document["results"], preferred_columns=_RESULTS_COLUMNS).rstrip("\n"))
        lines.append("")

    receipt = document.get("receipt")
    if isinstance(receipt, dict):
        lines.append("## Receipt")
        lines.append("")
        receipt_fields = {k: v for k, v in receipt.items() if k != "packages"}
        lines.append(_markdown_fields(receipt_fields).rstrip("\n"))
        lines.append("")
        if isinstance(receipt.get("packages"), list):
            lines.append("### Packages")
            lines.append("")
            lines.append(_markdown_table(receipt["packages"], preferred_columns=_PACKAGES_COLUMNS).rstrip("\n"))
            lines.append("")

    tag = document.get("tag")
    if tag is not None or "previousTag" in document:
        lines.append("## Tag")
        lines.append("")
        if isinstance(tag, dict):
            lines.append(_markdown_fields(tag).rstrip("\n"))
        elif tag is not None:
            lines.append(_markdown_fields({"tag": tag}).rstrip("\n"))
        if "previousTag" in document:
            lines.append("")
            lines.append(f"Previous tag: `{document['previousTag']}`" if document["previousTag"] else "Previous tag: _none_")
        lines.append("")

    draft = document.get("draft")
    if isinstance(draft, dict):
        lines.append("## Draft")
        lines.append("")
        lines.append(_markdown_fields(draft).rstrip("\n"))
        lines.append("")

    stable_bump = document.get("stableBump")
    if isinstance(stable_bump, dict):
        lines.append("## Stable bump")
        lines.append("")
        lines.append(_markdown_fields(stable_bump).rstrip("\n"))
        lines.append("")

    extra = {
        key: value
        for key, value in document.items()
        if key not in _CONSUMED_TOP_LEVEL_KEYS
    }
    if extra:
        lines.append("## Additional fields")
        lines.append("")
        lines.append(_markdown_fields(extra).rstrip("\n"))
        lines.append("")

    return "\n".join(lines).rstrip("\n") + "\n"
