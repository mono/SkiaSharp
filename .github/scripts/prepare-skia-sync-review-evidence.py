#!/usr/bin/env python3
"""Build canonical, secret-free Skia sync review evidence from mechanical results."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path
from typing import Any


SECRET_PATTERNS = (
    re.compile(r"-----BEGIN (?:[A-Z ]+ )?PRIVATE KEY-----"),
    re.compile(r"\bgh[pousr]_[A-Za-z0-9_]{20,}\b"),
    re.compile(r"\bgithub_pat_[A-Za-z0-9_]{20,}\b"),
    re.compile(r"\bAKIA[0-9A-Z]{16}\b"),
    re.compile(r"\bxox[baprs]-[A-Za-z0-9-]{20,}\b"),
)
SOURCE_KEYS = {"path", "summary", "diff", "oldDiff", "newDiff", "patchDiff", "relatedFiles"}
DEP_KEYS = {"name", "summary", "url", "revision", "oldUrl", "oldRevision", "newUrl", "newRevision"}


class EvidenceError(ValueError):
    """Evidence cannot be safely published."""


def load(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise EvidenceError(f"{path.name} must contain a JSON object.")
    return value


def require(value: Any, description: str, expected_type: type | tuple[type, ...]) -> Any:
    if not isinstance(value, expected_type):
        raise EvidenceError(f"{description} has the wrong type.")
    return value


def equal(actual: Any, expected: Any, description: str) -> None:
    if actual != expected:
        raise EvidenceError(f"{description} does not match the frozen review contract.")


def require_summary(section: dict[str, Any], name: str) -> tuple[str, list[str]]:
    summary = require(section.get("summary"), f"{name}.summary", str)
    recommendations = require(section.get("recommendations"), f"{name}.recommendations", list)
    if not summary or not all(isinstance(item, str) for item in recommendations):
        raise EvidenceError(f"{name} must contain a summary and string recommendations.")
    return summary, recommendations


def clean_items(items: Any, allowed: set[str], required: str, name: str) -> list[dict[str, Any]]:
    clean: list[dict[str, Any]] = []
    for item in require(items, name, list):
        require(item, name, dict)
        value = item.get(required)
        if not isinstance(value, str) or not value:
            raise EvidenceError(f"{name} item lacks {required}.")
        clean.append({key: item[key] for key in allowed if key in item and key != "summary"})
    return clean


def with_summaries(
    raw_items: list[dict[str, Any]],
    ai_items: Any,
    identity: str,
    name: str,
) -> list[dict[str, Any]]:
    ai_items = require(ai_items, name, list)
    if len(raw_items) != len(ai_items):
        raise EvidenceError(f"{name} item count does not match mechanical evidence.")
    result = []
    for raw_item, ai_item in zip(raw_items, ai_items):
        require(ai_item, name, dict)
        equal(ai_item.get(identity), raw_item[identity], f"{name}.{identity}")
        summary = require(ai_item.get("summary"), f"{name}.summary", str)
        if not summary:
            raise EvidenceError(f"{name} item summary is empty.")
        result.append({**raw_item, "summary": summary})
    return result


def source_section(raw: dict[str, Any], ai: dict[str, Any], name: str) -> dict[str, Any]:
    added = with_summaries(
        clean_items(raw.get("added", []), SOURCE_KEYS, "path", f"{name}.added"),
        ai.get("added", []), "path", f"{name}.added",
    )
    removed = with_summaries(
        clean_items(raw.get("removed", []), SOURCE_KEYS, "path", f"{name}.removed"),
        ai.get("removed", []), "path", f"{name}.removed",
    )
    changed = with_summaries(
        clean_items(raw.get("changed", []), SOURCE_KEYS, "path", f"{name}.changed"),
        ai.get("changed", []), "path", f"{name}.changed",
    )
    unchanged = require(raw.get("unchanged"), f"{name}.unchanged", int)
    status = "REVIEW_REQUIRED" if added or removed or changed else "PASS"
    equal(raw.get("status"), status, f"{name}.status")
    summary, recommendations = require_summary(ai, name)
    return {
        "status": status,
        "summary": summary,
        "recommendations": recommendations,
        "added": added,
        "removed": removed,
        "changed": changed,
        "unchanged": unchanged,
    }


def deps_section(raw: dict[str, Any], ai: dict[str, Any]) -> dict[str, Any]:
    added = with_summaries(
        clean_items(raw.get("added", []), DEP_KEYS, "name", "depsAudit.added"),
        ai.get("added", []), "name", "depsAudit.added",
    )
    removed = with_summaries(
        clean_items(raw.get("removed", []), DEP_KEYS, "name", "depsAudit.removed"),
        ai.get("removed", []), "name", "depsAudit.removed",
    )
    changed = with_summaries(
        clean_items(raw.get("changed", []), DEP_KEYS, "name", "depsAudit.changed"),
        ai.get("changed", []), "name", "depsAudit.changed",
    )
    unchanged = require(raw.get("unchanged"), "depsAudit.unchanged", int)
    status = "REVIEW_REQUIRED" if added or removed or changed else "PASS"
    equal(raw.get("status"), status, "depsAudit.status")
    summary, recommendations = require_summary(ai, "depsAudit")
    return {
        "status": status,
        "summary": summary,
        "recommendations": recommendations,
        "added": added,
        "removed": removed,
        "changed": changed,
        "unchanged": unchanged,
    }


def generated_section(raw: dict[str, Any], ai: dict[str, Any]) -> dict[str, Any]:
    status = raw.get("status")
    if status not in {"PASS", "FAIL"}:
        raise EvidenceError("generatedFiles.status must be PASS or FAIL.")
    checked = require(raw.get("checked"), "generatedFiles.checked", list)
    if not all(isinstance(item, str) for item in checked):
        raise EvidenceError("generatedFiles.checked must contain strings.")
    result: dict[str, Any] = {"status": status, "checked": checked}
    for key in ("mismatches", "generatorError"):
        if key in raw:
            result[key] = raw[key]
    summary, recommendations = require_summary(ai, "generatedFiles")
    result["summary"] = summary
    result["recommendations"] = recommendations
    return result


def companion_section(raw: dict[str, Any], ai: dict[str, Any], parent_head: str, parent_pr: int) -> dict[str, Any]:
    added = with_summaries(
        clean_items(raw.get("added", []), SOURCE_KEYS, "path", "companionPr.added"),
        ai.get("added", []), "path", "companionPr.added",
    )
    changed = with_summaries(
        clean_items(raw.get("changed", []), SOURCE_KEYS, "path", "companionPr.changed"),
        ai.get("changed", []), "path", "companionPr.changed",
    )
    unchanged = require(raw.get("unchanged"), "companionPr.unchanged", int)
    status = "REVIEW_REQUIRED" if added or changed else "PASS"
    equal(raw.get("status"), status, "companionPr.status")
    equal(raw.get("prNumber"), parent_pr, "companionPr.prNumber")
    summary, recommendations = require_summary(ai, "companionPr")
    return {
        "prNumber": parent_pr,
        "headSha": parent_head,
        "status": status,
        "summary": summary,
        "recommendations": recommendations,
        "added": added,
        "changed": changed,
        "unchanged": unchanged,
    }


def build(raw: dict[str, Any], report: dict[str, Any], contract: dict[str, Any]) -> dict[str, Any]:
    parent = require(contract.get("parent"), "contract.parent", dict)
    native = require(contract.get("native"), "contract.native", dict)
    raw_meta = require(raw.get("meta"), "raw.meta", dict)
    parent_pr, native_pr = parent["number"], native["number"]
    equal(raw_meta.get("skiaPrNumber"), native_pr, "raw native PR")
    equal(raw_meta.get("skiasharpPrNumber"), parent_pr, "raw parent PR")
    raw_shas = require(raw_meta.get("shas"), "raw.meta.shas", dict)
    equal(raw_shas.get("prHead"), native["head_sha"], "raw native head")
    equal(raw_shas.get("base"), native["base_sha"], "raw native base")

    generated = generated_section(require(raw.get("generatedFiles"), "raw.generatedFiles", dict), require(report.get("generatedFiles"), "report.generatedFiles", dict))
    upstream = source_section(require(raw.get("upstreamIntegrity"), "raw.upstreamIntegrity", dict), require(report.get("upstreamIntegrity"), "report.upstreamIntegrity", dict), "upstreamIntegrity")
    interop = source_section(require(raw.get("interopIntegrity"), "raw.interopIntegrity", dict), require(report.get("interopIntegrity"), "report.interopIntegrity", dict), "interopIntegrity")
    deps = deps_section(require(raw.get("depsAudit"), "raw.depsAudit", dict), require(report.get("depsAudit"), "report.depsAudit", dict))
    companion = companion_section(require(raw.get("companionPr"), "raw.companionPr", dict), require(report.get("companionPr"), "report.companionPr", dict), parent["head_sha"], parent_pr)
    risk = "HIGH" if generated["status"] == "FAIL" or upstream["status"] == "REVIEW_REQUIRED" else "MEDIUM" if interop["status"] == "REVIEW_REQUIRED" or deps["status"] == "REVIEW_REQUIRED" or companion["status"] == "REVIEW_REQUIRED" else "LOW"
    summary = require(report.get("summary"), "report.summary", str)
    recommendations = require(report.get("recommendations"), "report.recommendations", list)
    if not summary or not recommendations or not all(isinstance(item, str) for item in recommendations):
        raise EvidenceError("report must contain a summary and string recommendations.")

    return {
        "meta": {
            "schemaVersion": "1.0",
            "skiaPrNumber": native_pr,
            "skiasharpPrNumber": parent_pr,
            "repo": "mono/skia",
            "upstreamBranch": raw_meta["upstreamBranch"],
            "oldUpstreamBranch": raw_meta["oldUpstreamBranch"],
            "analyzedAt": raw_meta["analyzedAt"],
            "shas": {"prHead": native["head_sha"], "base": native["base_sha"], "upstream": raw_shas["upstream"]},
        },
        "summary": summary,
        "recommendations": recommendations,
        "generatedFiles": generated,
        "upstreamIntegrity": upstream,
        "interopIntegrity": interop,
        "depsAudit": deps,
        "riskAssessment": risk,
        "companionPr": companion,
    }


def scan_secrets(value: dict[str, Any]) -> None:
    text = json.dumps(value, sort_keys=True)
    for pattern in SECRET_PATTERNS:
        if pattern.search(text):
            raise EvidenceError("Review evidence contains a credential-like value.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--raw", required=True, type=Path)
    parser.add_argument("--report", required=True, type=Path)
    parser.add_argument("--contract", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    try:
        result = build(load(args.raw), load(args.report), load(args.contract))
        scan_secrets(result)
        args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
        print(hashlib.sha256(args.output.read_bytes()).hexdigest())
    except (EvidenceError, KeyError, OSError, json.JSONDecodeError) as error:
        print(f"prepare-skia-sync-review-evidence: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
