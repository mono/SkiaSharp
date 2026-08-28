#!/usr/bin/env python3
"""Regenerates the fixed golden JSON vectors under this directory using the
*actual* production `scripts/infra/release` Python modules -- never a
hand-rolled re-implementation of the canonical-JSON/digest rules.

These files exist purely to prove the C# port (SkiaSharp.ReleaseTool) is
byte-for-byte and digest-for-digest compatible with the Python tool it is
migrating away from. They are not executed as part of any test run; rerun
this script only if the golden vectors need to be regenerated (e.g. after
an intentional, reviewed change to a schema or to canonical_json itself).

Usage (from the repository root):
    python3 utils/SkiaSharp.ReleaseTool.Tests/GoldenVectors/generate_golden_vectors.py
"""

from __future__ import annotations

import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(REPO_ROOT / "scripts" / "infra" / "release"))

import release_common as common  # noqa: E402

OUT_DIR = Path(__file__).resolve().parent

# A café/🎉-flavoured string exercising both a BMP non-ASCII code point
# (é, ☕) and a non-BMP one (🎉, which `ensure_ascii=True` must render as a
# UTF-16 surrogate pair \ud83c\udf89) in the same fixture.
UNICODE_NOTE = "Migration note: café ☕ review complete 🎉 (non-ASCII + surrogate pair)"


def write_plan(name: str, plan: dict, schema_name: str) -> None:
    path = OUT_DIR / name
    common.write_plan(path, plan, schema_name=schema_name)
    print(f"wrote {path.relative_to(REPO_ROOT)}")


def write_result(name: str, document: dict, *, validate=None) -> None:
    if validate is not None:
        validate(document)
    path = OUT_DIR / name
    common.write_json_file(path, document)
    print(f"wrote {path.relative_to(REPO_ROOT)}")


def build_prepare_plan() -> dict:
    return {
        "schemaVersion": 1,
        "operation": "prepare",
        "generatedAt": "2025-06-01T12:00:00Z",
        "toolingSha": "a" * 40,
        "nextAction": "apply",
        "input": {
            "integrationTarget": "release/3.119.x",
            # Nullable-but-required: the auto-detected-next-preview path
            # never received an explicit --version.
            "requestedVersion": None,
        },
        "release": {
            "identity": "3.119.0-preview.2",
            "version": "3.119.0-preview.2",
            "numeric": "3.119.0",
            "label": "preview.2",
            "releaseType": "preview",
            "branch": "release/3.119.0-preview.2",
            "integrationBranch": "release/3.119.x",
            "isHotfix": False,
            "stable": False,
        },
        "base": {
            "ref": "refs/remotes/origin/release/3.119.x",
            "sha": "b" * 40,
        },
        "maintenanceBranch": {
            "name": "release/3.119.x",
            "exists": True,
            "action": "none",
            # Nullable-but-required: no maintenance-branch creation is
            # planned, so there is no base SHA for one.
            "baseSha": None,
        },
        "skia": {
            "sha": "c" * 40,
            "releaseBranch": "release/3.119.0-preview.2",
            "remoteState": "missing",
        },
        "skiaSharpRemoteState": "missing",
        "versions": {
            "skiaSharp": "3.119.0",
            "requiresPackageBump": False,
        },
        "operations": [
            {
                "id": "create-maintenance-branch",
                "kind": "git-ref",
                "status": "done",
                "detail": "release/3.119.x",
            },
            {
                "id": "create-skia-ref",
                "kind": "github-ref",
                "status": "pending",
                "detail": f"mono/skia:release/3.119.0-preview.2@{'c' * 40}",
            },
            {
                "id": "create-release-branch",
                "kind": "git-ref",
                "status": "pending",
                # `detail` is optional per the schema; omitted here to
                # prove the C# port tolerates its absence.
            },
        ],
        # Nullable-but-required: null here because this fixture is a
        # preview cut (`release_type != "stable"`); only a stable cut of
        # a non-hotfix release ever populates `stableBump` (see
        # `plan_stable_bump`/`StableBumpInfo`'s XML doc for why every
        # nested field is always present together once it does).
        "stableBump": None,
        "warnings": [UNICODE_NOTE],
    }


def build_finish_plan() -> dict:
    return {
        "schemaVersion": 1,
        "operation": "finish",
        "generatedAt": "2025-06-02T08:30:00Z",
        "toolingSha": "d" * 40,
        "nextAction": "plan-publication",
        "input": {"requestedVersion": "3.119.0-preview.2.12345.7"},
        "receipt": {
            "skiaSharpVersion": "3.119.0-preview.2.12345.7",
            "base": "3.119.0",
            "label": "preview.2",
            # Nullable-but-required: a five-digit date-prefixed build
            # revision is present here, so this fixture instead exercises
            # the null case on `tag.existingSha`/`previousTag` below.
            "buildRevision": "12345.7",
            "sourceCommit": "e" * 40,
            "sourceBranch": "release/3.119.0-preview.2",
            "harfBuzzSharpVersion": "1.8.8.3",
            "packages": [
                {
                    "id": "SkiaSharp",
                    "version": "3.119.0-preview.2.12345.7",
                    "sourceCommit": "e" * 40,
                    "sourceBranch": "release/3.119.0-preview.2",
                },
                {
                    "id": "HarfBuzzSharp",
                    "version": "1.8.8.3",
                    "sourceCommit": "e" * 40,
                    "sourceBranch": "release/3.119.0-preview.2",
                },
            ],
        },
        "release": {
            "identity": "3.119.0-preview.2",
            "version": "3.119.0-preview.2.12345.7",
            "branch": "release/3.119.0-preview.2",
            "raw": "3.119.0-preview.2",
            "numeric": "3.119.0",
            "label": "preview.2",
            "releaseType": "preview",
            "stable": False,
            "title": "Version 3.119.0 (Preview 2)",
            "tag": "v3.119.0-preview.2",
        },
        "tag": {
            "name": "v3.119.0-preview.2",
            "targetCommit": "e" * 40,
            # Nullable-but-required: the tag does not exist on the
            # remote yet.
            "existingSha": None,
            "status": "pending",
        },
        # Nullable-but-required: the very first tag ever cut has no
        # predecessor.
        "previousTag": None,
        "draft": {
            "exists": False,
            "isPublished": False,
            "status": "pending",
            "hasManagedMarkers": False,
        },
        "warnings": [UNICODE_NOTE],
    }


def build_finish_pending_report() -> dict:
    return {
        "schemaVersion": 1,
        "operation": "finish-plan-pending",
        "generatedAt": "2025-06-02T08:35:00Z",
        "toolingSha": "f" * 40,
        "nextAction": "pending",
        "requestedVersion": "3.119.0-preview.2.12345.7",
        "missingPackages": [
            {"id": "SkiaSharp", "version": "3.119.0-preview.2.12345.7"},
            {"id": "HarfBuzzSharp", "version": "1.8.8.3"},
        ],
        "elapsedSeconds": 1234.5,
        "deadlineSeconds": 1200.0,
        "message": f"2 package(s) not yet visible/listed on NuGet.org. {UNICODE_NOTE}",
    }


def build_result_envelope() -> dict:
    return {
        "toolingSha": "1" * 40,
        "planDigest": "2" * 64,
        "nextAction": "done",
        "release": {
            "identity": "3.119.0-preview.2",
            "version": "3.119.0-preview.2.12345.7",
            "branch": "release/3.119.0-preview.2",
        },
        # Forward-extensibility probe: a field no schema/DTO here knows
        # about ahead of time, which the C# port must preserve via
        # JsonExtensionData rather than reject or silently drop.
        "note": UNICODE_NOTE,
    }


def build_publication_report() -> dict:
    return {
        "toolingSha": "3" * 40,
        "planDigest": "4" * 64,
        "nextAction": "closeout",
        "release": {
            "identity": "3.119.0-preview.2",
            "version": "3.119.0-preview.2.12345.7",
            "branch": "release/3.119.0-preview.2",
        },
        "tag": "v3.119.0-preview.2",
        "draftUrl": "https://github.com/mono/SkiaSharp/releases/tag/v3.119.0-preview.2",
        "isDraft": False,
        "isPublished": True,
        "bodySha256": "5" * 64,
        "hasManagedMarkers": True,
        "readyToPublish": False,
        "note": UNICODE_NOTE,
    }


def build_plan_summary() -> dict:
    return {
        "schemaVersion": 1,
        "operation": "finish",
        "toolingSha": "6" * 40,
        "nextAction": "plan-publication",
        "releaseIdentity": "3.119.0-preview.2",
        "releaseBranch": "release/3.119.0-preview.2",
        "releaseVersion": "3.119.0-preview.2.12345.7",
        "planDigest": "7" * 64,
        "warnings": [UNICODE_NOTE, "second warning, plain ASCII"],
    }


def build_prepare_plan_stable_bump() -> dict:
    """A second prepare-plan fixture for a *stable* cut, exercising the
    non-null `stableBump` shape (never covered by the primary preview
    fixture above, since only a non-hotfix stable release populates it).
    """

    plan = build_prepare_plan()
    plan["generatedAt"] = "2025-06-03T09:00:00Z"
    plan["input"] = {"integrationTarget": "release/3.119.x", "requestedVersion": "3.119.0"}
    plan["release"] = {
        "identity": "3.119.0",
        "version": "3.119.0",
        "numeric": "3.119.0",
        "label": "stable",
        "releaseType": "stable",
        "branch": "release/3.119.0",
        "integrationBranch": "release/3.119.x",
        "isHotfix": False,
        "stable": True,
    }
    plan["maintenanceBranch"]["baseSha"] = "b" * 40
    plan["skia"]["releaseBranch"] = "release/3.119.0"
    plan["versions"]["skiaSharp"] = "3.119.0"
    plan["operations"].append(
        {
            "id": "open-stable-bump-pr",
            "kind": "github-pull-request",
            "status": "awaiting-user",
            "detail": "bump-version-3.119.1",
        }
    )
    plan["stableBump"] = {
        "integrationBranch": "release/3.119.x",
        "bumpBranch": "bump-version-3.119.1",
        "skiaSharpVersion": "3.119.1",
        "harfBuzzSharpVersion": "1.8.8.4",
        "status": "awaiting-user",
        "pullRequestUrl": "https://github.com/mono/SkiaSharp/pull/9999",
        "title": "Bump to the next version (3.119.1) after release",
    }
    return plan


def main() -> None:
    write_plan("prepare-plan.golden.json", build_prepare_plan(), "prepare-plan.schema.json")
    write_plan(
        "prepare-plan-stable-bump.golden.json", build_prepare_plan_stable_bump(), "prepare-plan.schema.json"
    )
    write_plan("finish-plan.golden.json", build_finish_plan(), "finish-plan.schema.json")
    write_result(
        "finish-pending-report.golden.json",
        build_finish_pending_report(),
        validate=lambda d: common.validate_against_schema(d, "finish-pending.schema.json"),
    )
    write_result(
        "result-envelope.golden.json",
        build_result_envelope(),
        validate=common.validate_result_envelope,
    )
    write_result(
        "publication-report.golden.json",
        build_publication_report(),
        validate=common.validate_result_envelope,
    )
    write_result(
        "plan-summary.golden.json",
        build_plan_summary(),
        validate=lambda d: common.validate_against_schema(d, "plan-summary.schema.json"),
    )


if __name__ == "__main__":
    main()
